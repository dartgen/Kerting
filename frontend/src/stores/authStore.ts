import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import router from '@/router';
import { jwtDecode } from 'jwt-decode';
import { isAxiosError } from 'axios';
import type { AuthenticatedUser, TokenPayload, UserProfileResponse } from '@/types/auth';

// Auth store felelőssége:
// - JWT token életciklus kezelése,
// - bejelentkezett felhasználó alapadatai,
// - profil adatok letöltése és cache-elése,
// - auth állapottól függő navigáció támogatása.
export const useAuthStore = defineStore('auth', () => {
  // --- ÁLLAPOT (State) ---
  // A tokenben lévő azonosító + username kivonata.
  const felhasznalo = ref<AuthenticatedUser | null>(null);

  // Teljes profilobjektum, amit a backend /GetMyProfile endpoint ad vissza.
  const profilAdatok = ref<UserProfileResponse | null>(null);

  // Iniciális token localStorage-ból, hogy oldalfrissítés után se vesszen el a session.
  const token = ref<string | null>(authService.getToken());

  // UI állapotok: aszinkron folyamat jelzései.
  const isLoading = ref(false);
  const authError = ref<string | null>(null);

  // Egyidejű profil-lekérés deduplikáció: ha már fut egy kérés,
  // ugyanarra a Promise-ra várunk, nem indítunk párhuzamos másodikat.
  const profileRequest = ref<Promise<void> | null>(null);

  // --- SZÁMÍTOTT ADATOK (Getters) ---
  // Logikai auth flag, route guard is ezt figyeli.
  const isAuthenticated = computed(() => !!token.value);

  // Profilkép fájlnév a fejléc/avatar komponensek számára.
  const profileImageName = computed(() => profilAdatok.value?.imgString || null);

  // --- SEGÉDFÜGGVÉNYEK ---
  // JWT token adattartalom dekódolás kliens oldalon.
  // Ha a token invalid, azonnal kijelentkeztetünk, hogy ne maradjon hibás session állapot.
  const decodeUserFromToken = (t: string) => {
    try {
      const decoded = jwtDecode<TokenPayload>(t);
      felhasznalo.value = {
        felhasznaloNev: decoded.sub,
        id: decoded.Id
      };
    } catch (error) {
      console.error("Hiba a token dekódolásakor:", error);
      kijelentkezes();
    }
  };

  // Profil adatletöltés deduplikált módon.
  // A profileRequest mechanizmus miatt route váltáskor sem küldünk felesleges többszörös hívásokat.
  const fetchUserProfile = async () => {
    if (!token.value) {
      profilAdatok.value = null;
      return;
    }

    if (profileRequest.value) {
      await profileRequest.value;
      return;
    }

    // A belső IIFE adja azt a Promise-t, amit más hívók is await-elni tudnak.
    profileRequest.value = (async () => {
      try {
        const response = await authService.getProfile();
        profilAdatok.value = response.data;
      } catch (error: unknown) {
        // A hibát típusra bontjuk, hogy el tudjuk különíteni:
        // - valódi auth hibákat (401),
        // - megszakított kérést,
        // - egyéb infrastrukturális problémákat.
        const status = isAxiosError(error) ? error.response?.status : undefined;
        const errorCode = isAxiosError(error) ? error.code : undefined;
        const errorName = error instanceof Error ? error.name : '';
        const errorMessage = error instanceof Error ? error.message : String(error ?? '');
        const isCanceled =
          errorCode === 'ERR_CANCELED' ||
          errorName === 'CanceledError' ||
          errorMessage.toLowerCase().includes('aborted');

        if (status === 401) {
          // Lejárt/érvénytelen token esetén hard reseteljük az auth állapotot.
          authService.kijelentkezes();
          token.value = null;
          felhasznalo.value = null;
          profilAdatok.value = null;
          return;
        }

        if (isCanceled) {
          return;
        }

        // Nem auth jellegű hibáknál nem logolunk ki automatikusan,
        // de a konzolban nyomot hagyunk hibakereséshez.
        console.error("Nem sikerült lekérni a profiladatokat:", error);
      } finally {
        // Minden ág végén felszabadítjuk a deduplikációs lockot.
        profileRequest.value = null;
      }
    })();

    await profileRequest.value;
  };

  // --- INICIALIZÁLÁS ---
  if (token.value) {
    decodeUserFromToken(token.value);
    void fetchUserProfile(); // Oldalfrissítéskor rögtön lekérjük a képet is
  }

  // --- MŰVELETEK (Actions) ---
  // Bejelentkezési folyamat:
  // 1) token kérés,
  // 2) token tárolás,
  // 3) user adatok dekódolása + profil letöltés,
  // 4) first-login alapján irányítás.
  const bejelentkezes = async (felhasznaloNev: string, jelszo: string) => {
    isLoading.value = true;
    authError.value = null;
    try {
      const response = await authService.bejelentkezes(felhasznaloNev, jelszo);
      token.value = response.data.token;
      localStorage.setItem('userToken', token.value!);

      // A tokenből kivesszük a legalapabb felhasználói azonosító adatokat.
      decodeUserFromToken(token.value!);

      // A vizuális elemek (profilkép, role stb.) helyes megjelenítéséhez azonnal letöltjük a profilt.
      await fetchUserProfile(); // Bejelentkezés után profil betöltése

      // Első bejelentkezéskor profile oldali onboarding jellegű kitöltésre irányítunk.
      const firstLoginRes = await authService.isFirstLogin(felhasznalo.value!.id);
      if (firstLoginRes.data.isFirstLogin) {
        await router.push('/profile');
      } else {
        await router.push('/');
      }
    } catch (error) {
      authError.value = "Hibás felhasználónév vagy jelszó.";
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  // Kijelentkezéskor kliens oldali auth állapot teljes törlése.
  const kijelentkezes = () => {
    authService.kijelentkezes();
    token.value = null;
    felhasznalo.value = null;
    profilAdatok.value = null;
    void router.push('/');
  };

  return {
    token,
    felhasznalo,
    profilAdatok,
    profileImageName,
    isLoading,
    authError,
    isAuthenticated,
    bejelentkezes,
    fetchUserProfile,
    kijelentkezes,
    checkUsername: async (username: string) => (await authService.checkUsername(username)).data,
    regisztracio: async (user: string, pass: string) => await authService.regisztracio(user, pass)
  };
});

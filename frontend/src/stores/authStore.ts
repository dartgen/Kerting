import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import router from '@/router';
import { jwtDecode } from 'jwt-decode';
import type { AuthenticatedUser, TokenPayload, UserProfileResponse } from '@/types/auth';

export const useAuthStore = defineStore('auth', () => {
  // --- ÁLLAPOT (State) ---
  const felhasznalo = ref<AuthenticatedUser | null>(null);
  const profilAdatok = ref<UserProfileResponse | null>(null);
  const token = ref<string | null>(authService.getToken());
  const isLoading = ref(false);
  const authError = ref<string | null>(null);
  const profileRequest = ref<Promise<void> | null>(null);

  // --- SZÁMÍTOTT ADATOK (Getters) ---
  const isAuthenticated = computed(() => !!token.value);

  // Kinyerjük a profilkép fájlnevét a profil adatokból
  const profileImageName = computed(() => profilAdatok.value?.imgString || null);
  // --- SEGÉDFÜGGVÉNYEK ---
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

  const fetchUserProfile = async () => {
    if (!token.value) {
      profilAdatok.value = null;
      return;
    }

    if (profileRequest.value) {
      await profileRequest.value;
      return;
    }

    profileRequest.value = (async () => {
      try {
        const response = await authService.getProfile();
        profilAdatok.value = response.data;
      } catch (error: any) {
        const status = error?.response?.status;
        const isCanceled =
          error?.code === 'ERR_CANCELED' ||
          error?.name === 'CanceledError' ||
          String(error?.message || '').toLowerCase().includes('aborted');

        if (status === 401) {
          authService.kijelentkezes();
          token.value = null;
          felhasznalo.value = null;
          profilAdatok.value = null;
          return;
        }

        if (isCanceled) {
          return;
        }

        console.error("Nem sikerült lekérni a profiladatokat:", error);
      } finally {
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
  const bejelentkezes = async (felhasznaloNev: string, jelszo: string) => {
    isLoading.value = true;
    authError.value = null;
    try {
      const response = await authService.bejelentkezes(felhasznaloNev, jelszo);
      token.value = response.data.token;
      localStorage.setItem('userToken', token.value!);

      decodeUserFromToken(token.value!);
      await fetchUserProfile(); // Bejelentkezés után profil betöltése

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

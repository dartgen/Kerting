import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import router from '@/router';
import { jwtDecode } from 'jwt-decode';

// 1. Definiáljuk a tokenünk felépítését
interface MyTokenPayload {
  sub: string;
  Id: string;
  exp?: number;
  jti?: string;
}

export const useAuthStore = defineStore('auth', () => {
  // --- ÁLLAPOT (State) ---
  const felhasznalo = ref<{ id: string; felhasznaloNev: string } | null>(null);
  const profilAdatok = ref<any | null>(null);
  const token = ref<string | null>(authService.getToken());
  const isLoading = ref(false);
  const authError = ref<string | null>(null);

  // --- SZÁMÍTOTT ADATOK (Getters) ---
  const isAuthenticated = computed(() => !!token.value);

  // Kinyerjük a profilkép fájlnevét a profil adatokból
  const profileImageName = computed(() => profilAdatok.value?.imgString || null);
  // --- SEGÉDFÜGGVÉNYEK ---
  const decodeUserFromToken = (t: string) => {
    try {
      const decoded = jwtDecode<MyTokenPayload>(t);
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
    try {
      const response = await authService.getProfile();
      profilAdatok.value = response.data;
    } catch (error) {
      console.error("Nem sikerült lekérni a profiladatokat:", error);
    }
  };

  // --- INICIALIZÁLÁS ---
  if (token.value) {
    decodeUserFromToken(token.value);
    fetchUserProfile(); // Oldalfrissítéskor rögtön lekérjük a képet is
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
    router.push('/');
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

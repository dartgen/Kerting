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
  const token = ref<string | null>(authService.getToken());
  const isLoading = ref(false);
  const authError = ref<string | null>(null);

  // --- SZÁMÍTOTT ADATOK (Getters) ---
  const isAuthenticated = computed(() => !!token.value);

  // --- SEGÉDFÜGGVÉNYEK ---
  const decodeUserFromToken = (t: string) => {
    try {
      // Itt adjuk át a saját típusunkat a jwtDecode-nak
      const decoded = jwtDecode<MyTokenPayload>(t);

      felhasznalo.value = {
        felhasznaloNev: decoded.sub,
        id: decoded.Id
      };
    } catch (error) {
      console.error("Hiba a token dekódolásakor:", error);
      kijelentkezes(); // Ha érvénytelen a token, kidobjuk a usert
    }
  };

  // --- INICIALIZÁLÁS ---
  // Ha oldalfrissítéskor van már tokenünk, rögtön dekódoljuk
  if (token.value) {
    decodeUserFromToken(token.value);
  }

  // --- MŰVELETEK (Actions) ---
  const bejelentkezes = async (felhasznaloNev: string, jelszo: string) => {
    isLoading.value = true;
    authError.value = null;
    try {
      const response = await authService.bejelentkezes(felhasznaloNev, jelszo);
      token.value = response.data.token;
      localStorage.setItem('userToken', token.value!);

      // Dekódoljuk a friss tokent, így beállítódik a 'felhasznalo' változó is
      decodeUserFromToken(token.value!);
      if (await (await authService.isFirstLogin(felhasznalo.value!.id)).data.isFirstLogin) {
        await router.push('/profile');
      } else {
        await router.push('/');
      }
    } catch (error) {
      authError.value = "Hibás felhasználónév vagy jelszó.";
      console.error("Bejelentkezési hiba:", error);
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  const regisztracio = async (felhasznaloNev: string, jelszo: string) => {
    isLoading.value = true;
    authError.value = null;
    try {
      await authService.regisztracio(felhasznaloNev, jelszo);
      return true;
    } catch (error) {
      authError.value = "Hiba történt a regisztráció során.";
      console.error("Regisztrációs hiba:", error);
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  const checkUsername = async (username: string) => {
    try {
      const response = await authService.checkUsername(username);
      return response.data; // { isTaken: boolean }
    } catch (error) {
      console.error("Nem sikerült ellenőrizni a felhasználónevet", error);
      throw error;
    }
  };

  const kijelentkezes = () => {
    authService.kijelentkezes();
    token.value = null;
    felhasznalo.value = null;
    router.push('/');
  };

  // --- VISSZATÉRÉS (Exportálás a komponensek felé) ---
  return {
    token,
    felhasznalo,
    isLoading,
    authError,
    isAuthenticated,
    bejelentkezes,
    regisztracio,
    checkUsername,
    kijelentkezes
  };
});

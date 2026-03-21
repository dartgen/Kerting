import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import router from '@/router';

export const useAuthStore = defineStore('auth', () => {
  const felhasznalo = ref(null);
  const token = ref<string | null>(authService.getToken());
  const isLoading = ref(false);
  const authError = ref<string | null>(null);

  const isAuthenticated = computed(() => !!token.value);

  const bejelentkezes = async (felhasznaloNev: string, jelszo: string) => {
    isLoading.value = true;
    authError.value = null;
    try {
      const response = await authService.bejelentkezes(felhasznaloNev, jelszo);
      token.value = response.data.token;
      localStorage.setItem('userToken', token.value!);

      // Itt esetleg lekérhetjük a felhasználó adatait is, ha a backend támogatja
      // felhasznalo.value = response.data.user;

      await router.push('/');
    } catch (error: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
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
    } catch (error: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
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


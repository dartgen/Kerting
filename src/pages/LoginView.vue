<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center overflow-hidden font-sans text-earth-100">
    <!-- Background Image with Overlay -->
    <div class="absolute inset-0 z-0">
      <img :src="bgImage" alt="Background" class="w-full h-full object-cover" />
      <div class="absolute inset-0 bg-black/40 backdrop-blur-[2px]"></div>
    </div>

    <!-- Login/Register Card Container -->
    <div class="relative z-10 w-full max-w-md p-4 perspective-[1000px]">

      <div
        class="relative w-full h-[550px] transition-transform duration-700 transform-3d"
        :class="{ 'rotate-y-180': isFlipped }"
      >

        <!-- Front: Bejelentkezés -->
        <div class="absolute inset-0 w-full h-full backface-hidden face-front">
          <div class="relative h-full flex flex-col justify-center px-8 py-10 bg-earth-900/70 backdrop-blur-xl border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)]">

            <!-- Back Arrow -->
            <button
              @click="router.push('/')"
              class="absolute top-4 left-4 text-earth-50 hover:text-earth-200 transition-colors p-2 z-20"
              aria-label="Vissza a főoldalra"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18" />
              </svg>
            </button>

            <h1 class="text-3xl font-bold text-center text-earth-50 mb-8 tracking-wide drop-shadow-md">Bejelentkezés</h1>

            <form @submit.prevent="bejelentkezes" class="space-y-6">
              <div class="space-y-2">
                <label for="email" class="block text-sm font-semibold text-earth-100/90 ml-1">Felhasználónév</label>
                <div class="relative group">
                  <i class="fa-regular fa-envelope absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 transition-colors"></i>
                  <input
                    type="text"
                    id="email"
                    v-model="felhasznaloNev"
                    placeholder="Felhasználóneved"
                    required
                    class="w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3 pl-11 pr-4 text-earth-50 placeholder-earth-200/50 focus:outline-none focus:ring-2 focus:ring-earth-400 focus:bg-earth-50/20 transition-all duration-300 shadow-inner"
                  >
                </div>
              </div>

              <div class="space-y-2">
                <label for="password" class="block text-sm font-semibold text-earth-100/90 ml-1">Jelszó</label>
                <div class="relative group">
                  <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 transition-colors"></i>
                  <input
                    type="password"
                    id="password"
                    v-model="jelszo"
                    placeholder="••••••••"
                    required
                    class="w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3 pl-11 pr-4 text-earth-50 placeholder-earth-200/50 focus:outline-none focus:ring-2 focus:ring-earth-400 focus:bg-earth-50/20 transition-all duration-300 shadow-inner"
                  >
                </div>
              </div>

              <button
                type="submit"
                class="w-full mt-4 bg-linear-to-r from-earth-700 to-earth-500 hover:from-earth-800 hover:to-earth-600 text-earth-50 font-bold py-3.5 rounded-xl shadow-lg transform hover:-translate-y-0.5 transition-all duration-300 disabled:opacity-70 disabled:cursor-not-allowed disabled:transform-none"
                :disabled="isLoading"
              >
                {{ isLoading ? 'Kérjük, várjon...' : 'Belépés' }}
              </button>
            </form>

            <div class="relative flex items-center py-6">
              <div class="grow border-t border-earth-200/30"></div>
              <span class="shrink-0 mx-4 text-xs font-semibold text-earth-200/70 tracking-wider">VAGY</span>
              <div class="grow border-t border-earth-200/30"></div>
            </div>

            <div class="text-center">
              <button
                type="button"
                class="text-sm font-semibold text-earth-200 hover:text-white transition-colors underline-offset-4 hover:underline"
                @click.prevent="isFlipped = true"
              >
                Nincs még fiókod? <span class="text-earth-400">Regisztráció</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Back: Regisztráció -->
        <div class="absolute inset-0 w-full h-full backface-hidden face-back">
          <div class="relative h-full flex flex-col justify-center px-8 py-8 bg-earth-900/70 backdrop-blur-xl border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)]">

            <!-- Back Arrow -->
             <button
              @click="router.push('/')"
              class="absolute top-4 left-4 text-earth-50 hover:text-earth-200 transition-colors p-2 z-20"
              aria-label="Vissza a főoldalra"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18" />
              </svg>
            </button>

            <h1 class="text-3xl font-bold text-center text-earth-50 mb-6 tracking-wide drop-shadow-md">Regisztráció</h1>

            <form @submit.prevent="regisztracio" class="space-y-4">
              <div class="space-y-1">
                <label for="reg-username" class="block text-sm font-semibold text-earth-100/90 ml-1">Felhasználónév</label>
                <div class="relative group">
                  <i class="fa-regular fa-user absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 transition-colors"></i>
                  <input
                    type="text"
                    id="reg-username"
                    v-model="regFelhasznaloNev"
                    placeholder="Válassz egy nevet"
                    @blur="felhasznaloNevEllenorzes"
                    required
                    class="w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3 pl-11 pr-4 text-earth-50 placeholder-earth-200/50 focus:outline-none focus:ring-2 focus:ring-earth-400 focus:bg-earth-50/20 transition-all duration-300 shadow-inner"
                    :class="{ 'border-red-400 focus:ring-red-400': isNevFoglalt }"
                  >
                </div>
                <!-- Error Message -->
                <span v-if="isNevFoglalt" class="text-xs font-semibold text-red-300 ml-1 block mt-1 animate-pulse">
                  {{ nevHibaUzenet }}
                </span>
              </div>

              <div class="space-y-1">
                <label for="reg-password" class="block text-sm font-semibold text-earth-100/90 ml-1">Jelszó</label>
                <div class="relative group">
                  <i class="fa-regular fa-envelope absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 transition-colors"></i>
                  <input
                    type="password"
                    id="reg-password"
                    v-model="regJelszo"
                    placeholder="••••••••"
                    required
                    class="w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3 pl-11 pr-4 text-earth-50 placeholder-earth-200/50 focus:outline-none focus:ring-2 focus:ring-earth-400 focus:bg-earth-50/20 transition-all duration-300 shadow-inner"
                  >
                </div>
              </div>

              <div class="space-y-1">
                <label for="reg-passwordS" class="block text-sm font-semibold text-earth-100/90 ml-1">Jelszó ismét</label>
                <div class="relative group">
                  <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 transition-colors"></i>
                  <input
                    type="password"
                    id="reg-passwordS"
                    v-model="regJelszos"
                    placeholder="••••••••"
                    required
                    class="w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3 pl-11 pr-4 text-earth-50 placeholder-earth-200/50 focus:outline-none focus:ring-2 focus:ring-earth-400 focus:bg-earth-50/20 transition-all duration-300 shadow-inner"
                  >
                </div>
              </div>

              <button
                type="submit"
                class="w-full mt-4 bg-linear-to-r from-earth-700 to-earth-500 hover:from-earth-800 hover:to-earth-600 text-earth-50 font-bold py-3.5 rounded-xl shadow-lg transform hover:-translate-y-0.5 transition-all duration-300 disabled:opacity-70 disabled:cursor-not-allowed disabled:transform-none"
                :disabled="isRegLoading"
              >
                {{ isRegLoading ? 'Kérjük, várjon...' : 'Regisztráció' }}
              </button>
            </form>

            <div class="relative flex items-center py-5">
              <div class="grow border-t border-earth-200/30"></div>
              <span class="shrink-0 mx-4 text-xs font-semibold text-earth-200/70 tracking-wider">VAGY</span>
              <div class="grow border-t border-earth-200/30"></div>
            </div>

            <div class="text-center">
              <button
                type="button"
                class="text-sm font-semibold text-earth-200 hover:text-white transition-colors underline-offset-4 hover:underline"
                @click.prevent="isFlipped = false"
              >
                Már van fiókod? <span class="text-earth-400">Bejelentkezés</span>
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import bgImage from '@/assets/background.jpg';

const router = useRouter();
const authStore = useAuthStore();

// Új állapotok az ellenőrzéshez
const isNevFoglalt = ref(false);
const nevHibaUzenet = ref('');

// --- ÁLLAPOTOK ---
const isFlipped = ref(false); // Ez vezérli a flip animációt

// Bejelentkezés állapotok
const felhasznaloNev = ref('');
const jelszo = ref('');
const isLoading = ref(false);

// Regisztráció állapotok (Példa változók)
const regFelhasznaloNev = ref('');
const regJelszo = ref('');
const regJelszos = ref('');
const isRegLoading = ref(false);


// --- BEJELENTKEZÉS LOGIKA ---
const bejelentkezes = async () => {
  isLoading.value = true;
  try {
    await authStore.bejelentkezes(felhasznaloNev.value, jelszo.value);
  } catch {
    alert("Hiba történt! Hibás felhasználónév vagy jelszó.");
  } finally {
    isLoading.value = false;
  }
};

const regisztracio = async () => {
  isRegLoading.value = true;
  if (regJelszos.value != regJelszo.value) {
    alert("A jelszavak nem egyeznek!");
    isRegLoading.value = false;
    regJelszo.value = '';
    regJelszos.value = '';
  } else {
      try {
        await authStore.regisztracio(regFelhasznaloNev.value, regJelszo.value);

        alert("Sikeres regisztráció! Most jelentkezz be.");
        isFlipped.value = false; // Visszafordítjuk a login oldalra

        // Mezők ürítése
        regFelhasznaloNev.value = '';
        regJelszo.value = '';
        regJelszos.value = '';
      } catch {
        alert("Hiba történt a regisztráció során (lehet hogy foglalt a név).");
      } finally {
        isRegLoading.value = false;
      }
  }
};


// Ez a függvény fut le, amikor kikattintanak a felhasználónév mezőből
const felhasznaloNevEllenorzes = async () => {
  // Ha üres a mező, nem küldünk felesleges kérést
  if (!regFelhasznaloNev.value.trim()) {
    isNevFoglalt.value = false;
    nevHibaUzenet.value = '';
    return;
  }

  try {
    // Megkérdezzük a backendet, hogy létezik-e már ez a név
    const data = await authStore.checkUsername(regFelhasznaloNev.value);

    if (data.isTaken) {
      isNevFoglalt.value = true;
      nevHibaUzenet.value = 'Ez a felhasználónév már foglalt!';
    } else {
      isNevFoglalt.value = false;
      nevHibaUzenet.value = ''; // A név szabad!
    }
  } catch {
    console.error("Nem sikerült ellenőrizni a felhasználónevet");
  }
};
</script>

<style scoped>
.perspective-1000 {
  perspective: 1000px;
}

.transform-3d {
  transform-style: preserve-3d;
}

.backface-hidden {
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
}

.rotate-y-180 {
  transform: rotateY(180deg);
}

.face-front {
  transform: rotateY(0deg) translateZ(1px);
}

.face-back {
  transform: rotateY(180deg) translateZ(1px);
}
</style>

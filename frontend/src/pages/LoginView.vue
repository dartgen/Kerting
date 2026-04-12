<template>
  <div class="flex-1 flex items-start sm:items-center justify-center w-full h-full min-h-0 px-4 sm:px-6 py-3 sm:py-0 overflow-y-auto">
    <div class="relative w-full max-w-md p-4 perspective-[1000px]">
      <div
        class="relative w-full h-[min(550px,calc(100dvh-2rem))] sm:h-[550px] transition-transform duration-700 transform-3d"
        :class="{ 'rotate-y-180': isFlipped }"
      >

        <div class="absolute inset-0 w-full h-full backface-hidden">
          <div class="h-full flex flex-col justify-center px-5 sm:px-8 py-6 sm:py-10 bg-earth-900/70 border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-y-auto overscroll-contain">
            <button
              type="button"
              class="absolute top-4 left-4 z-30 p-1 text-white transition-colors hover:text-earth-200 focus:outline-none"
              @click="visszaAFooldalra"
              aria-label="Vissza a főoldalra"
            >
              <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                <path d="M15 6L9 12L15 18" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
              </svg>
            </button>
            <h1 class="text-3xl font-bold text-center text-earth-50 mb-10 tracking-wide drop-shadow-md">Bejelentkezés</h1>
            <form @submit.prevent="bejelentkezes" class="space-y-8">

              <div class="relative group">
                <i class="fa-regular fa-user absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 z-20 pointer-events-none"></i>
                <input type="text" id="login-user" autocomplete="username" v-model="felhasznaloNev" placeholder=" " required
                       class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10">
                <label for="login-user" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">
                  Felhasználónév
                </label>
              </div>

              <div class="relative group">
                <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 z-20 pointer-events-none"></i>
                <input type="password" id="login-pass" autocomplete="current-password" v-model="jelszo" placeholder=" " required
                       class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10">
                <label for="login-pass" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">
                  Jelszó
                </label>
              </div>

              <div class="relative flex h-2">
                <button type="button" @click="mutasdAzElfelejtettJelszot" class="text-xs font-semibold text-earth-200/70 hover:text-white transition-colors underline absolute right-0">Elfelejtett jelszó?</button>
              </div>

              <button type="submit" :disabled="isLoading" class="w-full bg-linear-to-r from-earth-700 to-earth-500 text-earth-50 font-bold py-3.5 rounded-xl shadow-lg hover:-translate-y-0.5 transition-all disabled:opacity-50">
                {{ isLoading ? 'Belépés...' : 'Belépés' }}
              </button>
            </form>

            <div class="relative flex items-center py-6">
              <div class="grow border-t border-earth-200/30"></div>
              <span class="shrink-0 mx-4 text-xs font-semibold text-earth-200/70 tracking-wider">VAGY</span>
              <div class="grow border-t border-earth-200/30"></div>
            </div>
            <div class="text-center">
              <button type="button" class="text-sm font-semibold text-earth-200 hover:text-white transition-colors underline-offset-4 hover:underline" @click.prevent="mutasdARegisztraciot">
                Nincs még fiókod? <span class="text-earth-400">Regisztráció</span>
              </button>
            </div>
          </div>
        </div>

        <div class="absolute inset-0 w-full h-full backface-hidden rotate-y-180">
          <button
            type="button"
            class="absolute top-4 left-4 z-30 p-1 text-white transition-colors hover:text-earth-200 focus:outline-none"
            @click="visszaAFooldalra"
            aria-label="Vissza a főoldalra"
          >
            <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path d="M15 6L9 12L15 18" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
          </button>
          <div class="h-full flex flex-col justify-center px-5 sm:px-8 py-6 sm:py-8 bg-earth-900/70 border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-y-auto overscroll-contain">

            <div v-if="hatlapTipus === 'regisztracio'">
              <h1 class="text-3xl font-bold text-center text-earth-50 mb-8 tracking-wide">Regisztráció</h1>
              <form @submit.prevent="regisztracio" class="space-y-7">

                <div class="relative group">
                  <i class="fa-regular fa-user absolute left-4 top-1/2 -translate-y-1/2 z-20 pointer-events-none transition-colors"
                     :class="regFelhasznaloNev ? (isNevFoglalt ? 'text-red-400' : 'text-green-400') : 'text-earth-200/70'"></i>
                  <input type="text" id="reg-user" autocomplete="username" v-model="regFelhasznaloNev" @blur="felhasznaloNevEllenorzes" placeholder=" " required
                         class="peer w-full bg-earth-50/10 border rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 transition-all z-10 shadow-inner"
                         :class="regFelhasznaloNev ? (isNevFoglalt ? 'border-red-500 focus:ring-red-500 bg-red-500/10' : 'border-green-500 focus:ring-green-500 bg-green-500/10') : 'border-earth-200/30 focus:ring-earth-400'">
                  <label for="reg-user" class="absolute text-sm duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none"
                         :class="regFelhasznaloNev ? (isNevFoglalt ? 'text-red-400' : 'text-green-400') : 'text-earth-200/70'">
                    {{ isNevFoglalt ? 'Foglalt név!' : 'Felhasználónév' }}
                  </label>
                  <p v-if="regFelhasznaloNev && !isNevFoglalt && !isRegLoading" class="mt-1 text-[10px] text-green-400 font-medium">Ez a név szabad!</p>
                </div>

                <div class="relative group">
                  <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 z-20 pointer-events-none text-earth-200/70"></i>
                  <input type="password" id="reg-pass1" autocomplete="new-password" v-model="regJelszo" placeholder=" " required
                         class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10 shadow-inner"
                         :class="regJelszos ? (regJelszo === regJelszos ? 'border-green-500 focus:ring-green-500 bg-green-500/10' : 'border-red-500 focus:ring-red-500 bg-red-500/10') : 'border-earth-200/30 focus:ring-earth-400'">
                  <label for="reg-pass1" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">Jelszó</label>
                </div>

                <div class="relative group">
                  <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 z-20 pointer-events-none transition-colors"
                     :class="regJelszos ? (regJelszo === regJelszos ? 'text-green-400' : 'text-red-400') : 'text-earth-200/70'"></i>
                  <input type="password" id="reg-pass2" autocomplete="new-password" v-model="regJelszos" placeholder=" " required
                         class="peer w-full bg-earth-50/10 border rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 transition-all z-10 shadow-inner"
                         :class="regJelszos ? (regJelszo === regJelszos ? 'border-green-500 focus:ring-green-500 bg-green-500/10' : 'border-red-500 focus:ring-red-500 bg-red-500/10') : 'border-earth-200/30 focus:ring-earth-400'">
                  <label for="reg-pass2" class="absolute text-sm duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none"
                         :class="regJelszos ? (regJelszo === regJelszos ? 'text-green-400' : 'text-red-400') : 'text-earth-200/70'">
                    {{ regJelszos ? (regJelszo === regJelszos ? 'Jelszavak egyeznek' : 'Nem egyezik!') : 'Jelszó ismét' }}
                  </label>
                </div>

                <button type="submit" :disabled="isRegLoading || isNevFoglalt || regJelszo !== regJelszos"
                        class="w-full bg-linear-to-r from-earth-700 to-earth-500 text-earth-50 font-bold py-3.5 rounded-xl shadow-lg transition-all disabled:opacity-50">
                  Regisztráció
                </button>
              </form>
            </div>

            <div v-else-if="hatlapTipus === 'elfelejtett'">
              <h1 class="text-3xl font-bold text-center text-earth-50 mb-4 tracking-wide">Új jelszó igénylés</h1>
              <p class="text-center text-earth-200/80 text-sm mb-10">Add meg az e-mail címedet!</p>
              <form @submit.prevent="elfelejtettJelszoKeres" class="space-y-8">
                <div class="relative group">
                  <i class="fa-regular fa-paper-plane absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 z-20 pointer-events-none"></i>
                  <input id="reset-mail" type="text" v-model="resetAzonosito" required placeholder=" "
                         class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10 shadow-inner">
                  <label for="reset-mail" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">E-mail cím</label>
                </div>
                <button type="submit" class="w-full bg-linear-to-r from-earth-700 to-earth-500 text-earth-50 font-bold py-3.5 rounded-xl shadow-lg">Küldés</button>
              </form>
            </div>

            <div class="relative flex items-center py-6">
              <div class="grow border-t border-earth-200/30"></div>
              <span class="shrink-0 mx-4 text-xs font-semibold text-earth-200/70 tracking-wider">VAGY</span>
              <div class="grow border-t border-earth-200/30"></div>
            </div>
            <div class="text-center">
              <button type="button" class="text-sm font-semibold text-earth-200 hover:text-white transition-colors underline-offset-4 hover:underline" @click.prevent="isFlipped = false">
                Vissza a <span class="text-earth-400">Bejelentkezéshez</span>
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
import { useToastStore } from '@/stores/toast'

const toastStore = useToastStore()
const router = useRouter();
const authStore = useAuthStore();

// Flipkártya állapot: hátlap regisztráció vagy elfelejtett jelszó nézet.
const isFlipped = ref(false);
const hatlapTipus = ref<'regisztracio' | 'elfelejtett'>('regisztracio');

// Bejelentkezés űrlap.
const felhasznaloNev = ref('');
const jelszo = ref('');
const isLoading = ref(false);

// Regisztráció űrlap.
const regFelhasznaloNev = ref('');
const regJelszo = ref('');
const regJelszos = ref('');
const isRegLoading = ref(false);
const isNevFoglalt = ref(false);

// Elfelejtett jelszó űrlap.
const resetAzonosito = ref('');

// Kártyaflip segédfüggvények.
const mutasdARegisztraciot = () => { hatlapTipus.value = 'regisztracio'; isFlipped.value = true; };
const mutasdAzElfelejtettJelszot = () => { hatlapTipus.value = 'elfelejtett'; isFlipped.value = true; };

// Felhasználónév-foglaltság ellenőrzés regisztráció közben.
const felhasznaloNevEllenorzes = async () => {
  if (!regFelhasznaloNev.value.trim()) return;
  try {
    const data = await authStore.checkUsername(regFelhasznaloNev.value);
    isNevFoglalt.value = Boolean(data.isTaken);
  } catch { console.error("Hiba"); }
};

// Bejelentkezés meghívja az auth store centralizált login folyamatait.
const bejelentkezes = async () => {
  isLoading.value = true;
  try {
    await authStore.bejelentkezes(felhasznaloNev.value, jelszo.value);
  } catch {
    toastStore.addToast(' Hibás felhasználónév vagy jelszó!', 4000, 'error')
  } finally {
    isLoading.value = false;
  }
};

// Új user regisztrálása, siker után visszavált login nézetre.
const regisztracio = async () => {
  isRegLoading.value = true;
    try {
      await authStore.regisztracio(regFelhasznaloNev.value, regJelszo.value);
      toastStore.addToast('Sikeres regisztráció, most jelentkezz be!', 4000, 'success')

      // Visszafordítjuk a login oldalra.
      isFlipped.value = false;

      // Űrlap állapot reset.
      regFelhasznaloNev.value = '';
      regJelszo.value = '';
      regJelszos.value = '';
    } catch {
      toastStore.addToast('Hiba történt a regisztráció során!', 4000, 'error')
    } finally {
      isRegLoading.value = false;
    }
};

// Placeholder workflow, amíg valódi jelszó-visszaállítás végpont nincs.
const elfelejtettJelszoKeres = () => {
  toastStore.addToast('A jelszó visszaálitás igényelve!', 4000, 'success')
  isFlipped.value = false;
  resetAzonosito.value = '';
};

const visszaAFooldalra = () => {
  router.push('/');
};

</script>

<style scoped>
/* 3D flipkártya stílusok */

.transform-3d {
  transform-style: preserve-3d;
  -webkit-transform-style: preserve-3d;
  will-change: transform;
}

.backface-hidden {
  -webkit-backface-visibility: hidden;
  backface-visibility: hidden;
  /* Firefox-specifikus javítás translateZ trükkel */
  transform: translateZ(0);
}

.rotate-y-180 {
  transform: rotateY(180deg);
}
</style>

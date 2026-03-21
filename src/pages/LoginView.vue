<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center overflow-hidden font-sans text-earth-100">
    <div class="absolute inset-0 z-0">
      <img :src="bgImage" alt="Background" class="w-full h-full object-cover" />
      <div class="absolute inset-0 bg-black/40 backdrop-blur-[2px]"></div>
    </div>

    <div class="relative z-10 w-full max-w-md p-4 perspective-[1000px]">
      <div
        class="relative w-full h-[550px] transition-transform duration-700 transform-3d"
        :class="{ 'rotate-y-180': isFlipped }"
      >
        <div class="absolute inset-0 w-full h-full backface-hidden">
          <div class="h-full flex flex-col justify-center px-8 py-10 bg-earth-900/70 backdrop-blur-xl border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)]">
            <h1 class="text-3xl font-bold text-center text-earth-50 mb-10 tracking-wide drop-shadow-md">Bejelentkezés</h1>
            <form @submit.prevent="bejelentkezes" class="space-y-8">

              <div class="relative group">
                <i class="fa-regular fa-user absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 z-20 pointer-events-none"></i>
                <input type="text" id="login-user" v-model="felhasznaloNev" placeholder=" " required
                       class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10">
                <label for="login-user" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">
                  Felhasználónév
                </label>
              </div>

              <div class="relative group">
                <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 group-focus-within:text-earth-400 z-20 pointer-events-none"></i>
                <input type="password" id="login-pass" v-model="jelszo" placeholder=" " required
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
          <div class="h-full flex flex-col justify-center px-8 py-8 bg-earth-800/80 backdrop-blur-xl border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)]">

            <div v-if="hatlapTipus === 'regisztracio'">
              <h1 class="text-3xl font-bold text-center text-earth-50 mb-8 tracking-wide">Regisztráció</h1>
              <form @submit.prevent="regisztracio" class="space-y-7">

                <div class="relative group">
                  <i class="fa-regular fa-user absolute left-4 top-1/2 -translate-y-1/2 z-20 pointer-events-none transition-colors"
                     :class="regFelhasznaloNev ? (isNevFoglalt ? 'text-red-400' : 'text-green-400') : 'text-earth-200/70'"></i>
                  <input type="text" id="reg-user" v-model="regFelhasznaloNev" @blur="felhasznaloNevEllenorzes" placeholder=" " required
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
                  <input type="password" id="reg-pass1" v-model="regJelszo" placeholder=" " required
                         class="peer w-full bg-earth-50/10 border border-earth-200/30 rounded-xl py-3.5 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-earth-400 transition-all z-10 shadow-inner"
                         :class="regJelszos ? (regJelszo === regJelszos ? 'border-green-500 focus:ring-green-500 bg-green-500/10' : 'border-red-500 focus:ring-red-500 bg-red-500/10') : 'border-earth-200/30 focus:ring-earth-400'">
                  <label for="reg-pass1" class="absolute text-sm text-earth-200/70 duration-300 transform -translate-y-1/2 top-1/2 left-11 z-20 peer-placeholder-shown:scale-100 peer-focus:top-0 peer-focus:scale-90 peer-focus:-translate-y-5 peer-focus:left-2 peer-focus:text-earth-400 peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:scale-90 peer-[:not(:placeholder-shown)]:-translate-y-5 peer-[:not(:placeholder-shown)]:left-2 pointer-events-none">Jelszó</label>
                </div>

                <div class="relative group">
                  <i class="fa-solid fa-lock absolute left-4 top-1/2 -translate-y-1/2 z-20 pointer-events-none transition-colors"
                     :class="regJelszos ? (regJelszo === regJelszos ? 'text-green-400' : 'text-red-400') : 'text-earth-200/70'"></i>
                  <input type="password" id="reg-pass2" v-model="regJelszos" placeholder=" " required
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
import { useAuthStore } from '@/stores/authStore';
import bgImage from '@/assets/background.jpg';

const authStore = useAuthStore();
const isFlipped = ref(false);
const hatlapTipus = ref<'regisztracio' | 'elfelejtett'>('regisztracio');

const felhasznaloNev = ref('');
const jelszo = ref('');
const isLoading = ref(false);

const regFelhasznaloNev = ref('');
const regJelszo = ref('');
const regJelszos = ref('');
const isRegLoading = ref(false);
const isNevFoglalt = ref(false);

const resetAzonosito = ref('');

const mutasdARegisztraciot = () => { hatlapTipus.value = 'regisztracio'; isFlipped.value = true; };
const mutasdAzElfelejtettJelszot = () => { hatlapTipus.value = 'elfelejtett'; isFlipped.value = true; };

const felhasznaloNevEllenorzes = async () => {
  if (!regFelhasznaloNev.value.trim()) return;
  try {
    const data = await authStore.checkUsername(regFelhasznaloNev.value);
    isNevFoglalt.value = data.isTaken;
  } catch { console.error("Hiba"); }
};

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

</script>


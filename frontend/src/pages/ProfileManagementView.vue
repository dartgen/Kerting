<template>
  <div class="flex-1 flex items-start sm:items-center justify-center w-full h-full min-h-0 px-4 sm:px-6 py-6 sm:py-0 overflow-y-auto">
    <div class="relative w-full max-w-4xl p-4">
      <div class="relative w-full bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden">

        <button
          type="button"
          class="absolute top-4 left-4 z-30 p-1 text-white transition-colors hover:text-earth-200 focus:outline-none"
          @click="vissza"
          aria-label="Vissza"
        >
          <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" aria-hidden="true">
            <path d="M15 6L9 12L15 18" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>

        <div class="px-5 pt-8 pb-4 border-b border-earth-200/20">
          <h1 class="text-3xl font-bold text-center text-earth-50 tracking-wide drop-shadow-md">
            {{ authStore.felhasznalo?.felhasznaloNev || 'Profil szerkesztése' }}
          </h1>
        </div>

        <div class="px-5 sm:px-8 py-6">
          <form @submit.prevent="adatokMentese" class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6">

            <div class="space-y-6">
              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Vezetéknév</label>
                <input type="text" v-model="profilAdatok.vezetekNev" placeholder="Pl.: Nagy"
                       class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">E-mail cím</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
                  </svg>
                  <input type="email" v-model="profilAdatok.email" placeholder="example@email.com"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Telefonszám</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 6.75c0 8.284 6.716 15 15 15h2.25a2.25 2.25 0 0 0 2.25-2.25v-1.372c0-.516-.351-.966-.852-1.091l-4.423-1.106c-.44-.11-.902.055-1.173.417l-.97 1.293c-2.896-1.596-5.539-4.239-7.135-7.135l1.293-.97c.363-.271.527-.734.417-1.173L6.963 3.102a1.125 1.125 0 0 0-1.091-.852H4.5A2.25 2.25 0 0 0 2.25 4.5v2.25Z" />
                  </svg>
                  <input type="tel" v-model="profilAdatok.telefon" placeholder="+36301234567"
                         @input="telefonHiba = false"
                         :class="[
                           'w-full bg-earth-50/10 border rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 transition-all shadow-inner placeholder-earth-200/50',
                           telefonHiba ? 'border-red-500 focus:ring-red-400' : 'border-earth-200/30 focus:ring-green-400'
                         ]">
                </div>
                <span v-if="telefonHiba" class="text-red-400 text-xs mt-1 ml-1">
                  Kérjük, érvényes formátumot adj meg! (Pl.: +36301234567 vagy 06301234567)
                </span>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Település</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1 1 15 0Z" />
                  </svg>
                  <input type="text" v-model="profilAdatok.telepules" placeholder="Kecskemét"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Ki vagy?</label>
                <select v-model="profilAdatok.roleId"
                        :disabled="profilAdatok.roleId === 1"
                        class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                  <option v-for="role in szurtRoles" :key="role.id" :value="role.id" class="bg-earth-800 text-earth-50">
                    {{ role.name }}
                  </option>
                  <option v-if="profilAdatok.roleId === 1" :value="1" class="bg-earth-800 text-earth-50">Adminisztrátor</option>
                </select>
              </div>
            </div>

            <div class="flex flex-col h-full">
              <div class="flex items-start justify-between gap-4 mb-6">
                <div class="flex flex-col flex-1">
                  <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Keresztnév</label>
                  <input type="text" v-model="profilAdatok.keresztNev" placeholder="Pl.: Géza"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
                <ProfileUploader v-model="profilAdatok.IMGString" />
              </div>

              <div class="flex flex-col flex-1 gap-4">
                <div class="flex flex-col flex-1 transition-all duration-300">
                  <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Rólam</label>
                  <textarea v-model="profilAdatok.rolam" placeholder="Én..."
                            class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner resize-y placeholder-earth-200/50 min-h-[120px] flex-1"></textarea>
                </div>

                <div v-if="profilAdatok.roleId == 4 || profilAdatok.roleId == 5"
                     class="relative flex flex-col flex-1 border border-green-500/80 rounded-lg bg-earth-50/5 focus-within:ring-2 focus-within:ring-green-400 transition-all z-20">
                  <div class="p-2.5">
                    <label class="block text-sm font-bold text-earth-100 mb-1 ml-1">Mivel foglalkozol?</label>
                    <div class="flex items-center text-earth-50 justify-between">
                      <div class="flex items-center flex-1">
                        <span class="text-earth-200/50 text-xl font-light ml-1 mr-2">+</span>
                        <input ref="cimkeInput" type="text" v-model="ujCimke"
                               @keydown.enter.prevent="addCimke"
                               @focus="mutasdAzAjanlasokat = true"
                               @blur="mutasdAzAjanlasokat = false"
                               placeholder="Locsolás"
                               class="w-full bg-transparent border-none focus:outline-none placeholder-earth-200/50 text-earth-50">
                      </div>
                      <button type="button" @click="addCimke" class="ml-2 p-1 text-earth-300 hover:text-green-400 transition-colors">
                        <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                        </svg>
                      </button>
                    </div>

                    <ul v-if="mutasdAzAjanlasokat && szurtAjanlasok.length > 0"
                        class="absolute left-0 right-0 top-[76px] bg-earth-800 border border-earth-600 rounded-lg shadow-xl overflow-hidden z-30 max-h-40 overflow-y-auto">
                      <li v-for="ajanlas in szurtAjanlasok" :key="ajanlas"
                          @mousedown.prevent="selectAjanlas(ajanlas)"
                          class="px-4 py-2 text-sm text-earth-50 hover:bg-green-600 cursor-pointer transition-colors border-b border-earth-700 last:border-0">
                        {{ ajanlas }}
                      </li>
                    </ul>
                  </div>

                  <div class="h-px bg-green-500/30 mx-2"></div>

                  <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1 text-earth-50">
                    <span v-for="(cimke, index) in cimkek" :key="index"
                          @click="removeCimke(index)"
                          class="pl-3 pr-1.5 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all cursor-pointer hover:opacity-80"
                          :class="getCimkeStyle(index)?.tag">
                      {{ cimke }}
                      <div class="w-5 h-5 rounded-full flex items-center justify-center" :class="getCimkeStyle(index)?.btn">
                        <svg xmlns="http://www.w3.org/2000/svg" class="w-3 h-3 text-white" viewBox="0 0 20 20" fill="currentColor">
                          <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                      </div>
                    </span>
                    <span v-if="cimkek.length === 0" class="text-earth-200/50 text-sm italic mt-1 ml-1">Írd be mivel foglalkozol!</span>
                  </div>
                </div>
              </div>
            </div>

            <div class="md:col-span-2 pt-6 flex justify-center mt-auto border-t border-earth-200/20">
              <button type="submit" :disabled="isLoading"
                      class="w-full md:w-1/2 bg-green-500/90 hover:bg-green-400 text-white font-bold py-3.5 rounded-xl shadow-lg hover:-translate-y-0.5 transition-all disabled:opacity-50 border border-green-300/50">
                <i v-if="isLoading" class="fa-solid fa-spinner fa-spin mr-2"></i>
                {{ isLoading ? 'Mentés folyamatban...' : 'Adatok mentése' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import ProfileUploader from '@/components/ProfileUploader.vue';
import { useAuthStore } from '@/stores/authStore.ts';
import { authService } from '@/services/authService';
import { useToastStore } from '@/stores/toast';

const authStore = useAuthStore();
const router = useRouter();
const toastStore = useToastStore();

const isLoading = ref(false);
const telefonHiba = ref(false); // Validációs változó a telefonszámhoz
const roles = ref<{ id: number, name: string }[]>([]);

// Reaktív adatok (meg kell egyeznie a backend DTO-val)
const profilAdatok = reactive({
  vezetekNev: '',
  keresztNev: '',
  email: '',
  telefon: '',
  telepules: '',
  roleId: 1,
  rolam: '',
  IMGString: ''
});

// Címke kezelés adatai
const cimkeInput = ref<HTMLInputElement | null>(null);
const ujCimke = ref('');
const cimkek = ref<string[]>([]);
const eloreDefinialtCimkek = ['Locsolás', 'Metszés', 'Permetezés', 'Fűnyírás', 'Gyomlálás', 'Betakarítás', 'Szüretelés', 'Tereprendezés', 'Faültetés', 'Vetés', 'Öntözés', 'Trágyázás', 'Ültetés'];
const mutasdAzAjanlasokat = ref(false);

// SZŰRÉS: Ne kínálja fel az Admin (ID: 1) szerepkört
const szurtRoles = computed(() => {
  return roles.value.filter(r => r.id !== 1);
});

// Autocomplete szűrés címkékhez
const szurtAjanlasok = computed(() => {
  const keresoSzoveg = ujCimke.value.toLowerCase().trim();
  if (!keresoSzoveg) return [];
  return eloreDefinialtCimkek.filter(c => c.toLowerCase().includes(keresoSzoveg) && !cimkek.value.includes(c));
});

// Adatok betöltése
const adatokBetoltese = async () => {
  try {
    const rolesRes = await authService.getRoles();
    roles.value = rolesRes.data;

    const profileRes = await authService.getProfile();
    const d = profileRes.data;

    profilAdatok.IMGString = d.imgString || '';
    profilAdatok.vezetekNev = d.vezetekNev || '';
    profilAdatok.keresztNev = d.keresztNev || '';
    profilAdatok.email = d.email || '';
    profilAdatok.telefon = d.telefon || '';
    profilAdatok.telepules = d.telepules || '';
    profilAdatok.rolam = d.rolam || '';
    profilAdatok.roleId = d.roleId || 2; // Alapértelmezett pl. Kertes
  } catch (error) {
    console.error("Betöltési hiba:", error);
    toastStore.addToast('Hiba az adatok betöltésekor!', 4000, 'error');
  }
};

onMounted(() => {
  adatokBetoltese();
});

// Műveletek
const selectAjanlas = (cimke: string) => {
  ujCimke.value = cimke;
  addCimke();
  cimkeInput.value?.focus();
};

const addCimke = () => {
  const trimmed = ujCimke.value.trim();
  if (trimmed && !cimkek.value.some(c => c.toLowerCase() === trimmed.toLowerCase())) {
    cimkek.value.push(trimmed);
  }
  ujCimke.value = '';
};

const removeCimke = (index: number) => {
  cimkek.value.splice(index, 1);
};

const getCimkeStyle = (index: number) => {
  const styles = [
    { tag: 'bg-gray-200 text-gray-700', btn: 'bg-gray-400' },
    { tag: 'bg-green-100 text-green-700', btn: 'bg-green-500' },
    { tag: 'bg-orange-100 text-orange-800', btn: 'bg-orange-400' }
  ];
  return styles[index % styles.length];
};

const adatokMentese = async () => {
  // 1. Telefonszám ellenőrzése
  if (profilAdatok.telefon) {
    const tisztitottSzam = profilAdatok.telefon.replace(/[\s-]/g, '');
    const telefonRegex = /^(\+36|06)\d{8,9}$/;

    if (!telefonRegex.test(tisztitottSzam)) {
      telefonHiba.value = true;
      toastStore.addToast('Hibás telefonszám formátum!', 4000, 'error');
      return; // Mentés megállítása
    }

    profilAdatok.telefon = tisztitottSzam; // Tiszta adat elmentése az objektumba
  }

  // 2. Adatok mentése a szerverre
  isLoading.value = true;
  try {
    await authService.updateProfile(profilAdatok);
    toastStore.addToast('Sikeres mentés!', 4000, 'success');
  } catch (error) {
    console.error("Mentési hiba:", error);
    toastStore.addToast('Hiba történt a mentés során!', 4000, 'error');
  } finally {
    isLoading.value = false;
  }
};

const vissza = () => router.push('/');
</script>

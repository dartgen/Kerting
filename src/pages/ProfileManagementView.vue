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
          <h1 class="text-3xl font-bold text-center text-earth-50 tracking-wide drop-shadow-md">{{authStore.felhasznalo?.felhasznaloNev}}</h1>
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
                  <i class="fa-regular fa-envelope absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none"></i>
                  <input type="email" v-model="profilAdatok.email" placeholder="example@email.com"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Telefonszám</label>
                <div class="relative">
                  <i class="fa-solid fa-phone absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none"></i>
                  <input type="tel" v-model="profilAdatok.telefon" placeholder="+36301234567"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Település</label>
                <div class="relative">
                  <i class="fa-solid fa-location-dot absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none"></i>
                  <input type="text" v-model="profilAdatok.telepules" placeholder="Kecskemét"
                         class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner placeholder-earth-200/50">
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Ki vagy?</label>
                <select v-model="profilAdatok.szerepkor"
                        class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner appearance-none cursor-pointer">
                  <option value="Kertes" class="bg-earth-800 text-earth-50">Kertes</option>
                  <option value="Kertesz" class="bg-earth-800 text-earth-50">Kertész</option>
                  <option value="Hobbi_Kertesz" class="bg-earth-800 text-earth-50">Hobbi kertész</option>
                  <option value="Latogato" class="bg-earth-800 text-earth-50">Látogató</option>
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
                <ProfileUploader/>
              </div>

              <div class="flex flex-col flex-1 gap-4">

                <div class="flex flex-col transition-all duration-300" :class="['Kertesz', 'Hobbi_Kertesz'].includes(profilAdatok.szerepkor) ? 'flex-none' : 'flex-1'">
                  <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Rólam</label>
                  <textarea v-model="profilAdatok.rolam" placeholder="Én..."
                            :rows="['Kertesz', 'Hobbi_Kertesz'].includes(profilAdatok.szerepkor) ? 3 : 5"
                            class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-400 transition-all shadow-inner resize-y placeholder-earth-200/50"
                            :class="['Kertesz', 'Hobbi_Kertesz'].includes(profilAdatok.szerepkor) ? 'min-h-[80px]' : 'h-full min-h-[120px]'"></textarea>
                </div>

                <div v-if="['Kertesz', 'Hobbi_Kertesz'].includes(profilAdatok.szerepkor)"
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
                      <button type="button" @click="addCimke"
                              class="ml-2 p-1 text-earth-300 hover:text-green-400 transition-colors focus:outline-none"
                              title="Hozzáadás">
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

                  <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1">
                      <span v-for="(cimke, index) in cimkek" :key="index"
                            @click="removeCimke(index)"
                            class="pl-3 pr-1.5 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all cursor-pointer hover:opacity-80"
                            :class="getCimkeStyle(index).tag"
                            title="Kattints a törléshez">
                          {{ cimke }}
                          <div class="w-5 h-5 rounded-full flex items-center justify-center transition-colors"
                               :class="getCimkeStyle(index).btn">
                              <svg xmlns="http://www.w3.org/2000/svg" class="w-3 h-3 text-white" viewBox="0 0 20 20" fill="currentColor">
                                  <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                              </svg>
                          </div>
                      </span>
                    <span v-if="cimkek.length === 0" class="text-earth-200/50 text-sm italic mt-1 ml-1">Írd be mivel foglalkozol, majd nyomj Entert vagy Pipát!</span>
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
import { ref, reactive, computed} from 'vue';
import { useRouter } from 'vue-router';
import ProfileUploader from '@/components/ProfileUploader.vue';
import {useAuthStore} from '@/stores/authStore.ts';

const authStore = useAuthStore();
const router = useRouter();

const isLoading = ref(false);

const profilAdatok = reactive({
  vezetekNev: '',
  keresztNev: '',
  email: '',
  telefon: '',
  telepules: '',
  szerepkor: 'Kertes',
  rolam: ''
});

const cimkeInput = ref<HTMLInputElement | null>(null);
const ujCimke = ref('');
const cimkek = ref<string[]>([]);

const eloreDefinialtCimkek = [
  'Locsolás', 'Metszés', 'Permetezés', 'Fűnyírás',
  'Gyomlálás', 'Betakarítás', 'Szüretelés', 'Tereprendezés',
  'Faültetés', 'Vetés', 'Öntözés', 'Trágyázás', 'Ültetés'
];
const mutasdAzAjanlasokat = ref(false);

const szurtAjanlasok = computed(() => {
  const keresoSzoveg = ujCimke.value.toLowerCase().trim();
  if (!keresoSzoveg) return [];

  return eloreDefinialtCimkek.filter(cimke =>
    cimke.toLowerCase().includes(keresoSzoveg) && !cimkek.value.includes(cimke)
  );
});

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
  cimkeInput.value?.focus();
};

const removeCimke = (index: number) => {
  cimkek.value.splice(index, 1);
};

const getCimkeStyle = (index: number) => {
  const styles = [
    { tag: 'bg-gray-200 text-gray-700', btn: 'bg-gray-400' },
    { tag: 'bg-green-100 text-green-700', btn: 'bg-green-500' },
    { tag: 'bg-orange-100 text-orange-800', btn: 'bg-orange-400' },
    { tag: 'bg-blue-100 text-blue-700', btn: 'bg-blue-400' },
    { tag: 'bg-purple-100 text-purple-700', btn: 'bg-purple-400' }
  ];
  return styles[index % styles.length];
};

const adatokMentese = async () => {
  isLoading.value = true;
  try {
    await new Promise(resolve => setTimeout(resolve, 800));
    alert("Sikeres mentés!");
  } catch (error) {
    console.error("Hiba", error);
  } finally {
    isLoading.value = false;
  }
};

const vissza = () => {
  router.push('/');
};
</script>

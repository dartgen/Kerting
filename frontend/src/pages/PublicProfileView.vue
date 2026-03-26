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
            {{ teljesNev || 'Profil betöltése...' }}
          </h1>
          <p class="text-center text-earth-200/70 mt-1 font-medium tracking-wider uppercase text-sm">
            {{ roleNev || 'Felhasználó' }}
          </p>
        </div>

        <div v-if="isLoading" class="flex justify-center items-center p-20">
          <i class="fa-solid fa-spinner fa-spin text-4xl text-green-400"></i>
        </div>

        <div v-else class="px-5 sm:px-8 py-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6">

            <div class="space-y-6">

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">E-mail cím</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
                  </svg>
                  <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner break-words min-h-[48px] flex items-center">
                    <a :href="'mailto:' + profilAdatok.email" class="hover:text-green-300 transition-colors">
                      {{ profilAdatok.email || 'Nincs megadva' }}
                    </a>
                  </div>
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Telefonszám</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 6.75c0 8.284 6.716 15 15 15h2.25a2.25 2.25 0 0 0 2.25-2.25v-1.372c0-.516-.351-.966-.852-1.091l-4.423-1.106c-.44-.11-.902.055-1.173.417l-.97 1.293c-2.896-1.596-5.539-4.239-7.135-7.135l1.293-.97c.363-.271.527-.734.417-1.173L6.963 3.102a1.125 1.125 0 0 0-1.091-.852H4.5A2.25 2.25 0 0 0 2.25 4.5v2.25Z" />
                  </svg>
                  <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner min-h-[48px] flex items-center">
                    <a :href="'tel:' + profilAdatok.telefon" class="hover:text-green-300 transition-colors">
                      {{ profilAdatok.telefon || 'Nincs megadva' }}
                    </a>
                  </div>
                </div>
              </div>

              <div class="flex flex-col">
                <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Település</label>
                <div class="relative">
                  <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1 1 15 0Z" />
                  </svg>
                  <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner min-h-[48px] flex items-center">
                    {{ profilAdatok.telepules || 'Nincs megadva' }}
                  </div>
                </div>
              </div>
            </div>

            <div class="flex flex-col h-full">

              <div class="flex items-start justify-center md:justify-end mb-6">
                <div class="w-32 h-32 sm:w-40 sm:h-40 rounded-full border-4 border-earth-200/30 shadow-xl overflow-hidden bg-earth-800">
                  <img :src="profilAdatok.IMGString || '/default-avatar.png'" alt="Profilkép" class="w-full h-full object-cover">
                </div>
              </div>

              <div class="flex flex-col flex-1 gap-4">

                <div class="flex flex-col flex-1 transition-all duration-300">
                  <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Rólam</label>
                  <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 shadow-inner min-h-[120px] flex-1 whitespace-pre-line italic font-light">
                    {{ profilAdatok.rolam || 'Ez a felhasználó még nem írt bemutatkozást.' }}
                  </div>
                </div>

                <div v-if="(profilAdatok.roleId == 4 || profilAdatok.roleId == 5) && cimkek.length > 0"
                     class="relative flex flex-col flex-1 border border-green-500/50 rounded-lg bg-earth-50/5 transition-all z-20">
                  <div class="p-2.5 border-b border-green-500/30 mx-2">
                    <label class="block text-sm font-bold text-earth-100 mb-1 ml-1">Vállalt tevékenységek</label>
                  </div>

                  <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1 text-earth-50">
                    <span v-for="(cimke, index) in cimkek" :key="index"
                          class="px-3 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all"
                          :class="getCimkeStyle(index)?.tag">
                      {{ cimke }}
                    </span>
                  </div>
                </div>

              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { authService } from '@/services/authService';
import { useToastStore } from '@/stores/toast';

const route = useRoute();
const router = useRouter();
const toastStore = useToastStore();

const isLoading = ref(true);
const roles = ref<{ id: number, name: string }[]>([]);

// Adatmodell
const profilAdatok = reactive({
  vezetekNev: '',
  keresztNev: '',
  email: '',
  telefon: '',
  telepules: '',
  roleId: 0,
  rolam: '',
  IMGString: '',
});

const cimkek = ref<string[]>([]);

// Számított tulajdonságok
const teljesNev = computed(() => {
  if (!profilAdatok.vezetekNev && !profilAdatok.keresztNev) return '';
  return `${profilAdatok.vezetekNev} ${profilAdatok.keresztNev}`.trim();
});

const roleNev = computed(() => {
  const role = roles.value.find(r => r.id === profilAdatok.roleId);
  return role ? role.name : '';
});

// Adatok betöltése
const adatokBetoltese = async () => {
  const userId = route.params.id as string;
  if (!userId) return;

  try {
    isLoading.value = true;

    // Szerepkörök betöltése
    const rolesRes = await authService.getRoles();
    roles.value = rolesRes.data;

    // Profil adatok betöltése
    const profileRes = await authService.getPublicProfile(userId);
    const d = profileRes.data;

    profilAdatok.IMGString = d.imgString || '';
    profilAdatok.vezetekNev = d.vezetekNev || '';
    profilAdatok.keresztNev = d.keresztNev || '';
    profilAdatok.email = d.email || '';
    profilAdatok.telefon = d.telefon || '';
    profilAdatok.telepules = d.telepules || '';
    profilAdatok.rolam = d.rolam || '';
    profilAdatok.roleId = d.roleId || 0;

    // Címkék betöltése
    if (d.cimkek && Array.isArray(d.cimkek)) {
      cimkek.value = d.cimkek.map((c: string) => c.trim());
    }
  } catch (error) {
    console.error("Betöltési hiba:", error);
    toastStore.addToast('Nem sikerült betölteni a profilt!', 4000, 'error');
    router.push('/'); // Hibás ID esetén vissza a főoldalra
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  adatokBetoltese();
});

// A címkék stílusa (pontosan ugyanaz, mint a szerkesztőben)
const getCimkeStyle = (index: number) => {
  const styles = [
    { tag: 'bg-gray-200 text-gray-700' },
    { tag: 'bg-green-100 text-green-700' },
    { tag: 'bg-orange-100 text-orange-800' }
  ];
  return styles[index % styles.length];
};

const vissza = () => {
  if (window.history.length > 1) {
    router.back();
  } else {
    router.push('/');
  }
};
</script>

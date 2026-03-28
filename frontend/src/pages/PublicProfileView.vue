<template>
  <div class="flex-1 w-full h-full overflow-y-auto">

    <div class="flex items-center justify-center min-h-full px-4 sm:px-6 py-6 sm:py-10">

      <div class="flex flex-col md:flex-row items-start w-full max-w-6xl gap-6">

        <div class="w-full md:w-64 shrink-0 bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden sticky top-6">
          <div class="p-4">
            <h2 class="text-earth-200/70 font-medium tracking-wider uppercase text-xs mb-4 ml-2">Menü</h2>
            <nav class="flex flex-col gap-2">
              <button
                @click="activeTab = 'profil'"
                :class="[
                  'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3',
                  activeTab === 'profil' ? 'bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner' : 'text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent'
                ]"
              >
                <i class="fa-solid fa-user"></i> Profil
              </button>

              <button
                @click="activeTab = 'galeria'"
                :class="[
                  'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3',
                  activeTab === 'galeria' ? 'bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner' : 'text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent'
                ]"
              >
                <i class="fa-solid fa-images"></i> Galéria
              </button>

              <button
                @click="activeTab = 'munka'"
                :class="[
                  'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3',
                  activeTab === 'munka' ? 'bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner' : 'text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent'
                ]"
              >
                <i class="fa-solid fa-briefcase"></i> Munka nézet
              </button>

              <button
                @click="activeTab = 'hozzaszolasok'"
                :class="[
                  'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3',
                  activeTab === 'hozzaszolasok' ? 'bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner' : 'text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent'
                ]"
              >
                <i class="fa-solid fa-comments"></i> Hozzászólások
              </button>
            </nav>
          </div>
        </div>

        <div class="relative w-full flex-1 bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden min-h-[700px]">

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

          <div class="relative px-5 pt-12 sm:pt-10 pb-6 border-b border-earth-200/20 flex flex-col sm:flex-row items-center gap-6">

            <div class="text-center sm:text-left flex-1 sm:pl-8">
              <h1 class="text-3xl font-bold text-earth-50 tracking-wide drop-shadow-md">
                {{ teljesNev || 'Profil betöltése...' }}
              </h1>
              <p
                class="mt-1 font-medium tracking-wider uppercase text-sm"
                :class="roleNev?.trim().toLowerCase() === 'admin' ? 'text-red-500 font-bold drop-shadow-md' : 'text-earth-200/70'"
              >
                {{ roleNev || 'Felhasználó' }}
              </p>
            </div>

            <div v-if="profilAdatok.ertekeles > 0" class="flex items-center justify-center gap-2 mt-2 sm:mt-0 sm:absolute sm:bottom-8 sm:left-1/2 sm:-translate-x-1/2 w-full sm:w-auto z-10">
              <span class="text-earth-50 font-bold text-lg tracking-wide">{{ profilAdatok.ertekeles }}</span>

              <div class="flex items-center text-yellow-400 drop-shadow gap-0.5">
                <template v-for="i in 5" :key="i">
                  <svg v-if="profilAdatok.ertekeles >= i" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
                    <path fill-rule="evenodd" d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.006 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006z" clip-rule="evenodd" />
                  </svg>
                  <svg v-else-if="profilAdatok.ertekeles >= i - 0.5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
                    <path fill-rule="evenodd" d="M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25zm-1.125 4.5a1.125 1.125 0 100 2.25 1.125 1.125 0 000-2.25zM12 18.354L7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006c.448-1.077 1.976-1.077 2.424 0l2.082 5.006 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354z" clip-rule="evenodd" />
                  </svg>
                  <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
                  </svg>
                </template>
              </div>

              <span class="text-earth-200/50 text-sm font-medium ml-1">({{ profilAdatok.ertekelesSzam }} értékelés)</span>
            </div>

            <div class="w-28 h-28 sm:w-32 sm:h-32 shrink-0 rounded-full border-4 border-earth-200/30 shadow-xl overflow-hidden bg-earth-800">
              <img :src="getImageUrl(profilAdatok.IMGString) || '/default-avatar.png'" alt="Profilkép" class="w-full h-full object-cover">
            </div>
          </div>

          <div v-if="isLoading" class="flex justify-center items-center p-20">
            <i class="fa-solid fa-spinner fa-spin text-4xl text-green-400"></i>
          </div>

          <div v-else class="px-5 sm:px-8 py-6">

            <div v-if="activeTab === 'profil'" class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 animate-fade-in">
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

              <div class="flex flex-col h-full gap-4">
                <div class="flex flex-col flex-1 transition-all duration-300">
                  <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Rólam</label>
                  <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 shadow-inner min-h-[120px] flex-1 whitespace-pre-line italic font-light">
                    {{ profilAdatok.rolam || 'Ez a felhasználó még nem írt bemutatkozást.' }}
                  </div>
                </div>

                <div v-if="(profilAdatok.roleId == 4 || profilAdatok.roleId == 5) && cimkek.length > 0" class="relative flex flex-col flex-1 border border-green-500/50 rounded-lg bg-earth-50/5 transition-all z-20">
                  <div class="p-2.5 border-b border-green-500/30 mx-2">
                    <label class="block text-sm font-bold text-earth-100 mb-1 ml-1">Vállalt tevékenységek</label>
                  </div>
                  <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1 text-earth-50">
                    <span v-for="(cimke, index) in cimkek" :key="index" class="px-3 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all" :class="getCimkeStyle(index)?.tag">
                      {{ cimke }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="activeTab === 'galeria'" class="animate-fade-in text-center text-earth-100 py-10">
              <i class="fa-solid fa-images text-4xl mb-4 text-earth-200/50"></i>
              <h3 class="text-xl font-semibold">Galéria</h3>
              <p class="mt-2 text-earth-200/70">Ide jöhetnek a feltöltött képek...</p>
            </div>

            <div v-else-if="activeTab === 'munka'" class="animate-fade-in text-center text-earth-100 py-10">
              <i class="fa-solid fa-briefcase text-4xl mb-4 text-earth-200/50"></i>
              <h3 class="text-xl font-semibold">Munka nézet</h3>
              <p class="mt-2 text-earth-200/70">Ide jöhetnek az eddigi munkák, tapasztalatok...</p>
            </div>

            <div v-else-if="activeTab === 'hozzaszolasok'" class="animate-fade-in text-center text-earth-100 py-10">
              <i class="fa-solid fa-comments text-4xl mb-4 text-earth-200/50"></i>
              <h3 class="text-xl font-semibold">Hozzászólások</h3>
              <p class="mt-2 text-earth-200/70">Itt jelenhetnek meg a felhasználóhoz írt értékelések vagy kommentek...</p>
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
import api from '@/services/axios'

const route = useRoute();
const router = useRouter();
const toastStore = useToastStore();

const isLoading = ref(true);
const roles = ref<{ id: number, name: string }[]>([]);
const activeTab = ref('profil');

const profilAdatok = reactive({
  vezetekNev: '',
  keresztNev: '',
  email: '',
  telefon: '',
  telepules: '',
  roleId: 0,
  rolam: '',
  IMGString: '',
  ertekeles: 0,
  ertekelesSzam: 0,
});

const cimkek = ref<string[]>([]);

const teljesNev = computed(() => {
  if (!profilAdatok.vezetekNev && !profilAdatok.keresztNev) return '';
  return `${profilAdatok.vezetekNev} ${profilAdatok.keresztNev}`.trim();
});

const roleNev = computed(() => {
  const role = roles.value.find(r => r.id === profilAdatok.roleId);
  return role ? role.name : '';
});

const adatokBetoltese = async () => {
  const userId = route.params.id as string;
  if (!userId) return;

  try {
    isLoading.value = true;

    const rolesRes = await authService.getRoles();
    roles.value = rolesRes.data;

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

    // Tesztadatok az értékeléshez, ha a backend még nem küldi vissza
    profilAdatok.ertekeles = d.ertekeles || 4.8;
    profilAdatok.ertekelesSzam = d.ertekelesSzam || 12;

    if (d.cimkek && Array.isArray(d.cimkek)) {
      cimkek.value = d.cimkek.map((c: string) => c.trim());
    }
  } catch (error) {
    console.error("Betöltési hiba:", error);
    toastStore.addToast('Nem sikerült betölteni a profilt!', 4000, 'error');
    router.push('/');
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  adatokBetoltese();
});

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

const getImageUrl = (fileName: string) => {
  if (!fileName || fileName.trim() === '') return null;

  const axiosBaseUrl = api.defaults.baseURL;
  if (!axiosBaseUrl) return null;

  const origin = new URL(axiosBaseUrl).origin;
  return `${origin}/resources/Profiles/${fileName}`;
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(5px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>

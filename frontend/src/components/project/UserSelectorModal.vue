<template>
  <div class="fixed inset-0 z-[100] flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="bezaras">
    <div class="bg-earth-900 border border-earth-100/20 w-full max-w-md rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[80vh]">

      <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
        <h2 class="text-lg font-bold text-earth-50">Új csapattag hozzáadása</h2>
        <button @click="bezaras" class="text-earth-400 hover:text-earth-100 transition-colors">
          <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
        </button>
      </div>

      <div class="p-5 bg-earth-900/40 flex flex-col gap-4 overflow-hidden">

        <div class="relative shrink-0">
          <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <input
            v-model="kereses"
            @input="keresesInditasa"
            type="text"
            placeholder="Keresés név alapján..."
            class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-3 pl-12 pr-4 focus:outline-none focus:border-green-500 transition-all shadow-inner"
          >
        </div>

        <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2 pr-1 min-h-[250px]">

          <div v-if="isSearching" class="text-center py-10 text-earth-400 flex flex-col items-center justify-center gap-3">
            <svg class="w-8 h-8 text-green-500 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
            <span>Keresés...</span>
          </div>

          <div v-else-if="users.length === 0 && kereses.length > 1" class="text-center py-10 text-earth-500 text-sm">
            Nincs találat erre a névre.
          </div>

          <div v-else-if="users.length === 0 && kereses.length <= 1" class="text-center py-10 text-earth-500 text-sm">
            Gépelj be legalább 2 karaktert a kereséshez!
          </div>

          <div v-for="user in users" :key="user.id" class="flex items-center justify-between p-3 bg-earth-800/40 hover:bg-earth-800 rounded-xl border border-earth-700 hover:border-earth-500 transition-all group">

            <div @click="profilMegnyitasa(user.id)" class="flex items-center gap-3 min-w-0 cursor-pointer flex-1">
              <div class="w-10 h-10 rounded-full bg-earth-950 border border-earth-600 overflow-hidden shrink-0 flex items-center justify-center text-earth-400 transition-transform group-hover:scale-105">
                <img v-if="user.avatar"
                     :src="getImageUrl(user.avatar)"
                     @error="user.avatar = null"
                     class="w-full h-full object-cover">
                <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
              </div>
              <div class="min-w-0">
                <p class="text-earth-50 font-bold text-sm truncate group-hover:text-green-400 transition-colors">{{ user.nev }}</p>
                <p class="text-earth-400 text-xs truncate">{{ user.szakma || 'Felhasználó' }}</p>
              </div>
            </div>

            <button @click.stop="kivalaszt(user)" class="bg-earth-900 group-hover:bg-green-600 text-earth-300 group-hover:text-white border border-earth-600 group-hover:border-green-500 px-3 py-1.5 rounded-lg text-xs font-bold transition-all shrink-0 active:scale-95 ml-3">
              Hozzáadás
            </button>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import apiClient from '@/services/axios'; // Fontos: A te Axios példányod a baseURL-hez

const emit = defineEmits(['close', 'select']);

const kereses = ref('');
const isSearching = ref(false);
const users = ref<any[]>([]);
let timeoutId: any = null;

const bezaras = () => emit('close');

// --- A TE TÖKÉLETES KÉP-URL GENERÁLÓ LOGIKÁD ---
const getImageUrl = (fileName: string) => {
  if (!fileName || fileName.trim() === '') return null;

  // Biztosíték: Ha már eleve teljes link lenne
  if (fileName.startsWith('http')) return fileName;

  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return null;

  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

const kivalaszt = (user: any) => {
  // Mielőtt átadjuk a kiválasztott embert, kicseréljük az avatart a formázott teljes URL-re,
  // így a ProjectDashboard listájában is rögtön jó lesz a kép!
  const processedUser = {
    ...user,
    avatar: getImageUrl(user.avatar)
  };
  emit('select', processedUser);
  bezaras();
};

const profilMegnyitasa = (userId: string | number) => {
  window.open(`/user/${userId}`, '_blank');
};

const keresesInditasa = () => {
  clearTimeout(timeoutId);

  if (kereses.value.trim().length < 2) {
    users.value = [];
    isSearching.value = false;
    return;
  }

  isSearching.value = true;

  timeoutId = setTimeout(async () => {
    try {
      const response = await apiClient.get(`/search?q=${kereses.value}`);
      users.value = response.data;
    } catch (error) {
      console.error("Hiba a felhasználók keresésekor:", error);
    } finally {
      isSearching.value = false;
    }
  }, 500);
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
</style>

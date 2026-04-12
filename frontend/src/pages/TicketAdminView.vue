<template>
  <div class="flex flex-col items-center w-full min-h-[calc(100dvh-5rem)] p-4 sm:p-6 bg-earth-950 relative">
    <div class="w-full max-w-[1200px] flex flex-col bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-2xl overflow-hidden animate-fade-in p-6">

      <div class="flex items-center gap-4 mb-8 border-b border-earth-100/10 pb-4">
        <i class="fa-solid fa-ticket text-3xl text-amber-500"></i>
        <div>
          <h1 class="text-2xl font-bold text-earth-50">Hibajegyek (Tickets)</h1>
          <p class="text-earth-400 text-sm">Felhasználói bejelentések és technikai problémák kezelése</p>
        </div>
      </div>

      <div v-if="isLoading" class="flex-1 flex flex-col items-center justify-center py-20">
        <svg class="w-10 h-10 text-amber-500 animate-spin mb-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
        <span class="text-earth-400 font-medium">Hibajegyek betöltése...</span>
      </div>

      <div v-else-if="tickets.length === 0" class="flex-1 bg-earth-950/50 rounded-xl border border-earth-100/10 p-10 flex flex-col items-center justify-center text-center">
        <i class="fa-solid fa-inbox text-5xl text-earth-600 mb-4"></i>
        <h3 class="text-lg font-bold text-earth-300">A hibajegyek listája üres</h3>
        <p class="text-sm text-earth-500 mt-2 max-w-md">Jelenleg nincs egyetlen bejelentett probléma sem a rendszerben.</p>
      </div>

      <div v-else class="flex flex-col gap-4 overflow-y-auto custom-scrollbar pr-2 max-h-[70vh]">
        <div v-for="ticket in tickets" :key="ticket.id"
             class="bg-earth-950/40 border rounded-xl p-5 transition-all"
             :class="ticket.isResolved ? 'border-green-500/30 opacity-70' : 'border-amber-500/30 shadow-md'">

          <div class="flex flex-col sm:flex-row justify-between gap-4">

            <div class="flex-1">
              <div class="flex items-center gap-3 mb-3">
                <span v-if="ticket.isResolved" class="bg-green-500/20 text-green-400 text-[10px] font-black uppercase px-2 py-1 rounded-md border border-green-500/20">Megoldva</span>
                <span v-else class="bg-amber-500/20 text-amber-400 text-[10px] font-black uppercase px-2 py-1 rounded-md border border-amber-500/20">Nyitott</span>

                <h3 class="text-lg font-bold text-earth-50">{{ ticket.title }}</h3>
              </div>

              <p class="text-earth-300 text-sm leading-relaxed mb-4 whitespace-pre-wrap">{{ ticket.description }}</p>

              <div class="flex items-center gap-2 text-xs text-earth-500">
                <i class="fa-regular fa-clock"></i>
                <span>{{ formatDate(ticket.createdAt) }}</span>
              </div>
            </div>

            <div class="flex flex-col sm:items-end justify-between shrink-0 gap-4 sm:w-48 border-t sm:border-t-0 sm:border-l border-earth-100/10 pt-4 sm:pt-0 sm:pl-4">

              <div class="flex items-center gap-3 cursor-pointer group" @click="goToProfile(ticket.userId)">
                <div class="text-right hidden sm:block">
                  <p class="text-xs text-earth-400 font-medium">Beküldő</p>
                  <p class="text-sm text-earth-100 font-bold truncate max-w-[120px] group-hover:text-amber-500 transition-colors">{{ ticket.bekuldoNeve }}</p>
                </div>

                <div class="w-11 h-11 rounded-full bg-earth-900 border-2 border-earth-700 overflow-hidden shrink-0 flex items-center justify-center transition-all group-hover:border-amber-500 group-hover:scale-105 shadow-inner relative">
                  <img v-if="isValidAvatar(ticket.bekuldoAvatar)" :src="getImageUrl(ticket.bekuldoAvatar)" class="w-full h-full object-cover">
                  <div v-else class="w-full h-full flex items-center justify-center bg-earth-800 text-amber-500 text-sm font-black uppercase shadow-inner tracking-widest">
                    {{ getUserInitials(ticket.bekuldoNeve) }}
                  </div>
                </div>

                <div class="sm:hidden">
                  <p class="text-sm text-earth-100 font-bold group-hover:text-amber-500 transition-colors">{{ ticket.bekuldoNeve }}</p>
                </div>
              </div>

              <button v-if="!ticket.isResolved" @click="resolveTicket(ticket.id)" class="w-full bg-amber-600 hover:bg-amber-500 text-white font-bold py-2 rounded-lg text-xs transition-all active:scale-95 shadow-lg flex items-center justify-center gap-2">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"/></svg>
                Lezárás
              </button>
            </div>

          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '@/stores/toast'; // <-- TOAST IMPORT
import apiClient from '@/services/axios';

const router = useRouter();
const toastStore = useToastStore(); // <-- TOAST PÉLDÁNY

interface Ticket {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  isResolved: boolean;
  bekuldoNeve: string;
  bekuldoAvatar: string | null;
  userId: number;
}

const tickets = ref<Ticket[]>([]);
const isLoading = ref(true);

const fetchTickets = async () => {
  isLoading.value = true;
  try {
    const response = await apiClient.get('/Ticket');
    tickets.value = response.data;
  } catch (error) {
    console.error("Hiba a hibajegyek betöltésekor:", error);
    // HIBÁT JELZŐ TOAST
    toastStore.addToast('Hiba történt a hibajegyek betöltésekor!', 3000, 'error');
  } finally {
    isLoading.value = false;
  }
};

const resolveTicket = async (id: number) => {
  // A confirm() marad, mert ez egy "Biztos vagy benne?" kérdés, nem csak értesítés!
  if(!confirm("Biztosan lezárod ezt a hibajegyet? A művelet nem vonható vissza.")) return;

  try {
    await apiClient.put(`/Ticket/${id}/resolve`);
    const ticketToUpdate = tickets.value.find(t => t.id === id);
    if (ticketToUpdate) {
      ticketToUpdate.isResolved = true;
    }

    // SIKERES TOAST az alert() helyett
    toastStore.addToast('Hibajegy sikeresen lezárva!', 3000, 'success');

  } catch (error) {
    console.error("Hiba a hibajegy lezárásakor:", error);
    // HIBÁT JELZŐ TOAST az alert() helyett
    toastStore.addToast('Nem sikerült lezárni a hibajegyet.', 3000, 'error');
  }
};

const getImageUrl = (fileName?: string | null): string | undefined => {
  if (!fileName || fileName.trim() === '') return undefined;
  if (fileName.startsWith('http')) return fileName;
  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return undefined;
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

const isValidAvatar = (avatarName: string | null | undefined) => {
  if (!avatarName || avatarName.trim() === '') return false;
  const lowerName = avatarName.toLowerCase();
  const badKeywords = ['default', 'placeholder', 'dev', 'weird', 'test'];
  for (const word of badKeywords) {
    if (lowerName.includes(word)) return false;
  }
  return true;
};

const getUserInitials = (fullName?: string) => {
  if (!fullName) return '??';
  const names = fullName.trim().split(' ');
  if (names.length === 0) return '??';
  let initials = names[0].charAt(0).toUpperCase();
  if (names.length > 1) {
    initials += names[names.length - 1].charAt(0).toUpperCase();
  }
  return initials;
};

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('hu-HU', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit'
  }).format(date);
};

const goToProfile = (userId?: number) => {
  if(userId) {
    router.push(`/user/${userId}`);
  }
};

onMounted(() => {
  fetchTickets();
});
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.3s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
</style>

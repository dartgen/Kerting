<template>
  <div class="flex items-center justify-center w-full h-full min-h-[calc(100dvh-4rem)] p-0 sm:p-6 bg-earth-950">
    <div class="relative w-full max-w-6xl h-full sm:h-[85vh] flex bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-none sm:rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden">

      <div class="w-full md:w-80 md:flex-shrink-0 lg:w-96 border-r border-earth-200/20 flex flex-col bg-earth-900/40"
           :class="{'hidden md:flex': aktivChatId !== null}">

        <div class="p-5 border-b border-earth-200/20">
          <h1 class="text-xl font-bold text-earth-50 mb-4 tracking-wide">Üzenetek</h1>
          <div class="relative">
            <svg class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-earth-200/50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input type="text" placeholder="Keresés..."
                   class="w-full bg-earth-50/5 border border-earth-200/20 rounded-lg py-2.5 pl-9 pr-4 text-sm text-earth-50 focus:outline-none focus:ring-1 focus:ring-green-400 transition-all placeholder-earth-200/40 shadow-inner">
          </div>
        </div>

        <div class="flex-1 overflow-y-auto custom-scrollbar">
          <div v-if="toltesLista" class="p-4 text-center text-earth-300">
            <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
          </div>

          <div v-else v-for="chat in beszelgetesek" :key="chat.id"
               @click="aktivChatId = chat.id"
               :class="[
                 'p-4 flex items-center gap-3 cursor-pointer transition-all border-l-4',
                 aktivChatId === chat.id
                  ? 'bg-green-500/10 border-green-500'
                  : 'border-transparent hover:bg-earth-50/5'
               ]">
            <div class="relative flex-shrink-0">
              <div class="w-12 h-12 rounded-full border border-earth-200/20 overflow-hidden bg-earth-800 shadow-md">
                <img v-if="chat.avatar" v-lazy="chat.avatarUrl" @error="chat.avatar = ''" class="w-full h-full object-cover">
                <div v-else class="w-full h-full flex items-center justify-center text-earth-400">
                  <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                </div>
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex justify-between items-baseline">
                <h3 class="text-earth-50 font-semibold truncate text-sm">{{ chat.nev }}</h3>
                <span class="text-[10px] text-earth-300">{{ chat.formazottDatum }}</span>
              </div>
              <p class="text-xs text-earth-200 truncate">{{ chat.utolsoUzenet }}</p>
            </div>
            <div v-if="chat.olvasatlan" class="w-2 h-2 bg-green-500 rounded-full shadow-[0_0_8px_rgba(34,197,94,0.6)]"></div>
          </div>
        </div>
      </div>

      <div class="flex-1 flex flex-col min-w-0" :class="{'hidden md:flex': aktivChatId === null}">
        <template v-if="aktivChatId">
          <div class="px-6 py-4 border-b border-earth-200/20 flex items-center justify-between bg-earth-900/40">
            <div class="flex items-center gap-4">
              <button @click="bezaras" class="md:hidden text-earth-200 hover:text-white transition-colors">
                <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7" /></svg>
              </button>
              <div class="w-10 h-10 rounded-full border border-earth-200/30 overflow-hidden bg-earth-800 shadow-sm">
                <img v-if="aktualisChat?.avatar" v-lazy="aktualisChat?.avatarUrl" @error="aktualisChat.avatar = ''" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-earth-400">
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                </div>
              </div>
              <div>
                <h2 class="text-earth-50 font-bold leading-tight">{{ aktualisChat?.nev }}</h2>
                <p class="text-[10px] text-earth-300 uppercase tracking-widest font-medium">Beszélgetés</p>
              </div>
            </div>
          </div>

          <div ref="uzenetekKontener" class="flex-1 overflow-y-auto p-4 sm:p-6 space-y-4 custom-scrollbar bg-earth-900/10">
            <div v-if="toltesUzenetek" class="w-full text-center text-earth-300 py-4">
              Betöltés...
            </div>

            <div v-for="(msg, index) in uzenetek" :key="index"
                 :class="['flex w-full', msg.sajat ? 'justify-end' : 'justify-start']">

              <div class="flex flex-col max-w-[85%] sm:max-w-[75%]" :class="msg.sajat ? 'items-end' : 'items-start'">
                <span v-if="aktualisChat?.isGroup && !msg.sajat" class="text-[10px] text-green-400 font-bold ml-2 mb-1 uppercase">
                  {{ msg.senderName }}
                </span>

                <div :class="[
                  'w-fit max-w-full min-w-[110px] px-4 py-3 rounded-2xl shadow-sm text-sm transition-all',
                  msg.sajat
                    ? 'bg-green-600/20 border border-green-500/30 text-earth-50 rounded-tr-none shadow-green-900/10'
                    : 'bg-earth-800/80 border border-earth-200/20 text-earth-100 rounded-tl-none shadow-black/20'
                ]">

                  <div v-if="msg.imageUrl" class="mb-2">
                    <img v-lazy="msg.fullImageUrl" alt="Kép" class="max-w-full h-auto max-h-64 rounded-xl object-contain border border-earth-200/20 cursor-pointer" />
                  </div>

                  <p v-if="!msg.imageUrl || msg.szoveg !== 'Fénykép'" class="leading-relaxed whitespace-pre-wrap break-all">{{ msg.szoveg }}</p>

                  <div class="text-[10px] mt-2 opacity-60 font-medium text-right leading-none whitespace-nowrap">
                    {{ msg.formazottDatum }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="p-4 bg-earth-900/40 border-t border-earth-200/20">
            <form @submit.prevent="uzenetKuldese" class="flex items-center gap-2 max-w-5xl mx-auto">
              <input type="file" ref="kepInput" @change="kepKivalasztva" accept="image/*" class="hidden" />
              <button type="button" @click="triggerKepFeltoltes" :disabled="kuldesFolyamatban"
                      class="p-3.5 text-earth-300 hover:text-green-400 bg-earth-50/5 hover:bg-earth-50/10 border border-earth-200/30 rounded-xl transition-all disabled:opacity-50 shrink-0">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </button>

              <div class="relative flex-1">
                <input
                  ref="uzenetInput"
                  v-model="ujUzenet"
                  :disabled="kuldesFolyamatban"
                  placeholder="Üzenet írása..."
                  class="w-full bg-earth-50/5 border border-earth-200/30 rounded-xl py-3.5 px-5 text-earth-50 focus:outline-none focus:ring-2 focus:ring-green-500/50 transition-all placeholder-earth-200/30 shadow-inner disabled:opacity-50"
                />
              </div>

              <button type="submit" :disabled="!ujUzenet.trim() || kuldesFolyamatban"
                      class="p-3.5 bg-green-500/90 hover:bg-green-400 text-white rounded-xl shadow-lg shadow-green-900/20 transition-all disabled:opacity-30 disabled:grayscale flex items-center justify-center shrink-0">
                <i v-if="kuldesFolyamatban" class="fa-solid fa-spinner fa-spin w-5 h-5 flex items-center justify-center"></i>
                <svg v-else class="w-5 h-5 fill-current rotate-45" viewBox="0 0 20 20">
                  <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
                </svg>
              </button>
            </form>
          </div>
        </template>

        <div v-else class="flex-1 flex flex-col items-center justify-center text-earth-300/50 p-10 text-center">
          <div class="w-24 h-24 mb-6 rounded-full bg-earth-800/30 flex items-center justify-center border border-earth-200/10">
            <svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </div>
          <h3 class="text-xl font-semibold text-earth-50/50">Nincs kiválasztott üzenet</h3>
          <p class="mt-2 max-w-xs text-sm">Válassz egyet a bal oldali listából a beszélgetés megnyitásához.</p>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { chatService } from '@/services/chatService';
import { useToastStore } from '@/stores/toast';
import api from '@/services/axios';

// JAVÍTVA: Kibővített interfészek
interface ChatListItem {
  id: number;
  nev: string;
  utolsoUzenet: string;
  utolsoIdo: string;
  olvasatlan: boolean;
  avatar: string | null;
  isGroup: boolean;
  avatarUrl?: string;
  formazottDatum?: string;
}

interface ChatMessage {
  id: number;
  szoveg: string;
  ido: string;
  sajat: boolean;
  senderName?: string;
  imageUrl?: string;
  fullImageUrl?: string;
  formazottDatum?: string;
}

const route = useRoute();
const router = useRouter();
const toastStore = useToastStore();

const aktivChatId = ref<number | null>(null);
const ujUzenet = ref('');
const beszelgetesek = ref<ChatListItem[]>([]);
const uzenetek = ref<ChatMessage[]>([]);

const toltesLista = ref(false);
const toltesUzenetek = ref(false);
const kuldesFolyamatban = ref(false);

const uzenetekKontener = ref<HTMLElement | null>(null);
const uzenetInput = ref<HTMLInputElement | null>(null);
const kepInput = ref<HTMLInputElement | null>(null);

const aktualisChat = computed(() => beszelgetesek.value.find(c => c.id === aktivChatId.value));

const formatumDatum = (dateStr: string, idovel: boolean = false) => {
  if (!dateStr) return '';

  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;

  const maiNap = d.toDateString() === new Date().toDateString();
  const ido = d.toLocaleTimeString('hu-HU', { hour: '2-digit', minute: '2-digit' });

  if (maiNap) return ido;

  const datum = d.toLocaleDateString('hu-HU', { year: 'numeric', month: '2-digit', day: '2-digit' });

  return idovel ? `${datum} ${ido}` : datum;
};

const getImageUrl = (fileName: string | null | undefined) => {
  if (!fileName || fileName.trim() === '') return undefined;
  const axiosBaseUrl = api.defaults.baseURL;
  if (!axiosBaseUrl) return undefined;
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

const getChatImageUrl = (fileName: string | null | undefined) => {
  if (!fileName || fileName.trim() === '') return undefined;
  const axiosBaseUrl = api.defaults.baseURL;
  if (!axiosBaseUrl) return undefined;
  return `${new URL(axiosBaseUrl).origin}/resources/ChatImages/${fileName}`;
};

const gorgetesLegalulra = async () => {
  await nextTick();
  if (uzenetekKontener.value) {
    uzenetekKontener.value.scrollTop = uzenetekKontener.value.scrollHeight;
  }
};

const loadConversations = async () => {
  toltesLista.value = true;
  try {
    const response = await chatService.getConversations();
    // JAVÍTVA: Előre legeneráljuk az URL-eket és dátumokat
    beszelgetesek.value = response.data.map((chat: ChatListItem) => ({
      ...chat,
      avatarUrl: getImageUrl(chat.avatar),
      formazottDatum: formatumDatum(chat.utolsoIdo, false)
    }));
  } catch (error) {
    console.error('Hiba a beszélgetések betöltésekor:', error);
    toastStore.addToast('Nem sikerült betölteni a beszélgetéseket.', 4000, 'error');
  } finally {
    toltesLista.value = false;
  }
};

const loadMessages = async (chatId: number) => {
  toltesUzenetek.value = true;
  try {
    const response = await chatService.getMessages(chatId);
    // JAVÍTVA: Előre legeneráljuk az URL-eket és dátumokat
    uzenetek.value = response.data.map((msg: ChatMessage) => ({
      ...msg,
      fullImageUrl: getChatImageUrl(msg.imageUrl),
      formazottDatum: formatumDatum(msg.ido, true)
    }));

    const chatIndex = beszelgetesek.value.findIndex(c => c.id === chatId);
    if (chatIndex !== -1) {
      const chatItem = beszelgetesek.value[chatIndex];
      if (chatItem) {
        chatItem.olvasatlan = false;
      }
    }

    gorgetesLegalulra();
  } catch (error) {
    console.error('Hiba az üzenetek betöltésekor:', error);
    toastStore.addToast('Nem sikerült betölteni az üzeneteket.', 4000, 'error');
  } finally {
    toltesUzenetek.value = false;
  }
};

const uzenetKuldese = async () => {
  if (!ujUzenet.value.trim() || !aktivChatId.value || kuldesFolyamatban.value) return;

  kuldesFolyamatban.value = true;
  try {
    const payload = {
      conversationId: aktivChatId.value,
      content: ujUzenet.value.trim()
    };

    const response = await chatService.sendMessage(payload);

    // JAVÍTVA: Az új üzenet is megkapja az előre formázott mezőket
    const ujMsg = {
      ...response.data,
      fullImageUrl: getChatImageUrl(response.data.imageUrl),
      formazottDatum: formatumDatum(response.data.ido, true)
    };

    uzenetek.value.push(ujMsg);

    const chatIndex = beszelgetesek.value.findIndex(c => c.id === aktivChatId.value);
    if (chatIndex !== -1) {
      const chatItem = beszelgetesek.value[chatIndex];
      if (!chatItem) return;

      chatItem.utolsoUzenet = response.data.szoveg;
      chatItem.utolsoIdo = response.data.ido;
      // JAVÍTVA: A bal oldali lista dátumát is frissítjük
      chatItem.formazottDatum = formatumDatum(response.data.ido, false);

      const moved = beszelgetesek.value.splice(chatIndex, 1)[0];
      if (moved) {
        beszelgetesek.value.unshift(moved);
      }
    }

    ujUzenet.value = '';
    gorgetesLegalulra();
  } catch (error) {
    console.error('Hiba az üzenet küldésekor:', error);
    toastStore.addToast('Nem sikerült elküldeni az üzenetet.', 4000, 'error');
  } finally {
    kuldesFolyamatban.value = false;

    await nextTick();
    if (uzenetInput.value) {
      uzenetInput.value.focus();
    }
  }
};

const triggerKepFeltoltes = () => {
  if (kepInput.value) {
    kepInput.value.click();
  }
};

const kepKivalasztva = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file || !aktivChatId.value) return;

  kuldesFolyamatban.value = true;
  try {
    const response = await chatService.sendImage(aktivChatId.value, file);

    // JAVÍTVA: Az új képes üzenet is megkapja az előre formázott mezőket
    const ujMsg = {
      ...response.data,
      fullImageUrl: getChatImageUrl(response.data.imageUrl),
      formazottDatum: formatumDatum(response.data.ido, true)
    };

    uzenetek.value.push(ujMsg);

    const chatIndex = beszelgetesek.value.findIndex(c => c.id === aktivChatId.value);
    if (chatIndex !== -1) {
      const chatItem = beszelgetesek.value[chatIndex];
      if (!chatItem) return;

      chatItem.utolsoUzenet = "Fénykép";
      chatItem.utolsoIdo = response.data.ido;
      // JAVÍTVA: A bal oldali lista dátumát is frissítjük
      chatItem.formazottDatum = formatumDatum(response.data.ido, false);

      const moved = beszelgetesek.value.splice(chatIndex, 1)[0];
      if (moved) {
        beszelgetesek.value.unshift(moved);
      }
    }

    gorgetesLegalulra();
  } catch (error) {
    console.error('Kép feltöltési hiba:', error);
    toastStore.addToast('Nem sikerült elküldeni a képet.', 4000, 'error');
  } finally {
    kuldesFolyamatban.value = false;
    if (kepInput.value) kepInput.value.value = '';
    await nextTick();
    if (uzenetInput.value) uzenetInput.value.focus();
  }
};

watch(aktivChatId, (newId) => {
  if (newId !== null) {
    uzenetek.value = [];
    loadMessages(newId);
  }
});

watch(uzenetek, async () => {
  await nextTick();
  await gorgetesLegalulra();
}, { deep: true });

const bezaras = () => {
  aktivChatId.value = null;
};

onMounted(async () => {
  await loadConversations();

  const targetId = route.query.targetId;

  if (targetId) {
    try {
      toltesLista.value = true;
      const response = await chatService.getOrCreateConversation(Number(targetId));
      const ujChatId = response.data;

      const letezoChat = beszelgetesek.value.find(c => c.id === ujChatId);
      if (!letezoChat) {
        await loadConversations();
      }

      aktivChatId.value = ujChatId;

    } catch (error: any) {
      console.error("Hiba a beszélgetés megnyitásakor:", error);

      if (error.response && error.response.status === 400) {
        toastStore.addToast('Saját magaddal nem indíthatsz beszélgetést!', 4000, 'warning');
      } else {
        toastStore.addToast('Nem sikerült megnyitni a beszélgetést.', 4000, 'error');
      }
    } finally {
      toltesLista.value = false;
      router.replace({ query: {} });
    }
  }
});
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.08); border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: rgba(74, 222, 128, 0.3); }
</style>

<template>
  <div class="w-full p-2 sm:p-4">
    <header class="header-card relative z-30">
      <div class="w-full mx-auto px-4 sm:px-6">
        <div class="relative h-16 sm:h-20">
          <div class="absolute left-1/2 transform -translate-x-1/2 top-1/2 -translate-y-1/2 h-full flex items-center">
            <RouterLink to="/" class="logo-text block text-center whitespace-nowrap"> KERTING </RouterLink>
          </div>

          <div class="absolute left-0 top-0 h-full flex items-center">
            <nav class="hidden lg:flex items-center gap-4 xl:gap-6">
              <AnimatePresence>
                <MotionDiv
                  v-for="link in displayedLinks"
                  :key="link.key"
                  :initial="{ opacity: 0, x: -20 }"
                  :animate="{ opacity: 1, x: 0 }"
                  :exit="{ opacity: 0, x: -20 }"
                  :transition="{ duration: 0.3, delay: link.delay || 0 }"
                >
                  <RouterLink :to="link.to" :class="link.bold ? 'nav-link-bold' : 'nav-link'">
                    {{ link.label }}
                  </RouterLink>
                </MotionDiv>
              </AnimatePresence>
            </nav>

            <div class="lg:hidden flex items-center">
              <button @click="mobileMenuOpen = !mobileMenuOpen" class="text-earth-100 p-1 z-50 relative">
                <svg v-if="!mobileMenuOpen" class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                </svg>
                <svg v-else class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div class="absolute right-0 top-0 h-full flex items-center gap-3 sm:gap-5">

            <div v-if="authStore.isAuthenticated" class="relative hidden sm:block">
              <div class="relative flex items-center">
                <input
                  v-model="kereses"
                  @input="keresesInditasa"
                  type="text"
                  placeholder="Keresés..."
                  class="bg-earth-950/50 border border-earth-100/10 text-earth-50 placeholder-earth-500 rounded-full py-2 pl-10 pr-4 focus:outline-none focus:border-green-500/50 transition-all w-48 lg:w-64 text-sm shadow-inner"
                >
                <svg class="w-4 h-4 absolute left-3.5 text-earth-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
              </div>

              <div v-if="showSearchResults" class="fixed inset-0 z-30" @click="showSearchResults = false"></div>
              <AnimatePresence>
                <MotionDiv
                  v-if="showSearchResults"
                  class="absolute right-0 mt-3 w-80 bg-earth-900/95 backdrop-blur-md border border-earth-100/20 rounded-2xl shadow-2xl z-50 overflow-hidden flex flex-col max-h-[400px]"
                  :initial="{ opacity: 0, y: -10 }"
                  :animate="{ opacity: 1, y: 0 }"
                  :exit="{ opacity: 0, y: -10 }"
                >
                  <div class="overflow-y-auto custom-scrollbar p-2">
                    <div v-if="isSearching" class="p-6 text-center text-earth-400 flex flex-col items-center gap-2">
                      <svg class="w-6 h-6 text-green-500 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                      <span class="text-xs">Keresés...</span>
                    </div>

                    <div v-else-if="foundUsers.length === 0" class="p-6 text-center text-earth-500 text-xs italic">
                      Nincs találat.
                    </div>

                    <div v-else v-for="user in foundUsers" :key="user.id"
                         @click="goToProfile(user.id)"
                         class="flex items-center gap-3 p-3 hover:bg-earth-800 rounded-xl cursor-pointer transition-all border border-transparent hover:border-earth-700 group">
                      <div class="w-10 h-10 rounded-full bg-earth-950 border border-earth-700 overflow-hidden shrink-0 flex items-center justify-center">
                        <img v-if="user.avatar" :src="getImageUrl(user.avatar)" class="w-full h-full object-cover">
                        <svg v-else class="w-5 h-5 text-earth-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                      </div>
                      <div class="min-w-0">
                        <p class="text-earth-50 font-bold text-sm truncate group-hover:text-green-400 transition-colors">{{ user.nev }}</p>
                        <p class="text-earth-400 text-[10px] uppercase tracking-wider truncate">{{ user.szakma || 'Felhasználó' }}</p>
                      </div>
                    </div>
                  </div>
                </MotionDiv>
              </AnimatePresence>
            </div>

            <button v-if="!authStore.isAuthenticated" @click="router.push('/login')" class="btn-primary hidden lg:block">
              Bejelentkezés
            </button>

            <div v-else class="relative">
              <button @click="profileMenuOpen = !profileMenuOpen" class="btn-profile h-10 w-10 flex-shrink-0 overflow-hidden flex items-center justify-center border-2 border-white/20 hover:border-white/50 transition-all bg-earth-800 shadow-md relative z-40 rounded-full">
                <img v-if="userProfileImage" :src="userProfileImage" class="w-full h-full object-cover block" alt="Profil" />
                <svg v-else class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />
                </svg>
              </button>

              <div v-if="profileMenuOpen" class="hidden md:block fixed inset-0 z-30" @click="profileMenuOpen = false"></div>

              <AnimatePresence>
                <MotionDiv v-if="profileMenuOpen" class="absolute -right-2 md:right-0 mt-3 w-64 bg-earth-800/95 backdrop-blur-md border border-earth-100/20 rounded-xl shadow-xl py-2 z-50 overflow-hidden" :initial="{ opacity: 0, y: -10, scale: 0.95 }" :animate="{ opacity: 1, y: 0, scale: 1 }" :exit="{ opacity: 0, y: -10, scale: 0.95 }" :transition="{ duration: 0.2, ease: 'easeOut' }">
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Profilom
                  </RouterLink>
                  <RouterLink to="/projects" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-solid fa-diagram-project mr-2 text-earth-200"></i> Projektek
                  </RouterLink>
                  <RouterLink to="/profile/gallery" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-image mr-2 text-earth-200"></i> Saját Galéria
                  </RouterLink>
                  <RouterLink to="/chat" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-comments mr-2 text-earth-200"></i> Csevegés
                  </RouterLink>
                  <RouterLink to="/calendar" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-calendar mr-2 text-earth-200"></i> Naptár
                  </RouterLink>

                  <template v-if="isAdmin">
                    <div class="my-1 border-t border-earth-100/10"></div>
                    <div class="px-4 py-1.5 text-[10px] font-black text-amber-500 uppercase tracking-widest">Adminisztráció</div>
                    <RouterLink to="/admin/featured-users" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                      <i class="fa-solid fa-star mr-2 text-amber-500"></i> Kiemelt felhasználók
                    </RouterLink>
                    <RouterLink to="/admin/tickets" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                      <i class="fa-solid fa-ticket mr-2 text-amber-500"></i> Hibajegyek (Tickets)
                    </RouterLink>
                  </template>

                  <div class="my-1 border-t border-earth-100/10"></div>

                  <button @click="openTicketModal" class="w-full text-left block px-4 py-2 text-sm text-earth-300 hover:text-earth-50 hover:bg-earth-700/50 transition-colors">
                    <i class="fa-solid fa-headset mr-2"></i> Hibajegy küldése
                  </button>

                  <button @click="handleKijelentkezes" class="w-full text-left block px-4 py-2 text-sm text-red-400 hover:bg-earth-700/50 transition-colors">
                    <i class="fa-solid fa-arrow-right-from-bracket mr-2"></i> Kijelentkezés
                  </button>
                </MotionDiv>
              </AnimatePresence>
            </div>
          </div>
        </div>
      </div>

      <AnimatePresence>
        <MotionDiv v-if="mobileMenuOpen" class="absolute top-full left-0 w-full bg-earth-800 border-t border-earth-100/10 shadow-2xl z-40 lg:hidden rounded-b-2xl overflow-hidden" :initial="{ opacity: 0, height: 0 }" :animate="{ opacity: 1, height: 'auto' }" :exit="{ opacity: 0, height: 0 }">
          <div class="px-4 py-6 flex flex-col space-y-4 max-h-[85vh] overflow-y-auto custom-scrollbar">
            <div v-if="authStore.isAuthenticated" class="relative">
              <input v-model="kereses" @input="keresesInditasa" type="text" placeholder="Keresés felhasználókra..." class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-3 pl-10 focus:outline-none">
              <svg class="w-5 h-5 absolute left-3 top-3 text-earth-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            </div>

            <div v-if="kereses.length >= 2 && foundUsers.length > 0" class="max-h-40 overflow-y-auto bg-earth-900/50 rounded-xl mb-4 custom-scrollbar">
              <div v-for="user in foundUsers" :key="user.id" @click="goToProfile(user.id)" class="p-3 flex items-center gap-3 border-b border-earth-100/5">
                <div class="w-8 h-8 rounded-full overflow-hidden bg-earth-950">
                  <img v-if="user.avatar" :src="getImageUrl(user.avatar)" class="w-full h-full object-cover">
                </div>
                <span class="text-sm text-earth-50">{{ user.nev }}</span>
              </div>
            </div>

            <RouterLink v-for="link in navLinks" :key="link.key" :to="link.to" class="nav-link text-lg text-center py-1" @click="mobileMenuOpen = false"> {{ link.label }} </RouterLink>

            <hr v-if="authStore.isAuthenticated" class="border-earth-100/10 mx-10" />

            <template v-if="authStore.isAuthenticated">
              <div class="grid grid-cols-2 gap-3 pt-2">
                <RouterLink to="/profile" class="flex items-center justify-center p-3 rounded-xl bg-earth-900/50 text-earth-100 text-sm font-medium transition-all active:scale-95 border border-earth-100/10" @click="mobileMenuOpen = false"> <i class="fa-regular fa-user mr-2"></i> Profilom </RouterLink>
                <RouterLink to="/projects" class="flex items-center justify-center p-3 rounded-xl bg-earth-900/50 text-earth-100 text-sm font-medium transition-all active:scale-95 border border-earth-100/10" @click="mobileMenuOpen = false"> <i class="fa-solid fa-diagram-project mr-2"></i> Projektek </RouterLink>
                <RouterLink to="/profile/gallery" class="flex items-center justify-center p-3 rounded-xl bg-earth-900/50 text-earth-100 text-sm font-medium transition-all active:scale-95 border border-earth-100/10" @click="mobileMenuOpen = false"> <i class="fa-regular fa-image mr-2"></i> Galéria </RouterLink>
                <RouterLink to="/chat" class="flex items-center justify-center p-3 rounded-xl bg-earth-900/50 text-earth-100 text-sm font-medium transition-all active:scale-95 border border-earth-100/10" @click="mobileMenuOpen = false"> <i class="fa-regular fa-comments mr-2"></i> Csevegés </RouterLink>
                <RouterLink to="/calendar" class="col-span-2 flex items-center justify-center p-3 rounded-xl bg-earth-900/50 text-earth-100 text-sm font-medium transition-all active:scale-95 border border-earth-100/10" @click="mobileMenuOpen = false"> <i class="fa-regular fa-calendar mr-2"></i> Naptár </RouterLink>
              </div>

              <div v-if="isAdmin" class="mt-4 pt-4 border-t border-earth-100/10">
                <p class="text-xs font-bold text-amber-500 uppercase tracking-widest mb-3 pl-2">Adminisztráció</p>
                <div class="grid grid-cols-1 gap-2">
                  <RouterLink to="/admin/featured-users" class="flex items-center p-3 rounded-xl bg-amber-900/20 text-amber-100 text-sm font-medium transition-all active:scale-95 border border-amber-500/20" @click="mobileMenuOpen = false">
                    <i class="fa-solid fa-star mr-3 text-amber-500 w-5 text-center"></i> Kiemelt felhasználók
                  </RouterLink>
                  <RouterLink to="/admin/tickets" class="flex items-center p-3 rounded-xl bg-amber-900/20 text-amber-100 text-sm font-medium transition-all active:scale-95 border border-amber-500/20" @click="mobileMenuOpen = false">
                    <i class="fa-solid fa-ticket mr-3 text-amber-500 w-5 text-center"></i> Hibajegyek (Tickets)
                  </RouterLink>
                </div>
              </div>

              <div class="mt-4 pt-4 border-t border-earth-100/10 flex flex-col gap-2">
                <button @click="openTicketModal" class="text-earth-300 hover:text-earth-100 font-bold py-2 transition-colors">
                  <i class="fa-solid fa-headset mr-2"></i> Hibajegy küldése
                </button>
                <button @click="handleKijelentkezes" class="text-red-400 font-bold py-2 transition-colors">
                  Kijelentkezés
                </button>
              </div>
            </template>

            <button v-if="!authStore.isAuthenticated" @click="router.push('/login'); mobileMenuOpen = false" class="btn-primary w-full py-3"> Bejelentkezés </button>
          </div>
        </MotionDiv>
      </AnimatePresence>
    </header>

    <div v-if="showTicketModal" class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4" @click.self="showTicketModal = false">
      <div class="bg-earth-900 border border-earth-100/20 w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col animate-fade-in">
        <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
          <h2 class="text-xl font-bold text-earth-50 flex items-center gap-3">
            <i class="fa-solid fa-headset text-green-500"></i>
            Hibajegy küldése
          </h2>
          <button @click="showTicketModal = false" class="text-earth-400 hover:text-earth-100 transition-colors">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>

        <form @submit.prevent="submitTicket" class="p-6 space-y-5 bg-earth-900/40">
          <p class="text-sm text-earth-300 leading-relaxed mb-4">
            Problémát tapasztaltál az oldalon, vagy segítségre van szükséged? Írd meg nekünk, és az adminisztrátorok hamarosan felveszik veled a kapcsolatot!
          </p>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Tárgy / Röviden a problémáról *</label>
            <input v-model="ticketForm.title" type="text" required placeholder="Pl.: Nem tudok képet feltölteni" class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 transition-all">
          </div>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Részletes leírás *</label>
            <textarea v-model="ticketForm.description" required rows="4" placeholder="Kérlek írd le minél pontosabban, hogy miben segíthetünk..." class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 transition-all resize-none"></textarea>
          </div>

          <div class="pt-4 border-t border-earth-100/10 flex justify-end gap-3 mt-2">
            <button type="button" @click="showTicketModal = false" class="px-5 py-2.5 text-sm font-bold text-earth-300 hover:text-earth-50 transition-colors">Mégse</button>
            <button type="submit" :disabled="isSendingTicket" class="bg-green-600 hover:bg-green-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-2.5 px-8 rounded-xl transition-all shadow-lg active:scale-95 flex items-center gap-2">
              <svg v-if="isSendingTicket" class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
              {{ isSendingTicket ? 'Küldés...' : 'Küldés' }}
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from "@/stores/authStore"
import { useToastStore } from '@/stores/toast'
import apiClient from '@/services/axios'

interface NavLink {
  key: string;
  label: string;
  to: string;
  bold?: boolean;
  delay?: number;
}

interface SearchUser {
  id: number | string;
  nev: string;
  avatar?: string | null;
  szakma?: string;
}

const authStore = useAuthStore()
const toastStore = useToastStore()
const router = useRouter()
const mobileMenuOpen = ref(false)
const profileMenuOpen = ref(false)

const isAdmin = computed(() => authStore.profilAdatok?.roleId === 1)

// --- TICKET LOGIKA ---
const showTicketModal = ref(false)
const isSendingTicket = ref(false)
const ticketForm = reactive({
  title: '',
  description: ''
})

const openTicketModal = () => {
  ticketForm.title = ''
  ticketForm.description = ''
  showTicketModal.value = true
  profileMenuOpen.value = false
  mobileMenuOpen.value = false
}

const submitTicket = async () => {
  if (!ticketForm.title.trim() || !ticketForm.description.trim()) {
    toastStore.addToast('Kérlek töltsd ki az összes mezőt!', 3000, 'warning');
    return;
  }

  isSendingTicket.value = true;
  try {
    await apiClient.post('/Ticket', {
      title: ticketForm.title,
      description: ticketForm.description
    });

    showTicketModal.value = false;
    toastStore.addToast('Hibajegy sikeresen elküldve! Hamarosan feldolgozzuk.', 4000, 'success');
  } catch (error) {
    console.error("Hiba a ticket küldésekor:", error);
    toastStore.addToast('Hiba történt a küldés során, kérlek próbáld újra!', 4000, 'error');
  } finally {
    isSendingTicket.value = false;
  }
}

// --- KERESŐ LOGIKA ---
const kereses = ref('')
const isSearching = ref(false)
const showSearchResults = ref(false)
const foundUsers = ref<SearchUser[]>([])
let timeoutId: ReturnType<typeof setTimeout> | null = null

const getImageUrl = (fileName?: string | null): string | undefined => {
  if (!fileName || fileName.trim() === '') return undefined
  if (fileName.startsWith('http')) return fileName
  const axiosBaseUrl = apiClient.defaults.baseURL
  if (!axiosBaseUrl) return undefined
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`
}

const keresesInditasa = () => {
  if (timeoutId !== null) {
    clearTimeout(timeoutId)
  }
  if (kereses.value.trim().length < 2) {
    foundUsers.value = []
    showSearchResults.value = false
    isSearching.value = false
    return
  }
  isSearching.value = true
  showSearchResults.value = true

  timeoutId = setTimeout(async () => {
    try {
      // JAVÍTVA: Helyes végpont a kereséshez!
      const response = await apiClient.get<SearchUser[]>(`/search?q=${kereses.value}`)
      foundUsers.value = response.data
    } catch (error) {
      console.error("Hiba a kereséskor:", error)
    } finally {
      isSearching.value = false
    }
  }, 500)
}

const goToProfile = (userId: number | string) => {
  kereses.value = ''
  showSearchResults.value = false
  mobileMenuOpen.value = false
  router.push(`/user/${userId}`)
}

const userProfileImage = computed(() => getImageUrl(authStore.profileImageName))

const navLinks: NavLink[] = [
  { key: 'forum', label: 'Fórum', to: '/forum', delay: 0.1 },
  { key: 'works', label: 'Munkák', to: '/works', delay: 0.2 },
  { key: 'gallery', label: 'Galéria', to: '/gallery', delay: 0.3 },
]

const displayedLinks = computed<NavLink[]>(() => {
  const aboutLink: NavLink = { key: 'about', label: 'Rólunk', to: '/about', bold: true, delay: 0.4 }
  return authStore.isAuthenticated ? [...navLinks, aboutLink] : [aboutLink]
})

const handleKijelentkezes = () => {
  authStore.kijelentkezes()
  profileMenuOpen.value = false
  mobileMenuOpen.value = false
  toastStore.addToast('Sikeresen kijelentkeztél!', 3000, 'success');
  router.push('/')
}

const MotionDiv = motion.div
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
.logo-text { font-family: 'Outfit', sans-serif; font-weight: 900; letter-spacing: 0.1em; color: white; }
.nav-link { color: #d1d5db; font-weight: 500; transition: color 0.2s; }
.nav-link:hover { color: white; }
.nav-link-bold { color: #22c55e; font-weight: 800; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
</style>

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
                  :transition="{ duration: 0.3, delay: link.delay }"
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
                <MotionDiv v-if="profileMenuOpen" class="absolute -right-2 md:right-0 mt-3 w-56 md:w-48 bg-earth-800/95 backdrop-blur-md border border-earth-100/20 rounded-xl shadow-xl py-2 z-50 overflow-hidden" :initial="{ opacity: 0, y: -10, scale: 0.95 }" :animate="{ opacity: 1, y: 0, scale: 1 }" :exit="{ opacity: 0, y: -10, scale: 0.95 }" :transition="{ duration: 0.2, ease: 'easeOut' }">
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Profilom
                  </RouterLink>
                  <RouterLink to="/calendar" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-calendar mr-2 text-earth-200"></i> Naptár
                  </RouterLink>
                  <div class="my-1 border-t border-earth-100/10"></div>
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
          <div class="px-4 py-6 flex flex-col space-y-4">
            <div v-if="authStore.isAuthenticated" class="relative">
              <input v-model="kereses" @input="keresesInditasa" type="text" placeholder="Keresés felhasználókra..." class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-3 pl-10 focus:outline-none">
              <svg class="w-5 h-5 absolute left-3 top-3 text-earth-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            </div>

            <div v-if="kereses.length >= 2 && foundUsers.length > 0" class="max-h-40 overflow-y-auto bg-earth-900/50 rounded-xl">
              <div v-for="user in foundUsers" :key="user.id" @click="goToProfile(user.id)" class="p-3 flex items-center gap-3 border-b border-earth-100/5">
                <div class="w-8 h-8 rounded-full overflow-hidden bg-earth-950">
                  <img v-if="user.avatar" :src="getImageUrl(user.avatar)" class="w-full h-full object-cover">
                </div>
                <span class="text-sm text-earth-50">{{ user.nev }}</span>
              </div>
            </div>

            <RouterLink v-for="link in navLinks" :key="link.key" :to="link.to" class="nav-link text-lg text-center" @click="mobileMenuOpen = false">
              {{ link.label }}
            </RouterLink>
            <button v-if="!authStore.isAuthenticated" @click="router.push('/login')" class="btn-primary w-full py-3">Bejelentkezés</button>
            <button v-else @click="handleKijelentkezes" class="text-red-400 font-bold py-2">Kijelentkezés</button>
          </div>
        </MotionDiv>
      </AnimatePresence>
    </header>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from "@/stores/authStore"
import apiClient from '@/services/axios'

const authStore = useAuthStore()
const router = useRouter()
const mobileMenuOpen = ref(false)
const profileMenuOpen = ref(false)

// --- KERESŐ LOGIKA ---
const kereses = ref('')
const isSearching = ref(false)
const showSearchResults = ref(false)
const foundUsers = ref<any[]>([])
let timeoutId: any = null

const getImageUrl = (fileName: string) => {
  if (!fileName || fileName.trim() === '') return null
  if (fileName.startsWith('http')) return fileName
  const axiosBaseUrl = apiClient.defaults.baseURL
  if (!axiosBaseUrl) return null
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`
}

const keresesInditasa = () => {
  clearTimeout(timeoutId)
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
      // JAVÍTVA: /User/search helyett a már bevált /search végpontot használjuk!
      const response = await apiClient.get(`/search?q=${kereses.value}`)
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

// --- EREDETI LOGIKA ---
const userProfileImage = computed(() => getImageUrl(authStore.profileImageName))

const navLinks = [
  { key: 'forum', label: 'Fórum', to: '/forum' },
  { key: 'works', label: 'Munkák', to: '/works' },
  { key: 'gallery', label: 'Galéria', to: '/gallery' },
]

const displayedLinks = computed(() => {
  return authStore.isAuthenticated ? [...navLinks, { key: 'about', label: 'Rólunk', to: '/about', bold: true }] : [{ key: 'about', label: 'Rólunk', to: '/about', bold: true }]
})

const handleKijelentkezes = () => {
  authStore.kijelentkezes()
  profileMenuOpen.value = false
  mobileMenuOpen.value = false
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
</style>

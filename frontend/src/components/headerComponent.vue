<template>
  <div class="w-full p-2 sm:p-4">
    <header class="header-card relative z-30">
      <div class="w-full mx-auto px-4 sm:px-6">
        <div class="relative h-16 sm:h-20">
          <!-- Logó középen (abszolút pozíció az egész oldal szélességéhez képest) -->
          <div class="absolute left-1/2 transform -translate-x-1/2 top-1/2 -translate-y-1/2 h-full flex items-center">
            <RouterLink to="/" class="logo-text block text-center whitespace-nowrap"> KERTING </RouterLink>
          </div>

          <!-- Bal oldal: Navigation -->
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

          <!-- Jobb oldal: Bejelentkezés, Profil -->
          <div class="absolute right-0 top-0 h-full flex items-center gap-3 sm:gap-5">
            <button v-if="!authStore.isAuthenticated" @click="router.push('/login')" class="btn-primary hidden lg:block">
              Bejelentkezés
            </button>

            <div v-else class="relative">
              <button
                @click="profileMenuOpen = !profileMenuOpen"
                class="btn-profile h-10 w-10 flex-shrink-0 overflow-hidden flex items-center justify-center border-2 border-white/20 hover:border-white/50 transition-all bg-earth-800 shadow-md relative z-40 rounded-full"
              >
                <img
                  v-if="userProfileImage"
                  :src="userProfileImage"
                  class="w-full h-full object-cover block"
                  alt="Profil"
                  @error="console.error('Kép a Headerben nem tudott betölteni')"
                />
                <svg v-else class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />
                </svg>
              </button>

              <div v-if="profileMenuOpen" class="hidden md:block fixed inset-0 z-30" @click="profileMenuOpen = false"></div>

              <AnimatePresence>
                <MotionDiv
                  v-if="profileMenuOpen"
                  class="absolute -right-2 md:right-0 mt-3 w-56 md:w-48 bg-earth-800/95 backdrop-blur-md border border-earth-100/20 rounded-xl shadow-xl py-2 z-50 overflow-hidden"
                  :initial="{ opacity: 0, y: -10, scale: 0.95 }"
                  :animate="{ opacity: 1, y: 0, scale: 1 }"
                  :exit="{ opacity: 0, y: -10, scale: 0.95 }"
                  :transition="{ duration: 0.2, ease: 'easeOut' }"
                >
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Profilom
                  </RouterLink>
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Projekt
                  </RouterLink>
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Munkák
                  </RouterLink>
                  <RouterLink to="/profile/gallery" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Saját Galéria
                  </RouterLink>
                  <RouterLink to="/chat" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Csevegés
                  </RouterLink>
                  <RouterLink
                    v-if="isAdmin"
                    to="/admin/featured-users"
                    class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors"
                    @click="profileMenuOpen = false"
                  >
                    <i class="fa-solid fa-star mr-2 text-earth-200"></i> Kiemelt felhasználók
                  </RouterLink>
                  <RouterLink to="/profile" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-regular fa-user mr-2 text-earth-200"></i> Naptár
                  </RouterLink>
                  <div class="my-1 border-t border-earth-100/10"></div>
                  <RouterLink to="/settings" class="block px-4 py-2 text-sm text-earth-50 hover:bg-earth-700/50 transition-colors" @click="profileMenuOpen = false">
                    <i class="fa-solid fa-gear mr-2 text-earth-200"></i> Beállítások
                  </RouterLink>
                  <button @click="handleKijelentkezes" class="w-full text-left block px-4 py-2 text-sm text-red-400 hover:bg-earth-700/50 transition-colors">
                    <i class="fa-solid fa-arrow-right-from-bracket mr-2"></i> Kijelentkezés
                  </button>
                </MotionDiv>
              </AnimatePresence>
            </div>
          </div>
        </div>
      </div>

      <Transition name="fade">
        <div v-if="mobileMenuOpen" class="fixed inset-0 bg-earth-900/60 backdrop-blur-sm z-30" @click="mobileMenuOpen = false" />
      </Transition>

      <AnimatePresence>
        <MotionDiv
          v-if="mobileMenuOpen"
          class="absolute top-full left-0 w-full bg-earth-800 border-t border-earth-100/10 shadow-2xl z-40 lg:hidden rounded-b-2xl overflow-hidden"
          :initial="{ opacity: 0, height: 0 }"
          :animate="{ opacity: 1, height: 'auto' }"
          :exit="{ opacity: 0, height: 0 }"
          :transition="{ duration: 0.3, ease: 'easeInOut' }"
        >
          <div class="px-4 py-6 flex flex-col space-y-5 text-center">

            <template v-if="authStore.isAuthenticated">
              <RouterLink v-for="link in navLinks" :key="link.key" :to="link.to" class="nav-link text-lg" @click="mobileMenuOpen = false">
                {{ link.label }}
              </RouterLink>
              <RouterLink :to="aboutLink.to" class="nav-link-bold text-lg!" @click="mobileMenuOpen = false"> {{ aboutLink.label }}</RouterLink>

              <hr class="border-earth-100/10 mx-10" />

              <div class="flex flex-col space-y-4 pt-2">
                <div class="text-xs font-bold text-earth-400 uppercase tracking-widest">Fiók</div>
                <RouterLink to="/profile" class="nav-link text-lg" @click="mobileMenuOpen = false">Profilom</RouterLink>
                <RouterLink to="/profile/gallery" class="nav-link text-lg" @click="mobileMenuOpen = false">Saját Galéria</RouterLink>
                <RouterLink to="/chat" class="nav-link text-lg" @click="mobileMenuOpen = false">Csevegés</RouterLink>
                <RouterLink v-if="isAdmin" to="/admin/featured-users" class="nav-link text-lg" @click="mobileMenuOpen = false">Kiemelt felhasználók</RouterLink>
                <RouterLink to="/settings" class="nav-link text-lg" @click="mobileMenuOpen = false">Beállítások</RouterLink>
                <button @click="handleKijelentkezes" class="font-bold text-lg text-red-400 hover:text-red-300 transition-colors pt-2">
                  Kijelentkezés
                </button>
              </div>
            </template>

            <template v-else>
              <RouterLink :to="aboutLink.to" class="nav-link-bold text-lg!" @click="mobileMenuOpen = false"> {{ aboutLink.label }}</RouterLink>
              <hr class="border-earth-100/10 mx-10" />
              <button @click="router.push('/login'); mobileMenuOpen = false" class="btn-primary w-full py-3">Bejelentkezés</button>
            </template>

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
import { useAuthStore } from "@/stores/authStore.ts"
import apiClient from '@/services/axios'

interface NavLink {
  key: string
  label: string
  to: string
  delay?: number
  bold?: boolean
}

const authStore = useAuthStore();
const router = useRouter();
const mobileMenuOpen = ref(false);
const profileMenuOpen = ref(false); // Új állapot a profil menünek

const userProfileImage = computed(() => {
  const fileName = authStore.profileImageName;
  if (!fileName) return null;
  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return null;
  try {
    const origin = new URL(axiosBaseUrl).origin;
    return `${origin}/resources/Profiles/${fileName}`;
  } catch (e) {
    console.error("URL generálási hiba:", e);
    return null;
  }
});

const navLinks: NavLink[] = [
  { key: 'forum', label: 'Fórum', to: '/forum', delay: 0.05 },
  { key: 'works', label: 'Munkák', to: '/works', delay: 0.1 },
  { key: 'gallery', label: 'Galéria', to: '/gallery', delay: 0.15 },
]

const aboutLink: NavLink = { key: 'about', label: 'Rólunk', to: '/about', bold: true }

const displayedLinks = computed<NavLink[]>(() => {
  if (authStore.isAuthenticated) {
    return [...navLinks, { ...aboutLink, delay: 0.2 }]
  }
  return [{ ...aboutLink, delay: 0 }]
})

const isAdmin = computed(() => authStore.profilAdatok?.roleId === 1)

// Közös kijelentkezés funkció ami bezárja a menüket is
const handleKijelentkezes = () => {
  authStore.kijelentkezes();
  profileMenuOpen.value = false;
  mobileMenuOpen.value = false;
  router.push('/'); // Vissza a főoldalra
}

const MotionDiv = motion.div
</script>

<style scoped>
:deep(.fade-enter-active),
:deep(.fade-leave-active) {
  transition: opacity 0.2s ease;
}
:deep(.fade-enter-from),
:deep(.fade-leave-to) {
  opacity: 0;
}
</style>

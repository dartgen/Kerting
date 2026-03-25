<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
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
const mobileMenuOpen = ref(false)

// Biztosítjuk, hogy legyen profiladatunk
onMounted(() => {
  if (authStore.isAuthenticated && !authStore.profilAdatok) {
    authStore.fetchUserProfile();
  }
});
// Profilkép URL generálása
const userProfileImage = computed(() => {
  // 1. Ellenőrizzük, hogy a store-ban egyáltalján megvan-e az adat
  const fileName = authStore.profileImageName;

  if (!fileName) return null;

  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return null;

  try {
    const origin = new URL(axiosBaseUrl).origin;
    const finalUrl = `${origin}/resources/profiles/${fileName}`;
    return finalUrl;
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

const MotionDiv = motion.div
</script>

<template>
  <div class="w-full p-2 sm:p-4">
    <header class="header-card">
      <div class="w-full mx-auto px-4 sm:px-6">
        <div class="flex items-center justify-between h-16 sm:h-20">
          <div class="flex items-center">
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
              <button @click="mobileMenuOpen = !mobileMenuOpen" class="text-earth-100 p-1">
                <svg v-if="!mobileMenuOpen" class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                </svg>
                <svg v-else class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div class="absolute left-1/2 transform -translate-x-1/2 w-[56%] sm:w-auto px-2">
            <RouterLink to="/" class="logo-text block text-center truncate"> KERTING </RouterLink>
          </div>

          <div class="flex items-center gap-3 sm:gap-5">
            <button v-if="!authStore.isAuthenticated" @click="router.push('/login')" class="btn-primary hidden lg:block">
              Bejelentkezés
            </button>

            <button
              v-else
              @click="authStore.kijelentkezes()"
              class="btn-profile overflow-hidden flex items-center justify-center border-2 border-white/20 hover:border-white/50 transition-all bg-earth-800 shadow-md"
              style="width: 40px; height: 40px; min-width: 40px; border-radius: 50%;"
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
          </div>
        </div>
      </div>

      <Transition name="fade">
        <div v-if="mobileMenuOpen" class="fixed inset-0 z-10" @click="mobileMenuOpen = false" />
      </Transition>

      <AnimatePresence>
        <MotionDiv
          v-if="mobileMenuOpen"
          class="dropdown-menu lg:hidden"
          :initial="{ opacity: 0, scaleY: 0.8 }"
          :animate="{ opacity: 1, scaleY: 1 }"
          :exit="{ opacity: 0, scaleY: 0.8 }"
          :transition="{ duration: 0.2, ease: 'easeOut' }"
        >
          <div class="px-4 py-4 flex flex-col space-y-4 text-center">
            <template v-if="authStore.isAuthenticated">
              <RouterLink v-for="link in navLinks" :key="link.key" :to="link.to" class="nav-link">
                {{ link.label }}
              </RouterLink>
              <hr class="border-earth-100/10" />
            </template>
            <RouterLink :to="aboutLink.to" class="nav-link-bold text-lg!"> {{ aboutLink.label }}</RouterLink>
            <template v-if="!authStore.isAuthenticated">
              <hr class="border-earth-100/10" />
              <button @click="router.push('/login'); mobileMenuOpen = false" class="nav-link-bold text-lg! w-full">Bejelentkezés</button>
            </template>
          </div>
        </MotionDiv>
      </AnimatePresence>
    </header>
  </div>
</template>

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

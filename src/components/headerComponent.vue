<script setup lang="ts">
import { ref, computed } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import { RouterLink } from 'vue-router'

const isLoggedIn = ref(false)
const mobileMenuOpen = ref(false)

const navLinks = [
  { key: 'forum', label: 'Fórum', to: '/forum', delay: 0.05 },
  { key: 'works', label: 'Munkák', to: '/works', delay: 0.1 },
  { key: 'gallery', label: 'Galléria', to: '/gallery', delay: 0.15 },
]

const aboutLink = { key: 'about', label: 'Rólunk', to: '/about', bold: true }

const displayedLinks = computed(() => {
  if (isLoggedIn.value) {
    return [
      ...navLinks,
      { ...aboutLink, delay: 0.2 },
    ]
  }
  return [{ ...aboutLink, delay: 0 }]
})

const toggleLogin = () => {
  isLoggedIn.value = !isLoggedIn.value
  if (!isLoggedIn.value) {
    mobileMenuOpen.value = false
  }
}
</script>

<template>
  <div class="w-full p-2 sm:p-4">
    <header class="header-card">
      <div class="w-full mx-auto px-4 sm:px-6">
        <div class="flex items-center justify-between h-16 sm:h-20">
          <!-- Baloldali menü és mobilmenü -->
          <div class="flex items-center">
            <!-- Gép Navbar -->
            <nav class="hidden lg:flex items-center gap-4 xl:gap-6">
              <AnimatePresence>
                <motion.div
                  v-for="link in displayedLinks"
                  :key="link.key"
                  :initial="{ opacity: 0, x: -20 }"
                  :animate="{ opacity: 1, x: 0 }"
                  :exit="{ opacity: 0, x: -20 }"
                  :transition="{ duration: 0.3, delay: link.delay }"
                >
                  <RouterLink
                    :to="link.to"
                    :class="link.bold ? 'nav-link-bold' : 'nav-link'"
                  >
                    {{ link.label }}
                  </RouterLink>
                </motion.div>
              </AnimatePresence>
            </nav>


            <!-- Telefonos Navbar -->
            <div class="lg:hidden flex items-center">
              <template v-if="!isLoggedIn">
                <RouterLink :to="aboutLink.to" class="nav-link-bold !text-base !tracking-normal"> {{ aboutLink.label }} </RouterLink>
              </template>
              <template v-else>
                <!-- Hamburger menü -->
                <button
                  @click="mobileMenuOpen = !mobileMenuOpen"
                  :aria-expanded="mobileMenuOpen"
                  aria-label="Toggle mobile menu"
                  class="text-earth-100 hover:text-earth-200 focus:outline-none p-1 transition-colors"
                >
                  <svg
                    v-if="!mobileMenuOpen"
                    class="w-7 h-7"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M4 6h16M4 12h16M4 18h16"
                    />
                  </svg>
                  <svg
                    v-else
                    class="w-7 h-7"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </template>
            </div>
          </div>

          <!-- Középsô logó -->
          <div class="absolute left-1/2 transform -translate-x-1/2">
            <RouterLink to="/" class="logo-text"> KERTING </RouterLink>
          </div>

          <!-- Jobb oldal: Keresés és Login/Profile -->
          <div class="flex items-center gap-3 sm:gap-5">
            <!-- Nagy keresômezô -->
            <div class="relative hidden lg:block group">
              <input type="text" placeholder="Search..." class="search-input" />
              <svg
                class="w-4 h-4 text-white absolute right-3 top-1/2 transform -translate-y-1/2 transition-colors duration-300"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
            </div>

            <!-- Telefon csak keresô gomb -->
            <button
              class="lg:hidden text-earth-100/90 hover:text-earth-400 transition-colors p-1"
              aria-label="Search"
            >
              <svg
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
            </button>

            <!-- Bejelentkezés / Profil gomb -->
            <button
              v-if="!isLoggedIn"
              @click="toggleLogin"
              aria-label="Log in"
              class="btn-primary"
            >
              Login
            </button>

            <button
              v-else
              @click="toggleLogin"
              aria-label="Profile — click to log out"
              class="btn-profile"
            >
              <svg
                class="w-4 h-4 sm:w-5 sm:h-5 text-earth-50"
                fill="currentColor"
                viewBox="0 0 20 20"
                aria-hidden="true"
              >
                <path
                  fill-rule="evenodd"
                  d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"
                  clip-rule="evenodd"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Overlay para cerrar el menú al hacer clic fuera -->
      <Transition name="fade">
        <div
          v-if="mobileMenuOpen"
          class="fixed inset-0 z-10"
          aria-hidden="true"
          @click="mobileMenuOpen = false"
        /></Transition>

      <!-- Telefonos dropdown menü -->
      <AnimatePresence>
        <motion.div
          v-if="isLoggedIn && mobileMenuOpen"
          class="dropdown-menu relative z-20 lg:hidden"
          :initial="{ opacity: 0, scaleY: 0.8 }"
          :animate="{ opacity: 1, scaleY: 1 }"
          :exit="{ opacity: 0, scaleY: 0.8 }"
          :transition="{ duration: 0.2, ease: 'easeOut' }"
        >
          <div class="px-4 py-4 flex flex-col space-y-4">
            <RouterLink
              v-for="link in navLinks"
              :key="link.key"
              :to="link.to"
              class="nav-link text-center"
            >
              {{ link.label }}
            </RouterLink>
            <hr class="border-earth-700" />
            <RouterLink :to="aboutLink.to" class="nav-link-bold !text-lg text-center"> {{ aboutLink.label }}</RouterLink>
          </div>
        </motion.div>
      </AnimatePresence>
    </header>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>

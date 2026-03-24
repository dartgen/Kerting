import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const pre = "🌱|";
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../pages/HomeView.vue'),
      meta: {
        title: `${pre} Kerting`,
        requiresGuest: true
      },
    },
    {
      path: '/gallery',
      name: 'gallery',
      component: () => import('../pages/GalleryView.vue'),
      meta: {
        title: `${pre} Galléria`,
        requiresAuth: true
      }
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('../pages/ProfileManagementView.vue'),
      meta: {
        title: `${pre} Profil`,
        requiresAuth: true
      }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../pages/LoginView.vue'),
      meta: {
        title: `${pre} Bejelentkezés`,
        hideHeader: true,
        fullPage: true,
        requiresGuest: true // <-- 1. EZT ADJUK HOZZÁ A LOGIN OLDALHOZ
      }
    }
  ],
})

router.beforeEach((to, from, next) => {
  document.title = to.meta.title as string;

  const authStore = useAuthStore();

  // 2. KIBŐVÍTETT LOGIKA AZ ÚTVONAL ŐRBEN
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Védett oldal, de NINCS bejelentkezve -> irány a login
    next({ name: 'login' });
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    // Vendég oldal (pl. login, regisztráció), de MÁR be van jelentkezve -> irány a főoldal (vagy profil)
    next({ name: 'home' });
  } else {
    // Minden más esetben mehet a dolgára
    next();
  }
});

export default router;

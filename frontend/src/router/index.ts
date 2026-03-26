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
        title: `${pre} Kerting`
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
      path: '/user/:id', // A :id egy változó, amit a komponensben le tudunk kérdezni
      name: 'public-profile',
      component: () => import('../pages/PublicProfileView.vue'),
      meta: {
        title: `${pre} Felhasználó megtekintése`,
        requiresAuth: false // Ez fontos, ha azt akarod, hogy vendégek is láthassák!
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
        requiresGuest: true
      }
    },
    {
      path: '/works',
      name: 'works',
      component: () => import('../pages/WorksView.vue'),
      meta: {
        title: `${pre} Munkák`,
        requiresAuth: true,
      }
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../pages/AboutView.vue'),
      meta: {
        title: `${pre} Rólunk`
      }
    },
    {
      path: '/forum',
      name: 'forum',
      component: () => import('../pages/ForumView.vue'),
      meta: {
        title: `${pre} Fórum`,
        requiresAuth: true
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

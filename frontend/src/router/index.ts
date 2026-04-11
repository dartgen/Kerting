import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import CalendarView from '@/pages/CalendarView.vue'

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
      path: '/calendar',
      name: 'calendar',
      component: CalendarView,
      meta: {
        title: `${pre} Naptár`
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
      path: '/gallery/:id',
      name: 'gallery-detail',
      component: () => import('../pages/GalleryDetailView.vue'),
      meta: {
        title: `${pre} Galléria részletek`,
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
    // JAVÍTVA: Itt az új Projektek útvonal!
    {
      path: '/projects',
      name: 'projects',
      component: () => import('../pages/ProjectDashboard.vue'),
      meta: {
        title: `${pre} Projektek`,
        requiresAuth: true
      }
    },
    {
      path: '/user/:id',
      name: 'public-profile',
      component: () => import('../pages/PublicProfileView.vue'),
      meta: {
        title: `${pre} Felhasználó megtekintése`,
        requiresAuth: false
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
        workScope: 'visible'
      }
    },
    {
      path: '/profile/works',
      name: 'profile-works',
      component: () => import('../pages/WorksView.vue'),
      meta: {
        title: `${pre} Saját munkák`,
        requiresAuth: true,
        workScope: 'own'
      }
    },
    {
      path: '/work/:id',
      name: 'work-detail',
      component: () => import('../pages/WorkDetailView.vue'),
      meta: {
        title: `${pre} Munka részletei`,
        requiresAuth: false, // Could be public
      }
    },
    {
      path: '/work/create',
      name: 'work-create',
      component: () => import('../pages/WorkCreateView.vue'),
      meta: {
        title: `${pre} Új Munka`,
        requiresAuth: true,
      }
    },
    {
      path: '/work/:id/edit',
      name: 'work-edit',
      component: () => import('../pages/WorkEditView.vue'),
      meta: {
        title: `${pre} Munka Szerkesztése`,
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
      path: '/chat',
      name: 'chat',
      component: () => import('../pages/ChatView.vue'),
      meta: {
        title: `${pre} Chat`,
        requiresAuth: true
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
    },
    {
      path: '/forum/:id',
      name: 'forum-post',
      component: () => import('../pages/ForumPostView.vue'),
      meta: {
        title: `${pre} Fórum bejegyzés`,
        requiresAuth: true
      }
    },
    {
      path: '/profile/gallery',
      name: 'profile-gallery',
      component: () => import('../pages/ProfileGalleryView.vue'),
      meta: {
        title: `${pre} Saját Galéria`,
        requiresAuth: true
      }
    },
    {
      path: '/admin/featured-users',
      name: 'admin-featured-users',
      component: () => import('../pages/AdminFeaturedUsersView.vue'),
      meta: {
        title: `${pre} Kiemelt felhasználók`,
        requiresAuth: true,
        requiresAdmin: true,
      }
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  document.title = to.meta.title as string;

  const authStore = useAuthStore();

  if (authStore.isAuthenticated && !authStore.profilAdatok) {
    await authStore.fetchUserProfile();
  }

  // 2. KIBŐVÍTETT LOGIKA AZ ÚTVONAL ŐRBEN
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Védett oldal, de NINCS bejelentkezve -> irány a login
    next({ name: 'login' });
  } else if (to.meta.requiresAdmin && authStore.profilAdatok?.roleId !== 1) {
    next({ name: 'home' });
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    // Vendég oldal (pl. login, regisztráció), de MÁR be van jelentkezve -> irány a főoldal (vagy profil)
    next({ name: 'home' });
  } else {
    // Minden más esetben mehet
    next();
  }
});

export default router;

import { createRouter, createWebHistory } from 'vue-router'

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
      path: '/forum',
      name: 'forum',
      component: () => import('../pages/ForumView.vue'),
      meta: {
        title: `${pre} Fórum`
      },
    },
    {
      path: '/works',
      name: 'works',
      component: () => import('../pages/WorksView.vue'),
      meta: {
        title: `${pre} Munkák`
      }
    },
    {
      path: '/gallery',
      name: 'gallery',
      component: () => import('../pages/GalleryView.vue'),
      meta: {
        title: `${pre} Galléria`
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
      path: '/login',
      name: 'login',
      component: () => import('../pages/LoginView.vue'),
      meta: {
        title: `${pre} Bejelentkezés`,
        hideHeader: true,
        fullPage: true,
      }
    }
  ],
})

router.beforeEach((to, from, next) => {
  document.title = to.meta.title as string;
  next();
});

export default router

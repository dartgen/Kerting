<template>
  <div class="flex-1 w-full h-full overflow-y-auto">
    <div class="flex items-center justify-center min-h-full px-4 sm:px-6 py-6 sm:py-10">
      <div class="flex flex-col md:flex-row items-start w-full max-w-6xl gap-6">

        <div class="w-full md:w-64 shrink-0 bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden sticky top-6">
          <div class="p-4">
            <h2 class="text-earth-200/70 font-medium tracking-wider uppercase text-xs mb-4 ml-2">Menü</h2>
            <nav class="flex flex-col gap-2">
              <button @click="activeTab = 'profil'" :class="getTabClass('profil')">
                <i class="fa-solid fa-user"></i> Profil
              </button>
              <button @click="activeTab = 'galeria'" :class="getTabClass('galeria')">
                <i class="fa-solid fa-images"></i> Galéria
              </button>
              <button @click="activeTab = 'munka'" :class="getTabClass('munka')">
                <i class="fa-solid fa-briefcase"></i> Munka nézet
              </button>
              <button @click="activeTab = 'hozzaszolasok'" :class="getTabClass('hozzaszolasok')">
                <i class="fa-solid fa-comments"></i> Értékelések
              </button>
            </nav>

            <div class="mt-6 pt-6 border-t border-earth-200/20">
              <button @click="uzenetKuldeseNyitas"
                      class="w-full flex items-center justify-center gap-2 bg-green-500/90 hover:bg-green-400 text-white px-4 py-3 rounded-xl transition-all shadow-lg hover:-translate-y-0.5 border border-green-300/50 font-bold">
                <i class="fa-solid fa-paper-plane"></i> Üzenet küldése
              </button>
            </div>

          </div>
        </div>

        <div class="relative w-full flex-1 bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden min-h-[700px]">

          <button type="button" class="absolute top-4 left-4 z-30 p-1 text-white transition-colors hover:text-earth-200 focus:outline-none" @click="vissza" aria-label="Vissza">
            <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path d="M15 6L9 12L15 18" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
          </button>

          <div class="relative px-5 pt-12 sm:pt-10 pb-6 border-b border-earth-200/20 flex flex-col sm:flex-row items-center gap-6">

            <div class="text-center sm:text-left flex-1 sm:pl-8">
              <h1 class="text-3xl font-bold text-earth-50 tracking-wide drop-shadow-md">
                {{ teljesNev || 'Profil betöltése...' }}
              </h1>
              <p class="mt-1 font-medium tracking-wider uppercase text-sm" :class="roleNev?.trim().toLowerCase() === 'admin' ? 'text-red-500 font-bold drop-shadow-md' : 'text-earth-200/70'">
                {{ roleNev || 'Felhasználó' }}
              </p>
            </div>

            <div class="flex items-center justify-center mt-4 sm:mt-0 sm:absolute sm:bottom-8 sm:left-1/2 sm:-translate-x-1/2 w-full sm:w-auto z-10">
              <div v-if="profilAdatok.ertekelesSzam > 0" class="flex items-center gap-3 bg-earth-900/60 backdrop-blur-md px-5 py-2 rounded-full border border-earth-100/10 shadow-lg">
                <span class="text-earth-50 font-bold text-xl tracking-wide">{{ profilAdatok.ertekeles }}</span>
                <div class="flex items-center drop-shadow-sm gap-0.5">
                  <svg v-for="i in Math.floor(profilAdatok.ertekeles)" :key="'full-'+i" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-yellow-400">
                    <path fill-rule="evenodd" d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.007 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006z" clip-rule="evenodd" />
                  </svg>

                  <div v-if="(profilAdatok.ertekeles % 1) >= 0.5" class="relative w-5 h-5">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="absolute inset-0 w-5 h-5 text-earth-300/40">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
                    </svg>
                    <div class="absolute inset-0 overflow-hidden" style="width: 50%;">
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-yellow-400">
                        <path fill-rule="evenodd" d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.007 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006z" clip-rule="evenodd" />
                      </svg>
                    </div>
                  </div>

                  <svg v-for="i in (5 - Math.ceil(profilAdatok.ertekeles))" :key="'empty-'+i" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5 text-earth-300/40">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
                  </svg>
                </div>
                <span class="text-earth-200/70 text-sm font-medium border-l border-earth-100/20 pl-3">
                  {{ profilAdatok.ertekelesSzam }} vélemény
                </span>
              </div>

              <div v-else class="flex items-center gap-2 bg-earth-900/40 backdrop-blur-md px-5 py-2 rounded-full border border-earth-100/10 shadow-lg">
                <div class="flex items-center text-earth-300/30 gap-0.5">
                  <svg v-for="i in 5" :key="'zero-'+i" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
                  </svg>
                </div>
                <span class="text-earth-300 text-sm font-medium italic border-l border-earth-100/20 pl-3">
                  Még nincs értékelés
                </span>
              </div>
            </div>

            <div class="w-28 h-28 sm:w-32 sm:h-32 shrink-0 rounded-full border-4 border-earth-200/30 shadow-xl overflow-hidden bg-earth-800 z-20">
              <img
                :src="getImageUrl(profilAdatok.IMGString) || '/default-avatar.png'"
                @error="profilAdatok.IMGString = ''"
                alt="Profilkép"
                class="w-full h-full object-cover">
            </div>
          </div>

          <div v-if="isLoading" class="flex justify-center items-center p-20">
            <i class="fa-solid fa-spinner fa-spin text-4xl text-green-400"></i>
          </div>

          <div v-else class="px-5 sm:px-8 py-6">
            <ProfileTab v-if="activeTab === 'profil'" :profilAdatok="profilAdatok" :cimkek="cimkek" />
            <GalleryComponent v-else-if="activeTab === 'galeria'" :userId="String(route.params.id)" title="A felhasználó munkái" subtitle="Nézd meg a feltöltött képeket és referenciákat." />
            <div v-else-if="activeTab === 'munka'" class="text-center text-earth-100 py-10 animate-fade-in">
              <i class="fa-solid fa-briefcase text-4xl mb-4 text-earth-200/50"></i>
              <h3 class="text-xl font-semibold">Munka nézet</h3>
              <p class="mt-2 text-earth-200/70">Ide jöhetnek az eddigi munkák...</p>
            </div>
            <CommentsTab v-else-if="activeTab === 'hozzaszolasok'" :userId="String(route.params.id)" @review-changed="adatokBetoltese" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { authService } from '@/services/authService';
import { useToastStore } from '@/stores/toast';
import api from '@/services/axios';
import type { PublicProfileResponse, RoleDto } from '@/types/auth';

import ProfileTab from '@/components/profile/ProfileTab.vue';
import GalleryComponent from '@/components/gallery/GalleryLayout.vue';
import CommentsTab from '@/components/profile/UserReviewsList.vue';

interface PublicProfileViewState {
  vezetekNev: string;
  keresztNev: string;
  email: string;
  telefon: string;
  telepules: string;
  roleId: number;
  rolam: string;
  IMGString: string;
  ertekeles: number;
  ertekelesSzam: number;
  facebook?: string;
  instagram?: string;
  tiktok?: string;
  username?: string;
}

const route = useRoute();
const router = useRouter();
const toastStore = useToastStore();

const isLoading = ref(true);
const roles = ref<RoleDto[]>([]);
const activeTab = ref('profil');
const cimkek = ref<string[]>([]);

const profilAdatok = reactive<PublicProfileViewState>({
  vezetekNev: '', keresztNev: '', email: '', telefon: '', telepules: '', roleId: 0, rolam: '',
  IMGString: '', ertekeles: 0, ertekelesSzam: 0, facebook: '', instagram: '', tiktok: '',
  username: ''
});

const teljesNev = computed(() => {
  const nev = `${profilAdatok.vezetekNev} ${profilAdatok.keresztNev}`.trim();
  return nev ? nev : profilAdatok.username;
});

const roleNev = computed(() => {
  const role = roles.value.find(r => r.id === profilAdatok.roleId);
  return role ? role.name : '';
});

const getTabClass = (tabName: string) => {
  const baseClass = 'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3';
  return activeTab.value === tabName
    ? `${baseClass} bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner`
    : `${baseClass} text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent`;
};

const uzenetKuldeseNyitas = () => {
  const targetUserId = route.params.id;
  router.push({
    name: 'chat',
    query: { targetId: targetUserId }
  });
};

const vissza = () => {
  if (window.history.length > 1) { router.back(); }
  else { router.push('/'); }
};

const getImageUrl = (fileName: string) => {
  if (!fileName || fileName.trim() === '') return null;
  const axiosBaseUrl = api.defaults.baseURL;
  if (!axiosBaseUrl) return null;
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

const adatokBetoltese = async () => {
  const userId = route.params.id as string;
  if (!userId) return;

  try {
    isLoading.value = true;
    const rolesRes = await authService.getRoles();
    roles.value = rolesRes.data;

    const profileRes = await authService.getPublicProfile(userId);
    const d: PublicProfileResponse = profileRes.data;

    Object.assign(profilAdatok, {
      IMGString: d.imgString || '', vezetekNev: d.vezetekNev || '', keresztNev: d.keresztNev || '',
      email: d.email || '', telefon: d.telefon || '', telepules: d.telepules || '',
      rolam: d.rolam || '', roleId: d.roleId || 0, facebook: d.facebook || '',
      instagram: d.instagram || '', tiktok: d.tiktok || '', ertekeles: d.ertekeles || 0,
      ertekelesSzam: d.ertekelesSzam || 0,
      username: d.username || ''
    });

    if (d.cimkek && Array.isArray(d.cimkek)) {
      cimkek.value = d.cimkek.map((c: string) => c.trim());
    }
  } catch (error) {
    console.error("Betöltési hiba:", error);
    toastStore.addToast('Nem sikerült betölteni a profilt!', 4000, 'error');
    await router.push('/');
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => { adatokBetoltese(); });
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>

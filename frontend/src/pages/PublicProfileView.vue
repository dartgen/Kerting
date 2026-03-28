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
                <i class="fa-solid fa-comments"></i> Hozzászólások
              </button>
            </nav>
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

            <div v-if="profilAdatok.ertekeles > 0" class="flex items-center justify-center gap-2 mt-2 sm:mt-0 sm:absolute sm:bottom-8 sm:left-1/2 sm:-translate-x-1/2 w-full sm:w-auto z-10">
              <span class="text-earth-50 font-bold text-lg tracking-wide">{{ profilAdatok.ertekeles }}</span>
              <div class="flex items-center text-yellow-400 drop-shadow gap-0.5">
                <i class="fa-solid fa-star" v-for="i in Math.floor(profilAdatok.ertekeles)" :key="'full'+i"></i>
                <i class="fa-solid fa-star-half-stroke" v-if="profilAdatok.ertekeles % 1 >= 0.5"></i>
              </div>
              <span class="text-earth-200/50 text-sm font-medium ml-1">({{ profilAdatok.ertekelesSzam }} értékelés)</span>
            </div>

            <div class="w-28 h-28 sm:w-32 sm:h-32 shrink-0 rounded-full border-4 border-earth-200/30 shadow-xl overflow-hidden bg-earth-800">
              <img :src="getImageUrl(profilAdatok.IMGString) || '/default-avatar.png'" alt="Profilkép" class="w-full h-full object-cover">
            </div>
          </div>

          <div v-if="isLoading" class="flex justify-center items-center p-20">
            <i class="fa-solid fa-spinner fa-spin text-4xl text-green-400"></i>
          </div>

          <div v-else class="px-5 sm:px-8 py-6">

            <ProfileTab
              v-if="activeTab === 'profil'"
              :profilAdatok="profilAdatok"
              :cimkek="cimkek"
            />

            <GalleryComponent
              v-else-if="activeTab === 'galeria'"
              :userId="String(route.params.id)"
              title="A felhasználó munkái"
              subtitle="Nézd meg a feltöltött képeket és referenciákat."
            />

            <div v-else-if="activeTab === 'munka'" class="text-center text-earth-100 py-10 animate-fade-in">
              <i class="fa-solid fa-briefcase text-4xl mb-4 text-earth-200/50"></i>
              <h3 class="text-xl font-semibold">Munka nézet</h3>
              <p class="mt-2 text-earth-200/70">Ide jöhetnek az eddigi munkák...</p>
            </div>

            <CommentsTab
              v-else-if="activeTab === 'hozzaszolasok'"
              :userId="String(route.params.id)"
            />

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
import api from '@/services/axios'
import type { PublicProfileResponse, RoleDto } from '@/types/auth';

// A KISZERVEZETT KOMPONENSEK IMPORTÁLÁSA
import ProfileTab from '@/components/profile/ProfileTab.vue';
import GalleryComponent from '@/components/gallery/GalleryLayout.vue';
import CommentsTab from '@/components/profile/CommentsTab.vue';

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
  IMGString: '', ertekeles: 0, ertekelesSzam: 0, facebook: '', instagram: '', tiktok: ''
});

const teljesNev = computed(() => {
  if (!profilAdatok.vezetekNev && !profilAdatok.keresztNev) return '';
  return `${profilAdatok.vezetekNev} ${profilAdatok.keresztNev}`.trim();
});

const roleNev = computed(() => {
  const role = roles.value.find(r => r.id === profilAdatok.roleId);
  return role ? role.name : '';
});

// Menügomb stílusának kiszervezése fgv-be, hogy rövidebb legyen a HTML
const getTabClass = (tabName: string) => {
  const baseClass = 'text-left px-4 py-3 rounded-xl transition-all font-medium flex items-center gap-3';
  return activeTab.value === tabName
    ? `${baseClass} bg-earth-50/10 text-green-400 border border-green-500/30 shadow-inner`
    : `${baseClass} text-earth-100 hover:bg-earth-50/5 hover:text-earth-50 border border-transparent`;
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
      ertekelesSzam: d.ertekelesSzam || 0
    });

    if (d.cimkek && Array.isArray(d.cimkek)) {
      cimkek.value = d.cimkek.map(c => c.trim());
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

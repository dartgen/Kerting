<template>
  <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 animate-fade-in">
    <div class="space-y-6">
      <div class="flex flex-col">
        <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">E-mail cím</label>
        <div class="relative">
          <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
          </svg>
          <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner break-words min-h-[48px] flex items-center">
            <a :href="'mailto:' + profilAdatok.email" class="hover:text-green-300 transition-colors">
              {{ profilAdatok.email || 'Nincs megadva' }}
            </a>
          </div>
        </div>
      </div>

      <div class="flex flex-col">
        <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Telefonszám</label>
        <div class="relative">
          <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 6.75c0 8.284 6.716 15 15 15h2.25a2.25 2.25 0 0 0 2.25-2.25v-1.372c0-.516-.351-.966-.852-1.091l-4.423-1.106c-.44-.11-.902.055-1.173.417l-.97 1.293c-2.896-1.596-5.539-4.239-7.135-7.135l1.293-.97c.363-.271.527-.734.417-1.173L6.963 3.102a1.125 1.125 0 0 0-1.091-.852H4.5A2.25 2.25 0 0 0 2.25 4.5v2.25Z" />
          </svg>
          <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner min-h-[48px] flex items-center">
            <a :href="'tel:' + profilAdatok.telefon" class="hover:text-green-300 transition-colors">
              {{ profilAdatok.telefon || 'Nincs megadva' }}
            </a>
          </div>
        </div>
      </div>

      <div class="flex flex-col">
        <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Település</label>
        <div class="relative">
          <svg class="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-earth-200/70 pointer-events-none" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
            <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1 1 15 0Z" />
          </svg>
          <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 pl-11 pr-4 text-earth-50 shadow-inner min-h-[48px] flex items-center">
            {{ profilAdatok.telepules || 'Nincs megadva' }}
          </div>
        </div>
      </div>
    </div>

    <div class="flex flex-col h-full gap-4">
      <div class="flex flex-col flex-1 transition-all duration-300">
        <label class="text-sm font-semibold text-earth-100 mb-1 ml-1">Rólam</label>
        <div class="w-full bg-earth-50/10 border border-earth-200/30 rounded-lg py-3 px-4 text-earth-50 shadow-inner min-h-[120px] flex-1 whitespace-pre-line italic font-light">
          {{ profilAdatok.rolam || 'Ez a felhasználó még nem írt bemutatkozást.' }}
        </div>
      </div>

      <div v-if="(profilAdatok.roleId == 4 || profilAdatok.roleId == 5) && cimkek.length > 0" class="relative flex flex-col flex-1 border border-green-500/50 rounded-lg bg-earth-50/5 transition-all z-20">
        <div class="p-2.5 border-b border-green-500/30 mx-2">
          <label class="block text-sm font-bold text-earth-100 mb-1 ml-1">Vállalt tevékenységek</label>
        </div>
        <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1 text-earth-50">
          <span v-for="(cimke, index) in cimkek" :key="index" class="px-3 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all" :class="getCimkeStyle(index)?.tag">
            {{ cimke }}
          </span>
        </div>
      </div>
    </div>

    <div v-if="profilAdatok.facebook || profilAdatok.instagram || profilAdatok.tiktok"
         class="md:col-span-2 pt-6 mt-2 border-t border-earth-200/20 flex justify-center gap-6">

      <a v-if="profilAdatok.facebook" :href="formatUrl(profilAdatok.facebook)" target="_blank" rel="noopener noreferrer"
         class="p-3 rounded-xl bg-blue-600 hover:bg-blue-500 text-white transition-all shadow-md hover:-translate-y-1"
         title="Facebook profil">
        <svg class="w-6 h-6 fill-current" viewBox="0 0 24 24">
          <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.469h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.469h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
        </svg>
      </a>

      <a v-if="profilAdatok.instagram" :href="formatUrl(profilAdatok.instagram)" target="_blank" rel="noopener noreferrer"
         class="p-3 rounded-xl bg-gradient-to-tr from-yellow-400 via-pink-500 to-purple-600 hover:opacity-90 text-white transition-all shadow-md hover:-translate-y-1"
         title="Instagram profil">
        <svg class="w-6 h-6 fill-current" viewBox="0 0 24 24">
          <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z"/>
        </svg>
      </a>

      <a v-if="profilAdatok.tiktok" :href="formatUrl(profilAdatok.tiktok)" target="_blank" rel="noopener noreferrer"
         class="p-3 rounded-xl bg-black hover:bg-gray-900 border border-gray-700 text-white transition-all shadow-md hover:-translate-y-1"
         title="TikTok profil">
        <svg class="w-6 h-6 fill-current" viewBox="0 0 24 24">
          <path d="M19.59 6.69a4.83 4.83 0 0 1-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 0 1-5.2 1.74 2.89 2.89 0 0 1 2.31-4.64 2.93 2.93 0 0 1 .88.13V9.4a6.84 6.84 0 0 0-1-.05A6.33 6.33 0 0 0 5 20.1a6.34 6.34 0 0 0 10.86-4.43v-7a8.16 8.16 0 0 0 4.77 1.52v-3.4a4.85 4.85 0 0 1-1-.1z"/>
        </svg>
      </a>
    </div>
  </div>
</template>

<script setup lang="ts">
// Ha van közös type fájlod, importálhatod is a PublicProfileViewState-t
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

const props = defineProps<{
  profilAdatok: PublicProfileViewState;
  cimkek: string[];
}>();

const formatUrl = (url?: string) => {
  if (!url) return '#';
  if (!/^https?:\/\//i.test(url)) {
    return `https://${url}`;
  }
  return url;
};

const getCimkeStyle = (index: number) => {
  const styles = [
    { tag: 'bg-gray-200 text-gray-700' },
    { tag: 'bg-green-100 text-green-700' },
    { tag: 'bg-orange-100 text-orange-800' }
  ];
  return styles[index % styles.length];
};
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>

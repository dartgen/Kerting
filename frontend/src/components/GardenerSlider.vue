<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useGardenStore } from '@/stores/gardenStore';
import { useRouter } from 'vue-router';
import { motion } from 'motion-v';
import apiClient from '@/services/axios';

const MotionDiv = motion.div;

const store = useGardenStore();
const router = useRouter();
const cards = computed(() => store.professionals);

const currentIndex = ref(2);
let intervalId: number | null = null;

const getCardStyle = (index: number) => {
  const totalCards = cards.value.length;
  // Üres lista kezelése
  if (totalCards === 0) return {};

  let distance = index - currentIndex.value;

  if (distance > totalCards / 2) distance -= totalCards;
  if (distance < -totalCards / 2) distance += totalCards;

  // Vizuális állapotok
  if (distance === 0) {
    return { scale: 1.1, opacity: 1, zIndex: 10, filter: 'blur(0px)' };
  }
  if (Math.abs(distance) === 1) {
    return { scale: 0.9, opacity: 0.8, zIndex: 5, filter: 'blur(1px)' };
  }
  return { scale: 0.75, opacity: 0.5, zIndex: 1, filter: 'blur(3px)' };
};

const getCardOrder = (index: number) => {
  const totalCards = cards.value.length;
  if (totalCards === 0) return 0;
  let distance = index - currentIndex.value;
  if (distance > totalCards / 2) distance -= totalCards;
  if (distance < -totalCards / 2) distance += totalCards;

  return distance + totalCards;
};

const rotateCarousel = () => {
    if (cards.value.length)
        currentIndex.value = (currentIndex.value + 1) % cards.value.length;
};

const getImageUrl = (fileName?: string) => {
  if (!fileName || fileName.trim() === '') return null;
  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return null;
  try {
    return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
  } catch {
    return null;
  }
};

const hasHalfStar = (rating: number) => (rating % 1) >= 0.5;

const goToPublicProfile = (userId: number) => {
  router.push({ name: 'public-profile', params: { id: String(userId) } });
};

onMounted(() => {
  void store.loadFeaturedProfessionals();
  intervalId = setInterval(rotateCarousel, 3000);
});

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId);
});
</script>

<template>
  <div class="w-full flex flex-col justify-center relative py-2 lg:py-0 h-full">
    <div v-if="store.featuredLoading" class="flex justify-center items-center min-h-[320px] text-earth-100">
      <i class="pi pi-spin pi-spinner text-2xl"></i>
    </div>

    <div v-else-if="store.featuredError" class="flex justify-center items-center min-h-[320px] text-red-200 font-semibold px-4 text-center">
      {{ store.featuredError }}
    </div>

    <div v-else-if="cards.length === 0" class="flex justify-center items-center min-h-[320px] text-earth-100/80 font-semibold px-4 text-center">
      Jelenleg nincs megjeleníthető kiemelt szakember.
    </div>

    <div v-else class="flex justify-center items-center gap-2 md:gap-6 h-full relative w-full perspective-1000 min-h-[280px]">
      <MotionDiv
        v-for="(pro, index) in cards"
        :key="pro.id"
        class="absolute md:relative flex flex-col items-center bg-white rounded-xl shadow-2xl p-3 md:p-6 w-[180px] md:w-[240px] lg:w-[280px] h-[240px] md:h-[320px] lg:h-[380px] shrink-0 border border-earth-400/30"
        :style="{ order: getCardOrder(index) }"
        layout
        :initial="false"
        :animate="getCardStyle(index)"
        :transition="{ type: 'spring', stiffness: 200, damping: 25 }"
      >
        <div
          class="w-full h-12 md:h-20 lg:h-24 flex items-center justify-center rounded-md mb-2 md:mb-4 text-2xl md:text-4xl overflow-hidden shadow-inner shrink-0"
          :class="getCardStyle(index).scale === 1.1 ? 'bg-earth-100 text-earth-600' : 'bg-earth-800 text-earth-200'"
        >
          <img
            v-if="getImageUrl(pro.image)"
            :src="getImageUrl(pro.image) || ''"
            :alt="pro.name"
            class="w-full h-full object-cover"
          />
          <i v-else class="pi pi-image text-xl md:text-3xl"></i>
        </div>

        <h3 class="text-sm md:text-xl font-bold mb-1 text-earth-900 truncate w-full text-center">{{ pro.name }}</h3>

        <div class="mb-1 md:mb-2 text-xs md:text-lg w-full flex flex-col items-center">
          <div v-if="pro.ertekelesSzam > 0" class="flex items-center gap-1 text-yellow-500">
            <span class="text-earth-800 text-xs md:text-sm font-semibold mr-1">{{ pro.ertekeles.toFixed(1) }}</span>
            <svg v-for="i in Math.floor(pro.ertekeles)" :key="`full-${pro.id}-${i}`" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-3.5 h-3.5 md:w-4 md:h-4">
              <path fill-rule="evenodd" d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.007 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006z" clip-rule="evenodd" />
            </svg>
            <div v-if="hasHalfStar(pro.ertekeles)" class="relative w-3.5 h-3.5 md:w-4 md:h-4">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="absolute inset-0 w-3.5 h-3.5 md:w-4 md:h-4 text-earth-400/50">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
              </svg>
              <div class="absolute inset-0 overflow-hidden" style="width: 50%;">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-3.5 h-3.5 md:w-4 md:h-4 text-yellow-500">
                  <path fill-rule="evenodd" d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.007 5.404.433c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.433 2.082-5.006z" clip-rule="evenodd" />
                </svg>
              </div>
            </div>
            <svg v-for="i in (5 - Math.ceil(pro.ertekeles))" :key="`empty-${pro.id}-${i}`" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-3.5 h-3.5 md:w-4 md:h-4 text-earth-400/50">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
            </svg>
          </div>
          <div v-else class="text-earth-500 text-[10px] md:text-xs italic">
            Még nincs értékelés
          </div>
        </div>

        <p class="text-[10px] md:text-sm text-gray-600 text-left line-clamp-3 md:line-clamp-4 leading-snug md:leading-relaxed mb-auto w-full px-1 md:px-2">{{ pro.desc }}</p>

        <button
          class="mt-2 md:mt-4 w-full py-1.5 md:py-2 uppercase text-[10px] md:text-sm font-bold tracking-wide bg-gradient-to-r from-earth-500 to-earth-700 text-white rounded hover:from-earth-500 hover:to-earth-600 transition-all shadow-md"
          @click="goToPublicProfile(pro.id)"
        >
          Profil megtekintése
        </button>
      </MotionDiv>
    </div>
  </div>
</template>

<style scoped>
</style>

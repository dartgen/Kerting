<script setup lang="ts">
import { ref } from 'vue';

interface ImagePair {
  before: string;
  after: string;
}

defineProps<{
  imagePairs: ImagePair[];
}>();

// Egyetlen slider pozícióval egyszerre az összes pár összehasonlítható.
const sliderPosition = ref(50);

const handleSliderChange = (e: Event) => {
  sliderPosition.value = Number((e.target as HTMLInputElement).value);
};

// Normalizáljuk az URL-eket, hogy abszolút és relatív útvonal esetben is működjön.
const getImageUrl = (url: string) => {
  if (!url) return '';

  if (/^https?:\/\//i.test(url)) {
    // Már teljes URL, további átalakítást nem igényel.
    return url;
  }

  const normalized = String(url).replace(/\\/g, '/').trim();

  if (normalized.startsWith('/')) {
    return `https://localhost:7067${normalized}`;
  }

  // Fájlnév esetén a Work resource mappa alapútvonalat kapja.
  return `https://localhost:7067/resources/Work/${normalized}`;
};
</script>

<template>
  <div v-if="imagePairs.length > 0" class="space-y-6">
    <h4 class="text-lg font-semibold text-earth-100 flex items-center gap-2">
      <span class="text-green-500">◁▷</span> Előtte / Utána
    </h4>
    <div class="space-y-4">
      <div
        v-for="(pair, idx) in imagePairs"
        :key="idx"
        class="relative w-full rounded-lg overflow-hidden border border-earth-700 bg-black"
      >
        <!-- Háttérkép: az "utána" állapot -->
        <img
          v-lazy="getImageUrl(pair.after)"
          :alt="'After ' + idx"
          class="w-full h-32 sm:h-48 md:h-64 object-cover"
        />

        <!-- Előtérkép: az "előtte" állapot clip-el fedve -->
        <div
          class="absolute inset-0 h-32 sm:h-48 md:h-64 overflow-hidden"
          :style="{ width: sliderPosition + '%' }"
        >
          <img
            v-lazy="getImageUrl(pair.before)"
            :alt="'Before ' + idx"
            class="w-full h-32 sm:h-48 md:h-64 object-cover"
            :style="{ width: (100 / (sliderPosition || 1)) * 100 + '%' }"
          />
        </div>

        <!-- Teljes felületű csúszka input -->
        <input
          type="range"
          min="0"
          max="100"
          :value="sliderPosition"
          @input="handleSliderChange"
          class="absolute inset-0 w-full h-32 sm:h-48 md:h-64 opacity-0 cursor-col-resize z-40"
        />

        <!-- Vizuális elválasztó vonal és állapot címke -->
        <div
          class="absolute top-0 bottom-0 w-1 bg-yellow-500 z-30 pointer-events-none"
          :style="{ left: sliderPosition + '%' }"
        >
          <div class="absolute top-2 left-1/2 -translate-x-1/2 text-xs text-yellow-500 font-bold whitespace-nowrap bg-black/70 px-2 py-1 rounded">
            ◁ Előtte {{ Math.round(sliderPosition) }}% Utána ▷
          </div>
        </div>

        <!-- Sarkokban fix állapotcímkék -->
        <div class="absolute bottom-2 left-2 text-xs text-white font-semibold bg-black/70 px-2 py-1 rounded">
          Előtte
        </div>
        <div class="absolute bottom-2 right-2 text-xs text-white font-semibold bg-black/70 px-2 py-1 rounded">
          Utána
        </div>
      </div>
    </div>
  </div>
</template>

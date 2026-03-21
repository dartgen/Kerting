<script setup lang="ts">
import { useGardenStore } from '@/stores/gardenStore';
const store = useGardenStore();
</script>

<template>
  <div class="w-full mt-2 lg:mt-4 pb-1 px-4 sm:px-0 max-w-7xl mx-auto">
    <h2 class="font-serif text-xl md:text-2xl mb-2 text-earth-100 drop-shadow-md text-center md:text-left pl-2">Legfrissebb munkák</h2>

    <div class="w-full bg-gradient-to-b from-earth-50 to-earth-200/80 p-3 md:p-4 rounded-lg border border-earth-300 shadow-xl flex flex-col md:flex-row flex-wrap items-stretch justify-between gap-3 md:gap-4">

      <div
        v-for="(work, index) in store.works"
        :key="work.id"
        class="items-center flex-1 min-w-[200px] gap-2 transition hover:bg-earth-100/30 p-2 rounded"
        :class="[
          work.highlight ? 'ring-2 ring-earth-400 bg-earth-50 shadow-inner' : '',
          index >= 2 ? 'hidden md:flex' : 'flex'
        ]"
      >
        <div class="w-8 h-8 bg-earth-600 rounded-full text-white flex items-center justify-center shrink-0 shadow-md">
           <i class="pi pi-briefcase text-xs"></i>
        </div>

        <div class="flex flex-col justify-center">
          <h4 class="text-earth-900 font-bold text-sm leading-tight">{{ work.title }}</h4>
          <p class="text-earth-700 text-xs leading-tight mt-0.5 line-clamp-2">{{ work.desc }}</p>
        </div>
      </div>

      <div class="w-full md:w-auto flex items-center justify-center mt-2 md:mt-0">
         <button class="w-full md:w-auto bg-gradient-to-b from-yellow-600 to-yellow-800 hover:from-yellow-500 hover:to-yellow-700 border border-yellow-600 text-white px-4 py-2 font-bold rounded shadow-md text-xs whitespace-nowrap transition-all uppercase tracking-wide">
            További munkák
         </button>
      </div>

    </div>
  </div>
</template>

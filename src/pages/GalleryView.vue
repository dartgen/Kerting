<script setup lang="ts">
import { ref } from 'vue'
import { motion } from 'motion-v'

interface GalleryItem {
  id: number
  imageUrl: string
  description: string
  uploaderName: string
  uploadedAt: string
}

const MotionDiv = motion.div
const activeCardId = ref<number | null>(null)

const isCardActive = (id: number) => activeCardId.value === id
const toggleCard = (id: number) => {
  activeCardId.value = activeCardId.value === id ? null : id
}

const closeActiveCard = () => {
  activeCardId.value = null
}

const galleryItems: GalleryItem[] = [
  {
    id: 1,
    imageUrl: 'https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?auto=format&fit=crop&w=900&q=80',
    description: 'Reggeli harmat a levendulasor mellett.',
    uploaderName: 'Kiss Nóra',
    uploadedAt: '2026.03.02',
  },
  {
    id: 2,
    imageUrl: 'https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=700&q=80',
    description: 'Új virágágyás frissen mulcsozva.',
    uploaderName: 'Szabó Márk',
    uploadedAt: '2026.02.27',
  },
  {
    id: 3,
    imageUrl: 'https://images.unsplash.com/photo-1459156212016-c812468e2115?auto=format&fit=crop&w=1200&q=80',
    description: 'Kerti pihenő árnyékban, késő délután.',
    uploaderName: 'Tóth Emese',
    uploadedAt: '2026.02.25',
  },
  {
    id: 4,
    imageUrl: 'https://images.unsplash.com/photo-1598902108854-10e335adac99?auto=format&fit=crop&w=800&q=80',
    description: 'Paradicsomok az emelt ágyásban.',
    uploaderName: 'Farkas Levente',
    uploadedAt: '2026.02.21',
  },
  {
    id: 5,
    imageUrl: 'https://images.unsplash.com/photo-1515150144380-bca9f1650ed9?auto=format&fit=crop&w=1100&q=80',
    description: 'Kőburkolat és díszfű harmonikus összhangban.',
    uploaderName: 'Nagy Petra',
    uploadedAt: '2026.02.18',
  },
  {
    id: 6,
    imageUrl: 'https://images.unsplash.com/photo-1523348837708-15d4a09cfac2?auto=format&fit=crop&w=750&q=80',
    description: 'Sziklakert tavasszal, friss zöld színekkel.',
    uploaderName: 'Varga Ádám',
    uploadedAt: '2026.02.14',
  },
  {
    id: 7,
    imageUrl: 'https://images.unsplash.com/photo-1490750967868-88aa4486c946?auto=format&fit=crop&w=1000&q=80',
    description: 'Vadrizsa és virágok természetes kompozícióban.',
    uploaderName: 'Molnár Hanna',
    uploadedAt: '2026.02.10',
  },
  {
    id: 8,
    imageUrl: 'https://images.unsplash.com/photo-1534710961216-75c88202f43e?auto=format&fit=crop&w=850&q=80',
    description: 'Kora esti fények a gyümölcsfák között.',
    uploaderName: 'Bíró Dániel',
    uploadedAt: '2026.02.06',
  },
  {
    id: 9,
    imageUrl: 'https://images.unsplash.com/photo-1438109382753-8368e7e1e7cf?auto=format&fit=crop&w=650&q=80',
    description: 'Friss füves sávok kerti nyomvonalakkal.',
    uploaderName: 'Lakatos Anna',
    uploadedAt: '2026.02.02',
  },
  {
    id: 10,
    imageUrl: 'https://images.unsplash.com/photo-1473448912268-2022ce9509d8?auto=format&fit=crop&w=980&q=80',
    description: 'Árnyéktűrő növények egy modern terasz mellett.',
    uploaderName: 'Horváth Gergő',
    uploadedAt: '2026.01.29',
  },
]
</script>

<template>
  <div class="w-full h-full min-h-0 py-2 sm:py-3" @click.self="closeActiveCard">
    <MotionDiv
      :initial="{ opacity: 0, y: 14 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.45, ease: 'easeOut' }"
      class="w-full h-full min-h-0 bg-earth-900/70 backdrop-blur-lg border border-earth-100/20 rounded-2xl sm:rounded-3xl px-3 py-4 sm:px-5 sm:py-6 lg:px-6 shadow-[0_20px_50px_rgba(0,0,0,0.35)] overflow-y-auto overscroll-contain"
      @click="closeActiveCard"
    >
      <div class="mb-5 sm:mb-6 pb-3 sm:pb-4 border-b border-earth-100/15">
        <h1 class="text-2xl sm:text-3xl md:text-4xl font-serif text-earth-50">Galéria</h1>
        <p class="mt-2 text-sm sm:text-base text-earth-200/90">
          Inspirálódj más kertekből és munkafolyamatokból.
        </p>
        <p class="mt-1 text-[11px] sm:text-xs text-earth-200/70">
          Mobilon: érintsd meg a képet a részletek megjelenítéséhez.
        </p>
      </div>

      <div class="columns-1 sm:columns-2 lg:columns-3 xl:columns-4 [column-gap:0.55rem] sm:[column-gap:0.7rem] lg:[column-gap:0.8rem]">
        <MotionDiv
          v-for="(item, index) in galleryItems"
          :key="item.id"
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.26, delay: index * 0.03, ease: 'easeOut' }"
          :whileHover="{ y: -2 }"
          class="group relative mb-2.5 sm:mb-3 break-inside-avoid overflow-hidden rounded-xl border border-earth-100/15 bg-earth-950/35 shadow-[0_12px_24px_rgba(0,0,0,0.28)]"
          tabindex="0"
          role="button"
          :aria-label="`Kép részletei: ${item.uploaderName}`"
          :aria-pressed="activeCardId === item.id"
          @click.stop="toggleCard(item.id)"
          @keyup.enter.stop="toggleCard(item.id)"
          @keyup.space.prevent.stop="toggleCard(item.id)"
        >
          <img
            :src="item.imageUrl"
            :alt="item.description"
            class="block w-full h-auto object-cover transition-transform duration-300 group-hover:scale-[1.02]"
            loading="lazy"
          />

          <div
            class="pointer-events-none absolute inset-0 transition-all duration-250 bg-black/0 backdrop-blur-0 group-hover:bg-black/28 group-hover:backdrop-blur-[1.5px] group-focus:bg-black/28 group-focus:backdrop-blur-[1.5px] group-active:bg-black/28 group-active:backdrop-blur-[1.5px]"
            :class="isCardActive(item.id) ? 'bg-black/28 backdrop-blur-[1.5px]' : ''"
          >
            <div
              class="absolute inset-0 flex items-end p-3 sm:p-4 opacity-0 group-hover:opacity-100 group-focus:opacity-100 group-active:opacity-100 transition-opacity duration-250"
              :class="isCardActive(item.id) ? 'opacity-100' : ''"
            >
              <div class="w-full flex items-end justify-between gap-3">
                <div>
                  <p class="text-earth-50 text-xs sm:text-sm md:text-[0.92rem] leading-snug drop-shadow-lg">
                    {{ item.description }}
                  </p>
                  <p class="mt-1 text-earth-200/90 text-[11px] sm:text-xs md:text-[0.82rem] font-semibold">
                    {{ item.uploaderName }}
                  </p>
                </div>
                <p class="shrink-0 text-earth-100/85 text-[10px] sm:text-xs md:text-[0.78rem] self-end">
                  {{ item.uploadedAt }}
                </p>
              </div>
            </div>
          </div>
        </MotionDiv>
      </div>
    </MotionDiv>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { motion, AnimatePresence } from 'motion-v'

interface GalleryItem {
  id: number
  imageUrl: string
  description: string
  uploaderName: string
  uploaderAvatarUrl: string
  uploadedAt: string
  comments: {
    id: number
    userName: string
    avatarUrl: string
    message: string
    createdAt: string
  }[]
}

const MotionDiv = motion.div
const previewCardId = ref<number | null>(null)
const expandedCardId = ref<number | null>(null)

const expandedCard = computed(() => galleryItems.find((item) => item.id === expandedCardId.value) ?? null)

const isCardPreviewed = (id: number) => previewCardId.value === id

const handleCardClick = (id: number) => {
  if (expandedCardId.value) return

  if (previewCardId.value === id) {
    expandedCardId.value = id
    return
  }

  previewCardId.value = id
}

const closeExpandedCard = () => {
  expandedCardId.value = null
  previewCardId.value = null
}

const galleryItems: GalleryItem[] = [
  {
    id: 1,
    imageUrl: 'https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?auto=format&fit=crop&w=900&q=80',
    description: 'Reggeli harmat a levendulasor mellett.',
    uploaderName: 'Kiss Nóra',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=32',
    uploadedAt: '2026.03.02',
    comments: [
      { id: 101, userName: 'Papp Lili', avatarUrl: 'https://i.pravatar.cc/72?img=47', message: 'Nagyon hangulatos a színvilág!', createdAt: '2026.03.03' },
      { id: 102, userName: 'Sánta Áron', avatarUrl: 'https://i.pravatar.cc/72?img=12', message: 'A levendulaágyás tényleg csodás lett.', createdAt: '2026.03.04' },
    ],
  },
  {
    id: 2,
    imageUrl: 'https://images.unsplash.com/photo-1501004318641-b39e6451bec6?auto=format&fit=crop&w=700&q=80',
    description: 'Új virágágyás frissen mulcsozva.',
    uploaderName: 'Szabó Márk',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=14',
    uploadedAt: '2026.02.27',
    comments: [
      { id: 201, userName: 'Németh Dóra', avatarUrl: 'https://i.pravatar.cc/72?img=5', message: 'Szuper rendezett munka, gratulálok!', createdAt: '2026.02.28' },
      { id: 202, userName: 'Gulyás Máté', avatarUrl: 'https://i.pravatar.cc/72?img=23', message: 'A mulcs textúrája nagyon jól mutat.', createdAt: '2026.03.01' },
    ],
  },
  {
    id: 3,
    imageUrl: 'https://images.unsplash.com/photo-1459156212016-c812468e2115?auto=format&fit=crop&w=1200&q=80',
    description: 'Kerti pihenő árnyékban, késő délután.',
    uploaderName: 'Tóth Emese',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=45',
    uploadedAt: '2026.02.25',
    comments: [
      { id: 301, userName: 'Károlyi Zsófi', avatarUrl: 'https://i.pravatar.cc/72?img=49', message: 'Ide tényleg ki lehet ülni órákra.', createdAt: '2026.02.25' },
      { id: 302, userName: 'Kovács Levente', avatarUrl: 'https://i.pravatar.cc/72?img=18', message: 'A fények nagyon szépen dolgoznak.', createdAt: '2026.02.26' },
    ],
  },
  {
    id: 4,
    imageUrl: 'https://images.unsplash.com/photo-1598902108854-10e335adac99?auto=format&fit=crop&w=800&q=80',
    description: 'Paradicsomok az emelt ágyásban.',
    uploaderName: 'Farkas Levente',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=13',
    uploadedAt: '2026.02.21',
    comments: [
      { id: 401, userName: 'Balla Nóri', avatarUrl: 'https://i.pravatar.cc/72?img=40', message: 'Ez már profi konyhakert szint.', createdAt: '2026.02.22' },
      { id: 402, userName: 'Bodnár Péter', avatarUrl: 'https://i.pravatar.cc/72?img=29', message: 'Nagyon tetszik az emelt ágyás aránya.', createdAt: '2026.02.22' },
    ],
  },
  {
    id: 5,
    imageUrl: 'https://images.unsplash.com/photo-1515150144380-bca9f1650ed9?auto=format&fit=crop&w=1100&q=80',
    description: 'Kőburkolat és díszfű harmonikus összhangban.',
    uploaderName: 'Nagy Petra',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=9',
    uploadedAt: '2026.02.18',
    comments: [
      { id: 501, userName: 'Fodor Luca', avatarUrl: 'https://i.pravatar.cc/72?img=28', message: 'Modern, mégis nagyon barátságos.', createdAt: '2026.02.18' },
      { id: 502, userName: 'Sipos Bence', avatarUrl: 'https://i.pravatar.cc/72?img=11', message: 'A burkolat és a zöldek együtt tökéletesek.', createdAt: '2026.02.19' },
    ],
  },
  {
    id: 6,
    imageUrl: 'https://images.unsplash.com/photo-1523348837708-15d4a09cfac2?auto=format&fit=crop&w=750&q=80',
    description: 'Sziklakert tavasszal, friss zöld színekkel.',
    uploaderName: 'Varga Ádám',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=15',
    uploadedAt: '2026.02.14',
    comments: [
      { id: 601, userName: 'Mészáros Anna', avatarUrl: 'https://i.pravatar.cc/72?img=20', message: 'Gyönyörű rétegzés, nagyon tetszik!', createdAt: '2026.02.15' },
      { id: 602, userName: 'Kis Roland', avatarUrl: 'https://i.pravatar.cc/72?img=36', message: 'A sziklakert textúrája nagyon ütős.', createdAt: '2026.02.16' },
    ],
  },
  {
    id: 7,
    imageUrl: 'https://images.unsplash.com/photo-1490750967868-88aa4486c946?auto=format&fit=crop&w=1000&q=80',
    description: 'Vadrizsa és virágok természetes kompozícióban.',
    uploaderName: 'Molnár Hanna',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=52',
    uploadedAt: '2026.02.10',
    comments: [
      { id: 701, userName: 'Gerencsér Panni', avatarUrl: 'https://i.pravatar.cc/72?img=34', message: 'Annyira természetes és élő a kép.', createdAt: '2026.02.11' },
      { id: 702, userName: 'Oláh Marci', avatarUrl: 'https://i.pravatar.cc/72?img=10', message: 'A színátmenetek nagyon szépek.', createdAt: '2026.02.12' },
    ],
  },
  {
    id: 8,
    imageUrl: 'https://images.unsplash.com/photo-1534710961216-75c88202f43e?auto=format&fit=crop&w=850&q=80',
    description: 'Kora esti fények a gyümölcsfák között.',
    uploaderName: 'Bíró Dániel',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=24',
    uploadedAt: '2026.02.06',
    comments: [
      { id: 801, userName: 'Tari Kitti', avatarUrl: 'https://i.pravatar.cc/72?img=56', message: 'Mesebeli ez a fény!', createdAt: '2026.02.07' },
      { id: 802, userName: 'Balázs Noé', avatarUrl: 'https://i.pravatar.cc/72?img=26', message: 'A gyümölcsfák között nagyon jó a perspektíva.', createdAt: '2026.02.08' },
    ],
  },
  {
    id: 9,
    imageUrl: 'https://images.unsplash.com/photo-1438109382753-8368e7e1e7cf?auto=format&fit=crop&w=650&q=80',
    description: 'Friss füves sávok kerti nyomvonalakkal.',
    uploaderName: 'Lakatos Anna',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=37',
    uploadedAt: '2026.02.02',
    comments: [
      { id: 901, userName: 'Berkes Misi', avatarUrl: 'https://i.pravatar.cc/72?img=7', message: 'Nagyon rendezett és tiszta összkép.', createdAt: '2026.02.03' },
      { id: 902, userName: 'Végh Orsi', avatarUrl: 'https://i.pravatar.cc/72?img=35', message: 'A nyomvonalak ritmusa nagyon jó.', createdAt: '2026.02.04' },
    ],
  },
  {
    id: 10,
    imageUrl: 'https://images.unsplash.com/photo-1473448912268-2022ce9509d8?auto=format&fit=crop&w=980&q=80',
    description: 'Árnyéktűrő növények egy modern terasz mellett.',
    uploaderName: 'Horváth Gergő',
    uploaderAvatarUrl: 'https://i.pravatar.cc/96?img=16',
    uploadedAt: '2026.01.29',
    comments: [
      { id: 1001, userName: 'Cseri Dalma', avatarUrl: 'https://i.pravatar.cc/72?img=41', message: 'Nagyon elegáns lett ez a rész.', createdAt: '2026.01.30' },
      { id: 1002, userName: 'Péterfy Ákos', avatarUrl: 'https://i.pravatar.cc/72?img=19', message: 'A terasz mellett tökéletes növényválasztás.', createdAt: '2026.01.31' },
    ],
  },
]
</script>

<template>
  <div class="w-full h-full min-h-0 py-2 sm:py-3">
    <MotionDiv
      :initial="{ opacity: 0, y: 14 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.45, ease: 'easeOut' }"
      class="w-full h-full min-h-0 bg-earth-900/70 backdrop-blur-lg border border-earth-100/20 rounded-2xl sm:rounded-3xl px-3 py-4 sm:px-5 sm:py-6 lg:px-6 shadow-[0_20px_50px_rgba(0,0,0,0.35)] overscroll-contain"
      :class="expandedCard ? 'overflow-hidden' : 'overflow-y-auto'"
    >
      <div v-if="!expandedCard" class="mb-5 sm:mb-6 pb-3 sm:pb-4 border-b border-earth-100/15">
        <h1 class="text-2xl sm:text-3xl md:text-4xl font-serif text-earth-50">Galéria</h1>
        <p class="mt-2 text-sm sm:text-base text-earth-200/90">
          Inspirálódj más kertekből és munkafolyamatokból.
        </p>
        <p class="mt-1 text-[11px] sm:text-xs text-earth-200/70">
          Első érintés: részletek. Második ugyanazon képen: nagy nézet.
        </p>
      </div>

      <AnimatePresence>
        <MotionDiv
          v-if="expandedCard"
          key="expanded-view"
          :initial="{ opacity: 0, y: 14 }"
          :animate="{ opacity: 1, y: 0 }"
          :exit="{ opacity: 0, y: 10 }"
          :transition="{ duration: 0.28, ease: 'easeOut' }"
          class="relative h-full min-h-[420px] lg:min-h-[560px]"
        >
          <button
            type="button"
            class="absolute right-2 top-2 sm:right-3 sm:top-3 z-20 w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-earth-900/85 border border-earth-100/20 text-earth-50 hover:bg-earth-800 transition-colors"
            aria-label="Nagy nézet bezárása"
            @click="closeExpandedCard"
          >
            ✕
          </button>

          <div class="grid grid-cols-1 lg:grid-cols-[minmax(0,1.4fr)_minmax(280px,0.75fr)] gap-3 sm:gap-4 h-full min-h-[420px] lg:min-h-[560px]">
            <MotionDiv
              :initial="{ opacity: 0.6, scale: 0.985 }"
              :animate="{ opacity: 1, scale: 1 }"
              :transition="{ duration: 0.25, ease: 'easeOut' }"
              class="relative rounded-2xl overflow-hidden border border-earth-100/20 bg-earth-950/50"
            >
              <img
                :src="expandedCard.imageUrl"
                :alt="expandedCard.description"
                class="w-full h-full min-h-[280px] lg:min-h-full object-cover"
              />
              <div class="absolute bottom-3 right-3 px-2 py-1 rounded-md bg-black/45 text-earth-100 text-xs border border-earth-100/15">
                {{ expandedCard.uploadedAt }}
              </div>
            </MotionDiv>

            <MotionDiv
              :initial="{ opacity: 0, x: 16 }"
              :animate="{ opacity: 1, x: 0 }"
              :transition="{ duration: 0.3, ease: 'easeOut' }"
              class="rounded-2xl border border-earth-100/20 bg-earth-950/45 p-4 sm:p-5 overflow-hidden flex flex-col min-h-0"
            >
              <div class="flex items-center gap-3 pb-3 border-b border-earth-100/10">
                <img
                  :src="expandedCard.uploaderAvatarUrl"
                  :alt="`${expandedCard.uploaderName} profilképe`"
                  class="w-12 h-12 sm:w-14 sm:h-14 rounded-full object-cover border border-earth-100/25"
                />
                <div>
                  <p class="text-earth-100 font-semibold text-sm sm:text-base">{{ expandedCard.uploaderName }}</p>
                  <p class="text-earth-200/70 text-xs">Feltöltő</p>
                </div>
              </div>

              <div class="pt-4">
                <h2 class="text-earth-50 font-semibold text-sm sm:text-base">Leírás</h2>
                <p class="mt-2 text-earth-200/90 text-sm leading-relaxed">{{ expandedCard.description }}</p>
              </div>

              <div class="pt-5 flex-1 min-h-0">
                <h3 class="text-earth-50 font-semibold text-sm sm:text-base">Kommentek</h3>
                <div class="mt-3 space-y-3 overflow-y-auto max-h-[260px] sm:max-h-[320px] lg:max-h-[420px] pr-1">
                  <div
                    v-for="comment in expandedCard.comments"
                    :key="comment.id"
                    class="rounded-xl border border-earth-100/10 bg-earth-900/55 p-3"
                  >
                    <div class="flex items-center justify-between gap-2">
                      <div class="flex items-center gap-2">
                        <img
                          :src="comment.avatarUrl"
                          :alt="`${comment.userName} profilképe`"
                          class="w-8 h-8 rounded-full object-cover border border-earth-100/20"
                        />
                        <span class="text-earth-100 text-xs sm:text-sm font-medium">{{ comment.userName }}</span>
                      </div>
                      <span class="text-earth-200/70 text-[11px]">{{ comment.createdAt }}</span>
                    </div>
                    <p class="mt-2 text-earth-200/95 text-xs sm:text-sm leading-relaxed">{{ comment.message }}</p>
                  </div>
                </div>
              </div>
            </MotionDiv>
          </div>
        </MotionDiv>

        <MotionDiv
          v-else
          key="grid-view"
          :initial="{ opacity: 0 }"
          :animate="{ opacity: 1 }"
          :exit="{ opacity: 0 }"
          :transition="{ duration: 0.2, ease: 'easeOut' }"
          class="columns-1 sm:columns-2 lg:columns-3 xl:columns-4 [column-gap:0.55rem] sm:[column-gap:0.7rem] lg:[column-gap:0.8rem]"
        >
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
            :aria-label="`Kép megnyitása: ${item.uploaderName}`"
            :aria-pressed="previewCardId === item.id"
            @click.stop="handleCardClick(item.id)"
            @keyup.enter.stop="handleCardClick(item.id)"
            @keyup.space.prevent.stop="handleCardClick(item.id)"
          >
            <img
              :src="item.imageUrl"
              :alt="item.description"
              class="block w-full h-auto object-cover transition-transform duration-300 group-hover:scale-[1.02]"
              loading="lazy"
            />

            <div
              class="pointer-events-none absolute inset-0 transition-all duration-250 bg-black/0 backdrop-blur-0 group-hover:bg-black/28 group-hover:backdrop-blur-[1.5px] group-focus:bg-black/28 group-focus:backdrop-blur-[1.5px]"
              :class="isCardPreviewed(item.id) ? 'bg-black/28 backdrop-blur-[1.5px]' : ''"
            >
              <div
                class="absolute inset-0 flex items-end p-3 sm:p-4 opacity-0 group-hover:opacity-100 group-focus:opacity-100 transition-opacity duration-250"
                :class="isCardPreviewed(item.id) ? 'opacity-100' : ''"
              >
                <div class="w-full flex items-end justify-between gap-3">
                  <div>
                    <p class="text-earth-50 text-xs sm:text-sm md:text-[0.92rem] leading-snug drop-shadow-lg">
                      {{ item.description }}
                    </p>
                    <p class="mt-1 text-earth-200/90 text-[11px] sm:text-xs md:text-[0.82rem] font-semibold">
                      {{ item.uploaderName }}
                    </p>
                    <p class="mt-1 text-earth-200/75 text-[10px] sm:text-[11px]">
                      Koppints még egyszer a nagy nézethez
                    </p>
                  </div>
                  <p class="shrink-0 text-earth-100/85 text-[10px] sm:text-xs md:text-[0.78rem] self-end">
                    {{ item.uploadedAt }}
                  </p>
                </div>
              </div>
            </div>
          </MotionDiv>
        </MotionDiv>
      </AnimatePresence>
    </MotionDiv>
  </div>
</template>

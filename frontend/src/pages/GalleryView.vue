<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import api from '@/services/axios'
import { useAuthStore } from '@/stores/authStore'

interface GalleryItem {
  id: number
  imageUrl: string
  title: string
  description: string
  uploaderName: string
  uploaderAvatarUrl: string
  uploadedAt: string
  likesCount: number
  dislikesCount: number
  myReaction: boolean | null
  comments: {
    id: number
    userName: string
    avatarUrl: string
    message: string
    createdAt: string
  }[]
}

interface GalleryFeedItemDto {
  id: number
  imageUrl: string
  title?: string
  description?: string
  uploaderName: string
  profileImageUrl?: string | null
  createdAtUtc: string
  likesCount?: number
  dislikesCount?: number
}

interface GalleryDetailCommentDto {
  id: number
  userName: string
  message: string
  createdAtUtc: string
  profileImageUrl?: string | null
}

interface GalleryDetailDto {
  id: number
  profileImageUrl?: string | null
  likesCount?: number
  dislikesCount?: number
  myReaction?: boolean | null
  comments?: GalleryDetailCommentDto[]
}

const galleryItems = ref<GalleryItem[]>([])
const commentDraft = ref('')
const isSubmittingComment = ref(false)
const commentSubmitError = ref('')

const MotionDiv = motion.div
const authStore = useAuthStore()
// A kártyák interakciós állapota: előnézet (touch) és nagyított nézet.
const previewCardId = ref<number | null>(null)
const expandedCardId = ref<number | null>(null)
const isDesktopInteraction = ref(false)

// Az aktuálisan nagyított elem teljes adatait adja vissza.
const expandedCard = computed(() => galleryItems.value.find((item) => item.id === expandedCardId.value) ?? null)

const isCardPreviewed = (id: number) => previewCardId.value === id

const openExpandedCard = async (id: number) => {
  commentSubmitError.value = ''
  try {
    const { data } = await api.get<GalleryDetailDto>(`/Gallery/${id}`)
    const itemIndex = galleryItems.value.findIndex(i => i.id === id)
    if (itemIndex !== -1) {
      const existingItem = galleryItems.value[itemIndex]
      if (existingItem) {
        galleryItems.value[itemIndex] = {
          id: existingItem.id,
          imageUrl: existingItem.imageUrl,
          title: existingItem.title,
          description: existingItem.description,
          uploaderName: existingItem.uploaderName,
          uploaderAvatarUrl: data.profileImageUrl
            ? getFullImageUrl(data.profileImageUrl)
            : existingItem.uploaderAvatarUrl,
          uploadedAt: existingItem.uploadedAt,
          likesCount: data.likesCount ?? existingItem.likesCount,
          dislikesCount: data.dislikesCount ?? existingItem.dislikesCount,
          myReaction: data.myReaction ?? null,
          comments: data.comments?.map((c) => ({
            id: c.id,
            userName: c.userName,
            avatarUrl: c.profileImageUrl
              ? getFullImageUrl(c.profileImageUrl)
              : `https://i.pravatar.cc/72?u=${c.id}`,
            message: c.message,
            createdAt: new Date(c.createdAtUtc).toLocaleDateString()
          })) || []
        }
      }
    }
  } catch (err) {
    console.error("Hiba a részletek lekérésekor", err)
  }
  expandedCardId.value = id
}

const handleCardClick = async (id: number) => {
  // Ha már nyitva van nagy nézet, új kattintást nem kezelünk.
  if (expandedCardId.value) return

  // Desktopon azonnal nyitunk; érintőn először előnézet, második érintésre nagy nézet.
  if (isDesktopInteraction.value) {
    await openExpandedCard(id)
    return
  }

  if (previewCardId.value === id) {
    await openExpandedCard(id)
    return
  }

  previewCardId.value = id
}

const closeExpandedCard = () => {
  // Bezáráskor minden kiválasztási állapotot nullázunk.
  expandedCardId.value = null
  previewCardId.value = null
  commentDraft.value = ''
  commentSubmitError.value = ''
}

const submitComment = async () => {
  if (!expandedCardId.value) return

  const trimmedMessage = commentDraft.value.trim()
  if (!trimmedMessage || isSubmittingComment.value) return

  isSubmittingComment.value = true
  commentSubmitError.value = ''

  try {
    await api.post(`/Gallery/${expandedCardId.value}/comment`, trimmedMessage)
    commentDraft.value = ''
    await openExpandedCard(expandedCardId.value)
  } catch (err) {
    console.error('Hiba komment küldésekor', err)
    commentSubmitError.value = 'A komment küldése nem sikerült. Próbáld újra.'
  } finally {
    isSubmittingComment.value = false
  }
}

const toggleReaction = async (isLike: boolean) => {
  if (!expandedCardId.value || !authStore.isAuthenticated) return

  try {
    await api.post(`/Gallery/${expandedCardId.value}/react`, null, { params: { isLike } })
    await openExpandedCard(expandedCardId.value)
  } catch (err) {
    console.error('Hiba reakció küldésekor', err)
  }
}

const desktopQuery = '(hover: hover) and (pointer: fine)'
let mediaQuery: MediaQueryList | null = null

const syncInteractionMode = () => {
  // A media query alapján eldöntjük, hogy egér/alapú (desktop) interakció legyen-e.
  isDesktopInteraction.value = Boolean(mediaQuery?.matches)
}

const getFullImageUrl = (url: string) => {
  if (!url) return ''
  if (url.startsWith('http')) return url
  const baseUrl = api.defaults.baseURL?.replace('/api', '') || 'http://localhost:44351'
  return `${baseUrl}${url.startsWith('/') ? '' : '/'}${url}`
}

const fetchFeed = async () => {
  try {
    const { data } = await api.get<GalleryFeedItemDto[]>('/Gallery/feed')
    galleryItems.value = data.map((item) => ({
      id: item.id,
      imageUrl: getFullImageUrl(item.imageUrl),
      title: item.title || '',
      description: item.description || '',
      uploaderName: item.uploaderName,
      uploaderAvatarUrl: item.profileImageUrl
        ? getFullImageUrl(item.profileImageUrl)
        : `https://i.pravatar.cc/96?u=${item.id}`,
      uploadedAt: new Date(item.createdAtUtc).toLocaleDateString(),
      likesCount: item.likesCount ?? 0,
      dislikesCount: item.dislikesCount ?? 0,
      myReaction: null,
      comments: []
    }))
  } catch (err) {
    console.error("Hiba a galéria betöltésekor", err)
  }
}

onMounted(() => {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') return
  mediaQuery = window.matchMedia(desktopQuery)
  syncInteractionMode()
  mediaQuery.addEventListener('change', syncInteractionMode)

  fetchFeed()
})

onUnmounted(() => {
  mediaQuery?.removeEventListener('change', syncInteractionMode)
})

watch(isDesktopInteraction, (isDesktop) => {
  // Desktop váltásnál töröljük az érintős előnézeti állapotot.
  if (isDesktop) previewCardId.value = null
})
</script>

<template>
  <div class="w-full h-full min-h-0 py-2 sm:py-3">
    <!-- Fő galéria konténer animációval; nagy nézetben fix, egyébként görgethető. -->
    <MotionDiv
      :initial="{ opacity: 0, y: 14 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.45, ease: 'easeOut' }"
      class="w-full h-full min-h-0 bg-earth-900/70 backdrop-blur-lg border border-earth-100/20 rounded-2xl sm:rounded-3xl px-3 py-4 sm:px-5 sm:py-6 lg:px-6 shadow-[0_20px_50px_rgba(0,0,0,0.35)] overscroll-contain"
      :class="expandedCard ? 'overflow-y-auto lg:overflow-hidden' : 'overflow-y-auto'"
    >
      <div v-if="!expandedCard" class="mb-5 sm:mb-6 pb-3 sm:pb-4 border-b border-earth-100/15 text-center">
        <h1 class="text-2xl sm:text-3xl md:text-4xl font-serif text-earth-50">Galéria</h1>
        <p class="mt-2 text-sm sm:text-base text-earth-200/90">
          Inspirálódj más kertekből és munkafolyamatokból.
        </p>
      </div>

      <AnimatePresence>
        <!-- Kiválasztott kép részletes, nagyított nézete. -->
        <MotionDiv
          v-if="expandedCard"
          key="expanded-view"
          :initial="{ opacity: 0, y: 14 }"
          :animate="{ opacity: 1, y: 0 }"
          :exit="{ opacity: 0, y: 10 }"
          :transition="{ duration: 0.28, ease: 'easeOut' }"
          class="relative min-h-[420px] lg:h-full lg:min-h-[560px]"
        >
          <div class="grid grid-cols-1 lg:grid-cols-[minmax(0,1.4fr)_minmax(280px,0.75fr)] gap-3 sm:gap-4 min-h-[420px] lg:h-full lg:min-h-[560px]">
            <MotionDiv
              :initial="{ opacity: 0.6, scale: 0.985 }"
              :animate="{ opacity: 1, scale: 1 }"
              :transition="{ duration: 0.25, ease: 'easeOut' }"
              class="relative flex items-center justify-center rounded-2xl overflow-hidden border border-earth-100/20 bg-earth-950/50"
            >
              <img
                :src="expandedCard.imageUrl"
                :alt="expandedCard.description"
                class="w-full h-full min-h-[280px] lg:min-h-full object-contain gallery-protected-image"
                draggable="false"
                @dragstart.prevent
                @contextmenu.prevent
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
              <div class="flex items-center justify-between gap-3 pb-3 border-b border-earth-100/10">
                <div class="flex items-center gap-3">
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

                <div class="flex items-center gap-2 shrink-0">
                  <button
                    type="button"
                    class="h-9 sm:h-10 rounded-full border px-3 sm:px-3.5 text-xs sm:text-sm font-semibold transition-colors inline-flex items-center gap-1.5"
                    :class="expandedCard.myReaction === true
                      ? 'border-earth-100/40 bg-earth-100/20 text-white'
                      : 'border-earth-100/25 bg-earth-900/65 text-white hover:bg-earth-800'"
                    :disabled="!authStore.isAuthenticated"
                    @click="toggleReaction(true)"
                  >
                    <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                      <rect x="3" y="9" width="4" height="12" rx="1.25" />
                      <path d="M14.5 9.5V5.9c0-1-.52-1.92-1.37-2.41l-3.4 5.92a2.2 2.2 0 0 0-.3 1.1V19a2 2 0 0 0 2 2h5.84a2 2 0 0 0 1.97-1.65l1.02-6a2 2 0 0 0-1.97-2.35H14.5Z" />
                    </svg>
                    <span>{{ expandedCard.likesCount }}</span>
                  </button>

                  <button
                    type="button"
                    class="h-9 sm:h-10 rounded-full border px-3 sm:px-3.5 text-xs sm:text-sm font-semibold transition-colors inline-flex items-center gap-1.5"
                    :class="expandedCard.myReaction === false
                      ? 'border-earth-100/40 bg-earth-100/20 text-white'
                      : 'border-earth-100/25 bg-earth-900/65 text-white hover:bg-earth-800'"
                    :disabled="!authStore.isAuthenticated"
                    @click="toggleReaction(false)"
                  >
                    <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                      <rect x="3" y="3" width="4" height="12" rx="1.25" />
                      <path d="M14.5 14.5v3.6c0 1-.52 1.92-1.37 2.41l-3.4-5.92a2.2 2.2 0 0 1-.3-1.1V5a2 2 0 0 1 2-2h5.84a2 2 0 0 1 1.97 1.65l1.02 6a2 2 0 0 1-1.97 2.35H14.5Z" />
                    </svg>
                    <span>{{ expandedCard.dislikesCount }}</span>
                  </button>

                  <button
                    type="button"
                    class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-earth-900/85 border border-earth-100/20 text-earth-50 hover:bg-earth-800 transition-colors"
                    aria-label="Nagy nézet bezárása"
                    @click="closeExpandedCard"
                  >
                    ✕
                  </button>
                </div>
              </div>

              <div class="pt-4">
                <h2 class="text-earth-50 font-semibold text-base sm:text-lg">{{ expandedCard.title }}</h2>
              </div>

              <div class="pt-3">
                <p class="mt-2 text-earth-200/90 text-sm leading-relaxed">{{ expandedCard.description }}</p>
              </div>

              <div class="pt-4 flex-1 min-h-0 flex flex-col">
                <div class="h-px w-full bg-gradient-to-r from-transparent via-earth-100/20 to-transparent mb-5"></div>
                <h3 class="text-earth-50 font-semibold text-sm sm:text-base">Kommentek</h3>
                <div class="mt-3 min-h-0 flex-1 overflow-y-auto pr-1 space-y-3">
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

                <div class="mt-3 shrink-0 sticky bottom-0 z-10 rounded-2xl border border-earth-100/15 bg-earth-950/90 backdrop-blur-sm px-3 py-3 shadow-[0_12px_24px_rgba(0,0,0,0.25)]">
                  <div class="flex items-end gap-2 sm:gap-3">
                    <textarea
                      v-model="commentDraft"
                      rows="2"
                      maxlength="1000"
                      placeholder="Írj egy kommentet..."
                      class="w-full resize-none rounded-xl border border-earth-100/20 bg-earth-900/60 px-3 py-2 text-sm text-earth-100 placeholder:text-earth-200/55 focus:outline-none focus:ring-2 focus:ring-earth-300/35 focus:border-earth-100/35"
                      :disabled="isSubmittingComment"
                      @keydown.enter.exact.prevent="submitComment"
                    />

                    <button
                      type="button"
                      class="h-[42px] sm:h-[44px] shrink-0 rounded-full border border-earth-100/25 bg-earth-700/85 px-5 text-xs sm:text-sm font-semibold text-earth-50 transition-colors hover:bg-earth-600 disabled:cursor-not-allowed disabled:opacity-60"
                      :disabled="isSubmittingComment || !commentDraft.trim()"
                      @click="submitComment"
                    >
                      {{ isSubmittingComment ? 'Küldés...' : 'Küldés' }}
                    </button>
                  </div>

                  <p v-if="commentSubmitError" class="mt-2 text-[11px] sm:text-xs text-red-300">
                    {{ commentSubmitError }}
                  </p>
                  <p v-else class="mt-2 text-[11px] sm:text-xs text-earth-200/70">
                    Enter: küldés, Shift+Enter: új sor
                  </p>
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
          <!-- Alapértelmezett mozaiknézet: képkártyák előnézettel és megnyitási interakcióval. -->
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
              class="block w-full h-auto object-cover transition-transform duration-300 group-hover:scale-[1.02] gallery-protected-image"
              loading="lazy"
              draggable="false"
              @dragstart.prevent
              @contextmenu.prevent
            />

            <div
              class="pointer-events-none absolute inset-0 transition-all duration-250 bg-black/0 backdrop-blur-0 group-hover:bg-black/28 group-hover:backdrop-blur-[1.5px] group-focus:bg-black/28 group-focus:backdrop-blur-[1.5px]"
              :class="isCardPreviewed(item.id) ? 'bg-black/28 backdrop-blur-[1.5px]' : ''"
            >
              <div
                class="absolute inset-0 flex items-center justify-center px-4 text-center opacity-0 group-hover:opacity-100 group-focus:opacity-100 transition-opacity duration-250"
                :class="isCardPreviewed(item.id) ? 'opacity-100' : ''"
              >
                <span class="rounded-full border border-earth-100/30 bg-black/45 px-3 py-1.5 text-[11px] sm:text-xs font-semibold tracking-wide text-earth-50 shadow-md">
                  <span v-if="isDesktopInteraction">Kattints középre a nagy nézet megnyitásához</span>
                  <span v-else>Koppints középre a nagy nézet megnyitásához</span>
                </span>
              </div>

              <div
                class="absolute inset-0 flex items-end p-3 sm:p-4 opacity-0 group-hover:opacity-100 group-focus:opacity-100 transition-opacity duration-250"
                :class="isCardPreviewed(item.id) ? 'opacity-100' : ''"
              >
                <div class="w-full flex items-end justify-between gap-3">
                  <div>
                    <p class="text-earth-50 text-xs sm:text-sm md:text-[0.92rem] leading-snug drop-shadow-lg">
                      {{ item.title }}
                    </p>
                    <p class="mt-1 text-earth-200/90 text-[11px] sm:text-xs md:text-[0.82rem] font-semibold">
                      {{ item.uploaderName }}
                    </p>
                    <p class="mt-1 text-earth-200/75 text-[10px] sm:text-[11px]">
                      <span v-if="isDesktopInteraction">Kattints a nagy nézet megnyitásához</span>
                      <span v-else>Koppints még egyszer a nagy nézethez</span>
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

<style scoped>
.gallery-protected-image {
  user-select: none;
  -webkit-user-select: none;
  -webkit-user-drag: none;
  -webkit-touch-callout: none;
}
</style>

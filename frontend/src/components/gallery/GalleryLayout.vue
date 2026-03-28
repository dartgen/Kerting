<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import api from '@/services/axios'
import { useAuthStore } from '@/stores/authStore'

interface GalleryItem {
  id: number
  userId: number
  imageUrl: string
  title: string
  description: string
  uploaderName: string
  uploaderAvatarUrl: string
  uploadedAt: string
  likesCount: number
  dislikesCount: number
  myReaction: boolean | null
  isPublished: boolean
  isDeleted: boolean
  canEdit: boolean
  canDelete: boolean
  canPublishToggle: boolean
  comments: GalleryComment[]
}

interface GalleryComment {
  id: number
  userId: number
  userName: string
  avatarUrl: string
  message: string
  createdAt: string
  isDeleted: boolean
  canDelete: boolean
}

interface GalleryFeedItemDto {
  id: number
  userId: number
  imageUrl: string
  title?: string
  description?: string
  uploaderName: string
  profileImageUrl?: string | null
  createdAtUtc: string
  likesCount?: number
  dislikesCount?: number
  isPublished?: boolean
  isDeleted?: boolean
  canEdit?: boolean
  canDelete?: boolean
  canPublishToggle?: boolean
}

interface GalleryDetailCommentDto {
  id: number
  userId: number
  userName: string
  message: string
  createdAtUtc: string
  profileImageUrl?: string | null
  isDeleted?: boolean
  canDelete?: boolean
}

interface GalleryDetailDto {
  id: number
  userId: number
  title?: string
  description?: string
  profileImageUrl?: string | null
  likesCount?: number
  dislikesCount?: number
  myReaction?: boolean | null
  isPublished?: boolean
  isDeleted?: boolean
  canEdit?: boolean
  canDelete?: boolean
  canPublishToggle?: boolean
  comments?: GalleryDetailCommentDto[]
}

const props = withDefaults(defineProps<{
  mode?: 'main' | 'own'
  title?: string
  subtitle?: string
}>(), {
  mode: 'main',
  title: 'Galéria',
  subtitle: 'Inspirálódj más kertekből és munkafolyamatokból.'
})

const galleryItems = ref<GalleryItem[]>([])
const commentDraft = ref('')
const isSubmittingComment = ref(false)
const commentSubmitError = ref('')
const showDeleted = ref(false)
const isSavingItem = ref(false)
const isEditingItem = ref(false)
const editTitle = ref('')
const editDescription = ref('')

const MotionDiv = motion.div
const authStore = useAuthStore()
const previewCardId = ref<number | null>(null)
const expandedCardId = ref<number | null>(null)
const isDesktopInteraction = ref(false)

const isOwnMode = computed(() => props.mode === 'own')
const isAdmin = computed(() => authStore.profilAdatok?.roleId === 1)

const expandedCard = computed(() => galleryItems.value.find((item) => item.id === expandedCardId.value) ?? null)

const isCardPreviewed = (id: number) => previewCardId.value === id

const syncEditorFromExpanded = () => {
  if (!expandedCard.value) return
  editTitle.value = expandedCard.value.title
  editDescription.value = expandedCard.value.description
}

const openExpandedCard = async (id: number) => {
  commentSubmitError.value = ''
  try {
    const { data } = await api.get<GalleryDetailDto>(`/Gallery/${id}`, {
      params: {
        includeDeleted: isAdmin.value && showDeleted.value
      }
    })

    const itemIndex = galleryItems.value.findIndex(i => i.id === id)
    if (itemIndex !== -1) {
      const existingItem = galleryItems.value[itemIndex]
      if (existingItem) {
        galleryItems.value[itemIndex] = {
          id: existingItem.id,
          userId: data.userId ?? existingItem.userId,
          imageUrl: existingItem.imageUrl,
          title: data.title ?? existingItem.title,
          description: data.description ?? existingItem.description,
          uploaderName: existingItem.uploaderName,
          uploaderAvatarUrl: data.profileImageUrl
            ? getFullImageUrl(data.profileImageUrl)
            : existingItem.uploaderAvatarUrl,
          uploadedAt: existingItem.uploadedAt,
          likesCount: data.likesCount ?? existingItem.likesCount,
          dislikesCount: data.dislikesCount ?? existingItem.dislikesCount,
          myReaction: data.myReaction ?? null,
          isPublished: data.isPublished ?? existingItem.isPublished,
          isDeleted: data.isDeleted ?? existingItem.isDeleted,
          canEdit: data.canEdit ?? existingItem.canEdit,
          canDelete: data.canDelete ?? existingItem.canDelete,
          canPublishToggle: data.canPublishToggle ?? existingItem.canPublishToggle,
          comments: data.comments?.map((c) => ({
            id: c.id,
            userId: c.userId,
            userName: c.userName,
            avatarUrl: c.profileImageUrl
              ? getFullImageUrl(c.profileImageUrl)
              : `https://i.pravatar.cc/72?u=${c.id}`,
            message: c.message,
            createdAt: new Date(c.createdAtUtc).toLocaleDateString(),
            isDeleted: c.isDeleted ?? false,
            canDelete: c.canDelete ?? false
          })) || []
        }
      }
    }
  } catch (err) {
    console.error('Hiba a részletek lekérésekor', err)
  }
  expandedCardId.value = id
  isEditingItem.value = false
  syncEditorFromExpanded()
}

const handleCardClick = async (id: number) => {
  if (expandedCardId.value) return

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
  expandedCardId.value = null
  previewCardId.value = null
  commentDraft.value = ''
  commentSubmitError.value = ''
  isEditingItem.value = false
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

const deleteComment = async (commentId: number) => {
  if (!expandedCardId.value) return
  if (!window.confirm('Biztosan törölni szeretnéd ezt a kommentet?')) return

  try {
    await api.delete(`/Gallery/comment/${commentId}`)
    await openExpandedCard(expandedCardId.value)
  } catch (err) {
    console.error('Hiba komment törlésekor', err)
  }
}

const restoreComment = async (commentId: number) => {
  if (!expandedCardId.value || !isAdmin.value) return
  if (!window.confirm('Biztosan vissza szeretnéd állítani ezt a kommentet?')) return

  try {
    await api.patch(`/Gallery/comment/${commentId}/restore`)
    await openExpandedCard(expandedCardId.value)
  } catch (err) {
    console.error('Hiba komment visszaállításakor', err)
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

const saveItemMetadata = async () => {
  if (!expandedCard.value || isSavingItem.value) return

  const itemId = expandedCard.value.id
  isSavingItem.value = true
  try {
    await api.patch(`/Gallery/${itemId}`, {
      title: editTitle.value,
      description: editDescription.value
    })
    isEditingItem.value = false
    await fetchFeed()
    await openExpandedCard(itemId)
  } catch (err) {
    console.error('Hiba a bejegyzés mentésekor', err)
  } finally {
    isSavingItem.value = false
  }
}

const setPublishState = async (isPublished: boolean) => {
  if (!expandedCard.value) return
  const itemId = expandedCard.value.id
  try {
    await api.patch(`/Gallery/${itemId}/publish`, null, { params: { isPublished } })
    await fetchFeed()
    await openExpandedCard(itemId)
  } catch (err) {
    console.error('Hiba publikáció állításakor', err)
  }
}

const restoreItem = async () => {
  if (!expandedCard.value || !isAdmin.value) return
  if (!window.confirm('Biztosan vissza szeretnéd állítani ezt a publikációt? Visszaállítás után nem lesz publikus.')) return

  const itemId = expandedCard.value.id
  try {
    await api.patch(`/Gallery/${itemId}/restore`)
    await fetchFeed()
    await openExpandedCard(itemId)
  } catch (err) {
    console.error('Hiba publikáció visszaállításakor', err)
  }
}

const softDeleteItem = async () => {
  if (!expandedCard.value) return
  if (!window.confirm('Biztosan törölni szeretnéd ezt a publikációt?')) return

  try {
    await api.delete(`/Gallery/${expandedCard.value.id}`)
    closeExpandedCard()
    await fetchFeed()
  } catch (err) {
    console.error('Hiba publikáció törlésekor', err)
  }
}

const desktopQuery = '(hover: hover) and (pointer: fine)'
let mediaQuery: MediaQueryList | null = null

const syncInteractionMode = () => {
  isDesktopInteraction.value = Boolean(mediaQuery?.matches)
}

const getApiOrigin = () => {
  const baseURL = String(api.defaults.baseURL || '').trim()
  if (!baseURL) return window.location.origin

  try {
    return new URL(baseURL, window.location.origin).origin
  } catch {
    return window.location.origin
  }
}

const getFullImageUrl = (url: string) => {
  if (!url) return ''
  if (url.startsWith('http')) return url

  const origin = getApiOrigin()
  return `${origin}${url.startsWith('/') ? '' : '/'}${url}`
}

const endpoint = computed(() => isOwnMode.value ? '/Gallery/mine' : '/Gallery/feed')

const fetchFeed = async () => {
  try {
    const { data } = await api.get<GalleryFeedItemDto[]>(endpoint.value, {
      params: {
        includeDeleted: isAdmin.value && showDeleted.value
      }
    })

    galleryItems.value = data.map((item) => ({
      id: item.id,
      userId: item.userId,
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
      isPublished: item.isPublished ?? true,
      isDeleted: item.isDeleted ?? false,
      canEdit: item.canEdit ?? false,
      canDelete: item.canDelete ?? false,
      canPublishToggle: item.canPublishToggle ?? false,
      comments: []
    }))
  } catch (err) {
    console.error('Hiba a galéria betöltésekor', err)
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

watch(isDesktopInteraction, (value) => {
  if (value) previewCardId.value = null
})

watch(showDeleted, async () => {
  closeExpandedCard()
  await fetchFeed()
})

watch(expandedCard, () => {
  syncEditorFromExpanded()
})
</script>

<template>
  <div class="w-full h-full min-h-0 py-2 sm:py-3">
    <MotionDiv
      :initial="{ opacity: 0, y: 14 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.45, ease: 'easeOut' }"
      class="w-full h-full min-h-0 bg-earth-900/70 backdrop-blur-lg border border-earth-100/20 rounded-2xl sm:rounded-3xl px-3 py-4 sm:px-5 sm:py-6 lg:px-6 shadow-[0_20px_50px_rgba(0,0,0,0.35)] overscroll-contain"
      :class="expandedCard ? 'overflow-y-auto lg:overflow-hidden' : 'overflow-y-auto'"
    >
      <div v-if="!expandedCard" class="mb-5 sm:mb-6 pb-3 sm:pb-4 border-b border-earth-100/15 text-center">
        <h1 class="text-2xl sm:text-3xl md:text-4xl font-serif text-earth-50">{{ title }}</h1>
        <p class="mt-2 text-sm sm:text-base text-earth-200/90">
          {{ subtitle }}
        </p>

        <div v-if="isAdmin" class="mt-4 flex justify-center">
          <label class="inline-flex items-center gap-2 text-xs sm:text-sm text-earth-100">
            <input v-model="showDeleted" type="checkbox" class="h-4 w-4 rounded border-earth-200/25 bg-earth-950/40" />
            Törölt elemek megjelenítése
          </label>
        </div>
      </div>

      <AnimatePresence>
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
              class="relative flex items-center justify-center rounded-2xl overflow-hidden border bg-earth-950/50"
              :class="expandedCard.isDeleted ? 'border-red-400/85' : 'border-earth-100/20'"
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
              <div v-if="expandedCard.isDeleted" class="absolute top-3 left-3 px-2 py-1 rounded-md bg-red-600/90 text-white text-xs border border-red-300/70">
                Törölt
              </div>
              <div v-if="!expandedCard.isPublished" class="absolute top-3 right-3 px-2 py-1 rounded-md bg-amber-600/90 text-white text-xs border border-amber-300/70">
                Nem publikus
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

              <div class="pt-4" v-if="isOwnMode && expandedCard.canEdit">
                <div class="flex items-center justify-between gap-2">
                  <h2 class="text-earth-50 font-semibold text-base sm:text-lg">{{ expandedCard.title }}</h2>
                  <button
                    type="button"
                    class="text-xs px-3 py-1 rounded-full border border-earth-100/25 bg-earth-900/65 text-earth-50 hover:bg-earth-800"
                    @click="isEditingItem = !isEditingItem"
                  >
                    {{ isEditingItem ? 'Mégse' : 'Szerkesztés' }}
                  </button>
                </div>
              </div>
              <div class="pt-4" v-else>
                <h2 class="text-earth-50 font-semibold text-base sm:text-lg">{{ expandedCard.title }}</h2>
              </div>

              <div class="pt-3" v-if="isEditingItem && isOwnMode && expandedCard.canEdit">
                <input
                  v-model="editTitle"
                  maxlength="150"
                  class="w-full rounded-xl border border-earth-100/20 bg-earth-900/60 px-3 py-2 text-sm text-earth-100"
                />
                <textarea
                  v-model="editDescription"
                  rows="3"
                  maxlength="2000"
                  class="mt-2 w-full resize-none rounded-xl border border-earth-100/20 bg-earth-900/60 px-3 py-2 text-sm text-earth-100"
                />
                <div class="mt-2 flex gap-2">
                  <button
                    type="button"
                    class="h-9 rounded-full border border-emerald-300/35 bg-emerald-700/80 px-4 text-xs text-white"
                    :disabled="isSavingItem"
                    @click="saveItemMetadata"
                  >
                    {{ isSavingItem ? 'Mentés...' : 'Mentés' }}
                  </button>
                  <button
                    type="button"
                    class="h-9 rounded-full border border-earth-100/25 bg-earth-900/70 px-4 text-xs text-earth-50"
                    @click="isEditingItem = false"
                  >
                    Mégse
                  </button>
                </div>
              </div>
              <div class="pt-3" v-else>
                <p class="mt-2 text-earth-200/90 text-sm leading-relaxed">{{ expandedCard.description }}</p>
              </div>

              <div class="mt-4 flex flex-wrap gap-2" v-if="isOwnMode && expandedCard.canEdit">
                <button
                  type="button"
                  class="h-9 rounded-full border px-4 text-xs font-semibold"
                  :class="expandedCard.isPublished
                    ? 'border-amber-300/35 bg-amber-700/80 text-white'
                    : 'border-emerald-300/35 bg-emerald-700/80 text-white'"
                  @click="setPublishState(!expandedCard.isPublished)"
                >
                  {{ expandedCard.isPublished ? 'Elrejtés (Unpublish)' : 'Publikálás' }}
                </button>

                <button
                  type="button"
                  class="h-9 rounded-full border border-red-300/35 bg-red-700/80 px-4 text-xs font-semibold text-white"
                  @click="softDeleteItem"
                >
                  Publikáció törlése
                </button>
              </div>

              <div class="mt-2 flex flex-wrap gap-2" v-if="expandedCard.isDeleted && isAdmin">
                <button
                  type="button"
                  class="h-9 rounded-full border border-emerald-300/35 bg-emerald-700/80 px-4 text-xs font-semibold text-white"
                  @click="restoreItem"
                >
                  Visszaállítás (nem publikus)
                </button>
              </div>

              <div class="pt-4 flex-1 min-h-0 flex flex-col">
                <div class="h-px w-full bg-gradient-to-r from-transparent via-earth-100/20 to-transparent mb-5"></div>
                <h3 class="text-earth-50 font-semibold text-sm sm:text-base">Kommentek</h3>
                <div class="mt-3 min-h-0 flex-1 overflow-y-auto pr-1 space-y-3">
                  <div
                    v-for="comment in expandedCard.comments"
                    :key="comment.id"
                    class="group/comment rounded-xl border p-3 transition-colors"
                    :class="comment.isDeleted
                      ? 'border-red-400/90 bg-red-950/40'
                      : 'border-earth-100/10 bg-earth-900/55'"
                  >
                    <div class="flex items-center justify-between gap-2">
                      <div class="flex items-center gap-2">
                        <img
                          :src="comment.avatarUrl"
                          :alt="`${comment.userName} profilképe`"
                          class="w-8 h-8 rounded-full object-cover border border-earth-100/20"
                        />
                        <span class="text-earth-100 text-xs sm:text-sm font-medium">{{ comment.userName }}</span>
                        <span
                          v-if="comment.isDeleted"
                          class="rounded-full border border-red-300/70 bg-red-700/70 px-2 py-0.5 text-[10px] font-semibold text-white"
                        >
                          Törölt
                        </span>
                      </div>

                      <div class="flex items-center gap-2">
                        <span class="text-earth-200/70 text-[11px]">{{ comment.createdAt }}</span>
                        <button
                          v-if="comment.canDelete && !comment.isDeleted"
                          type="button"
                          class="opacity-0 group-hover/comment:opacity-100 transition-opacity text-red-300 hover:text-red-200 text-sm font-bold px-1"
                          aria-label="Komment törlése"
                          @click="deleteComment(comment.id)"
                        >
                          X
                        </button>
                        <button
                          v-if="comment.isDeleted && isAdmin"
                          type="button"
                          class="text-emerald-300 hover:text-emerald-200 text-[11px] font-semibold px-1"
                          aria-label="Komment visszaállítása"
                          @click="restoreComment(comment.id)"
                        >
                          Visszaállít
                        </button>
                      </div>
                    </div>
                    <p class="mt-2 text-xs sm:text-sm leading-relaxed" :class="comment.isDeleted ? 'text-red-200/85' : 'text-earth-200/95'">
                      {{ comment.message }}
                    </p>
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
          <MotionDiv
            v-for="(item, index) in galleryItems"
            :key="item.id"
            :initial="{ opacity: 0, y: 16 }"
            :animate="{ opacity: 1, y: 0 }"
            :transition="{ duration: 0.26, delay: index * 0.03, ease: 'easeOut' }"
            :whileHover="{ y: -2 }"
            class="group relative mb-2.5 sm:mb-3 break-inside-avoid overflow-hidden rounded-xl border bg-earth-950/35 shadow-[0_12px_24px_rgba(0,0,0,0.28)]"
            :class="item.isDeleted ? 'border-red-400/85' : 'border-earth-100/15'"
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
              class="pointer-events-none absolute inset-0 transition-all duration-250 backdrop-blur-0 group-hover:backdrop-blur-[1.5px] group-focus:backdrop-blur-[1.5px]"
              :class="[
                item.isDeleted
                  ? 'bg-red-900/5 group-hover:bg-red-900/18 group-focus:bg-red-900/18'
                  : 'bg-black/0 group-hover:bg-black/28 group-focus:bg-black/28',
                isCardPreviewed(item.id) ? (item.isDeleted ? 'bg-red-900/18 backdrop-blur-[1.5px]' : 'bg-black/28 backdrop-blur-[1.5px]') : ''
              ]"
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

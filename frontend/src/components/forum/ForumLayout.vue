<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue'
import PageTitle from '@/components/ui/PageTitle.vue'
import { forumService, type ForumSort } from '@/services/forumService'
import { authService } from '@/services/authService'
import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toast'

interface ForumFeedItem {
  id: number
  userId: number
  title: string
  description: string
  createdAtUtc: string
  updatedAtUtc?: string | null
  lastActivityAtUtc: string
  viewCount: number
  likesCount: number
  dislikesCount: number
  netScore: number
  commentsCount: number
  isDeleted: boolean
  isPinned: boolean
  isLocked: boolean
  lockReason?: string | null
  authorName: string
  authorRoleName: string
  profileImageUrl?: string | null
  attachedImageUrl?: string | null
  tags: string[]
  canEdit: boolean
  canDelete: boolean
  canRestore: boolean
  canModerate: boolean
}

interface ForumComment {
  id: number
  parentCommentId?: number | null
  userId: number
  message: string
  isDeleted: boolean
  createdAtUtc: string
  updatedAtUtc?: string | null
  authorName: string
  authorRoleName: string
  profileImageUrl?: string | null
  likesCount: number
  dislikesCount: number
  myReaction?: boolean | null
  canDelete: boolean
  canRestore: boolean
  hasMoreReplies: boolean
  nextReplyCursor: number
  replies: ForumComment[]
}

interface ForumDetail {
  id: number
  userId: number
  title: string
  description: string
  createdAtUtc: string
  updatedAtUtc?: string | null
  lastActivityAtUtc: string
  viewCount: number
  likesCount: number
  dislikesCount: number
  myReaction?: boolean | null
  isDeleted: boolean
  isPinned: boolean
  isLocked: boolean
  lockReason?: string | null
  authorName: string
  authorRoleName: string
  profileImageUrl?: string | null
  attachedImageUrl?: string | null
  tags: string[]
  comments: ForumComment[]
  nextCommentCursor: number
  hasMoreComments: boolean
  canEdit: boolean
  canDelete: boolean
  canRestore: boolean
  canModerate: boolean
}

const props = withDefaults(defineProps<{ mode?: 'list' | 'detail' }>(), {
  mode: 'list'
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const isDetailMode = computed(() => props.mode === 'detail')
const isAdmin = computed(() => authStore.profilAdatok?.roleId === 1)

const roles = ref<Array<{ id: number; name: string }>>([])
const allTags = ref<string[]>([])

const filters = reactive({
  sort: 'latest' as ForumSort,
  search: '',
  maxAgeDays: 30,
  selectedRoleIds: [] as number[],
  selectedTags: [] as string[]
})

const feedItems = ref<ForumFeedItem[]>([])
const feedPage = ref(1)
const feedHasMore = ref(false)
const feedLoading = ref(false)

const showCreateForm = ref(false)
const createForm = reactive({
  title: '',
  description: '',
  attachedGalleryItemId: '',
  tagInput: ''
})
const createTags = ref<string[]>([])
const savingPost = ref(false)

const detail = ref<ForumDetail | null>(null)
const detailLoading = ref(false)
const loadingMoreComments = ref(false)
const detailCommentDraft = ref('')
const replyingToComment = ref<number | null>(null)
const replyDraft = ref('')
const replyVisibility = ref<Record<number, boolean>>({})
const loadingReplies = ref<Record<number, boolean>>({})

const sortOptions: Array<{ value: ForumSort; label: string }> = [
  { value: 'latest', label: 'Legújabb -> Legrégebbi' },
  { value: 'oldest', label: 'Legrégebbi -> Legújabb' },
  { value: 'netdesc', label: 'Legtöbb kedvelés pontszám' },
  { value: 'netasc', label: 'Legkevesebb kedvelés pontszám' }
]

const canCreateTag = computed(() => {
  const role = (authStore.profilAdatok?.roleId ?? 0)
  return role === 3 || role === 4 || role === 5 || role === 1
})

const selectedRoleSet = computed(() => new Set(filters.selectedRoleIds))
const selectedTagSet = computed(() => new Set(filters.selectedTags.map(t => t.toLowerCase())))

const parseIntQuery = (value: unknown, fallback: number) => {
  const n = Number(value)
  return Number.isFinite(n) ? n : fallback
}

const normalizeText = (value: string) => value.trim()

const getFullImageUrl = (url?: string | null) => {
  if (!url) return ''
  if (url.startsWith('http')) return url
  const baseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace('/api', '') || 'https://localhost:7067'
  return `${baseUrl}${url.startsWith('/') ? '' : '/'}${url}`
}

const excerpt = (text: string, maxLength = 220) => {
  if (!text || text.length <= maxLength) return text
  return `${text.slice(0, maxLength).trim()}...`
}

const formatDateTime = (raw?: string | null) => {
  if (!raw) return '-'
  return new Date(raw).toLocaleString('hu-HU')
}

const getApiErrorMessage = (error: unknown, fallback: string) => {
  const message = (error as { response?: { data?: unknown } })?.response?.data
  if (typeof message === 'string' && message.trim()) {
    return message
  }
  if (typeof message === 'object' && message !== null) {
    const objectMessage = (message as { message?: string }).message
    if (typeof objectMessage === 'string' && objectMessage.trim()) {
      return objectMessage
    }
  }
  return fallback
}

const syncFiltersFromQuery = () => {
  if (isDetailMode.value) return

  filters.sort = (route.query.sort as ForumSort) || 'latest'
  filters.search = (route.query.search as string) || ''
  filters.maxAgeDays = Math.min(365, Math.max(0, parseIntQuery(route.query.maxAgeDays, 30)))

  const roleIds = Array.isArray(route.query.roleIds) ? route.query.roleIds : route.query.roleIds ? [route.query.roleIds] : []
  filters.selectedRoleIds = roleIds.map(v => Number(v)).filter(v => Number.isFinite(v))

  const tagNames = Array.isArray(route.query.tagNames) ? route.query.tagNames : route.query.tagNames ? [route.query.tagNames] : []
  filters.selectedTags = tagNames.map(v => String(v).trim()).filter(Boolean)
}

const syncQueryFromFilters = () => {
  if (isDetailMode.value) return

  router.replace({
    query: {
      sort: filters.sort,
      search: filters.search || undefined,
      maxAgeDays: String(filters.maxAgeDays),
      roleIds: filters.selectedRoleIds.length ? filters.selectedRoleIds : undefined,
      tagNames: filters.selectedTags.length ? filters.selectedTags : undefined
    }
  })
}

const loadMeta = async () => {
  const [roleRes, tagRes] = await Promise.all([authService.getRoles(), authService.GetCimekek()])
  roles.value = (roleRes.data || []).map((role: { id: number; name: string }) => ({
    id: role.id,
    name: (role.name || '').trim()
  }))

  allTags.value = (tagRes.data || [])
    .map((t: string) => t.trim())
    .filter(Boolean)

  if (!filters.selectedRoleIds.length) {
    filters.selectedRoleIds = roles.value.map(r => r.id)
  }
}

const fetchFeed = async (append = false) => {
  if (feedLoading.value) return
  feedLoading.value = true

  try {
    const pageToLoad = append ? feedPage.value + 1 : 1
    const { data } = await forumService.getFeed({
      page: pageToLoad,
      pageSize: 12,
      sort: filters.sort,
      search: normalizeText(filters.search) || undefined,
      maxAgeDays: filters.maxAgeDays,
      roleIds: filters.selectedRoleIds,
      tagNames: filters.selectedTags
    })

    const incoming = (data.items || []) as ForumFeedItem[]
    feedItems.value = append ? [...feedItems.value, ...incoming] : incoming
    feedPage.value = pageToLoad
    feedHasMore.value = Boolean(data.hasMore)
  } catch (error) {
    console.error('Forum feed betöltési hiba', error)
    toastStore.addToast('Nem sikerült a fórum listát betölteni.', 4000, 'error')
  } finally {
    feedLoading.value = false
  }
}

const fetchDetail = async (appendComments = false) => {
  if (!isDetailMode.value) return

  const postId = Number(route.params.id)
  if (!Number.isFinite(postId)) return

  if (!appendComments) detailLoading.value = true
  else loadingMoreComments.value = true

  try {
    const { data } = await forumService.getPostById(postId, {
      commentCursor: appendComments && detail.value ? detail.value.comments.length : 0,
      commentPageSize: 20,
      replyPageSize: 4,
      includeDeleted: isAdmin.value
    })

    const mappedComments = ((data.comments || []) as ForumComment[]).map(mapComment)

    if (!appendComments || !detail.value) {
      detail.value = {
        ...data,
        comments: mappedComments
      }
      const initialVisibility: Record<number, boolean> = {}
      mappedComments.forEach(c => {
        initialVisibility[c.id] = c.replies.length <= 3
      })
      replyVisibility.value = initialVisibility
    } else {
      detail.value = {
        ...detail.value,
        comments: [...detail.value.comments, ...mappedComments],
        nextCommentCursor: data.nextCommentCursor,
        hasMoreComments: data.hasMoreComments
      }
      mappedComments.forEach(c => {
        replyVisibility.value[c.id] = c.replies.length <= 3
      })
    }
  } catch (error) {
    console.error('Forum részlet betöltési hiba', error)
    toastStore.addToast('Nem sikerült a fórum bejegyzést betölteni.', 4000, 'error')
  } finally {
    detailLoading.value = false
    loadingMoreComments.value = false
  }
}

const mapComment = (raw: ForumComment): ForumComment => ({
  ...raw,
  replies: (raw.replies || []).map(mapComment)
})

const toggleRole = (roleId: number) => {
  if (selectedRoleSet.value.has(roleId)) {
    filters.selectedRoleIds = filters.selectedRoleIds.filter(id => id !== roleId)
    return
  }
  filters.selectedRoleIds = [...filters.selectedRoleIds, roleId]
}

const toggleTag = (tag: string) => {
  const normalized = tag.trim()
  if (!normalized) return

  if (selectedTagSet.value.has(normalized.toLowerCase())) {
    filters.selectedTags = filters.selectedTags.filter(t => t.toLowerCase() !== normalized.toLowerCase())
    return
  }
  filters.selectedTags = [...filters.selectedTags, normalized]
}

const addCreateTag = () => {
  const tag = createForm.tagInput.trim()
  if (!tag) return
  if (createTags.value.some(t => t.toLowerCase() === tag.toLowerCase())) {
    createForm.tagInput = ''
    return
  }

  if (!allTags.value.some(t => t.toLowerCase() === tag.toLowerCase()) && !canCreateTag.value) {
    toastStore.addToast('Új címke létrehozásához nincs jogosultságod.', 4000, 'warning')
    return
  }

  createTags.value = [...createTags.value, tag]
  createForm.tagInput = ''
}

const removeCreateTag = (tag: string) => {
  createTags.value = createTags.value.filter(t => t.toLowerCase() !== tag.toLowerCase())
}

const openDetail = (postId: number) => {
  router.push({
    name: 'forum-post',
    params: { id: postId },
    query: route.query
  })
}

const goBackToList = () => {
  router.push({
    name: 'forum',
    query: route.query
  })
}

const submitCreatePost = async () => {
  if (savingPost.value) return

  const title = createForm.title.trim()
  const description = createForm.description.trim()
  const rawAttachmentId = (createForm.attachedGalleryItemId || '').trim()
  const parsedAttachmentId = rawAttachmentId ? Number(rawAttachmentId) : null

  if (!title || !description) {
    toastStore.addToast('A cím és leírás kötelező.', 3500, 'warning')
    return
  }

  if (parsedAttachmentId !== null && (!Number.isInteger(parsedAttachmentId) || parsedAttachmentId <= 0)) {
    toastStore.addToast('A csatolt kép azonosítója csak pozitív egész szám lehet.', 4000, 'warning')
    return
  }

  savingPost.value = true
  try {
    await forumService.createPost({
      title,
      description,
      attachedGalleryItemId: parsedAttachmentId,
      tags: createTags.value
    })

    toastStore.addToast('Bejegyzés létrehozva.', 3000, 'success')
    createForm.title = ''
    createForm.description = ''
    createForm.attachedGalleryItemId = ''
    createForm.tagInput = ''
    createTags.value = []
    showCreateForm.value = false
    await fetchFeed(false)
  } catch (error) {
    console.error('Forum post létrehozási hiba', error)
    toastStore.addToast(getApiErrorMessage(error, 'Nem sikerült létrehozni a bejegyzést.'), 5000, 'error')
  } finally {
    savingPost.value = false
  }
}

const deletePost = async (postId: number) => {
  if (!window.confirm('Biztosan törlöd ezt a bejegyzést?')) return
  try {
    await forumService.deletePost(postId)
    toastStore.addToast('Bejegyzés törölve.', 3000, 'success')
    if (isDetailMode.value) {
      goBackToList()
      return
    }
    await fetchFeed(false)
  } catch (error) {
    console.error('Forum post törlési hiba', error)
    toastStore.addToast('A törlés nem sikerült.', 3500, 'error')
  }
}

const restorePost = async (postId: number) => {
  try {
    await forumService.restorePost(postId)
    toastStore.addToast('Bejegyzés visszaállítva.', 3000, 'success')
    if (isDetailMode.value) {
      await fetchDetail(false)
      return
    }
    await fetchFeed(false)
  } catch (error) {
    console.error('Forum post visszaállítási hiba', error)
    toastStore.addToast('A visszaállítás nem sikerült.', 3500, 'error')
  }
}

const togglePinned = async (postId: number, target: boolean) => {
  try {
    await forumService.setPinned(postId, target)
    await (isDetailMode.value ? fetchDetail(false) : fetchFeed(false))
  } catch (error) {
    console.error('Pin állapot frissítési hiba', error)
    toastStore.addToast('Pin állapot nem módosítható.', 3500, 'error')
  }
}

const toggleLocked = async (postId: number, target: boolean) => {
  let reason = ''
  if (target) {
    reason = window.prompt('Miért zárod a bejegyzést?')?.trim() || ''
  }

  try {
    await forumService.setLocked(postId, target, reason)
    await (isDetailMode.value ? fetchDetail(false) : fetchFeed(false))
  } catch (error) {
    console.error('Lock állapot frissítési hiba', error)
    toastStore.addToast('Lock állapot nem módosítható.', 3500, 'error')
  }
}

const reactPost = async (isLike: boolean) => {
  if (!detail.value) return
  try {
    await forumService.reactPost(detail.value.id, isLike)
    await fetchDetail(false)
  } catch (error) {
    console.error('Post reakció hiba', error)
    toastStore.addToast('A reakció mentése nem sikerült.', 3500, 'error')
  }
}

const reactComment = async (commentId: number, isLike: boolean) => {
  try {
    await forumService.reactComment(commentId, isLike)
    await fetchDetail(false)
  } catch (error) {
    console.error('Comment reakció hiba', error)
    toastStore.addToast('A reakció mentése nem sikerült.', 3500, 'error')
  }
}

const submitComment = async () => {
  if (!detail.value) return
  const message = detailCommentDraft.value.trim()
  if (!message) return

  try {
    await forumService.addComment(detail.value.id, { message })
    detailCommentDraft.value = ''
    await fetchDetail(false)
  } catch (error) {
    console.error('Comment küldési hiba', error)
    toastStore.addToast('A hozzászólás küldése nem sikerült.', 3500, 'error')
  }
}

const submitReply = async (parentCommentId: number) => {
  if (!detail.value) return
  const message = replyDraft.value.trim()
  if (!message) return

  try {
    await forumService.addComment(detail.value.id, { message, parentCommentId })
    replyDraft.value = ''
    replyingToComment.value = null
    await fetchDetail(false)
  } catch (error) {
    console.error('Reply küldési hiba', error)
    toastStore.addToast('A válasz küldése nem sikerült.', 3500, 'error')
  }
}

const deleteComment = async (commentId: number) => {
  if (!window.confirm('Biztosan törlöd ezt a hozzászólást?')) return
  try {
    await forumService.deleteComment(commentId)
    await fetchDetail(false)
  } catch (error) {
    console.error('Comment törlési hiba', error)
    toastStore.addToast('A hozzászólás törlése nem sikerült.', 3500, 'error')
  }
}

const restoreComment = async (commentId: number) => {
  try {
    await forumService.restoreComment(commentId)
    await fetchDetail(false)
  } catch (error) {
    console.error('Comment restore hiba', error)
    toastStore.addToast('A visszaállítás nem sikerült.', 3500, 'error')
  }
}

const toggleReplies = (commentId: number) => {
  replyVisibility.value[commentId] = !replyVisibility.value[commentId]
}

const loadMoreReplies = async (comment: ForumComment) => {
  if (loadingReplies.value[comment.id]) return

  loadingReplies.value[comment.id] = true
  try {
    const { data } = await forumService.getReplies(comment.id, comment.nextReplyCursor || comment.replies.length, 5, isAdmin.value)
    if (!data?.found) return

    const incoming = ((data.replies || []) as ForumComment[]).map(mapComment)
    comment.replies = [...comment.replies, ...incoming]
    comment.nextReplyCursor = data.nextCursor || comment.nextReplyCursor
    comment.hasMoreReplies = Boolean(data.hasMore)
  } catch (error) {
    console.error('Replies betöltési hiba', error)
    toastStore.addToast('Nem sikerült a válaszokat betölteni.', 3500, 'error')
  } finally {
    loadingReplies.value[comment.id] = false
  }
}

watch(
  () => [filters.sort, filters.search, filters.maxAgeDays, JSON.stringify(filters.selectedRoleIds), JSON.stringify(filters.selectedTags)],
  async () => {
    if (isDetailMode.value) return
    syncQueryFromFilters()
    await fetchFeed(false)
  }
)

watch(
  () => route.params.id,
  async () => {
    if (!isDetailMode.value) return
    await fetchDetail(false)
  }
)

onMounted(async () => {
  try {
    await loadMeta()
    syncFiltersFromQuery()

    if (isDetailMode.value) {
      await fetchDetail(false)
      return
    }

    await fetchFeed(false)
  } catch (error) {
    console.error('Forum inicializálási hiba', error)
    toastStore.addToast('A fórum nem inicializálható.', 4000, 'error')
  }
})
</script>

<template>
  <InnerPageLayout v-if="!isDetailMode">
    <PageTitle title="Fórum" />
    <p class="text-earth-200/90 mt-2 mb-6">Közösségi beszélgetések, tippek és kérdések.</p>

    <div class="grid grid-cols-1 lg:grid-cols-[1fr_3fr] gap-6">
      <aside class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4 space-y-5">
        <div>
          <label class="text-sm text-earth-100 block mb-2">Rendezés</label>
          <select v-model="filters.sort" class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10">
            <option v-for="opt in sortOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>

        <div>
          <label class="text-sm text-earth-100 block mb-2">Keresés</label>
          <input
            v-model="filters.search"
            type="text"
            class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
            placeholder="Cím vagy leírás..."
          />
        </div>

        <div>
          <label class="text-sm text-earth-100 block mb-2">Maximum életkor (nap)</label>
          <input v-model.number="filters.maxAgeDays" type="range" min="0" max="365" class="w-full" />
          <p class="text-xs text-earth-300 mt-1">{{ filters.maxAgeDays }} nap</p>
        </div>

        <div>
          <p class="text-sm text-earth-100 mb-2">Szerepkör szűrő</p>
          <div class="space-y-2 max-h-40 overflow-y-auto">
            <label v-for="role in roles" :key="role.id" class="flex items-center gap-2 text-earth-200 text-sm">
              <input
                type="checkbox"
                :checked="selectedRoleSet.has(role.id)"
                @change="toggleRole(role.id)"
              />
              {{ role.name }}
            </label>
          </div>
        </div>

        <div>
          <p class="text-sm text-earth-100 mb-2">Címkék</p>
          <div class="flex flex-wrap gap-2 max-h-44 overflow-y-auto">
            <button
              v-for="tag in allTags"
              :key="tag"
              type="button"
              @click="toggleTag(tag)"
              class="px-2.5 py-1 rounded-full text-xs border transition-colors"
              :class="selectedTagSet.has(tag.toLowerCase()) ? 'bg-green-500/25 border-green-400 text-green-100' : 'bg-earth-800 border-earth-100/10 text-earth-200'"
            >
              {{ tag }}
            </button>
          </div>
        </div>
      </aside>

      <section class="space-y-4">
        <div class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4">
          <div class="flex items-center justify-between mb-3">
            <h3 class="text-lg font-semibold text-earth-50">Új bejegyzés</h3>
            <button type="button" @click="showCreateForm = !showCreateForm" class="text-sm text-green-300 hover:text-green-200">
              {{ showCreateForm ? 'Bezárás' : 'Megnyitás' }}
            </button>
          </div>

          <div v-if="showCreateForm" class="space-y-3">
            <input v-model="createForm.title" type="text" placeholder="Cím"
              class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10" />
            <textarea v-model="createForm.description" rows="4" placeholder="Leírás"
              class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10" />
            <input v-model="createForm.attachedGalleryItemId" type="number" min="1" placeholder="Saját galéria kép azonosító (opcionális)"
              class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10" />

            <div class="flex gap-2">
              <input
                v-model="createForm.tagInput"
                type="text"
                placeholder="Címke"
                class="flex-1 rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
                @keydown.enter.prevent="addCreateTag"
              />
              <button type="button" @click="addCreateTag" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100">Hozzáad</button>
            </div>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="tag in createTags"
                :key="tag"
                type="button"
                class="px-2.5 py-1 rounded-full text-xs bg-green-500/20 border border-green-400 text-green-100"
                @click="removeCreateTag(tag)"
              >
                {{ tag }} ×
              </button>
            </div>

            <button type="button" @click="submitCreatePost" :disabled="savingPost"
              class="px-4 py-2 rounded-lg bg-green-600 hover:bg-green-500 text-white disabled:opacity-60">
              {{ savingPost ? 'Mentés...' : 'Létrehozás' }}
            </button>
          </div>
        </div>

        <article
          v-for="post in feedItems"
          :key="post.id"
          class="rounded-2xl border border-earth-100/10 bg-earth-900/40 overflow-hidden"
        >
          <div class="flex flex-col md:flex-row">
            <div v-if="post.attachedImageUrl" class="md:w-56 h-44 md:h-auto shrink-0">
              <img :src="getFullImageUrl(post.attachedImageUrl)" class="w-full h-full object-cover" alt="Forum kép" />
            </div>
            <div class="flex-1 p-4 space-y-3">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <h3 class="text-lg font-semibold text-earth-50">{{ post.title }}</h3>
                  <p class="text-xs text-earth-300">
                    {{ post.authorName }} · {{ post.authorRoleName || 'Szerepkör nélkül' }} · {{ formatDateTime(post.createdAtUtc) }}
                  </p>
                </div>
                <div class="flex gap-2">
                  <span v-if="post.isPinned" class="text-xs px-2 py-1 rounded bg-amber-500/20 text-amber-200">Pinned</span>
                  <span v-if="post.isLocked" class="text-xs px-2 py-1 rounded bg-red-500/20 text-red-200">Locked</span>
                </div>
              </div>

              <p class="text-earth-200/90">{{ excerpt(post.description) }}</p>

              <div class="flex flex-wrap gap-2">
                <span v-for="tag in post.tags" :key="tag" class="text-xs px-2 py-1 rounded-full bg-earth-800 border border-earth-100/10 text-earth-200">
                  {{ tag }}
                </span>
              </div>

              <div class="flex items-center justify-between gap-3 pt-2">
                <div class="text-xs text-earth-300 flex gap-4">
                  <span>👍 {{ post.likesCount }}</span>
                  <span>👎 {{ post.dislikesCount }}</span>
                  <span>💬 {{ post.commentsCount }}</span>
                </div>
                <div class="flex items-center gap-2">
                  <button type="button" class="px-3 py-1.5 rounded-lg text-sm bg-earth-700 text-earth-100" @click="openDetail(post.id)">Megnyitás</button>
                  <button v-if="post.canDelete" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-red-700/80 text-red-100" @click="deletePost(post.id)">Törlés</button>
                  <button v-if="post.canRestore" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-green-700/80 text-green-100" @click="restorePost(post.id)">Visszaállítás</button>
                </div>
              </div>
            </div>
          </div>
        </article>

        <div class="flex justify-center py-2">
          <button v-if="feedHasMore" type="button" :disabled="feedLoading" @click="fetchFeed(true)" class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100 disabled:opacity-60">
            {{ feedLoading ? 'Betöltés...' : 'További bejegyzések' }}
          </button>
        </div>
      </section>
    </div>
  </InnerPageLayout>

  <InnerPageLayout v-else>
    <button type="button" class="mb-4 text-earth-200 hover:text-earth-50" @click="goBackToList">← Vissza a fórumhoz</button>

    <div v-if="detailLoading" class="rounded-xl border border-earth-100/10 bg-earth-900/40 p-6 text-earth-200">Betöltés...</div>

    <template v-else-if="detail">
      <article class="rounded-2xl border border-earth-100/10 bg-earth-900/40 overflow-hidden mb-6">
        <img v-if="detail.attachedImageUrl" :src="getFullImageUrl(detail.attachedImageUrl)" alt="Forum kép" class="w-full h-64 object-cover" />
        <div class="p-5 space-y-4">
          <div class="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h1 class="text-2xl font-bold text-earth-50">{{ detail.title }}</h1>
              <p class="text-sm text-earth-300">
                {{ detail.authorName }} · {{ detail.authorRoleName || 'Szerepkör nélkül' }}
              </p>
            </div>
            <div class="flex gap-2">
              <button v-if="detail.canModerate" type="button" class="px-3 py-1 rounded-lg bg-earth-700 text-earth-100" @click="togglePinned(detail.id, !detail.isPinned)">
                {{ detail.isPinned ? 'Unpin' : 'Pin' }}
              </button>
              <button v-if="detail.canModerate" type="button" class="px-3 py-1 rounded-lg bg-earth-700 text-earth-100" @click="toggleLocked(detail.id, !detail.isLocked)">
                {{ detail.isLocked ? 'Feloldás' : 'Lezárás' }}
              </button>
              <button v-if="detail.canDelete" type="button" class="px-3 py-1 rounded-lg bg-red-700/80 text-red-100" @click="deletePost(detail.id)">Törlés</button>
              <button v-if="detail.canRestore" type="button" class="px-3 py-1 rounded-lg bg-green-700/80 text-green-100" @click="restorePost(detail.id)">Visszaállítás</button>
            </div>
          </div>

          <p class="text-earth-100 whitespace-pre-line">{{ detail.description }}</p>

          <div v-if="detail.isLocked && detail.lockReason" class="rounded-lg bg-red-500/20 border border-red-400/40 p-3 text-red-100 text-sm">
            Lezárás oka: {{ detail.lockReason }}
          </div>

          <div class="text-xs text-earth-300 grid sm:grid-cols-2 gap-2">
            <p>Létrehozva: {{ formatDateTime(detail.createdAtUtc) }}</p>
            <p>Módosítva: {{ formatDateTime(detail.updatedAtUtc || detail.createdAtUtc) }}</p>
            <p>Utolsó aktivitás: {{ formatDateTime(detail.lastActivityAtUtc) }}</p>
            <p>Megtekintések: {{ detail.viewCount }}</p>
          </div>

          <div class="flex flex-wrap gap-2">
            <span v-for="tag in detail.tags" :key="tag" class="text-xs px-2 py-1 rounded-full bg-earth-800 border border-earth-100/10 text-earth-200">
              {{ tag }}
            </span>
          </div>

          <div class="flex items-center gap-3 pt-1">
            <button type="button" class="px-3 py-1.5 rounded-lg bg-green-700/70 text-green-100" @click="reactPost(true)">👍 {{ detail.likesCount }}</button>
            <button type="button" class="px-3 py-1.5 rounded-lg bg-red-700/70 text-red-100" @click="reactPost(false)">👎 {{ detail.dislikesCount }}</button>
          </div>
        </div>
      </article>

      <section class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-5">
        <h2 class="text-lg font-semibold text-earth-50 mb-3">Hozzászólások</h2>

        <div class="flex gap-2 mb-4" v-if="!detail.isLocked">
          <textarea
            v-model="detailCommentDraft"
            rows="3"
            class="flex-1 rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
            placeholder="Írj hozzászólást..."
          />
          <button type="button" class="px-4 py-2 rounded-lg bg-green-600 text-white self-end" @click="submitComment">Küldés</button>
        </div>

        <div class="space-y-4">
          <article v-for="comment in detail.comments" :key="comment.id" class="rounded-xl border border-earth-100/10 bg-earth-800/40 p-4">
            <div class="flex items-center justify-between gap-3">
              <div>
                <p class="text-sm text-earth-100 font-medium">{{ comment.authorName }} · {{ comment.authorRoleName || 'Szerepkör nélkül' }}</p>
                <p class="text-xs text-earth-300">{{ formatDateTime(comment.createdAtUtc) }}</p>
              </div>
              <div class="flex gap-2">
                <button type="button" class="text-xs px-2 py-1 rounded bg-earth-700 text-earth-100" @click="reactComment(comment.id, true)">👍 {{ comment.likesCount }}</button>
                <button type="button" class="text-xs px-2 py-1 rounded bg-earth-700 text-earth-100" @click="reactComment(comment.id, false)">👎 {{ comment.dislikesCount }}</button>
                <button v-if="comment.canDelete" type="button" class="text-xs px-2 py-1 rounded bg-red-700/80 text-red-100" @click="deleteComment(comment.id)">Törlés</button>
                <button v-if="comment.canRestore" type="button" class="text-xs px-2 py-1 rounded bg-green-700/80 text-green-100" @click="restoreComment(comment.id)">Visszaállít</button>
              </div>
            </div>

            <p class="text-earth-100 mt-2 whitespace-pre-line">{{ comment.message }}</p>

            <div class="mt-3 flex items-center gap-3 text-xs">
              <button type="button" class="text-earth-300 hover:text-earth-50" @click="toggleReplies(comment.id)">
                {{ replyVisibility[comment.id] ? 'Válaszok elrejtése' : 'Válaszok megjelenítése' }} ({{ comment.replies.length }})
              </button>
              <button v-if="!detail.isLocked" type="button" class="text-green-300 hover:text-green-200" @click="replyingToComment = replyingToComment === comment.id ? null : comment.id">
                Válasz
              </button>
            </div>

            <div v-if="replyingToComment === comment.id && !detail.isLocked" class="mt-3 flex gap-2">
              <input
                v-model="replyDraft"
                type="text"
                class="flex-1 rounded-lg bg-earth-900/80 px-3 py-2 text-earth-50 border border-earth-100/10"
                placeholder="Válasz írása..."
                @keydown.enter.prevent="submitReply(comment.id)"
              />
              <button type="button" class="px-3 py-2 rounded-lg bg-green-600 text-white" @click="submitReply(comment.id)">Küld</button>
            </div>

            <div v-if="replyVisibility[comment.id]" class="mt-3 pl-4 border-l border-earth-100/10 space-y-3">
              <article v-for="reply in comment.replies" :key="reply.id" class="rounded-lg bg-earth-900/50 border border-earth-100/10 p-3">
                <div class="flex items-center justify-between gap-2">
                  <p class="text-sm text-earth-100">{{ reply.authorName }} · {{ reply.authorRoleName || 'Szerepkör nélkül' }}</p>
                  <p class="text-xs text-earth-300">{{ formatDateTime(reply.createdAtUtc) }}</p>
                </div>
                <p class="text-earth-100 mt-2 whitespace-pre-line">{{ reply.message }}</p>
                <div class="mt-2 flex gap-2">
                  <button type="button" class="text-xs px-2 py-1 rounded bg-earth-700 text-earth-100" @click="reactComment(reply.id, true)">👍 {{ reply.likesCount }}</button>
                  <button type="button" class="text-xs px-2 py-1 rounded bg-earth-700 text-earth-100" @click="reactComment(reply.id, false)">👎 {{ reply.dislikesCount }}</button>
                  <button v-if="reply.canDelete" type="button" class="text-xs px-2 py-1 rounded bg-red-700/80 text-red-100" @click="deleteComment(reply.id)">Törlés</button>
                  <button v-if="reply.canRestore" type="button" class="text-xs px-2 py-1 rounded bg-green-700/80 text-green-100" @click="restoreComment(reply.id)">Visszaállít</button>
                </div>
              </article>

              <button
                v-if="comment.hasMoreReplies"
                type="button"
                class="text-xs text-earth-300 hover:text-earth-100"
                :disabled="loadingReplies[comment.id]"
                @click="loadMoreReplies(comment)"
              >
                {{ loadingReplies[comment.id] ? 'Betöltés...' : 'További válaszok' }}
              </button>
            </div>
          </article>
        </div>

        <div class="pt-4 flex justify-center">
          <button
            v-if="detail.hasMoreComments"
            type="button"
            class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100"
            :disabled="loadingMoreComments"
            @click="fetchDetail(true)"
          >
            {{ loadingMoreComments ? 'Betöltés...' : 'További hozzászólások' }}
          </button>
        </div>
      </section>
    </template>
  </InnerPageLayout>
</template>

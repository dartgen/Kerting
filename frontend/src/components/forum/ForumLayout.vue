<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue'
import PageTitle from '@/components/ui/PageTitle.vue'
import ForumFiltersSidebar from '@/components/forum/ForumFiltersSidebar.vue'
import ForumCreatePostPanel from '@/components/forum/ForumCreatePostPanel.vue'
import ForumFeedPostCard from '@/components/forum/ForumFeedPostCard.vue'
import ForumDetailPanel from '@/components/forum/ForumDetailPanel.vue'
import ForumGalleryPickerModal from '@/components/forum/ForumGalleryPickerModal.vue'
import ForumEditPostModal from '@/components/forum/ForumEditPostModal.vue'
import { forumService, type ForumSort } from '@/services/forumService'
import api from '@/services/axios'
import { authService } from '@/services/authService'
import type { RoleDto } from '@/types/auth'
import type { ForumComment, ForumDetail, ForumFeedItem, OwnGalleryItem } from '@/types/forum'
import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toast'
import {
  applyReactionDelta,
  excerpt,
  formatDateTime,
  getApiErrorMessage,
  getFullImageUrl as resolveFullImageUrl,
  normalizeText,
  parseIntQuery
} from '@/components/forum/forumUtils'


const props = withDefaults(defineProps<{ mode?: 'list' | 'detail' }>(), {
  mode: 'list'
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

// Nézetmódok és jogosultsági segéd-computedek.
const isDetailMode = computed(() => props.mode === 'detail')
const isForumListRoute = computed(() => route.name === 'forum' && !isDetailMode.value)
const isAdmin = computed(() => authStore.profilAdatok?.roleId === 1)

const showFilters = ref(false)

// Metadata a szűrőkhöz (szerepkörök, címkék).
const roles = ref<RoleDto[]>([])
const allTags = ref<string[]>([])

// Feed szűrő állapot, URL queryvel szinkronizálva.
const filters = reactive({
  sort: 'latest' as ForumSort,
  search: '',
  maxAgeDays: 30,
  selectedRoleIds: [] as number[],
  selectedTags: [] as string[]
})
const showDeleted = ref(false)

// Listanézet feed állapot.
const feedItems = ref<ForumFeedItem[]>([])
const feedPage = ref(1)
const feedHasMore = ref(false)
const feedLoading = ref(false)
const reactingPostIds = ref<Record<number, boolean>>({})

// Létrehozás űrlap állapot.
const showCreateForm = ref(false)
const createForm = reactive({
  title: '',
  description: '',
  tagInput: ''
})
const createTags = ref<string[]>([])
const savingPost = ref(false)
const showTagSuggestions = ref(false)

// Szerkesztés modál állapot.
const editingPostId = ref<number | null>(null)
const editForm = reactive({
  title: '',
  description: '',
  tagInput: ''
})
const editTags = ref<string[]>([])
const editSaving = ref(false)
const editShowTagSuggestions = ref(false)

// Saját galéria picker állapot.
const ownGalleryItems = ref<OwnGalleryItem[]>([])
const selectedGalleryItemId = ref<number | null>(null)
const editSelectedGalleryItemId = ref<number | null>(null)
const pickerMode = ref<'create' | 'edit'>('create')
const pickerOpen = ref(false)
const pickerLoading = ref(false)
const pickerLoaded = ref(false)

// Részletnézet + kommentfolyam állapot.
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
const createTagSet = computed(() => new Set(createTags.value.map(t => t.toLowerCase())))
const editTagSet = computed(() => new Set(editTags.value.map(t => t.toLowerCase())))
const selectedGalleryItem = computed(() => ownGalleryItems.value.find(item => item.id === selectedGalleryItemId.value) || null)
const selectedEditGalleryItem = computed(() => ownGalleryItems.value.find(item => item.id === editSelectedGalleryItemId.value) || null)
const filteredTagSuggestions = computed(() => {
  const keyword = createForm.tagInput.toLowerCase().trim()
  if (!keyword) return []

  return allTags.value
    .filter(tag => tag.toLowerCase().includes(keyword) && !createTagSet.value.has(tag.toLowerCase()))
    .slice(0, 8)
})
const filteredEditTagSuggestions = computed(() => {
  const keyword = editForm.tagInput.toLowerCase().trim()
  if (!keyword) return []

  return allTags.value
    .filter(tag => tag.toLowerCase().includes(keyword) && !editTagSet.value.has(tag.toLowerCase()))
    .slice(0, 8)
})

// A fórum komponensekhez egységes kép URL feloldás.
const getFullImageUrl = (url?: string | null) => resolveFullImageUrl(String(api.defaults.baseURL || ''), url)

const getCurrentUserId = () => {
  const raw = authStore.felhasznalo?.id
  const parsed = Number(raw)
  return Number.isFinite(parsed) ? parsed : 0
}

const getCurrentUserName = () => {
  const vezetekNev = String(authStore.profilAdatok?.vezetekNev || '').trim()
  const keresztNev = String(authStore.profilAdatok?.keresztNev || '').trim()
  const fullName = `${vezetekNev} ${keresztNev}`.trim()
  if (fullName) return fullName
  return String(authStore.felhasznalo?.felhasznaloNev || 'Te')
}

const getCurrentUserRoleName = () => String(authStore.profilAdatok?.roleName || '').trim()

const getCurrentUserProfileImageUrl = () => {
  const fileName = String(authStore.profilAdatok?.imgString || '').trim()
  return fileName ? getFullImageUrl(`/resources/profiles/${fileName}`) : null
}

// API válaszokból esetenként eltérő ID mezőnév jön (id / Id), ezt normalizáljuk.
const extractCreatedCommentId = (responseData: unknown) => {
  if (!responseData || typeof responseData !== 'object') return undefined
  const candidate = responseData as { id?: unknown; Id?: unknown }
  const numeric = Number(candidate.id ?? candidate.Id)
  return Number.isFinite(numeric) ? numeric : undefined
}

// Optimista UI-hoz lokális komment objektumot építünk.
const createLocalComment = (message: string, id?: number, parentCommentId?: number | null): ForumComment => ({
  id: id ?? -Date.now(),
  parentCommentId: parentCommentId ?? null,
  userId: getCurrentUserId(),
  message,
  isDeleted: false,
  createdAtUtc: new Date().toISOString(),
  updatedAtUtc: null,
  authorName: getCurrentUserName(),
  authorRoleName: getCurrentUserRoleName(),
  profileImageUrl: getCurrentUserProfileImageUrl(),
  likesCount: 0,
  dislikesCount: 0,
  myReaction: null,
  canDelete: true,
  canRestore: false,
  hasMoreReplies: false,
  nextReplyCursor: 0,
  replies: []
})

// Query -> filter állapot szinkron a listanézet inicializálásakor.
const syncFiltersFromQuery = () => {
  if (!isForumListRoute.value) return

  filters.sort = (route.query.sort as ForumSort) || 'latest'
  filters.search = (route.query.search as string) || ''
  filters.maxAgeDays = Math.min(365, Math.max(0, parseIntQuery(route.query.maxAgeDays, 30)))

  const roleIds = Array.isArray(route.query.roleIds) ? route.query.roleIds : route.query.roleIds ? [route.query.roleIds] : []
  filters.selectedRoleIds = roleIds.map(v => Number(v)).filter(v => Number.isFinite(v))

  const tagNames = Array.isArray(route.query.tagNames) ? route.query.tagNames : route.query.tagNames ? [route.query.tagNames] : []
  filters.selectedTags = tagNames.map(v => String(v).trim()).filter(Boolean)
}

// Filter állapot -> query szinkron, hogy a linkelhetőség megmaradjon.
const syncQueryFromFilters = () => {
  if (!isForumListRoute.value) return

  const nextQuery = {
    sort: filters.sort,
    search: filters.search || undefined,
    maxAgeDays: String(filters.maxAgeDays),
    roleIds: filters.selectedRoleIds.length ? filters.selectedRoleIds.map(String) : undefined,
    tagNames: filters.selectedTags.length ? filters.selectedTags : undefined
  }

  const currentRoleIds = Array.isArray(route.query.roleIds)
    ? route.query.roleIds.map(v => String(v))
    : route.query.roleIds
      ? [String(route.query.roleIds)]
      : []

  const currentTagNames = Array.isArray(route.query.tagNames)
    ? route.query.tagNames.map(v => String(v))
    : route.query.tagNames
      ? [String(route.query.tagNames)]
      : []

  const sameQuery =
    String(route.query.sort || 'latest') === String(nextQuery.sort) &&
    String(route.query.search || '') === String(nextQuery.search || '') &&
    String(route.query.maxAgeDays || '30') === String(nextQuery.maxAgeDays) &&
    JSON.stringify(currentRoleIds) === JSON.stringify(nextQuery.roleIds || []) &&
    JSON.stringify(currentTagNames) === JSON.stringify(nextQuery.tagNames || [])

  if (sameQuery) return

  router.replace({
    name: 'forum',
    query: nextQuery
  })
}

// Szűrő metadata inicializálása.
const loadMeta = async () => {
  const [roleRes, tagRes] = await Promise.all([authService.getRoles(), authService.GetCimekek()])
  roles.value = (roleRes.data || []).map(role => ({
    id: role.id,
    name: (role.name || '').trim()
  }))

  allTags.value = (tagRes.data || [])
    .map((t: string) => t.trim())
    .filter(Boolean)
}

  // Fórum feed lekérése, opcionális laphozzáfűzéssel.
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
      tagNames: filters.selectedTags,
      includeDeleted: isAdmin.value && showDeleted.value
    })

    const incoming = ((data.items || []) as Array<Record<string, unknown>>).map((item) => {
      const likesCount = Number(item.likesCount ?? item.LikesCount ?? 0)
      const dislikesCount = Number(item.dislikesCount ?? item.DislikesCount ?? 0)
      const commentsCount = Number(item.commentsCount ?? item.CommentsCount ?? 0)
      const myReactionRaw = (item.myReaction ?? item.MyReaction) as boolean | null | undefined

      return {
        ...(item as unknown as ForumFeedItem),
        likesCount: Number.isFinite(likesCount) ? likesCount : 0,
        dislikesCount: Number.isFinite(dislikesCount) ? dislikesCount : 0,
        commentsCount: Number.isFinite(commentsCount) ? commentsCount : 0,
        myReaction: myReactionRaw === true ? true : myReactionRaw === false ? false : null
      }
    })
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

// Post részlet és kommentfolyam lekérése (kurzoros kommentlapozással).
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
      includeDeleted: isAdmin.value && showDeleted.value
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

// Szűrő toggles: role/tag listák karbantartása.
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

// Létrehozó/szerkesztő címke kezelés (duplikátumszűrés + jogosultság).
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
  showTagSuggestions.value = false
}

const selectCreateTagSuggestion = (tag: string) => {
  createForm.tagInput = tag
  addCreateTag()
}

const addEditTag = () => {
  const tag = editForm.tagInput.trim()
  if (!tag) return
  if (editTags.value.some(t => t.toLowerCase() === tag.toLowerCase())) {
    editForm.tagInput = ''
    return
  }

  if (!allTags.value.some(t => t.toLowerCase() === tag.toLowerCase()) && !canCreateTag.value) {
    toastStore.addToast('Új címke létrehozásához nincs jogosultságod.', 4000, 'warning')
    return
  }

  editTags.value = [...editTags.value, tag]
  editForm.tagInput = ''
  editShowTagSuggestions.value = false
}

const selectEditTagSuggestion = (tag: string) => {
  editForm.tagInput = tag
  addEditTag()
}

const handleCreateTagInput = () => {
  showTagSuggestions.value = createForm.tagInput.trim().length > 0
}

const handleEditTagInput = () => {
  editShowTagSuggestions.value = editForm.tagInput.trim().length > 0
}

const removeCreateTag = (tag: string) => {
  createTags.value = createTags.value.filter(t => t.toLowerCase() !== tag.toLowerCase())
}

const removeEditTag = (tag: string) => {
  editTags.value = editTags.value.filter(t => t.toLowerCase() !== tag.toLowerCase())
}

// Navigáció listából részletbe, aktuális query állapot átvitelével.
const openDetail = (postId: number) => {
  syncQueryFromFilters()
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

const fetchOwnGalleryItems = async () => {
  pickerLoading.value = true
  try {
    const { data } = await forumService.getOwnGalleryItems(1, 120, false)
    ownGalleryItems.value = ((data || []) as Array<{ id: number; title?: string; imageUrl?: string | null; isDeleted?: boolean }>)
      .filter(item => !item.isDeleted)
      .map(item => ({
        id: item.id,
        title: item.title?.trim() || `Kép #${item.id}`,
        imageUrl: getFullImageUrl(item.imageUrl || ''),
        isDeleted: Boolean(item.isDeleted)
      }))
    pickerLoaded.value = true
  } catch (error) {
    console.error('Saját galéria betöltési hiba', error)
    toastStore.addToast('Nem sikerült betölteni a saját galériádat.', 4000, 'error')
  } finally {
    pickerLoading.value = false
  }
}

const openGalleryPicker = async () => {
  pickerMode.value = 'create'
  pickerOpen.value = true
  if (!pickerLoaded.value) {
    await fetchOwnGalleryItems()
  }
}

const openEditGalleryPicker = async () => {
  pickerMode.value = 'edit'
  pickerOpen.value = true
  if (!pickerLoaded.value) {
    await fetchOwnGalleryItems()
  }
}

const closeGalleryPicker = () => {
  pickerOpen.value = false
}

const selectGalleryItem = (itemId: number) => {
  if (pickerMode.value === 'edit') {
    editSelectedGalleryItemId.value = itemId
  } else {
    selectedGalleryItemId.value = itemId
  }
  closeGalleryPicker()
}

const clearSelectedGalleryItem = () => {
  selectedGalleryItemId.value = null
}

const clearEditSelectedGalleryItem = () => {
  editSelectedGalleryItemId.value = null
}

const openEditPost = (post: {
  id: number
  title: string
  description: string
  attachedGalleryItemId?: number | null
  tags: string[]
}) => {
  editingPostId.value = post.id
  editForm.title = post.title || ''
  editForm.description = post.description || ''
  editForm.tagInput = ''
  editTags.value = [...(post.tags || [])]
  editSelectedGalleryItemId.value = post.attachedGalleryItemId ?? null
  editShowTagSuggestions.value = false
}

const closeEditPost = () => {
  editingPostId.value = null
  editForm.title = ''
  editForm.description = ''
  editForm.tagInput = ''
  editTags.value = []
  editSelectedGalleryItemId.value = null
  editShowTagSuggestions.value = false
}

// Bejegyzés szerkesztés mentése.
const submitEditPost = async () => {
  if (!editingPostId.value || editSaving.value) return

  const title = editForm.title.trim()
  const description = editForm.description.trim()
  if (!title || !description) {
    toastStore.addToast('A cím és leírás kötelező.', 3500, 'warning')
    return
  }

  editSaving.value = true
  try {
    await forumService.updatePost(editingPostId.value, {
      title,
      description,
      attachedGalleryItemId: editSelectedGalleryItemId.value,
      tags: editTags.value
    })

    toastStore.addToast('Bejegyzés frissítve.', 3000, 'success')
    const wasDetailMode = isDetailMode.value
    closeEditPost()
    await (wasDetailMode ? fetchDetail(false) : fetchFeed(false))
  } catch (error) {
    console.error('Forum post frissítési hiba', error)
    toastStore.addToast(getApiErrorMessage(error, 'Nem sikerült frissíteni a bejegyzést.'), 5000, 'error')
  } finally {
    editSaving.value = false
  }
}

// Rekurzív kereső segédfüggvény nested kommentfa elemeihez.
const findCommentById = (comments: ForumComment[], commentId: number): ForumComment | null => {
  for (const comment of comments) {
    if (comment.id === commentId) return comment
    const nested = findCommentById(comment.replies || [], commentId)
    if (nested) return nested
  }
  return null
}

// Új bejegyzés létrehozása és feed frissítése.
const submitCreatePost = async () => {
  if (savingPost.value) return

  const title = createForm.title.trim()
  const description = createForm.description.trim()
  const selectedAttachmentId = selectedGalleryItemId.value

  if (!title || !description) {
    toastStore.addToast('A cím és leírás kötelező.', 3500, 'warning')
    return
  }

  savingPost.value = true
  try {
    await forumService.createPost({
      title,
      description,
      attachedGalleryItemId: selectedAttachmentId,
      tags: createTags.value
    })

    toastStore.addToast('Bejegyzés létrehozva.', 3000, 'success')
    createForm.title = ''
    createForm.description = ''
    createForm.tagInput = ''
    createTags.value = []
    selectedGalleryItemId.value = null
    showTagSuggestions.value = false
    showCreateForm.value = false
    await fetchFeed(false)
  } catch (error) {
    console.error('Forum post létrehozási hiba', error)
    toastStore.addToast(getApiErrorMessage(error, 'Nem sikerült létrehozni a bejegyzést.'), 5000, 'error')
  } finally {
    savingPost.value = false
  }
}

// Moderációs műveletek (törlés, visszaállítás, pin, lock).
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

// Optimista reakciófrissítés a részletnézetben.
const reactPost = async (isLike: boolean) => {
  if (!detail.value) return

  const previous = {
    myReaction: detail.value.myReaction,
    likesCount: detail.value.likesCount,
    dislikesCount: detail.value.dislikesCount
  }

  const delta = applyReactionDelta(previous.myReaction, isLike)
  detail.value.myReaction = delta.nextReaction
  detail.value.likesCount = Math.max(0, detail.value.likesCount + delta.likesDelta)
  detail.value.dislikesCount = Math.max(0, detail.value.dislikesCount + delta.dislikesDelta)

  try {
    await forumService.reactPost(detail.value.id, isLike)
  } catch (error) {
    detail.value.myReaction = previous.myReaction
    detail.value.likesCount = previous.likesCount
    detail.value.dislikesCount = previous.dislikesCount
    console.error('Post reakció hiba', error)
    toastStore.addToast('A reakció mentése nem sikerült.', 3500, 'error')
  }
}

// Optimista reakciófrissítés feed kártyán, majd szinkron a szerver válasszal.
const reactPostPreview = async (payload: { postId: number; isLike: boolean }) => {
  if (reactingPostIds.value[payload.postId]) return

  const targetPost = feedItems.value.find(item => item.id === payload.postId)
  if (!targetPost) return

  const previous = {
    myReaction: targetPost.myReaction,
    likesCount: targetPost.likesCount,
    dislikesCount: targetPost.dislikesCount
  }

  const delta = applyReactionDelta(previous.myReaction, payload.isLike)
  targetPost.myReaction = delta.nextReaction
  targetPost.likesCount = Math.max(0, targetPost.likesCount + delta.likesDelta)
  targetPost.dislikesCount = Math.max(0, targetPost.dislikesCount + delta.dislikesDelta)
  reactingPostIds.value[payload.postId] = true

  try {
    await forumService.reactPost(payload.postId, payload.isLike)
    const { data } = await forumService.getPostById(payload.postId, {
      commentCursor: 0,
      commentPageSize: 1,
      replyPageSize: 1,
      includeDeleted: isAdmin.value && showDeleted.value
    })

    targetPost.likesCount = Number(data?.likesCount ?? targetPost.likesCount)
    targetPost.dislikesCount = Number(data?.dislikesCount ?? targetPost.dislikesCount)
    const syncedReaction = data?.myReaction as boolean | null | undefined
    targetPost.myReaction = syncedReaction === true ? true : syncedReaction === false ? false : null
  } catch (error) {
    targetPost.myReaction = previous.myReaction
    targetPost.likesCount = previous.likesCount
    targetPost.dislikesCount = previous.dislikesCount
    console.error('Preview post reakció hiba', error)
    toastStore.addToast('A reakció mentése nem sikerült.', 3500, 'error')
  } finally {
    reactingPostIds.value[payload.postId] = false
  }
}

// Komment reakcio kezeles visszagorgetheto optimista logikaval.
const reactComment = async (commentId: number, isLike: boolean) => {
  if (!detail.value) return

  const targetComment = findCommentById(detail.value.comments, commentId)
  if (!targetComment) return

  const previous = {
    myReaction: targetComment.myReaction,
    likesCount: targetComment.likesCount,
    dislikesCount: targetComment.dislikesCount
  }

  const delta = applyReactionDelta(previous.myReaction, isLike)
  targetComment.myReaction = delta.nextReaction
  targetComment.likesCount = Math.max(0, targetComment.likesCount + delta.likesDelta)
  targetComment.dislikesCount = Math.max(0, targetComment.dislikesCount + delta.dislikesDelta)

  try {
    await forumService.reactComment(commentId, isLike)
  } catch (error) {
    targetComment.myReaction = previous.myReaction
    targetComment.likesCount = previous.likesCount
    targetComment.dislikesCount = previous.dislikesCount
    console.error('Comment reakció hiba', error)
    toastStore.addToast('A reakció mentése nem sikerült.', 3500, 'error')
  }
}

// Uj gyoker komment kuldese (optimista lokalis beszuras).
const submitComment = async () => {
  if (!detail.value) return
  const message = detailCommentDraft.value.trim()
  if (!message) return

  try {
    const { data } = await forumService.addComment(detail.value.id, { message })
    const createdId = extractCreatedCommentId(data)
    const localComment = createLocalComment(message, createdId, null)

    detail.value.comments = [localComment, ...detail.value.comments]
    replyVisibility.value[localComment.id] = true
    detailCommentDraft.value = ''
  } catch (error) {
    console.error('Comment küldési hiba', error)
    toastStore.addToast('A hozzászólás küldése nem sikerült.', 3500, 'error')
  }
}

// Valasz komment kuldese parent hozzarendelessel.
const submitReply = async (parentCommentId: number) => {
  if (!detail.value) return
  const message = replyDraft.value.trim()
  if (!message) return

  const parentComment = findCommentById(detail.value.comments, parentCommentId)
  if (!parentComment) return

  try {
    const { data } = await forumService.addComment(detail.value.id, { message, parentCommentId })
    const createdId = extractCreatedCommentId(data)
    const localReply = createLocalComment(message, createdId, parentCommentId)

    parentComment.replies = [localReply, ...parentComment.replies]
    parentComment.hasMoreReplies = false
    replyVisibility.value[parentCommentId] = true
    replyDraft.value = ''
    replyingToComment.value = null
  } catch (error) {
    console.error('Reply küldési hiba', error)
    toastStore.addToast('A válasz küldése nem sikerült.', 3500, 'error')
  }
}

// Komment moderáció (törlés/visszaállítás) + újratöltés.
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

// További válaszok kurzoros lapozása egy adott kommenthez.
const loadMoreReplies = async (comment: ForumComment) => {
  if (loadingReplies.value[comment.id]) return

  loadingReplies.value[comment.id] = true
  try {
    const { data } = await forumService.getReplies(
      comment.id,
      comment.nextReplyCursor || comment.replies.length,
      5,
      isAdmin.value && showDeleted.value
    )
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

// Szűrők változásakor automatikus feed újratöltés.
watch(
  () => [filters.sort, filters.search, filters.maxAgeDays, showDeleted.value, JSON.stringify(filters.selectedRoleIds), JSON.stringify(filters.selectedTags)],
  async () => {
    if (!isForumListRoute.value) return
    await fetchFeed(false)
  }
)

// Soft-deleted kapcsolo hatasa detail nezetben azonnal ujraszamitando.
watch(
  () => showDeleted.value,
  async () => {
    if (isDetailMode.value) {
      await fetchDetail(false)
    }
  }
)

// Dinamikus route param valtas (masik post) eseten detail refresh.
watch(
  () => route.params.id,
  async () => {
    if (!isDetailMode.value) return
    await fetchDetail(false)
  }
)

// Komponens inditasa: mode alapjan feed vagy detail inicializalas.
onMounted(async () => {
  if (isDetailMode.value) {
    try {
      await fetchDetail(false)
    } catch (error) {
      console.error('Forum detail inicializálási hiba', error)
      toastStore.addToast('A bejegyzés nem tölthető be.', 4000, 'error')
    }
    return
  }

  try {
    await loadMeta()
    syncFiltersFromQuery()
    await fetchFeed(false)
  } catch (error) {
    console.error('Forum inicializálási hiba', error)
    toastStore.addToast('A fórum nem inicializálható.', 4000, 'error')
  }
})
</script>

<template>
  <InnerPageLayout v-if="!isDetailMode">
    <div class="mb-6 flex flex-wrap items-baseline justify-between gap-2">
      <PageTitle title="Fórum" />
      <p class="text-earth-200/90">Közösségi beszélgetések, tippek és kérdések.</p>
    </div>

    <div class="mb-4 flex lg:hidden">
      <button
        @click="showFilters = !showFilters"
        class="flex items-center gap-2 px-4 py-2 bg-earth-800/80 border border-earth-100/20 rounded-lg text-earth-50 hover:bg-earth-700/80 transition-colors"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" />
        </svg>
        {{ showFilters ? 'Szűrők elrejtése' : 'Szűrők megjelenítése' }}
      </button>
    </div>

    <Transition name="slide">
      <div
        v-if="showFilters"
        class="mb-4 rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4 flex flex-col gap-5 lg:hidden"
      >
        <ForumFiltersSidebar
          :sort-value="filters.sort"
          :search-value="filters.search"
          :max-age-days-value="filters.maxAgeDays"
          :sort-options="sortOptions"
          :roles="roles"
          :all-tags="allTags"
          :selected-role-set="selectedRoleSet"
          :selected-tag-set="selectedTagSet"
          :is-admin="isAdmin"
          :show-deleted="showDeleted"
          @update:sort-value="filters.sort = $event; showFilters = false"
          @update:search-value="filters.search = $event"
          @update:max-age-days-value="filters.maxAgeDays = $event"
          @toggle-role="toggleRole"
          @toggle-tag="toggleTag"
          @update:show-deleted="showDeleted = $event"
        />
      </div>
    </Transition>

    <div class="grid grid-cols-1 lg:grid-cols-[1fr_3fr] gap-6">
      <ForumFiltersSidebar
        class="hidden lg:block"
        :sort-value="filters.sort"
        :search-value="filters.search"
        :max-age-days-value="filters.maxAgeDays"
        :sort-options="sortOptions"
        :roles="roles"
        :all-tags="allTags"
        :selected-role-set="selectedRoleSet"
        :selected-tag-set="selectedTagSet"
        :is-admin="isAdmin"
        :show-deleted="showDeleted"
        @update:sort-value="filters.sort = $event"
        @update:search-value="filters.search = $event"
        @update:max-age-days-value="filters.maxAgeDays = $event"
        @toggle-role="toggleRole"
        @toggle-tag="toggleTag"
        @update:show-deleted="showDeleted = $event"
      />

      <section class="space-y-4">
        <ForumCreatePostPanel
          :show-create-form="showCreateForm"
          :title-value="createForm.title"
          :description-value="createForm.description"
          :tag-input-value="createForm.tagInput"
          :create-tags="createTags"
          :selected-gallery-item="selectedGalleryItem"
          :saving-post="savingPost"
          :show-tag-suggestions="showTagSuggestions"
          :filtered-tag-suggestions="filteredTagSuggestions"
          @update:title-value="createForm.title = $event"
          @update:description-value="createForm.description = $event"
          @update:tag-input-value="createForm.tagInput = $event"
          @toggle-form="showCreateForm = !showCreateForm"
          @open-gallery-picker="openGalleryPicker"
          @clear-selected-gallery-item="clearSelectedGalleryItem"
          @add-tag="addCreateTag"
          @remove-tag="removeCreateTag"
          @select-tag-suggestion="selectCreateTagSuggestion"
          @handle-tag-input="handleCreateTagInput"
          @update:show-tag-suggestions="showTagSuggestions = $event"
          @submit="submitCreatePost"
        />

        <ForumFeedPostCard
          v-for="post in feedItems"
          :key="post.id"
          :post="post"
          :is-authenticated="authStore.isAuthenticated"
          :is-reacting="Boolean(reactingPostIds[post.id])"
          :get-full-image-url="getFullImageUrl"
          :excerpt="excerpt"
          :format-date-time="formatDateTime"
          @open-detail="openDetail"
          @react-post-preview="reactPostPreview"
          @open-edit="openEditPost"
          @delete-post="deletePost"
          @restore-post="restorePost"
        />

        <div v-if="!feedLoading && !feedItems.length" class="rounded-xl border border-earth-100/10 bg-earth-900/40 p-6 text-earth-200">
          Nincs találat a kiválasztott szűrőkre.
        </div>

        <div class="flex justify-center py-2">
          <button v-if="feedHasMore" type="button" :disabled="feedLoading" @click="fetchFeed(true)" class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100 disabled:opacity-60">
            {{ feedLoading ? 'Betöltés...' : 'További bejegyzések' }}
          </button>
        </div>
      </section>
    </div>
  </InnerPageLayout>

  <InnerPageLayout v-else>
    <ForumDetailPanel
      :detail="detail"
      :detail-loading="detailLoading"
      :detail-comment-draft="detailCommentDraft"
      :replying-to-comment="replyingToComment"
      :reply-draft="replyDraft"
      :reply-visibility="replyVisibility"
      :loading-replies="loadingReplies"
      :loading-more-comments="loadingMoreComments"
      :get-full-image-url="getFullImageUrl"
      :format-date-time="formatDateTime"
      @go-back="goBackToList"
      @toggle-pinned="togglePinned"
      @toggle-locked="toggleLocked"
      @open-edit="openEditPost"
      @delete-post="deletePost"
      @restore-post="restorePost"
      @react-post="reactPost"
      @update:detail-comment-draft="detailCommentDraft = $event"
      @submit-comment="submitComment"
      @react-comment="reactComment"
      @delete-comment="deleteComment"
      @restore-comment="restoreComment"
      @toggle-replies="toggleReplies"
      @toggle-reply-form="replyingToComment = replyingToComment === $event ? null : $event"
      @update:reply-draft="replyDraft = $event"
      @submit-reply="submitReply"
      @load-more-replies="loadMoreReplies"
      @fetch-more-comments="fetchDetail(true)"
    />
  </InnerPageLayout>

  <ForumGalleryPickerModal
    :open="pickerOpen"
    :loading="pickerLoading"
    :items="ownGalleryItems"
    :selected-item-id="pickerMode === 'edit' ? editSelectedGalleryItemId : selectedGalleryItemId"
    @close="closeGalleryPicker"
    @select-item="selectGalleryItem"
  />

  <ForumEditPostModal
    :editing-post-id="editingPostId"
    :title-value="editForm.title"
    :description-value="editForm.description"
    :tag-input-value="editForm.tagInput"
    :edit-tags="editTags"
    :edit-saving="editSaving"
    :edit-show-tag-suggestions="editShowTagSuggestions"
    :filtered-edit-tag-suggestions="filteredEditTagSuggestions"
    :selected-edit-gallery-item="selectedEditGalleryItem"
    @update:title-value="editForm.title = $event"
    @update:description-value="editForm.description = $event"
    @update:tag-input-value="editForm.tagInput = $event"
    @close="closeEditPost"
    @open-gallery-picker="openEditGalleryPicker"
    @clear-selected-gallery-item="clearEditSelectedGalleryItem"
    @add-tag="addEditTag"
    @remove-tag="removeEditTag"
    @select-tag-suggestion="selectEditTagSuggestion"
    @handle-tag-input="handleEditTagInput"
    @update:edit-show-tag-suggestions="editShowTagSuggestions = $event"
    @submit="submitEditPost"
  />
</template>

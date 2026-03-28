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
  attachedGalleryItemId?: number | null
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
  attachedGalleryItemId?: number | null
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

interface OwnGalleryItem {
  id: number
  title: string
  imageUrl: string
  isDeleted: boolean
}

const props = withDefaults(defineProps<{ mode?: 'list' | 'detail' }>(), {
  mode: 'list'
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const isDetailMode = computed(() => props.mode === 'detail')
const isForumListRoute = computed(() => route.name === 'forum' && !isDetailMode.value)
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
const showDeleted = ref(false)

const feedItems = ref<ForumFeedItem[]>([])
const feedPage = ref(1)
const feedHasMore = ref(false)
const feedLoading = ref(false)

const showCreateForm = ref(false)
const createForm = reactive({
  title: '',
  description: '',
  tagInput: ''
})
const createTags = ref<string[]>([])
const savingPost = ref(false)
const showTagSuggestions = ref(false)

const editingPostId = ref<number | null>(null)
const editForm = reactive({
  title: '',
  description: '',
  tagInput: ''
})
const editTags = ref<string[]>([])
const editSaving = ref(false)
const editShowTagSuggestions = ref(false)

const ownGalleryItems = ref<OwnGalleryItem[]>([])
const selectedGalleryItemId = ref<number | null>(null)
const editSelectedGalleryItemId = ref<number | null>(null)
const pickerMode = ref<'create' | 'edit'>('create')
const pickerOpen = ref(false)
const pickerLoading = ref(false)
const pickerLoaded = ref(false)

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

const parseIntQuery = (value: unknown, fallback: number) => {
  const n = Number(value)
  return Number.isFinite(n) ? n : fallback
}

const normalizeText = (value: string) => value.trim()

const getFullImageUrl = (url?: string | null) => {
  if (!url) return ''
  if (url.startsWith('http')) return url
  const baseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace('/api', '') || 'https://localhost:44351'
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

const getCurrentUserId = () => {
  const raw = authStore.felhasznalo?.id
  const parsed = Number(raw)
  return Number.isFinite(parsed) ? parsed : 0
}

const getCurrentUserName = () => {
  const profile = authStore.profilAdatok || {}
  const vezetekNev = String(profile.vezetekNev || '').trim()
  const keresztNev = String(profile.keresztNev || '').trim()
  const fullName = `${vezetekNev} ${keresztNev}`.trim()
  if (fullName) return fullName
  return String(authStore.felhasznalo?.felhasznaloNev || 'Te')
}

const getCurrentUserRoleName = () => String(authStore.profilAdatok?.roleName || '').trim()

const getCurrentUserProfileImageUrl = () => {
  const fileName = String(authStore.profilAdatok?.imgString || '').trim()
  return fileName ? getFullImageUrl(`/resources/profiles/${fileName}`) : null
}

const extractCreatedCommentId = (responseData: unknown) => {
  if (!responseData || typeof responseData !== 'object') return undefined
  const candidate = responseData as { id?: unknown; Id?: unknown }
  const numeric = Number(candidate.id ?? candidate.Id)
  return Number.isFinite(numeric) ? numeric : undefined
}

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

const loadMeta = async () => {
  const [roleRes, tagRes] = await Promise.all([authService.getRoles(), authService.GetCimekek()])
  roles.value = (roleRes.data || []).map((role: { id: number; name: string }) => ({
    id: role.id,
    name: (role.name || '').trim()
  }))

  allTags.value = (tagRes.data || [])
    .map((t: string) => t.trim())
    .filter(Boolean)
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
      tagNames: filters.selectedTags,
      includeDeleted: isAdmin.value && showDeleted.value
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

const applyReactionDelta = (current: boolean | null | undefined, target: boolean) => {
  if (current === target) {
    return {
      nextReaction: null as boolean | null,
      likesDelta: target ? -1 : 0,
      dislikesDelta: target ? 0 : -1
    }
  }

  if (current === null || current === undefined) {
    return {
      nextReaction: target,
      likesDelta: target ? 1 : 0,
      dislikesDelta: target ? 0 : 1
    }
  }

  return {
    nextReaction: target,
    likesDelta: target ? 1 : -1,
    dislikesDelta: target ? -1 : 1
  }
}

const findCommentById = (comments: ForumComment[], commentId: number): ForumComment | null => {
  for (const comment of comments) {
    if (comment.id === commentId) return comment
    const nested = findCommentById(comment.replies || [], commentId)
    if (nested) return nested
  }
  return null
}

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

watch(
  () => [filters.sort, filters.search, filters.maxAgeDays, showDeleted.value, JSON.stringify(filters.selectedRoleIds), JSON.stringify(filters.selectedTags)],
  async () => {
    if (!isForumListRoute.value) return
    await fetchFeed(false)
  }
)

watch(
  () => showDeleted.value,
  async () => {
    if (isDetailMode.value) {
      await fetchDetail(false)
    }
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

        <div v-if="isAdmin">
          <label class="flex items-center gap-2 text-earth-200 text-sm">
            <input v-model="showDeleted" type="checkbox" />
            Törölt elemek megjelenítése
          </label>
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

            <div class="rounded-lg border border-earth-100/10 bg-earth-800/60 p-3 space-y-2">
              <p class="text-sm text-earth-200">Csatolt saját galéria kép (opcionális)</p>
              <p v-if="selectedGalleryItem" class="text-sm text-earth-100">
                Kiválasztva: <span class="font-semibold">{{ selectedGalleryItem.title }}</span>
              </p>
              <p v-else class="text-sm text-earth-300">Nincs kép kiválasztva</p>
              <div class="flex flex-wrap gap-2">
                <button type="button" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100" @click="openGalleryPicker">Kép választó megnyitása</button>
                <button v-if="selectedGalleryItem" type="button" class="px-3 py-2 rounded-lg bg-red-700/80 text-red-100" @click="clearSelectedGalleryItem">Kiválasztás törlése</button>
              </div>
            </div>

            <div class="relative flex gap-2">
              <div class="flex-1">
                <input
                  v-model="createForm.tagInput"
                  type="text"
                  placeholder="Címke"
                  class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
                  @keydown.enter.prevent="addCreateTag"
                  @focus="showTagSuggestions = true"
                  @blur="showTagSuggestions = false"
                  @input="handleCreateTagInput"
                />
                <ul
                  v-if="showTagSuggestions && filteredTagSuggestions.length"
                  class="absolute left-0 right-0 top-11 z-20 rounded-lg border border-earth-600 bg-earth-800 shadow-xl overflow-hidden max-h-44 overflow-y-auto"
                >
                  <li
                    v-for="suggestion in filteredTagSuggestions"
                    :key="suggestion"
                    class="px-3 py-2 text-sm text-earth-100 hover:bg-earth-700 cursor-pointer"
                    @mousedown.prevent="selectCreateTagSuggestion(suggestion)"
                  >
                    {{ suggestion }}
                  </li>
                </ul>
              </div>
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
                  <button v-if="post.canEdit" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-blue-700/80 text-blue-100" @click="openEditPost(post)">Szerkesztés</button>
                  <button v-if="post.canDelete" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-red-700/80 text-red-100" @click="deletePost(post.id)">Törlés</button>
                  <button v-if="post.canRestore" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-green-700/80 text-green-100" @click="restorePost(post.id)">Visszaállítás</button>
                </div>
              </div>
            </div>
          </div>
        </article>

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
              <button v-if="detail.canEdit" type="button" class="px-3 py-1 rounded-lg bg-blue-700/80 text-blue-100" @click="openEditPost(detail)">Szerkesztés</button>
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
            <button type="button" class="px-3 py-1.5 rounded-lg text-green-100" :class="detail.myReaction === true ? 'bg-green-600' : 'bg-green-700/70'" @click="reactPost(true)">👍 {{ detail.likesCount }}</button>
            <button type="button" class="px-3 py-1.5 rounded-lg text-red-100" :class="detail.myReaction === false ? 'bg-red-600' : 'bg-red-700/70'" @click="reactPost(false)">👎 {{ detail.dislikesCount }}</button>
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
                <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="comment.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="reactComment(comment.id, true)">👍 {{ comment.likesCount }}</button>
                <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="comment.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="reactComment(comment.id, false)">👎 {{ comment.dislikesCount }}</button>
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
                  <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="reply.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="reactComment(reply.id, true)">👍 {{ reply.likesCount }}</button>
                  <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="reply.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="reactComment(reply.id, false)">👎 {{ reply.dislikesCount }}</button>
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

    <div v-else class="rounded-xl border border-earth-100/10 bg-earth-900/40 p-6 text-earth-200">
      A bejegyzés nem található vagy jelenleg nem betölthető.
    </div>
  </InnerPageLayout>

  <div v-if="pickerOpen" class="fixed inset-0 z-[70] bg-black/60 flex items-center justify-center p-4" @click.self="closeGalleryPicker">
    <div class="w-full max-w-5xl max-h-[85vh] overflow-hidden rounded-2xl border border-earth-100/10 bg-earth-900/95 shadow-2xl">
      <div class="flex items-center justify-between px-4 py-3 border-b border-earth-100/10">
        <h3 class="text-lg font-semibold text-earth-50">Saját galéria képek</h3>
        <button type="button" class="text-earth-300 hover:text-earth-50" @click="closeGalleryPicker">Bezárás</button>
      </div>

      <div class="p-4 overflow-y-auto max-h-[70vh]">
        <div v-if="pickerLoading" class="text-earth-200">Képek betöltése...</div>
        <div v-else-if="!ownGalleryItems.length" class="text-earth-200">Nincs elérhető saját galéria képed.</div>
        <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
          <button
            v-for="item in ownGalleryItems"
            :key="item.id"
            type="button"
            class="group text-left rounded-xl overflow-hidden border border-earth-100/10 bg-earth-800/70 hover:border-green-400/60 transition-colors"
            :class="selectedGalleryItemId === item.id ? 'ring-2 ring-green-400' : ''"
            @click="selectGalleryItem(item.id)"
          >
            <img :src="item.imageUrl" :alt="item.title" class="w-full h-32 object-cover" />
            <div class="p-2">
              <p class="text-sm text-earth-100 truncate">{{ item.title }}</p>
            </div>
          </button>
        </div>
      </div>
    </div>
  </div>

  <div v-if="editingPostId" class="fixed inset-0 z-50 bg-black/70 flex items-center justify-center p-4" @click.self="closeEditPost">
    <div class="w-full max-w-3xl max-h-[90vh] overflow-y-auto rounded-2xl border border-earth-100/10 bg-earth-900 p-5 space-y-4 shadow-2xl">
      <div class="flex items-center justify-between">
        <h3 class="text-xl font-semibold text-earth-50">Bejegyzés szerkesztése</h3>
        <button type="button" class="text-earth-300 hover:text-earth-50" @click="closeEditPost">Bezárás</button>
      </div>

      <input v-model="editForm.title" type="text" placeholder="Cím"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10" />
      <textarea v-model="editForm.description" rows="5" placeholder="Leírás"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10" />

      <div class="rounded-lg border border-earth-100/10 bg-earth-800/60 p-3 space-y-2">
        <p class="text-sm text-earth-200">Csatolt saját galéria kép (opcionális)</p>
        <p v-if="selectedEditGalleryItem" class="text-sm text-earth-100">
          Kiválasztva: <span class="font-semibold">{{ selectedEditGalleryItem.title }}</span>
        </p>
        <p v-else class="text-sm text-earth-300">Nincs kép kiválasztva</p>
        <div class="flex flex-wrap gap-2">
          <button type="button" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100" @click="openEditGalleryPicker">Kép választó megnyitása</button>
          <button v-if="selectedEditGalleryItem" type="button" class="px-3 py-2 rounded-lg bg-red-700/80 text-red-100" @click="clearEditSelectedGalleryItem">Kiválasztás törlése</button>
        </div>
      </div>

      <div class="relative flex gap-2">
        <div class="flex-1">
          <input
            v-model="editForm.tagInput"
            type="text"
            placeholder="Címke"
            class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
            @keydown.enter.prevent="addEditTag"
            @focus="editShowTagSuggestions = true"
            @blur="editShowTagSuggestions = false"
            @input="handleEditTagInput"
          />
          <ul
            v-if="editShowTagSuggestions && filteredEditTagSuggestions.length"
            class="absolute left-0 right-0 top-11 z-20 rounded-lg border border-earth-600 bg-earth-800 shadow-xl overflow-hidden max-h-44 overflow-y-auto"
          >
            <li
              v-for="suggestion in filteredEditTagSuggestions"
              :key="suggestion"
              class="px-3 py-2 text-sm text-earth-100 hover:bg-earth-700 cursor-pointer"
              @mousedown.prevent="selectEditTagSuggestion(suggestion)"
            >
              {{ suggestion }}
            </li>
          </ul>
        </div>
        <button type="button" @click="addEditTag" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100">Hozzáad</button>
      </div>

      <div class="flex flex-wrap gap-2">
        <button
          v-for="tag in editTags"
          :key="tag"
          type="button"
          class="px-2.5 py-1 rounded-full text-xs bg-blue-500/20 border border-blue-400 text-blue-100"
          @click="removeEditTag(tag)"
        >
          {{ tag }} ×
        </button>
      </div>

      <div class="flex items-center justify-end gap-2 pt-2">
        <button type="button" class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100" @click="closeEditPost">Mégse</button>
        <button type="button" class="px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-white disabled:opacity-60" :disabled="editSaving" @click="submitEditPost">
          {{ editSaving ? 'Mentés...' : 'Mentés' }}
        </button>
      </div>
    </div>
  </div>
</template>

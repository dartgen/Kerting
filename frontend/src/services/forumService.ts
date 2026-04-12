import api from './axios'
import type { ForumSort, ForumFeedFilters, UpsertForumPostPayload, AddForumCommentPayload } from '@/types/forum'

// Feed szűrőkből URLSearchParams építése.
// A több-értékű mezőket (roleIds, tagNames) külön paraméterként küldjük, hogy a backend korrektül tudja parse-olni.
const buildForumFeedQuery = (filters: ForumFeedFilters) => {
  const params = new URLSearchParams()

  if (typeof filters.page === 'number') params.append('page', String(filters.page))
  if (typeof filters.pageSize === 'number') params.append('pageSize', String(filters.pageSize))
  if (filters.sort) params.append('sort', filters.sort)
  if (typeof filters.search === 'string' && filters.search.trim()) params.append('search', filters.search)
  if (typeof filters.maxAgeDays === 'number') params.append('maxAgeDays', String(filters.maxAgeDays))
  if (typeof filters.includeDeleted === 'boolean') params.append('includeDeleted', String(filters.includeDeleted))

  for (const roleId of filters.roleIds || []) {
    params.append('roleIds', String(roleId))
  }

  for (const tagName of filters.tagNames || []) {
    if (!tagName?.trim()) continue
    params.append('tagNames', tagName)
  }

  return params
}

// Fórum API szolgáltatás.
export const forumService = {
  // Fórum feed lapozott/szűrt listázása.
  getFeed(filters: ForumFeedFilters) {
    return api.get('/Forum/feed', {
      params: buildForumFeedQuery(filters),
      paramsSerializer: {
        serialize: (params) => params.toString()
      }
    })
  },

  // Egy poszt részletes adatlapja kommentekkel.
  getPostById(
    postId: number,
    params?: {
      commentCursor?: number
      commentPageSize?: number
      replyPageSize?: number
      includeDeleted?: boolean
    }
  ) {
    return api.get(`/Forum/${postId}`, { params })
  },

  // Komment válaszok cursor alapú lapozással.
  getReplies(commentId: number, cursor = 0, pageSize = 10, includeDeleted = false) {
    return api.get(`/Forum/comment/${commentId}/replies`, {
      params: { cursor, pageSize, includeDeleted }
    })
  },

  // Saját gallery elemek fórum csatoláshoz.
  getOwnGalleryItems(page = 1, pageSize = 100, includeDeleted = false) {
    return api.get('/Gallery/mine', {
      params: { page, pageSize, includeDeleted }
    })
  },

  // Poszt létrehozás/szerkesztés/törlés/visszaállítás.
  createPost(payload: UpsertForumPostPayload) {
    return api.post('/Forum', payload)
  },

  updatePost(postId: number, payload: UpsertForumPostPayload) {
    return api.patch(`/Forum/${postId}`, payload)
  },

  deletePost(postId: number) {
    return api.delete(`/Forum/${postId}`)
  },

  restorePost(postId: number) {
    return api.patch(`/Forum/${postId}/restore`)
  },

  // Moderációs állapotok (pin/lock).
  setPinned(postId: number, isPinned: boolean) {
    return api.patch(`/Forum/${postId}/pin`, null, { params: { isPinned } })
  },

  setLocked(postId: number, isLocked: boolean, reason?: string) {
    return api.patch(`/Forum/${postId}/lock`, { isLocked, reason })
  },

  // Reakciók posztra.
  reactPost(postId: number, isLike: boolean) {
    return api.post(`/Forum/${postId}/react`, null, { params: { isLike } })
  },

  // Komment létrehozás/törlés/visszaállítás + reakció.
  addComment(postId: number, payload: AddForumCommentPayload) {
    return api.post(`/Forum/${postId}/comment`, payload)
  },

  deleteComment(commentId: number) {
    return api.delete(`/Forum/comment/${commentId}`)
  },

  restoreComment(commentId: number) {
    return api.patch(`/Forum/comment/${commentId}/restore`)
  },

  reactComment(commentId: number, isLike: boolean) {
    return api.post(`/Forum/comment/${commentId}/react`, null, { params: { isLike } })
  }
}

// Típusok újraexportja a fogyasztó komponensek egyszerűbb importjához.
export type { ForumSort, ForumFeedFilters, UpsertForumPostPayload, AddForumCommentPayload } from '@/types/forum'

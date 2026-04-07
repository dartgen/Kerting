import api from './axios'
import type { ForumSort, ForumFeedFilters, UpsertForumPostPayload, AddForumCommentPayload } from '@/types/forum'

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

export const forumService = {
  getFeed(filters: ForumFeedFilters) {
    return api.get('/Forum/feed', {
      params: buildForumFeedQuery(filters),
      paramsSerializer: {
        serialize: (params) => params.toString()
      }
    })
  },

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

  getReplies(commentId: number, cursor = 0, pageSize = 10, includeDeleted = false) {
    return api.get(`/Forum/comment/${commentId}/replies`, {
      params: { cursor, pageSize, includeDeleted }
    })
  },

  getOwnGalleryItems(page = 1, pageSize = 100, includeDeleted = false) {
    return api.get('/Gallery/mine', {
      params: { page, pageSize, includeDeleted }
    })
  },

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

  setPinned(postId: number, isPinned: boolean) {
    return api.patch(`/Forum/${postId}/pin`, null, { params: { isPinned } })
  },

  setLocked(postId: number, isLocked: boolean, reason?: string) {
    return api.patch(`/Forum/${postId}/lock`, { isLocked, reason })
  },

  reactPost(postId: number, isLike: boolean) {
    return api.post(`/Forum/${postId}/react`, null, { params: { isLike } })
  },

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

// Re-export types for convenience
export type { ForumSort, ForumFeedFilters, UpsertForumPostPayload, AddForumCommentPayload } from '@/types/forum'

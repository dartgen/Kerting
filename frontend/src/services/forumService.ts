import api from './axios'

export type ForumSort = 'latest' | 'oldest' | 'netdesc' | 'netasc'

export interface ForumFeedFilters {
  page?: number
  pageSize?: number
  sort?: ForumSort
  search?: string
  maxAgeDays?: number
  roleIds?: number[]
  tagNames?: string[]
}

export interface UpsertForumPostPayload {
  title: string
  description: string
  attachedGalleryItemId?: number | null
  tags?: string[]
}

export interface AddForumCommentPayload {
  message: string
  parentCommentId?: number | null
}

export const forumService = {
  getFeed(filters: ForumFeedFilters) {
    return api.get('/Forum/feed', { params: filters })
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

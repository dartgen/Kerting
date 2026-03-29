import api from './axios'

export interface AddUserReviewPayload {
  parentReviewId?: number | null
  rating?: number | null
  message: string
}

export const userReviewService = {
  getReviews(targetUserId: string | number) {
    return api.get(`/UserReview/${targetUserId}`)
  },

  addReview(targetUserId: string | number, payload: AddUserReviewPayload) {
    return api.post(`/UserReview/${targetUserId}`, payload)
  },

  reactReview(reviewId: number, isLike: boolean) {
    return api.post(`/UserReview/${reviewId}/react`, null, { params: { isLike } })
  },

  deleteReview(reviewId: number) {
    return api.delete(`/UserReview/${reviewId}`)
  },

  // ÚJ FÜGGVÉNY A VISSZAÁLLÍTÁSHOZ:
  restoreReview(reviewId: number) {
    return api.patch(`/UserReview/${reviewId}/restore`)
  }
}

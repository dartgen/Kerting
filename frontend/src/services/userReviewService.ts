import api from './axios'
import type { AddUserReviewPayload } from '@/types/user-review'

// User review service:
// Értékelések listázása, létrehozása, reakciók és moderációs műveletek.
export const userReviewService = {
  // Egy felhasználóhoz tartozó értékelés-folyam lekérése.
  getReviews(targetUserId: string | number) {
    return api.get(`/UserReview/${targetUserId}`)
  },

  // Új értékelés vagy válasz létrehozása.
  addReview(targetUserId: string | number, payload: AddUserReviewPayload) {
    return api.post(`/UserReview/${targetUserId}`, payload)
  },

  // Like/dislike reakció kapcsolása.
  reactReview(reviewId: number, isLike: boolean) {
    return api.post(`/UserReview/${reviewId}/react`, null, { params: { isLike } })
  },

  // Soft/hard törlés backend szabály szerint.
  deleteReview(reviewId: number) {
    return api.delete(`/UserReview/${reviewId}`)
  },

  // Törölt értékelés admin visszaállítása.
  restoreReview(reviewId: number) {
    return api.patch(`/UserReview/${reviewId}/restore`)
  }
}

// Típus újraexport kényelmes importhoz.
export type { AddUserReviewPayload } from '@/types/user-review'

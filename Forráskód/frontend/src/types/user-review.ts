// Felhasználói értékelés létrehozó kérésadat:
// lehet önálló review vagy meglévő review-ra adott válasz (parentReviewId).
export interface AddUserReviewPayload {
  parentReviewId?: number | null
  rating?: number | null
  message: string
}

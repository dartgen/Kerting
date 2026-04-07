export interface AddUserReviewPayload {
  parentReviewId?: number | null
  rating?: number | null
  message: string
}

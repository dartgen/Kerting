export interface ForumFeedItem {
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

export interface ForumComment {
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

export interface ForumDetail {
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

export interface OwnGalleryItem {
  id: number
  title: string
  imageUrl: string
  isDeleted: boolean
}


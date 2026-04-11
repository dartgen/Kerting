export interface WorkUserSummary {
  id: number;
  vezetekNev?: string;
  keresztNev?: string;
  telefon?: string;
  email?: string;
  telepules?: string;
  roleId?: number;
  imgString?: string;
}

export interface WorkTagLink {
  tag?: {
    activity?: string;
  };
}

export interface WorkFilters {
  priceMin?: number;
  priceMax?: number;
  createdFrom?: string;
  createdTo?: string;
  targetAudience?: string;
  status?: string[];
}

export interface WorkImageMetadataUpdate {
  isShowcase?: boolean;
  relatedImageId?: number | null;
}

export interface Work {
  id?: number;
  authorId?: number;
  author?: WorkUserSummary;
  targetAudience: string;
  title: string;
  description: string;
  basePrice?: number;
  status?: string;
  createdAtUtc?: string;
  updatedAtUtc?: string;
  applicants?: WorkApplicant[];
  todos?: WorkTodo[];
  images?: WorkImage[];
  cimkek?: string[];
  tags?: WorkTagLink[];
  isCurrentUserRelated?: boolean;
}

export interface WorkApplicant {
  id?: number;
  workId: number;
  userId: number;
  user?: WorkUserSummary;
  offeredPrice?: number;
  status: string;
  createdAtUtc?: string;
}

export interface WorkTodo {
  id?: number;
  workId?: number;
  title: string;
  isDone?: boolean;
  doneMessage?: string;
  doneByUserId?: number;
}

export interface WorkImage {
  id?: number;
  workId: number;
  imageUrl: string;
  isShowcase: boolean;
  uploadedAtUtc?: string;
}

export interface FeaturedWork {
  id?: number;
  workId: number;
  work?: Work;
  featuredAtUtc?: string;
}

export interface PaginatedWorks {
  items: Work[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

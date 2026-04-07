export interface Work {
  id?: number;
  authorId?: number;
  author?: any;
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
  tags?: { tag: { activity: string } }[];
}

export interface WorkApplicant {
  id?: number;
  workId: number;
  userId: number;
  user?: any;
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

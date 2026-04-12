// Rövid felhasználói adatmodell work nézetekhez és listákhoz.
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

// Backend címke-kapcsoló struktúra (tag link + activity név).
export interface WorkTagLink {
  tag?: {
    activity?: string;
  };
}

// Work lista szűrők kliens oldali paraméterezéshez.
export interface WorkFilters {
  priceMin?: number;
  priceMax?: number;
  createdFrom?: string;
  createdTo?: string;
  targetAudience?: string;
  status?: string[];
}

// Kép metaadat frissítés (kiemelt kép, párosított before/after kapcsolat).
export interface WorkImageMetadataUpdate {
  isShowcase?: boolean;
  relatedImageId?: number | null;
}

// Fő work entitás a részletes oldalakhoz és szerkesztő felületekhez.
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

// Jelentkezés rekord egy adott munkára.
export interface WorkApplicant {
  id?: number;
  workId: number;
  userId: number;
  user?: WorkUserSummary;
  offeredPrice?: number;
  status: string;
  createdAtUtc?: string;
}

// Work-höz tartozó TODO elem (kis feladatlista).
export interface WorkTodo {
  id?: number;
  workId?: number;
  title: string;
  isDone?: boolean;
  doneMessage?: string;
  doneByUserId?: number;
}

// Work képadat rekord galéria/lista megjelenítéshez.
export interface WorkImage {
  id?: number;
  workId: number;
  imageUrl: string;
  isShowcase: boolean;
  uploadedAtUtc?: string;
}

// Kiemelt work rekord (homepage/admin featured lista).
export interface FeaturedWork {
  id?: number;
  workId: number;
  work?: Work;
  featuredAtUtc?: string;
}

// Lapozott work lista válaszmodell az API kompatibilitáshoz.
export interface PaginatedWorks {
  items: Work[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

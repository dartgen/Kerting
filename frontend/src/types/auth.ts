export interface TokenPayload {
  sub: string;
  Id: string;
  exp?: number;
  jti?: string;
}

export interface AuthenticatedUser {
  id: string;
  felhasznaloNev: string;
}

export interface RoleDto {
  id: number;
  name: string;
}

export interface LoginResponse {
  token: string;
}

export interface FirstLoginResponse {
  isFirstLogin: boolean;
}

export interface CheckUsernameResponse {
  isAvailable?: boolean;
  isTaken?: boolean;
  exists?: boolean;
  message?: string;
}

export interface ProfileSocialLinks {
  facebook?: string;
  instagram?: string;
  tiktok?: string;
}

export interface ProfileBaseFields {
  vezetekNev: string;
  keresztNev: string;
  email: string;
  telepules: string;
  rolam: string;
  roleId: number;
  telefon: string;
  cimkek: string[];
  facebook: string;
  instagram: string;
  tiktok: string;
  emailPublikus: boolean;
  telefonPublikus : boolean;
}

export interface UserProfileResponse extends ProfileBaseFields {
  imgString?: string;
  roleName?: string;
}

export interface PublicProfileResponse extends UserProfileResponse {
  ertekeles?: number;
  ertekelesSzam?: number;
}

export interface FeaturedCarouselProfile {
  slotNo: number;
  userId: number;
  name: string;
  bio: string;
  imgString?: string;
  ertekeles: number;
  ertekelesSzam: number;
}

export interface FeaturedSlotAssignment {
  slotNo: number;
  userId: number;
}

export interface FeaturedAdminUserOption {
  id: number;
  name: string;
}

export interface FeaturedAdminDataResponse {
  slots: FeaturedSlotAssignment[];
  users: FeaturedAdminUserOption[];
}

export interface UpdateProfilePayload extends ProfileBaseFields {
  IMGString?: string;
}

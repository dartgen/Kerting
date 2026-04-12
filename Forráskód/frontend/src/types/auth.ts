// JWT token minimális adattartalma, amelyet a kliens oldalon dekódolunk.
export interface TokenPayload {
  sub: string;
  Id: string;
  exp?: number;
  jti?: string;
}

// A bejelentkezett felhasználóhoz használt könnyített kliens modell.
export interface AuthenticatedUser {
  id: string;
  felhasznaloNev: string;
}

// Jogosultsági szerepkör DTO (backend listaelemekhez és szűréshez).
export interface RoleDto {
  id: number;
  name: string;
}

// Login végpont sikeres válasza.
export interface LoginResponse {
  token: string;
}

// Első belépés ellenőrzésének backend válasza.
export interface FirstLoginResponse {
  isFirstLogin: boolean;
}

// Felhasználónév ellenőrzés különböző backend válaszformátumokkal kompatibilisen.
export interface CheckUsernameResponse {
  isAvailable?: boolean;
  isTaken?: boolean;
  exists?: boolean;
  message?: string;
}

// Közösségi link mezők profilhoz.
export interface ProfileSocialLinks {
  facebook?: string;
  instagram?: string;
  tiktok?: string;
}

// Profil alapmezők közös típusa (szerkesztésnél és lekérdezésnél is használt mezők).
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

// Saját profil lekérdezés válasza, opcionális képpel/szerepnévvel.
export interface UserProfileResponse extends ProfileBaseFields {
  imgString?: string;
  roleName?: string;
  username?: string;
}

// Nyilvános profil modell kiegészítve értékelési összesítőkkel.
export interface PublicProfileResponse extends UserProfileResponse {
  ertekeles?: number;
  ertekelesSzam?: number;
}

// Kiemelt felhasználó kártya típusa a kezdőoldali carouselhez.
export interface FeaturedCarouselProfile {
  slotNo: number;
  userId: number;
  name: string;
  bio: string;
  imgString?: string;
  ertekeles: number;
  ertekelesSzam: number;
}

// Admin kiosztási rekord: melyik slothoz melyik user tartozik.
export interface FeaturedSlotAssignment {
  slotNo: number;
  userId: number;
}

// Admin felhasználóválasztó opciók a slot szerkesztéshez.
export interface FeaturedAdminUserOption {
  id: number;
  name: string;
}

// Admin felület összesített válasza (slotok + választható userek).
export interface FeaturedAdminDataResponse {
  slots: FeaturedSlotAssignment[];
  users: FeaturedAdminUserOption[];
}

// Profil mentés kérésadat: alapmezők + opcionális új képfájl név/base64.
export interface UpdateProfilePayload extends ProfileBaseFields {
  IMGString?: string;
}

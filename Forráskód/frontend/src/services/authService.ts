import apiClient from './axios';
import type {
  CheckUsernameResponse,
  FeaturedAdminDataResponse,
  FeaturedCarouselProfile,
  FeaturedSlotAssignment,
  FirstLoginResponse,
  LoginResponse,
  PublicProfileResponse,
  RoleDto,
  UpdateProfilePayload,
  UserProfileResponse
} from '@/types/auth';

// Az authService egy vekony API kliens reteg az azonositas/profil endpointokhoz.
// A store-ok ebben az allomanyban kozpontositan keresztulrik a backend auth vegpontjait,
// igy a route-oldali komponensek nem endpoint stringeket hasznalnak kozvetlenul.
export const authService = {
  // -----------------------------
  // BEJELENTKEZES / REGISZTRACIO
  // -----------------------------
  bejelentkezes(felhasznaloNev: string, jelszo: string) {
    return apiClient.post<LoginResponse>('/Login', {
      username: felhasznaloNev,
      password: jelszo,
    });
  },
  regisztracio(felhasznaloNev: string, jelszo: string) {
    return apiClient.post('/Register', {
      username: felhasznaloNev,
      password: jelszo,
    });
  },

  // -----------------------------
  // TOKEN KEZELES (KLIENS OLDAL)
  // -----------------------------
  // A token a localStorage-ben tarolodik egyszeru session-megorzes celjabol.
  // Fontos: ez nem helyettesiti a backend szerver oldali ervenyesitest.
  kijelentkezes() {
    localStorage.removeItem('userToken');
  },
  getToken() {
    return localStorage.getItem('userToken');
  },
  isAuthenticated() {
    return !!localStorage.getItem('userToken');
  },

  // ------------------------------------
  // TORZSADATOK: SZEREPKOROK ES CIMKEK
  // ------------------------------------
  getRoles() {
    return apiClient.get<RoleDto[]>('/GetRoles');
  },
  GetCimekek() {
    return apiClient.get<string[]>('/ActivityTag');
  },

  // ------------------------------------
  // REGISZTRÁCIÓ ELŐTTI VALIDÁCIÓK
  // ------------------------------------
  checkUsername(username: string) {
    return apiClient.get<CheckUsernameResponse>(`/CheckUsername?username=${username}`);
  },
  isFirstLogin(id: string) {
    return apiClient.post<FirstLoginResponse>(`/${id}/first-login`);
  },

  // ------------------------------------
  // PROFIL MŰVELETEK
  // ------------------------------------
  // A kérésadat mezőnevei a backend DTO elvárt neveivel egyeznek.
  updateProfile(profilAdatok: UpdateProfilePayload) {
    return apiClient.put('/UpdateMyProfile', {
      vezetekNev: profilAdatok.vezetekNev,
      keresztNev: profilAdatok.keresztNev,
      telefon: profilAdatok.telefon,
      email: profilAdatok.email,
      telepules: profilAdatok.telepules,
      rolam: profilAdatok.rolam,
      roleId: profilAdatok.roleId,
      cimkek: profilAdatok.cimkek,
      imgString: profilAdatok.IMGString,
      facebook: profilAdatok.facebook,
      instagram: profilAdatok.instagram,
      tiktok: profilAdatok.tiktok,
      emailPublikus: profilAdatok.emailPublikus,
      telefonPublikus: profilAdatok.telefonPublikus
    });
  },

  // Saját profil: azonosítás a JWT token alapján, nem URL paraméterből.
  getProfile() {
    return apiClient.get<UserProfileResponse>('/GetMyProfile');
  },

  // Publikus profil: URL-ben kapott user ID alapján.
  getPublicProfile(id: string | number) {
    return apiClient.get<PublicProfileResponse>(`/GetPublicProfile/${id}`);
  },

  // Kiemelt felhasználók a főoldali carouselhez.
  getFeaturedCarouselProfiles() {
    return apiClient.get<FeaturedCarouselProfile[]>('/featured-users');
  },

  // Admin felület: jelenlegi slot kiosztás + választható felhasználók.
  getFeaturedAdminData() {
    return apiClient.get<FeaturedAdminDataResponse>('/featured-users/admin/data');
  },

  // Admin mentés: pontosan 5 slotot vár a backend (1..5 sorrend).
  updateFeaturedSlots(assignments: FeaturedSlotAssignment[]) {
    return apiClient.put('/featured-users/admin/slots', assignments);
  }
};

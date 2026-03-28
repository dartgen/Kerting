import apiClient from './axios';
import type {
  CheckUsernameResponse,
  FirstLoginResponse,
  LoginResponse,
  PublicProfileResponse,
  RoleDto,
  UpdateProfilePayload,
  UserProfileResponse
} from '@/types/auth';

export const authService = {
  // Bejelentkezés és Regisztráció
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

  // Token kezelés
  kijelentkezes() {
    localStorage.removeItem('userToken');
  },
  getToken() {
    return localStorage.getItem('userToken');
  },
  isAuthenticated() {
    return !!localStorage.getItem('userToken');
  },

  // Szerepkörök és Címkék (Tagek) lekérése
  getRoles() {
    return apiClient.get<RoleDto[]>('/GetRoles');
  },
  GetCimekek() {
    return apiClient.get<string[]>('/ActivityTag');
  },

  // Felhasználói validációk
  checkUsername(username: string) {
    return apiClient.get<CheckUsernameResponse>(`/CheckUsername?username=${username}`);
  },
  isFirstLogin(id: string) {
    return apiClient.post<FirstLoginResponse>(`/${id}/first-login`);
  },

  // PROFIL KEZELÉS
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
      tiktok: profilAdatok.tiktok
    });
  },

  // Saját profil lekérése (Tokenből azonosít a backend)
  getProfile() {
    return apiClient.get<UserProfileResponse>('/GetMyProfile');
  },

  // Publikus profil lekérése ID alapján
  getPublicProfile(id: string | number) {
    return apiClient.get<PublicProfileResponse>(`/GetPublicProfile/${id}`);
  }
};

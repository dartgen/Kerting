import apiClient from './axios';

// 1. Kiegészített interfész a profilképnek
export interface ProfilAdatokDTO {
  vezetekNev: string;
  keresztNev: string;
  email: string;
  telepules: string;
  rolam: string;
  roleId: number;
  telefon: string;
  cimkek: string[];
  IMGString?: string; // Opcionális, mert nem biztos, hogy mindenkinek van képe
  facebook: string;
  instagram: string;
  tiktok: string;
}

export const authService = {
  // Bejelentkezés és Regisztráció
  bejelentkezes(felhasznaloNev: string, jelszo: string) {
    return apiClient.post('/Login', {
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
    return apiClient.get('/GetRoles');
  },
  GetCimekek() {
    return apiClient.get('/ActivityTag');
  },

  // Felhasználói validációk
  checkUsername(username: string) {
    return apiClient.get(`/CheckUsername?username=${username}`);
  },
  isFirstLogin(id: string) {
    return apiClient.post(`/${id}/first-login`);
  },

  // PROFIL KEZELÉS (Frissítve a kép és címkék elküldésével)
  updateProfile(profilAdatok: ProfilAdatokDTO) {
    return apiClient.put('/UpdateMyProfile', {
      vezetekNev: profilAdatok.vezetekNev,
      keresztNev: profilAdatok.keresztNev,
      telefon: profilAdatok.telefon,
      email: profilAdatok.email,
      telepules: profilAdatok.telepules,
      rolam: profilAdatok.rolam,
      roleId: profilAdatok.roleId,
      cimkek: profilAdatok.cimkek,      // Bekötve!
      imgString: profilAdatok.IMGString, // Bekötve! (C#-ban kisbetűvel várjuk)
      facebook: profilAdatok.facebook,
      instagram: profilAdatok.instagram,
      tiktok: profilAdatok.tiktok,
    });
  },

  // Saját profil lekérése (Tokenből azonosít a backend)
  getProfile() {
    return apiClient.get('/GetMyProfile');
  },

  // ÚJ: Publikus profil lekérése ID alapján (Más profiljának megtekintése)
  getPublicProfile(id: string | number) {
    return apiClient.get(`/GetPublicProfile/${id}`);
  }
};

import apiClient from './axios';

export interface ProfilAdatokDTO {
  vezetekNev: string;
  keresztNev: string;
  email: string;
  telepules: string;
  rolam: string;
  roleId: number;
  telefon: string;
}

export const authService = {
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
  kijelentkezes() {
    localStorage.removeItem('userToken');
  },
  getToken() {
    return localStorage.getItem('userToken');
  },
  isAuthenticated() {
    return !!localStorage.getItem('userToken');
  },getRoles() {
    return apiClient.get('/GetRoles');
  },
  checkUsername(username: string) {
    return apiClient.get(`/CheckUsername?username=${username}`);
  }, isFirstLogin(id: string) {
    return apiClient.post(`/${id}/first-login`);
  }, updateProfile(profilAdatok: ProfilAdatokDTO) {
    return apiClient.put('/UpdateMyProfile', {
      vezetekNev: profilAdatok.vezetekNev,
      keresztNev: profilAdatok.keresztNev,
      telefon: profilAdatok.telefon,
      email: profilAdatok.email,
      telepules: profilAdatok.telepules,
      rolam: profilAdatok.rolam,
      roleId: profilAdatok.roleId,
    });
  }, getProfile() {
    return apiClient.get('/GetMyProfile');
  },
};

import apiClient from './axios';
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
  },

  checkUsername(username: string) {
    return apiClient.get(`/CheckUsername?username=${username}`);
  }, isFirstLogin(id: string) {
    console.log(apiClient.get(`/${id}/first-login`))
    return apiClient.get(`/${id}/first-login`);
  }
};

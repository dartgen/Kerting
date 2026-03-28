import axios from 'axios';

const configuredBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim();
const normalizedBaseUrl = configuredBaseUrl
  ? `${configuredBaseUrl.replace(/\/+$/, '').replace(/\/api$/i, '')}/api`
  : '/api';

const apiClient = axios.create({
  baseURL: normalizedBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Kérés interceptor a token csatolásához
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('userToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Válasz interceptor a hibák kezeléséhez (pl. 401 lejárt token)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {

      // Megnézzük, hogy melyik végpontra ment a kérés.
      // Ha NEM a bejelentkezésre ment, akkor lejárt a tokenünk.
      const requestUrl = error.config.url || '';
      const isLoginRequest = requestUrl.toLowerCase().includes('/login');

      if (!isLoginRequest) {
        // Csak akkor dobjuk ki a felhasználót és frissítünk, ha ez NEM egy login kísérlet volt
        localStorage.removeItem('userToken');
        window.location.href = '/login';
      }
    }

    // Minden esetben továbbdobjuk a hibát, hogy a Vue komponens (try-catch) is lássa!
    return Promise.reject(error);
  }
);

export default apiClient;


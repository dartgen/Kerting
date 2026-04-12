import axios from 'axios';

// A base URL kiszámítása több forrásból:
// 1) .env konfiguracio,
// 2) localhost fejlesztői visszalépő érték,
// 3) végső visszalépő érték relatív /api.
const configuredBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim();
const isLocalHost = typeof window !== 'undefined' && /^(localhost|127\.0\.0\.1)$/i.test(window.location.hostname);
const localDevApiBaseUrl = isLocalHost ? 'https://localhost:5224/api' : null;
const normalizedBaseUrl = configuredBaseUrl
  ? `${configuredBaseUrl.replace(/\/+$/, '').replace(/\/api$/i, '')}/api`
  : localDevApiBaseUrl ?? '/api';

// Központi Axios kliens.
// Minden service ugyanazt a példányt használja, így a header- és hiba-kezelés konzisztens.
const apiClient = axios.create({
  baseURL: normalizedBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Kérési interceptor:
// ha van token a localStorage-ben, automatikusan Bearer headerbe tesszük.
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

// Válasz interceptor 401 kezeléshez.
// A login kísérlet hibáját és a passzív profile-lekérés hibáját külön kezeljük,
// hogy ne legyen felesleges redirect ciklus.
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Megnézzük, melyik végponton kaptuk a 401-et.
      const requestUrl = error.config.url || '';
      const normalizedUrl = requestUrl.toLowerCase();
      const isLoginRequest = normalizedUrl.includes('/login');
      const isProfileRequest = normalizedUrl.includes('/getmyprofile');

      if (!isLoginRequest) {
        // Nem login végpontnál a token már nem érvényes, töröljük kliens oldalon.
        localStorage.removeItem('userToken');

        // Profil végpontnál oldalbetöltéskor természetes lehet a 401,
        // ezért ott nem kényszerítünk azonnali login átirányítást.
        if (!isProfileRequest && window.location.pathname !== '/login') {
          window.location.href = '/login';
        }
      }
    }

    // A hibát mindenképpen továbbdobjuk, hogy a komponens-szintű kezelők is reagálhassanak.
    return Promise.reject(error);
  }
);

export default apiClient;


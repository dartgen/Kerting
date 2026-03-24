import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5224/api',
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
      // Itt kezelhetjük a lejárt tokent (pl. kijelentkeztetés, átirányítás)
      localStorage.removeItem('userToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;


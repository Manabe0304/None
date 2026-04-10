import axios from "axios";
import { getToken, clearAuth } from "./tokenService";

const api = axios.create({
  baseURL: "/api",
});

// ✅ REQUEST INTERCEPTOR
api.interceptors.request.use(
  (config) => {
    const token = getToken();

    // 🔥 FIX 1: đảm bảo headers luôn tồn tại
    config.headers = config.headers || {};

    // 🔥 FIX 2: chỉ set khi có token
    if (token && token !== "undefined" && token !== "null") {
      config.headers.Authorization = `Bearer ${token}`;
    } else {
      delete config.headers.Authorization;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// ✅ RESPONSE INTERCEPTOR
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;

    // 🔥 FIX 3: tránh redirect loop
    if (status === 401) {
      const currentPath = window.location.pathname;

      if (currentPath !== "/login") {
        clearAuth();
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  }
);

export default api;

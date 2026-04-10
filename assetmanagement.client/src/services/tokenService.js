const TOKEN_KEY = "access_token";
const LEGACY_TOKEN_KEY = "token";
const USER_KEY = "user_info";

export const getToken = () => {
  return (
    localStorage.getItem(TOKEN_KEY) ||
    localStorage.getItem(LEGACY_TOKEN_KEY)
  );
};

export const setToken = (token) => {
  if (!token) return;

  localStorage.setItem(TOKEN_KEY, token);
  localStorage.removeItem(LEGACY_TOKEN_KEY);
};

export const removeToken = () => {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(LEGACY_TOKEN_KEY);
};

export const getUserInfo = () => {
  const data = localStorage.getItem(USER_KEY);
  return data ? JSON.parse(data) : null;
};

export const setUserInfo = (user) => {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
};

export const removeUserInfo = () => {
  localStorage.removeItem(USER_KEY);
};

export const clearAuth = () => {
  removeToken();
  removeUserInfo();
};

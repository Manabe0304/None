import axios from "axios"

const axiosClient = axios.create({
  baseURL: "/"
})

axiosClient.interceptors.request.use((config) => {
  const token =
    localStorage.getItem("token") ||
    localStorage.getItem("accessToken") ||
    localStorage.getItem("jwtToken") ||
    sessionStorage.getItem("token") ||
    sessionStorage.getItem("accessToken") ||
    sessionStorage.getItem("jwtToken")

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

export default axiosClient

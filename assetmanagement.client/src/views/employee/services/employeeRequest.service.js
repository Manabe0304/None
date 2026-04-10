import axios from "axios"

const API_BASE = "https://localhost:7172/api"

function getToken() {
  const token = localStorage.getItem("token")
  if (token) return token

  try {
    const user = JSON.parse(localStorage.getItem("user"))
    return user?.token || user?.accessToken || null
  } catch {
    return null
  }
}

const axiosClient = axios.create({
  baseURL: API_BASE,
})

axiosClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export async function submitEmployeeRequest(payload) {
  const response = await axiosClient.post("/asset-requests", {
    assetType: payload.assetType,
    preferredModel: payload.preferredModel || null,
    reason: payload.reason,
    urgencyLevel: payload.urgency,
  })

  return response.data.data
}

export async function getEmployeeRequestHistory() {
  const response = await axiosClient.get("/asset-requests/my")
  return response.data.data
}

export async function getAvailableModelsByType(assetType) {
  if (!assetType) return []
  const response = await axiosClient.get("/asset-catalog/models", {
    params: { assetType },
  })
  return response.data.data || []
}

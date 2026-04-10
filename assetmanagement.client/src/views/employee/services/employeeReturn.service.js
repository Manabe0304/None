import axios from "axios"

function getToken() {
  const directToken = localStorage.getItem("token")
  if (directToken) return directToken

  const userRaw = localStorage.getItem("user")
  if (userRaw) {
    try {
      const user = JSON.parse(userRaw)
      if (user?.token) return user.token
    } catch {
      // ignore parse error
    }
  }

  return null
}

const API_BASE = "https://localhost:7172/api"

export async function getMyReturnedHistory() {
  const token = getToken()

  const response = await axios.get(`${API_BASE}/return-requests/my-history`, {
    headers: {
      ...(token && { Authorization: `Bearer ${token}` })
    }
  })

  return response.data.data
}

export async function getMyReturnedHistoryDetail(id) {
  const token = getToken()

  const response = await axios.get(`${API_BASE}/return-requests/my-history/${id}`, {
    headers: {
      ...(token && { Authorization: `Bearer ${token}` })
    }
  })

  return response.data.data
}

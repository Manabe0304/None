import axios from 'axios'

function getToken() {
  const directToken = localStorage.getItem('token')
  if (directToken) return directToken

  const userRaw = localStorage.getItem('user')
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

const API_BASE = 'https://localhost:7172/api'

export async function getApprovalRequests() {
  const token = getToken()

  const response = await axios.get(`${API_BASE}/manager/approvals`, {
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    }
  })

  return response.data.data
}

export async function approveApprovalRequest(id) {
  const token = getToken()

  await axios.post(
    `${API_BASE}/manager/approvals/${id}/approve`,
    {},
    {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    }
  )
}

export async function rejectApprovalRequest(id, reason) {
  const token = getToken()

  await axios.post(
    `${API_BASE}/manager/approvals/${id}/reject`,
    { reason },
    {
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    }
  )
}

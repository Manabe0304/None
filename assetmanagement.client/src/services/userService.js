import axios from "axios"

const API_URL = "/api/users"

function getToken() {
  return (
    localStorage.getItem("token") ||
    localStorage.getItem("accessToken") ||
    localStorage.getItem("jwtToken")
  )
}

function authHeader() {
  const token = getToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

export default {
  async getUsers(page = 1, search = "") {
    const res = await axios.get(API_URL, {
      params: { page, search },
      headers: authHeader()
    })
    return res.data
  },

  async getUser(id) {
    const res = await axios.get(`${API_URL}/${id}`, {
      headers: authHeader()
    })
    return res.data
  },

  async createUser(data) {
    const res = await axios.post(API_URL, data, {
      headers: authHeader()
    })
    return res.data
  },

  async getRoles() {
    const res = await axios.get('/api/roles', {
      headers: authHeader()
    })
    return res.data
  },

  async getDepartments() {
    const res = await axios.get('/api/departments', {
      headers: authHeader()
    })
    return res.data
  },

  async updateUser(id, data) {
    const res = await axios.put(`${API_URL}/${id}`, data, {
      headers: authHeader()
    })
    return res.data
  },

  async deleteUser(id) {
    const res = await axios.delete(`${API_URL}/${id}`, {
      headers: authHeader()
    })
    return res.data
  }
}

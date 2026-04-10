import { defineStore } from 'pinia'
import axiosClient from '@/services/axiosClient'
import router from '@/components/router'

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: localStorage.getItem("token") || null,
    user: JSON.parse(localStorage.getItem("user")) || null,
  }),

  getters: {
    userFullName: (state) => state.user?.fullName || state.user?.FullName || "",
    userRole: (state) => state.user?.role || localStorage.getItem("role") || "",
  },

  actions: {
    login(userData) {
      // accept token under multiple possible property names returned by server
      const tokenValue = userData?.token ?? userData?.Token ?? userData?.accessToken ?? userData?.TokenValue ?? null
      this.token = tokenValue
      this.user = {
        userId: userData.userId,
        email: userData.email,
        fullName: userData.fullName || userData.FullName,
        role: userData.role,
        departmentName: userData.departmentName || null,
        token: tokenValue,
      }
      if (this.token) {
        localStorage.setItem("token", this.token)
        // also persist under the new key used by api interceptors
        localStorage.setItem("access_token", this.token)
      }
      localStorage.setItem("user", JSON.stringify(this.user))
    },

    async logout() {
      const currentToken = this.token

      this.token = null
      this.user = null
      localStorage.clear()

      router.push('/login')

      try {
        await axiosClient.post('/api/v1/auth/logout', null, {
          headers: { Authorization: `Bearer ${currentToken}` }
        })
      } catch (e) {
        console.error("Server logout log failed", e)
      }
    }
  }
})

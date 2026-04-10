<template>
  <header class="header">
    <div class="logo">
      <i class="fa-solid fa-layer-group"></i>
      <span>Asset Management</span>
    </div>

    <div class="header-right">
      <div ref="accountBtn"
           class="icon-btn account-btn"
           :class="{ active: showProfilePopup || showMenu }"
           @click="toggleMenu">
        <i class="fa-solid fa-user"></i>

        <span class="user-info">
          <span class="user-name">{{ authStore.userFullName || "My Account" }}</span>
          <span class="user-role">{{ authStore.userRole }}</span>
        </span>

        <div v-if="showMenu" class="menu">
          <p @click.stop="goToProfile">Profile</p>
          <p @click.stop="exit">Exit</p>
        </div>

        <div v-if="showProfilePopup" ref="profilePopup" class="profile-popup">
          <div class="popup-box">
            <div class="popup-header">
              <div class="avatar">
                {{ (profileUser?.fullName || profileUser?.email || "U").charAt(0).toUpperCase() }}
              </div>

              <div>
                <div class="header-name">
                  {{ profileUser?.fullName || profileUser?.email || "User" }}
                </div>
                <div class="header-role-text">
                  {{ profileUser?.role || "" }}
                </div>
              </div>
            </div>

            <div class="popup-content">
              <div class="popup-row">
                <strong>Email:</strong> {{ profileUser?.email || "N/A" }}
              </div>

              <div class="popup-row">
                <strong>Department:</strong>
                {{ profileUser?.departmentName || profileUser?.department || "N/A" }}
              </div>
            </div>

            <div class="popup-actions">
              <button class="close-btn" @click.stop="showProfilePopup = false">Close</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<script>
  import { useAuthStore } from "../../stores/auth"
  import { useRouter } from "vue-router"

  export default {
    data() {
      return {
        showMenu: false,
        searchQuery: "",
        showProfilePopup: false,
        profileUser: null,
      }
    },

    setup() {
      const authStore = useAuthStore()
      const router = useRouter()
      return { authStore, router }
    },

    methods: {
      toggleMenu() {
        this.showMenu = !this.showMenu
        if (this.showMenu) {
          this.showProfilePopup = false
        }
      },

      goToProfile() {
        this.showMenu = false

        try {
          const stored = localStorage.getItem("user")
          const user = stored ? JSON.parse(stored) : null

          this.profileUser = {
            fullName: user?.fullName || user?.name || user?.username,
            email: user?.email,
            role: user?.role || user?.roleName,
            departmentName: user?.departmentName || user?.department,
          }
        } catch {
          this.profileUser = null
        }

        this.showProfilePopup = !this.showProfilePopup
      },

      exit() {
        this.authStore.logout?.()
        this.showMenu = false
        this.showProfilePopup = false
        this.router.push("/login")
      },

      handleSearch() {
        const keyword = this.searchQuery.trim()
        if (!keyword) return
        console.log("Search:", keyword)
      },

      onOutsideClick(event) {
        const popup = this.$refs.profilePopup
        const accountBtn = this.$refs.accountBtn

        if (!accountBtn) return
        if (accountBtn.contains(event.target)) return
        if (popup && popup.contains(event.target)) return

        this.showMenu = false
        this.showProfilePopup = false
      },
    },

    mounted() {
      document.addEventListener("click", this.onOutsideClick)
    },

    beforeUnmount() {
      document.removeEventListener("click", this.onOutsideClick)
    },
  }
</script>

<style scoped>
  .header {
    min-height: var(--app-header-height, 72px);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 20px;
    padding: 12px 20px;
    box-sizing: border-box;
    color: white;
    position: relative;
    z-index: 20;
    box-shadow: 0 10px 30px rgba(15, 23, 42, 0.12);
    transition: background 0.25s ease, box-shadow 0.25s ease;
  }

  .logo {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 20px;
    font-weight: 700;
    white-space: nowrap;
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .icon-btn {
    position: relative;
    display: flex;
    align-items: center;
    gap: 10px;
    cursor: pointer;
  }

  .account-btn {
    padding: 8px 12px;
    border-radius: 12px;
    transition: background 0.2s ease, color 0.2s ease;
  }

    .account-btn.active {
      background: rgba(255, 255, 255, 0.16);
    }

  .user-info {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    line-height: 1.3;
  }

  .user-name {
    font-size: 14px;
    font-weight: 600;
  }

  .user-role {
    font-size: 11px;
    opacity: 0.78;
    text-transform: uppercase;
  }

  .menu {
    position: absolute;
    top: calc(100% + 10px);
    right: 0;
    min-width: 160px;
    background: white;
    color: #0f172a;
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    box-shadow: 0 16px 40px rgba(15, 23, 42, 0.16);
    overflow: hidden;
    z-index: 30;
  }

    .menu p {
      margin: 0;
      padding: 12px 16px;
      font-size: 14px;
      font-weight: 600;
      border-bottom: 1px solid #f1f5f9;
    }

      .menu p:last-child {
        border-bottom: none;
      }

      .menu p:hover {
        background: #f8fafc;
        color: #2563eb;
      }

  .profile-popup {
    position: absolute;
    top: calc(100% + 10px);
    right: 0;
    z-index: 31;
  }

  .popup-box {
    min-width: 360px;
    background: white;
    color: #0f172a;
    border-radius: 14px;
    box-shadow: 0 20px 40px rgba(15, 23, 42, 0.18);
    overflow: hidden;
  }

  .popup-header {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 16px;
    border-bottom: 1px solid #e2e8f0;
  }

  .avatar {
    width: 46px;
    height: 46px;
    border-radius: 50%;
    background: #475569;
    color: white;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
  }

  .header-name {
    font-weight: 700;
    font-size: 15px;
  }

  .header-role-text {
    font-size: 12px;
    color: #64748b;
  }

  .popup-content {
    padding: 16px;
  }

  .popup-row {
    margin-bottom: 10px;
    font-size: 14px;
    line-height: 1.5;
  }

    .popup-row:last-child {
      margin-bottom: 0;
    }

  .popup-actions {
    padding: 12px 16px;
    border-top: 1px solid #e2e8f0;
    text-align: right;
  }

  .close-btn {
    border: none;
    border-radius: 10px;
    padding: 8px 16px;
    background: #dc2626;
    color: white;
    font-size: 13px;
    font-weight: 700;
    cursor: pointer;
  }

    .close-btn:hover {
      background: #b91c1c;
    }
</style>

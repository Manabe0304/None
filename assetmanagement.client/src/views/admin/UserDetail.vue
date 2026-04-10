<template>
  <div class="detail-shell">

    <div v-if="loading" class="loading-state">Loading user detail...</div>

    <div v-else-if="user" class="detail-card">
      <div class="detail-grid">
        <div class="detail-item">
          <span class="detail-label">Email</span>
          <span class="detail-value">{{ user.email }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Full Name</span>
          <span class="detail-value">{{ user.fullName || '—' }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Role</span>
          <span class="detail-value">{{ user.roleName || '—' }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Department</span>
          <span class="detail-value">{{ user.departmentName || '—' }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Status</span>
          <span class="detail-value">
            <span class="status-badge" :class="statusClass(user.status)">
              {{ user.status || 'ACTIVE' }}
            </span>
          </span>
        </div>
      </div>

      <div class="detail-actions">
        <button class="btn btn-secondary" @click="$emit('close')">Close</button>
        <button class="btn btn-primary" @click="$emit('edit', resolvedId)">Edit User</button>
      </div>
    </div>
  </div>
</template>

<script>
  import userService from "@/services/userService"

  export default {
    name: "UserDetail",
    props: {
      userId: {
        type: String,
        default: "",
      },
    },
    emits: ["close", "edit", "error"],
    data() {
      return {
        user: null,
        loading: false,
        errorMessage: "",
      }
    },
    computed: {
      resolvedId() {
        return this.userId || this.$route?.params?.id || null
      },
    },
    async mounted() {
      await this.loadUser()
    },
    methods: {
      statusClass(status) {
        return String(status || "ACTIVE").toUpperCase() === "ACTIVE"
          ? "status-active"
          : "status-inactive"
      },
      async loadUser() {
        try {
          this.loading = true
          this.user = await userService.getUser(this.resolvedId)
        } catch (error) {
          console.error(error)
          this.errorMessage = "Không thể tải thông tin user."
          this.$emit("error", this.errorMessage)
        } finally {
          this.loading = false
        }
      },
    },
  }
</script>

<style scoped>
  .detail-shell {
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  .detail-card {
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    padding: 18px;
    background: #f9fafb;
  }

  .detail-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;
  }

  .detail-item {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 14px;
    border-radius: 12px;
    background: #fff;
    border: 1px solid #e5e7eb;
  }

  .detail-label {
    font-size: 13px;
    font-weight: 700;
    color: #6b7280;
    text-transform: uppercase;
    letter-spacing: 0.03em;
  }

  .detail-value {
    color: #111827;
    font-weight: 600;
    word-break: break-word;
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    padding: 6px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
  }

  .status-active {
    background: #dcfce7;
    color: #166534;
  }

  .status-inactive {
    background: #fee2e2;
    color: #991b1b;
  }

  .detail-actions {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    margin-top: 18px;
  }

  .btn {
    border: none;
    border-radius: 10px;
    padding: 10px 14px;
    font-weight: 600;
    cursor: pointer;
  }

  .btn-primary {
    background: #1d4ed8;
    color: #fff;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }

  .loading-state {
    color: #6b7280;
    padding: 12px 0;
  }

  @media (max-width: 768px) {
    .detail-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

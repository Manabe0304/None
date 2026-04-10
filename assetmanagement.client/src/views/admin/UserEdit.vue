<template>
  <div class="user-form-shell">

    <form class="user-form" @submit.prevent="updateUser">
      <div class="form-grid">
        <div class="form-group">
          <label>Email</label>
          <input v-model.trim="email" type="text" placeholder="Enter email" />
        </div>

        <div class="form-group">
          <label>Full Name</label>
          <input v-model.trim="fullName"
                 type="text"
                 placeholder="Enter full name"
                 @input="formatFullName" />
        </div>

        <div class="form-group">
          <label>Password</label>
          <input v-model.trim="password" type="text" placeholder="Leave blank to keep current password" />
        </div>

        <div class="form-group">
          <label>Role</label>
          <select v-model="selectedRoleId" :disabled="isUserAdmin">
            <option disabled :value="null">Select role</option>
            <option v-for="role in filteredRoles" :key="role.id" :value="role.id">
              {{ role.name }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Department</label>
          <select v-model="selectedDepartmentId" :disabled="isAdminRole">
            <option disabled :value="null">Select department</option>
            <option v-for="department in filteredDepartments" :key="department.id" :value="department.id">
              {{ department.name }}
            </option>
          </select>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" @click="$emit('cancel')">Cancel</button>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Updating...' : 'Update User' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
  import userService from "@/services/userService"

  export default {
    name: "UserEdit",
    props: {
      userId: {
        type: String,
        default: "",
      },
    },
    emits: ["cancel", "success", "error"],
    data() {
      return {
        id: null,
        email: "",
        fullName: "",
        password: "",
        roles: [],
        selectedRoleId: null,
        departments: [],
        selectedDepartmentId: null,
        loading: false,
        errorMessage: "",
        isUserAdmin: false,
      }
    },
    computed: {
      resolvedId() {
        return this.userId || this.$route?.params?.id || null
      },
      isAdminRole() {
        const role = this.roles.find((item) => item.id === this.selectedRoleId)
        return role && role.name === "ADMIN"
      },
      filteredRoles() {
        if (this.isUserAdmin) return this.roles
        return this.roles.filter((role) => role.name !== "ADMIN")
      },
      filteredDepartments() {
        if (this.isUserAdmin) return this.departments
        return this.departments.filter((department) => department.name !== "Administration")
      },
    },
    watch: {
      selectedRoleId(newRoleId) {
        const role = this.roles.find((item) => item.id === newRoleId)
        if (role && role.name === "ADMIN") {
          const adminDepartment = this.departments.find((item) => item.name === "Administration")
          if (adminDepartment) {
            this.selectedDepartmentId = adminDepartment.id
          }
        }
      },
    },
    async mounted() {
      this.id = this.resolvedId
      await this.loadData()
    },
    methods: {
      async loadData() {
        try {
          const [roles, departments, user] = await Promise.all([
            userService.getRoles(),
            userService.getDepartments(),
            userService.getUser(this.id),
          ])

          this.roles = roles || []
          this.departments = departments || []

          this.email = user.email ?? ""
          this.fullName = user.fullName ?? ""
          this.selectedRoleId = user.roleId ?? null
          this.selectedDepartmentId = user.departmentId ?? null

          const role = this.roles.find((item) => item.id === user.roleId)
          this.isUserAdmin = !!(role && role.name === "ADMIN")
        } catch (error) {
          console.error(error)
          this.errorMessage = "Cannot load user data"
          this.$emit("error", this.errorMessage)
        }
      },
      formatFullName() {
        let value = this.fullName.replace(/[^a-zA-Z\s]/g, "")
        value = value
          .toLowerCase()
          .split(" ")
          .map((word) => (word ? word.charAt(0).toUpperCase() + word.slice(1) : ""))
          .join(" ")

        this.fullName = value
      },
      validateForm() {
        if (!this.email || !this.fullName || !this.selectedRoleId || !this.selectedDepartmentId) {
          return "Please fill in all required fields"
        }

        if (!/^[a-zA-Z0-9@.]+$/.test(this.email)) {
          return "Username chỉ được chứa chữ, số, @ và dấu ."
        }

        if (this.password && !/^[0-9]+$/.test(this.password)) {
          return "Password chỉ được chứa số"
        }

        if (!/^[A-Za-z\s]+$/.test(this.fullName)) {
          return "Full Name chỉ được chứa chữ cái"
        }

        return ""
      },
      async updateUser() {
        this.errorMessage = ""
        const validationMessage = this.validateForm()

        if (validationMessage) {
          this.errorMessage = validationMessage
          this.$emit("error", validationMessage)
          return
        }

        this.loading = true
        try {
          const payload = {
            email: this.email,
            fullName: this.fullName,
            roleId: this.selectedRoleId,
            departmentId: this.selectedDepartmentId,
          }

          if (this.password) {
            payload.password = this.password
          }

          await userService.updateUser(this.id, payload)
          this.$emit("success", "Cập nhật user thành công.")
        } catch (error) {
          console.error(error?.response?.data || error)
          const responseData = error?.response?.data

          if (responseData?.errors) {
            const messages = []
            for (const key in responseData.errors) {
              messages.push(...responseData.errors[key])
            }
            this.errorMessage = messages.join("; ")
          } else {
            this.errorMessage = responseData?.message || "Update failed"
          }

          this.$emit("error", this.errorMessage)
        } finally {
          this.loading = false
        }
      },
    },
  }
</script>

<style scoped>
  .user-form-shell {
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  .user-form {
    display: flex;
    flex-direction: column;
    gap: 18px;
  }

  .form-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

    .form-group label {
      font-weight: 600;
      color: #374151;
    }

    .form-group input,
    .form-group select {
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 12px;
      font-size: 14px;
      outline: none;
    }

      .form-group input:focus,
      .form-group select:focus {
        border-color: #1d4ed8;
      }

  .form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
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

  @media (max-width: 768px) {
    .form-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

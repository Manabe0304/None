<template>
  <div class="user-form-shell">

    <form class="user-form" @submit.prevent="createUser">
      <div class="form-grid">
        <div class="form-group">
          <label>Email</label>
          <input v-model.trim="email" type="text" placeholder="Enter email" />
        </div>

        <div class="form-group">
          <label>Full Name</label>
          <input v-model.trim="fullName"
                 type="text"
                 maxlength="50"
                 placeholder="Enter full name"
                 @input="formatFullName" />
        </div>

        <div class="form-group">
          <label>Password</label>
          <input v-model.trim="password" type="text" placeholder="Numbers only" />
        </div>

        <div class="form-group">
          <label>Role</label>
          <select v-model="selectedRoleId">
            <option disabled value="">Select role</option>
            <option v-for="role in roles" :key="role.id" :value="role.id">
              {{ role.name }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Department</label>
          <select v-model="selectedDepartmentId">
            <option disabled value="">Select department</option>
            <option v-for="department in departments" :key="department.id" :value="department.id">
              {{ department.name }}
            </option>
          </select>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" @click="$emit('cancel')">Cancel</button>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Creating...' : 'Create User' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
  import userService from "@/services/userService"

  export default {
    name: "UserCreate",
    emits: ["cancel", "success", "error"],
    data() {
      return {
        email: "",
        fullName: "",
        password: "",
        roles: [],
        selectedRoleId: "",
        departments: [],
        selectedDepartmentId: "",
        loading: false,
        errorMessage: "",
      }
    },
    async mounted() {
      try {
        const [roles, departments] = await Promise.all([
          userService.getRoles(),
          userService.getDepartments(),
        ])

        this.roles = (roles || []).filter((role) => !String(role.name).toLowerCase().includes("admin"))
        this.departments = (departments || []).filter(
          (department) => !String(department.name).toLowerCase().includes("administration")
        )
      } catch (error) {
        console.error(error)
        this.errorMessage = "Không tải được role hoặc department."
      }
    },
    methods: {
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
        if (!this.email || !this.fullName || !this.password || !this.selectedRoleId || !this.selectedDepartmentId) {
          return "Vui lòng điền đầy đủ thông tin."
        }

        if (!/^[a-zA-Z0-9@.]+$/.test(this.email)) {
          return "Tên đăng nhập chỉ được chứa chữ, số, @ và dấu ."
        }

        if (!/^[0-9]+$/.test(this.password)) {
          return "Password chỉ được chứa số."
        }

        if (!/^[A-Za-z\s]+$/.test(this.fullName)) {
          return "Full Name chỉ được chứa chữ cái."
        }

        return ""
      },
      async createUser() {
        this.errorMessage = ""
        const validationMessage = this.validateForm()

        if (validationMessage) {
          this.errorMessage = validationMessage
          this.$emit("error", validationMessage)
          return
        }

        this.loading = true
        try {
          await userService.createUser({
            email: this.email,
            password: this.password,
            fullName: this.fullName,
            roleId: this.selectedRoleId,
            departmentId: this.selectedDepartmentId,
          })

          this.$emit("success", "Tạo user thành công.")
        } catch (error) {
          console.error(error)
          const responseData = error?.response?.data

          if (responseData?.errors) {
            const messages = []
            for (const key in responseData.errors) {
              messages.push(...responseData.errors[key])
            }
            this.errorMessage = messages.join("; ")
          } else {
            this.errorMessage = responseData?.message || "Create user failed"
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

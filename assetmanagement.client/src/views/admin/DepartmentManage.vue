<template>
  <div class="page">
    <div class="page-header">
      <h2>Department Management</h2>
      <button class="btn-add" @click="openCreate">+ Add Department</button>
    </div>

    <!-- Table -->
    <table class="data-table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="dept in departments" :key="dept.id">
          <td>{{ dept.name }}</td>
          <td>
            <button class="btn-edit" @click="openEdit(dept)">Edit</button>
            <button class="btn-delete" @click="deactivate(dept.id)">Deactivate</button>
          </td>
        </tr>
        <tr v-if="departments.length === 0">
          <td colspan="2" style="text-align:center; color:#999">No departments found</td>
        </tr>
      </tbody>
    </table>

    <!-- Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal">
        <h3>{{ editingId ? 'Edit Department' : 'Create Department' }}</h3>
        <input v-model="form.name" placeholder="Department name" class="modal-input" />
        <div class="modal-actions">
          <button class="btn-save" @click="save">Save</button>
          <button class="btn-cancel" @click="closeModal">Cancel</button>
        </div>
        <p v-if="error" class="error">{{ error }}</p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios"

export default {
  name: "DepartmentManage",
  data() {
    return {
      departments: [],
      showModal: false,
      editingId: null,
      form: { name: "" },
      error: ""
    }
  },
  async mounted() {
    await this.load()
  },
  methods: {
    async load() {
      const res = await axios.get("/api/departments")
      this.departments = res.data
    },
    openCreate() {
      this.editingId = null
      this.form.name = ""
      this.error = ""
      this.showModal = true
    },
    openEdit(dept) {
      this.editingId = dept.id
      this.form.name = dept.name
      this.error = ""
      this.showModal = true
    },
    closeModal() {
      this.showModal = false
    },
    async save() {
      this.error = ""
      if (!this.form.name.trim()) {
        this.error = "Department name is required"
        return
      }
      try {
        if (this.editingId) {
          await axios.put(`/api/departments/${this.editingId}`, { name: this.form.name })
        } else {
          await axios.post("/api/departments", { name: this.form.name })
        }
        this.closeModal()
        await this.load()
      } catch (e) {
        this.error = "An error occurred. Please try again."
      }
    },
    async deactivate(id) {
      if (!confirm("Deactivate this department?")) return
      await axios.delete(`/api/departments/${id}`)
      await this.load()
    }
  }
}
</script>

<style scoped>
.page {
  padding: 24px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
.page-header h2 {
  font-size: 22px;
  font-weight: 600;
}
.btn-add {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
}
.data-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
}
.data-table th {
  background: #f1f5f9;
  padding: 12px 16px;
  text-align: left;
  font-weight: 600;
  color: #374151;
}
.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid #e5e7eb;
}
.btn-edit {
  background: #f59e0b;
  color: white;
  border: none;
  padding: 5px 12px;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 6px;
}
.btn-delete {
  background: #ef4444;
  color: white;
  border: none;
  padding: 5px 12px;
  border-radius: 4px;
  cursor: pointer;
}
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.modal {
  background: white;
  padding: 28px;
  border-radius: 10px;
  width: 360px;
}
.modal h3 {
  margin-bottom: 16px;
  font-size: 18px;
}
.modal-input {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  box-sizing: border-box;
}
.modal-actions {
  display: flex;
  gap: 10px;
  margin-top: 16px;
}
.btn-save {
  flex: 1;
  background: #3b82f6;
  color: white;
  border: none;
  padding: 9px;
  border-radius: 6px;
  cursor: pointer;
}
.btn-cancel {
  flex: 1;
  background: #6b7280;
  color: white;
  border: none;
  padding: 9px;
  border-radius: 6px;
  cursor: pointer;
}
.error {
  color: red;
  margin-top: 10px;
  font-size: 13px;
}
</style>

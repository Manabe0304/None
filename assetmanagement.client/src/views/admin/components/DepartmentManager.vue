<template>
  <div class="department-page">
    <div class="page-header">
      <div>
        <h1>Department Management</h1>
        <p>Create, update, and manage active departments.</p>
      </div>
      <button class="primary-btn" @click="openCreateForm">+ Add Department</button>
    </div>

    <div class="card">
      <div class="card-header">
        <h2>Departments</h2>
        <span class="count-badge">{{ departments.length }} total</span>
      </div>

      <div v-if="loading" class="empty-state">Loading departments...</div>
      <div v-else-if="!departments.length" class="empty-state">No departments found.</div>

      <div v-else class="table-wrapper">
        <table class="department-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Code</th>
              <th>Description</th>
              <th>Total Assets</th>
              <th>Shared Assets</th>
              <th>Status</th>
              <th class="action-col">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="department in departments" :key="department.id">
              <td>{{ department.name }}</td>
              <td>{{ department.code }}</td>
              <td>{{ department.description || '—' }}</td>
              <td>{{ department.totalAssetCount ?? 0 }}</td>
              <td>{{ department.sharedAssetCount ?? 0 }}</td>
              <td>
                <span class="status-pill" :class="department.isActive ? 'active' : 'inactive'">
                  {{ department.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="action-col">
                <button class="table-btn info" @click="openDetail(department)">View Detail</button>
                <button class="table-btn edit" @click="openEditForm(department)">Edit</button>
                <button class="table-btn toggle" @click="toggleDepartmentStatus(department)">
                  {{ department.isActive ? 'Deactivate' : 'Activate' }}
                </button>
                <button class="table-btn delete" @click="openDeleteConfirm(department)">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AppModal v-model="showForm" :title="isEditing ? 'Edit Department' : 'Create Department'" width="560px">
      <template #body>
        <form class="modal-form" @submit.prevent="submitForm">
          <div class="form-group">
            <label>Department Name</label>
            <input v-model="form.name" type="text" required placeholder="Enter department name" />
          </div>
          <div class="form-group">
            <label>Code</label>
            <input v-model="form.code" type="text" required placeholder="Enter department code" />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description" rows="4" placeholder="Enter department description" />
          </div>
          <div class="form-group checkbox-group">
            <input id="isActive" v-model="form.isActive" type="checkbox" />
            <label for="isActive">Active</label>
          </div>
        </form>
      </template>
      <template #footer>
        <button type="button" class="secondary-btn" @click="closeForm">Cancel</button>
        <button type="button" class="primary-btn" :disabled="submitting" @click="submitForm">
          {{ submitting ? 'Saving...' : isEditing ? 'Update' : 'Create' }}
        </button>
      </template>
    </AppModal>

    <AppModal v-model="showDetail" title="Department Detail" width="720px">
      <template #body>
        <div v-if="detail" class="detail-grid">
          <div class="detail-card"><span class="label">Employees</span><strong>{{ detail.employeeCount }}</strong></div>
          <div class="detail-card"><span class="label">Total Assets</span><strong>{{ detail.totalAssetCount }}</strong></div>
          <div class="detail-card"><span class="label">Shared Assets</span><strong>{{ detail.sharedAssetCount }}</strong></div>
          <div class="detail-card"><span class="label">Users moved to no department on delete</span><strong>{{ detail.unassignedUserCountOnDelete }}</strong></div>
        </div>

        <div v-if="detail" class="detail-section">
          <h4>Employees</h4>
          <div v-if="detail.employees?.length" class="detail-list">
            <div v-for="item in detail.employees" :key="item.userId" class="detail-row">
              <span>{{ item.employeeName }}</span>
              <strong>{{ item.assetCount }} asset(s)</strong>
            </div>
          </div>
          <p v-else class="empty-text">No employees.</p>
        </div>

        <div v-if="detail" class="detail-section">
          <h4>Shared Assets</h4>
          <div v-if="detail.sharedAssets?.length" class="detail-list">
            <div v-for="item in detail.sharedAssets" :key="item.assetId" class="detail-row">
              <span>{{ item.assetTag }} - {{ item.assetName || '—' }}</span>
              <strong>{{ item.status }}</strong>
            </div>
          </div>
          <p v-else class="empty-text">No shared assets.</p>
        </div>
      </template>
      <template #footer>
        <button class="secondary-btn" @click="showDetail = false">Close</button>
      </template>
    </AppModal>

    <AppModal v-model="showDeleteConfirm" title="Delete department" width="520px">
      <template #body>
        <p class="confirm-text">Deleting this department will move all employees in this department to <strong>no department</strong>. Shared department assets will be marked as <strong>AVAILABLE</strong>. Do you want to continue?</p>
      </template>
      <template #footer>
        <button class="secondary-btn" @click="closeDeleteConfirm">Cancel</button>
        <button class="danger-btn" @click="deleteDepartment">Delete</button>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
  import { onMounted, reactive, ref } from 'vue'
  import AppModal from '@/components/common/AppModal.vue'
  import { showNotification } from '@/stores/notificationStore'

  const API_BASE = '/api/departments'
  const departments = ref([])
  const showForm = ref(false)
  const showDetail = ref(false)
  const showDeleteConfirm = ref(false)
  const isEditing = ref(false)
  const submitting = ref(false)
  const loading = ref(false)
  const editingId = ref(null)
  const deletingDepartment = ref(null)
  const detail = ref(null)

  const initialForm = () => ({ name: '', code: '', description: '', isActive: true })
  const form = reactive(initialForm())

  function resetForm() {
    Object.assign(form, initialForm())
    editingId.value = null
    isEditing.value = false
  }

  function openCreateForm() { resetForm(); showForm.value = true }
  function closeForm() { showForm.value = false; resetForm() }
  function openEditForm(department) {
    isEditing.value = true
    editingId.value = department.id
    form.name = department.name
    form.code = department.code
    form.description = department.description || ''
    form.isActive = !!department.isActive
    showForm.value = true
  }
  function openDeleteConfirm(department) { deletingDepartment.value = department; showDeleteConfirm.value = true }
  function closeDeleteConfirm() { deletingDepartment.value = null; showDeleteConfirm.value = false }

  async function fetchDepartments() {
    const response = await fetch(`${API_BASE}?includeInactive=true`)
    if (!response.ok) throw new Error('Failed to fetch departments.')
    const data = await response.json()
    departments.value = Array.isArray(data) ? data : data.items || []
  }

  async function loadDepartments() {
    try {
      loading.value = true
      await fetchDepartments()
    } catch (error) {
      console.error(error)
      showNotification('Error', error.message || 'Unable to load departments.', 'error')
    } finally {
      loading.value = false
    }
  }

  async function submitForm() {
    try {
      submitting.value = true
      const payload = { name: form.name, code: form.code, description: form.description, isActive: form.isActive }
      const url = isEditing.value ? `${API_BASE}/${editingId.value}` : API_BASE
      const method = isEditing.value ? 'PUT' : 'POST'
      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      })
      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        throw new Error(errorData?.message || 'Failed to save department.')
      }
      closeForm()
      await loadDepartments()
      showNotification('Success', isEditing.value ? 'Department updated successfully.' : 'Department created successfully.', 'success')
    } catch (error) {
      console.error(error)
      showNotification('Error', error.message || 'Unable to save department.', 'error')
    } finally {
      submitting.value = false
    }
  }

  async function toggleDepartmentStatus(department) {
    try {
      const response = await fetch(`${API_BASE}/${department.id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isActive: !department.isActive }),
      })
      if (!response.ok) throw new Error('Failed to update department status.')
      await loadDepartments()
      showNotification('Success', `Department ${department.isActive ? 'deactivated' : 'activated'} successfully.`, 'success')
    } catch (error) {
      console.error(error)
      showNotification('Error', error.message || 'Unable to update status.', 'error')
    }
  }

  async function openDetail(department) {
    try {
      const response = await fetch(`${API_BASE}/${department.id}/detail`)
      if (!response.ok) throw new Error('Failed to fetch department detail.')
      detail.value = await response.json()
      showDetail.value = true
    } catch (error) {
      console.error(error)
      showNotification('Error', error.message || 'Unable to load detail.', 'error')
    }
  }

  async function deleteDepartment() {
    if (!deletingDepartment.value) return
    try {
      const response = await fetch(`${API_BASE}/${deletingDepartment.value.id}`, { method: 'DELETE' })
      if (!response.ok) throw new Error('Failed to delete department.')
      await loadDepartments()
      showNotification('Success', 'Department deleted successfully.', 'success')
    } catch (error) {
      console.error(error)
      showNotification('Error', error.message || 'Unable to delete department.', 'error')
    } finally {
      closeDeleteConfirm()
    }
  }

  onMounted(loadDepartments)
</script>

<style scoped>
  .department-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding: 24px;
  }

  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
  }

    .page-header h1 {
      margin: 0;
      font-size: 28px;
      color: #1b2a41;
    }

    .page-header p {
      margin: 6px 0 0;
      color: #6b7280;
    }

  .card {
    background: #fff;
    border-radius: 14px;
    padding: 20px;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
    border: 1px solid #e5e7eb;
  }

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .department-table {
    width: 100%;
    border-collapse: collapse;
  }

    .department-table th, .department-table td {
      padding: 14px 12px;
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
    }

    .department-table th {
      background: #f9fafb;
      color: #374151;
      font-size: 14px;
    }

  .action-col {
    white-space: nowrap;
  }

  .status-pill {
    display: inline-flex;
    align-items: center;
    border-radius: 999px;
    padding: 6px 10px;
    font-size: 12px;
    font-weight: 700;
  }

    .status-pill.active {
      background: #dcfce7;
      color: #166534;
    }

    .status-pill.inactive {
      background: #fee2e2;
      color: #991b1b;
    }

  .table-btn, .primary-btn, .secondary-btn, .danger-btn {
    border: none;
    border-radius: 10px;
    padding: 10px 14px;
    font-weight: 600;
    cursor: pointer;
  }

  .table-btn {
    margin-right: 8px;
    padding: 8px 12px;
  }

    .table-btn.edit, .primary-btn {
      background: #1d4ed8;
      color: #fff;
    }

    .table-btn.toggle {
      background: #f59e0b;
      color: #fff;
    }

    .table-btn.delete, .danger-btn {
      background: #dc2626;
      color: #fff;
    }

    .table-btn.info {
      background: #eff6ff;
      color: #1d4ed8;
    }

  .secondary-btn {
    background: #e5e7eb;
    color: #111827;
  }

  .count-badge {
    background: #eff6ff;
    color: #1d4ed8;
    border-radius: 999px;
    padding: 6px 12px;
    font-size: 13px;
    font-weight: 700;
  }

  .empty-state, .empty-text {
    text-align: center;
    color: #6b7280;
    padding: 24px 12px;
  }

  .modal-form {
    display: flex;
    flex-direction: column;
    gap: 14px;
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

    .form-group input, .form-group textarea {
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 12px;
      font-size: 14px;
      outline: none;
    }

  .checkbox-group {
    flex-direction: row;
    align-items: center;
  }

  .detail-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
  }

  .detail-card {
    background: #f8fafc;
    border-radius: 12px;
    padding: 14px;
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .label {
    font-size: 12px;
    text-transform: uppercase;
    color: #64748b;
    font-weight: 700;
  }

  .detail-section {
    margin-top: 20px;
  }

  .detail-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .detail-row {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding: 12px 14px;
    border: 1px solid #e5e7eb;
    border-radius: 10px;
  }

  .confirm-text {
    margin: 0;
    color: #334155;
    line-height: 1.6;
  }

  @media (max-width: 768px) {
    .detail-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

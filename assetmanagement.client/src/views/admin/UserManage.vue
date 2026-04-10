<template>
  <section class="card">
    <div class="toolbar">
      <div class="search-group">
        <input v-model.trim="search" class="search-input" placeholder="Search by email, full name, role..." @keyup.enter="applySearch" />
        <button class="btn btn-secondary" @click="applySearch">Search</button>
      </div>
      <button class="btn btn-primary" @click="openCreate">+ Add User</button>
    </div>

    <div class="table-wrapper">
      <table class="user-table">
        <thead>
          <tr><th>Email</th><th>Full Name</th><th>Role</th><th>Department</th><th>Status</th><th class="action-col">Action</th></tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id">
            <td>{{ user.email }}</td>
            <td>{{ user.fullName || '—' }}</td>
            <td>{{ user.roleName || '—' }}</td>
            <td>{{ user.departmentName || '—' }}</td>
            <td><span class="status-badge" :class="statusClass(user.status)">{{ user.status || 'ACTIVE' }}</span></td>
            <td class="action-col">
              <button class="btn btn-inline btn-view" @click="openDetail(user.id)">Detail</button>
              <button class="btn btn-inline btn-edit" @click="openEdit(user.id)">Edit</button>
              <button class="btn btn-inline btn-delete" @click="requestDelete(user.id)">Delete</button>
            </td>
          </tr>
          <tr v-if="!users.length && !loading"><td colspan="6" class="empty-cell">No users found.</td></tr>
          <tr v-if="loading"><td colspan="6" class="empty-cell">Loading users...</td></tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <button class="btn btn-secondary" @click="prevPage" :disabled="page === 1 || loading">Prev</button>
      <span class="page-indicator">Page {{ page }}</span>
      <button class="btn btn-secondary" @click="nextPage" :disabled="isLastPage || loading">Next</button>
    </div>

    <AppModal v-model="showModal" :title="modalTitle" :width="modalMode === 'detail' ? '680px' : '760px'">
      <template #body>
        <UserCreate v-if="modalMode === 'create'" @cancel="closeModal" @success="handleFormSuccess" @error="handleFormError" />
        <UserEdit v-else-if="modalMode === 'edit'" :user-id="selectedUserId" @cancel="closeModal" @success="handleFormSuccess" @error="handleFormError" />
        <UserDetail v-else-if="modalMode === 'detail'" :user-id="selectedUserId" @close="closeModal" @edit="switchToEdit" @error="handleFormError" />
      </template>
    </AppModal>

    <AppModal v-model="showDeleteConfirm" title="Delete user" width="480px">
      <template #body>
        <p class="confirm-text">Are you sure you want to delete this user?</p>
      </template>
      <template #footer>
        <button class="btn btn-secondary" @click="closeDeleteConfirm">Cancel</button>
        <button class="btn btn-delete" @click="deleteUser">Delete</button>
      </template>
    </AppModal>
  </section>
</template>

<script>
  import userService from '@/services/userService'
  import AppModal from '@/components/common/AppModal.vue'
  import { showNotification } from '@/stores/notificationStore'
  import UserCreate from '@/views/admin/UserCreate.vue'
  import UserEdit from '@/views/admin/UserEdit.vue'
  import UserDetail from '@/views/admin/UserDetail.vue'

  export default {
    name: 'UserManage',
    components: { AppModal, UserCreate, UserEdit, UserDetail },
    data() {
      return {
        users: [], search: '', page: 1, totalCount: 0, pageSize: 10, loading: false,
        showModal: false, modalMode: 'create', selectedUserId: null, showDeleteConfirm: false, deleteUserId: null,
      }
    },
    computed: {
      modalTitle() { if (this.modalMode === 'create') return 'Create User'; if (this.modalMode === 'edit') return 'Edit User'; return 'User Detail' },
      isLastPage() { return this.page * this.pageSize >= this.totalCount },
    },
    mounted() { this.loadUsers() },
    methods: {
      statusClass(status) { return String(status || 'ACTIVE').toUpperCase() === 'ACTIVE' ? 'status-active' : 'status-inactive' },
      async loadUsers() {
        try {
          this.loading = true
          const res = await userService.getUsers(this.page, this.search)
          this.users = res.items || []
          this.totalCount = res.totalCount || this.users.length
          this.pageSize = res.pageSize || 10
        } catch (error) {
          console.error(error)
          showNotification('Error', 'Không thể tải danh sách user.', 'error')
        } finally { this.loading = false }
      },
      async applySearch() { this.page = 1; await this.loadUsers() },
      async nextPage() { if (this.isLastPage) return; this.page += 1; await this.loadUsers() },
      async prevPage() { if (this.page <= 1) return; this.page -= 1; await this.loadUsers() },
      openCreate() { this.modalMode = 'create'; this.selectedUserId = null; this.showModal = true },
      openEdit(id) { this.modalMode = 'edit'; this.selectedUserId = id; this.showModal = true },
      openDetail(id) { this.modalMode = 'detail'; this.selectedUserId = id; this.showModal = true },
      closeModal() { this.showModal = false; this.selectedUserId = null },
      switchToEdit(id) { this.modalMode = 'edit'; this.selectedUserId = id || this.selectedUserId },
      requestDelete(id) { this.deleteUserId = id; this.showDeleteConfirm = true },
      closeDeleteConfirm() { this.showDeleteConfirm = false; this.deleteUserId = null },
      async handleFormSuccess(message) { this.closeModal(); await this.loadUsers(); showNotification('Success', message || 'Thao tác thành công.', 'success') },
      handleFormError(message) { if (message) showNotification('Error', message, 'error') },
      async deleteUser() {
        if (!this.deleteUserId) return
        try {
          await userService.deleteUser(this.deleteUserId)
          if (this.users.length === 1 && this.page > 1) this.page -= 1
          await this.loadUsers()
          showNotification('Success', 'Xóa user thành công.', 'success')
        } catch (error) {
          console.error(error)
          showNotification('Error', error?.response?.data?.message || 'Xóa user thất bại.', 'error')
        } finally { this.closeDeleteConfirm() }
      },
    },
  }
</script>

<style scoped>
  .card {
    background: #fff;
    border-radius: 14px;
    padding: 20px;
    border: 1px solid #e5e7eb;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
  }

  .toolbar {
    display: flex;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 18px;
    flex-wrap: wrap;
  }

  .search-group {
    display: flex;
    gap: 10px;
    flex: 1;
    min-width: min(100%, 420px);
  }

  .search-input {
    flex: 1;
    min-width: 0;
    border: 1px solid #d1d5db;
    border-radius: 10px;
    padding: 11px 14px;
    font-size: 14px;
  }

    .search-input:focus {
      outline: none;
      border-color: #1d4ed8;
    }

  .table-wrapper {
    overflow-x: auto;
  }

  .user-table {
    width: 100%;
    border-collapse: collapse;
  }

    .user-table th, .user-table td {
      padding: 14px 12px;
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
      vertical-align: middle;
    }

    .user-table th {
      background: #f9fafb;
      color: #374151;
      font-size: 14px;
      font-weight: 700;
    }

  .action-col {
    white-space: nowrap;
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

  .pagination {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    align-items: center;
    margin-top: 18px;
  }

  .page-indicator {
    color: #475569;
    font-weight: 600;
  }

  .btn {
    border: none;
    border-radius: 10px;
    padding: 10px 14px;
    font-weight: 600;
    cursor: pointer;
  }

  .btn-inline {
    padding: 8px 12px;
    margin-right: 8px;
  }

  .btn-primary, .btn-edit {
    background: #1d4ed8;
    color: #fff;
  }

  .btn-secondary, .btn-view {
    background: #e5e7eb;
    color: #111827;
  }

  .btn-delete {
    background: #dc2626;
    color: #fff;
  }

  .empty-cell {
    text-align: center;
    color: #6b7280;
  }

  .confirm-text {
    margin: 0;
    color: #334155;
    line-height: 1.6;
  }
</style>

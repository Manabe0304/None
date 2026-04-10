<template>
  <div class="asset-page">
    <div class="page-header">
      <div>
        <h1>Asset Management</h1>
        <p>Manage assets, shared department assets, and lifecycle actions.</p>
      </div>
    </div>

    <section class="card">
      <div class="toolbar">
        <div class="search-group">
          <input v-model.trim="search" class="search-input" placeholder="Search by asset tag, serial number, category..." @keyup.enter="applySearch" />
          <button class="btn btn-secondary" @click="applySearch">Search</button>
        </div>
        <div class="toolbar-actions">
          <button class="btn btn-primary" @click="openCreate">+ Add Asset</button>
          <button class="btn btn-secondary" :disabled="exporting" @click="exportExcel">Export</button>
          <button class="btn btn-secondary" :disabled="uploading" @click="triggerFile">Import</button>
          <input ref="fileInput" type="file" class="hidden-input" accept=".xlsx,.xls" @change="handleFile" />
        </div>
      </div>

      <div class="table-wrapper">
        <table class="asset-table">
          <thead>
            <tr><th>Asset Tag</th><th>Category</th><th>Model</th><th>Status</th><th>Assignment</th><th>Department</th><th>Current User</th><th class="action-col">Actions</th></tr>
          </thead>
          <tbody>
            <tr v-for="asset in assets" :key="asset.id">
              <td>{{ asset.assetTag }}</td>
              <td>{{ asset.category || '—' }}</td>
              <td>{{ asset.model || '—' }}</td>
              <td><span class="status-badge" :class="statusClass(asset.status)">{{ formatStatus(asset.status) }}</span></td>
              <td><span class="assignment-badge" :class="assignmentBadge(asset.assignmentType)">{{ asset.assignmentType || inferType(asset) }}</span></td>
              <td>{{ asset.departmentName || '—' }}</td>
              <td>{{ asset.currentUserName || '—' }}</td>
              <td class="action-col">
                <button class="btn btn-inline btn-secondary" @click="openDetail(asset.id)">Detail</button>
                <button class="btn btn-inline btn-edit" :disabled="loading || uploading || exporting" @click="openEdit(asset.id)">Update</button>
                <button v-if="canMarkBeyondRepair(asset)" class="btn btn-inline btn-warning" :disabled="loading || uploading || exporting" @click="openBeyondRepair(asset)">Beyond Repair</button>
                <button v-if="canLiquidate(asset)" class="btn btn-inline btn-danger" :disabled="loading || uploading || exporting" @click="openLiquidation(asset)">Liquidate</button>
                <button class="btn btn-inline btn-delete" :disabled="loading || uploading || exporting" @click="requestDelete(asset.id)">Delete</button>
              </td>
            </tr>
            <tr v-if="!assets.length && !loading"><td colspan="8" class="empty-cell">No assets found.</td></tr>
            <tr v-if="loading"><td colspan="8" class="empty-cell">Loading assets...</td></tr>
          </tbody>
        </table>
      </div>

      <div class="pagination">
        <button class="btn btn-secondary" :disabled="page === 1 || loading" @click="prevPage">Prev</button>
        <span class="page-indicator">Page {{ page }}
        <template v-if="totalCount > 0">
          / {{ totalPages }}
        </template></span>
        <button class="btn btn-secondary" :disabled="isLastPage || loading" @click="nextPage">Next</button>
      </div>
    </section>

    <AppModal v-model="showModal" :title="modalMode === 'create' ? 'Create Asset' : 'Update Asset'" width="860px">
      <template #body>
        <AssetCreate v-if="modalMode === 'create'" @cancel="closeModal" @success="handleFormSuccess" @error="handleFormError" />
        <AssetEdit v-else :asset-id="selectedAssetId" @cancel="closeModal" @success="handleFormSuccess" @error="handleFormError" />
      </template>
    </AppModal>

    <AppModal v-model="showDeleteConfirm" title="Delete asset" width="480px">
      <template #body>
        <p class="confirm-text">Are you sure you want to delete this asset?</p>
      </template>
      <template #footer>
        <button class="btn btn-secondary" @click="closeDeleteConfirm">Cancel</button>
        <button class="btn btn-delete" @click="remove">Delete</button>
      </template>
    </AppModal>

    <AssetDetailModal v-if="showDetailModal" :asset-id="selectedDetailAssetId" @close="showDetailModal = false" />
    <AssetLifecycleModal v-if="showLifecycleModal" v-model="showLifecycleModal" :asset="selectedLifecycleAsset" :mode="lifecycleMode" @submit="handleLifecycleSubmit" />
  </div>
</template>

<script>
  import axios from 'axios'
  import AppModal from '@/components/common/AppModal.vue'
  import AssetCreate from '@/components/AssetManage/AssetCreate.vue'
  import AssetEdit from '@/components/AssetManage/AssetEdit.vue'
  import AssetDetailModal from '@/components/assets/AssetDetailModal.vue'
  import AssetLifecycleModal from '@/components/assets/AssetLifecycleModal.vue'
  import { formatAssetStatus, assetStatusClass, canMarkBeyondRepair as canMarkBeyondRepairStatus, canLiquidate as canLiquidateStatus, inferAssignmentType } from '@/constants/assetStatus'
  import { markBeyondRepair, liquidateAsset } from '@/services/assetLifecycleService'
  import { showNotification } from '@/stores/notificationStore'

  export default {
    name: 'AssetListManage',
    components: { AppModal, AssetCreate, AssetEdit, AssetDetailModal, AssetLifecycleModal },
    data() {
      return {
        assets: [], page: 1, search: '', loading: false, uploading: false, exporting: false, totalCount: 0, pageSize: 10,
        showModal: false, modalMode: 'create', selectedAssetId: null, showDetailModal: false, selectedDetailAssetId: null,
        showLifecycleModal: false, lifecycleMode: 'beyondRepair', selectedLifecycleAsset: null, file: null, showDeleteConfirm: false, deleteAssetId: null,
      }
    },
    computed: {
      isLastPage() { return this.page * this.pageSize >= this.totalCount },
      totalPages() { return this.totalCount > 0 ? Math.ceil(this.totalCount / this.pageSize) : 1 },
    },
    mounted() { this.loadData() },
    methods: {
      inferType(asset) { return inferAssignmentType(asset) },
      formatStatus(status) { return formatAssetStatus(status) },
      statusClass(status) { return assetStatusClass(status) },
      assignmentBadge(type) { return type === 'DEPARTMENT' ? 'badge-department' : type === 'PERSONAL' ? 'badge-personal' : 'badge-unassigned' },
      canMarkBeyondRepair(asset) { return canMarkBeyondRepairStatus(asset) },
      canLiquidate(asset) { return canLiquidateStatus(asset) },
      openBeyondRepair(asset) { this.lifecycleMode = 'beyondRepair'; this.selectedLifecycleAsset = asset; this.showLifecycleModal = true },
      openLiquidation(asset) { this.lifecycleMode = 'liquidate'; this.selectedLifecycleAsset = asset; this.showLifecycleModal = true },
      async handleLifecycleSubmit(form) {
        try {
          if (!this.selectedLifecycleAsset?.id) return
          if (this.lifecycleMode === 'beyondRepair') { await markBeyondRepair(this.selectedLifecycleAsset.id, form.reason); showNotification('Success', 'Asset marked as beyond repair.', 'success') }
          else { await liquidateAsset(this.selectedLifecycleAsset.id, form); showNotification('Success', 'Asset liquidated successfully.', 'success') }
          this.showLifecycleModal = false; this.selectedLifecycleAsset = null; await this.loadData()
        } catch (error) { console.error(error); showNotification('Error', error?.response?.data?.message || 'Lifecycle action failed.', 'error') }
      },
      async loadData() {
        try {
          this.loading = true
          const res = await axios.get('/api/assets', { params: { page: this.page, search: this.search } })
          this.assets = (res.data.items || []).map((item) => ({ ...item, assignmentType: item.assignmentType || inferAssignmentType(item) }))
          this.totalCount = res.data.totalCount || this.assets.length
          this.pageSize = res.data.pageSize || 10
        } catch (error) { console.error(error); showNotification('Error', 'Không thể tải danh sách tài sản.', 'error') }
        finally { this.loading = false }
      },
      async applySearch() { this.page = 1; await this.loadData() },
      async nextPage() { if (this.isLastPage) return; this.page += 1; await this.loadData() },
      async prevPage() { if (this.page <= 1) return; this.page -= 1; await this.loadData() },
      openCreate() { this.modalMode = 'create'; this.selectedAssetId = null; this.showModal = true },
      openEdit(id) { this.modalMode = 'edit'; this.selectedAssetId = id; this.showModal = true },
      openDetail(assetId) { this.selectedDetailAssetId = assetId; this.showDetailModal = true },
      closeModal() { this.showModal = false; this.selectedAssetId = null },
      requestDelete(id) { this.deleteAssetId = id; this.showDeleteConfirm = true },
      closeDeleteConfirm() { this.deleteAssetId = null; this.showDeleteConfirm = false },
      async handleFormSuccess(message) { this.closeModal(); await this.loadData(); showNotification('Success', message || 'Thao tác thành công.', 'success') },
      handleFormError(message) { if (message) showNotification('Error', message, 'error') },
      async remove() {
        if (!this.deleteAssetId) return
        try {
          await axios.delete(`/api/assets/${this.deleteAssetId}`)
          if (this.assets.length === 1 && this.page > 1) this.page -= 1
          await this.loadData()
          showNotification('Success', 'Xóa tài sản thành công.', 'success')
        } catch (error) { console.error(error); showNotification('Error', error?.response?.data?.message || 'Xóa tài sản thất bại.', 'error') }
        finally { this.closeDeleteConfirm() }
      },
      triggerFile() { this.$refs.fileInput?.click() },
      handleFile(event) { const selectedFile = event.target.files?.[0] || null; if (!selectedFile) return; this.file = selectedFile; this.uploadFile() },
      async uploadFile() {
        if (!this.file) return
        const formData = new FormData(); formData.append('file', this.file)
        try {
          this.uploading = true
          const res = await axios.post('/api/admin/assets/import', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
          showNotification('Success', res.data?.message || 'Import Excel thành công.', 'success')
          this.file = null
          if (this.$refs.fileInput) this.$refs.fileInput.value = ''
          this.page = 1; await this.loadData()
        } catch (error) { console.error(error); showNotification('Error', error?.response?.data?.message || 'Import Excel thất bại.', 'error') }
        finally { this.uploading = false }
      },
      async exportExcel() {
        try {
          this.exporting = true
          const res = await axios.get('/api/admin/assets/export', { responseType: 'blob' })
          const blob = new Blob([res.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
          const url = window.URL.createObjectURL(blob)
          const link = document.createElement('a'); link.href = url; link.setAttribute('download', 'assets.xlsx'); document.body.appendChild(link); link.click(); link.remove(); window.URL.revokeObjectURL(url)
          showNotification('Success', 'Export Excel thành công.', 'success')
        } catch (error) { console.error(error); showNotification('Error', error?.response?.data?.message || 'Export Excel thất bại.', 'error') }
        finally { this.exporting = false }
      },
    },
  }
</script>

<style scoped>
  .asset-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .page-header h1 {
    margin: 0 0 6px;
    font-size: 28px;
    font-weight: 700;
    color: #1b2a41;
  }

  .page-header p {
    margin: 0;
    color: #6b7280;
  }

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

  .toolbar-actions {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
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

  .asset-table {
    width: 100%;
    border-collapse: collapse;
  }

    .asset-table th, .asset-table td {
      padding: 14px 12px;
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
      vertical-align: middle;
    }

    .asset-table th {
      background: #f9fafb;
      color: #374151;
      font-size: 14px;
      font-weight: 700;
    }

  .action-col {
    white-space: nowrap;
  }

  .status-badge, .assignment-badge {
    display: inline-flex;
    align-items: center;
    padding: 6px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
  }

  .status-available {
    background: #dcfce7;
    color: #166534;
  }

  .status-in-use {
    background: #dbeafe;
    color: #1d4ed8;
  }

  .status-shared {
    background: #ede9fe;
    color: #6d28d9;
  }

  .status-reported {
    background: #fef3c7;
    color: #92400e;
  }

  .status-broken, .status-lost {
    background: #fee2e2;
    color: #b91c1c;
  }

  .status-maintenance {
    background: #e0f2fe;
    color: #0f766e;
  }

  .status-beyond-repair {
    background: #e5e7eb;
    color: #334155;
  }

  .status-liquidated {
    background: #111827;
    color: #fff;
  }

  .badge-personal {
    background: #dbeafe;
    color: #1d4ed8;
  }

  .badge-department {
    background: #ede9fe;
    color: #6d28d9;
  }

  .badge-unassigned {
    background: #e5e7eb;
    color: #475569;
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

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }

  .btn-warning {
    background: #f59e0b;
    color: #fff;
  }

  .btn-danger, .btn-delete {
    background: #dc2626;
    color: #fff;
  }

  .empty-cell {
    text-align: center;
    color: #6b7280;
  }

  .hidden-input {
    display: none;
  }

  .confirm-text {
    margin: 0;
    color: #334155;
    line-height: 1.6;
  }
</style>

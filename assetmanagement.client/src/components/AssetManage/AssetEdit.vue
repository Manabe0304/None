<template>
  <div class="asset-form-shell">
    <form class="asset-form" @submit.prevent="updateAsset">
      <div class="form-grid">
        <div class="form-group"><label>Asset Tag</label><input v-model.trim="asset.assetTag" type="text" placeholder="Enter asset tag" /><span class="error-text" v-if="errors.assetTag">{{ errors.assetTag }}</span></div>
        <div class="form-group"><label>Category</label><input v-model.trim="asset.category" type="text" placeholder="Enter category" /><span class="error-text" v-if="errors.category">{{ errors.category }}</span></div>
        <div class="form-group"><label>Model</label><input v-model.trim="asset.model" type="text" placeholder="Enter model" /></div>
        <div class="form-group"><label>Serial Number</label><input v-model.trim="asset.serialNumber" type="text" placeholder="Enter serial number" /><span class="error-text" v-if="errors.serialNumber">{{ errors.serialNumber }}</span></div>
        <div class="form-group"><label>Purchase Date</label><input v-model="asset.purchaseDate" type="date" /><span class="error-text" v-if="errors.purchaseDate">{{ errors.purchaseDate }}</span></div>
        <div class="form-group"><label>Purchase Value</label><input v-model.number="asset.purchaseValue" type="number" min="0" step="0.01" placeholder="Enter purchase value" /><span class="error-text" v-if="errors.purchaseValue">{{ errors.purchaseValue }}</span></div>
        <div class="form-group"><label>Assignment Target</label><select v-model="asset.assignmentType"><option value="UNASSIGNED">Unassigned</option><option value="DEPARTMENT">Department Shared</option><option value="PERSONAL">Personal</option></select></div>
        <div class="form-group"><label>Status</label><select v-model="asset.status"><option v-for="status in statuses" :key="status" :value="status">{{ status }}</option></select><small class="helper-text">Use dedicated lifecycle actions for BEYOND_REPAIR and LIQUIDATED.</small></div>
        <div class="form-group form-group-full"><label>Department</label><select v-model="asset.departmentId"><option value="">No department</option><option v-for="department in departments" :key="department.id" :value="department.id">{{ department.name }}</option></select><span class="error-text" v-if="errors.departmentId">{{ errors.departmentId }}</span></div>
      </div>
      <div class="form-actions"><button type="button" class="btn btn-secondary" @click="$emit('cancel')">Cancel</button><button type="submit" class="btn btn-primary" :disabled="loading">{{ loading ? 'Updating...' : 'Update Asset' }}</button></div>
    </form>
  </div>
</template>

<script>
  import axios from 'axios'
  import { NON_TERMINAL_FORM_STATUSES, inferAssignmentType, ASSET_STATUS } from '@/constants/assetStatus'

  export default {
    name: 'AssetEdit', props: { assetId: { type: String, default: '' } }, emits: ['cancel', 'success', 'error'],
    data() { return { asset: { assetTag: '', category: '', model: '', serialNumber: '', purchaseDate: '', purchaseValue: 0, status: ASSET_STATUS.AVAILABLE, departmentId: '', currentUserId: null, assignmentType: 'UNASSIGNED' }, errors: {}, loading: false, departments: [], statuses: NON_TERMINAL_FORM_STATUSES } },
    computed: { resolvedId() { return this.assetId || this.$route?.params?.id || null } },
    async mounted() { await Promise.all([this.loadDepartments(), this.loadAsset()]) },
    watch: {
      'asset.assignmentType'(value) { if (value === 'DEPARTMENT') this.asset.status = ASSET_STATUS.IN_USE_SHARED; else if (value === 'UNASSIGNED') this.asset.status = ASSET_STATUS.AVAILABLE },
    },
    methods: {
      async loadDepartments() { try { const res = await axios.get('/api/departments'); this.departments = res.data || [] } catch (error) { console.error(error); this.$emit('error', 'Không tải được danh sách phòng ban.') } },
      async loadAsset() {
        try {
          const res = await axios.get(`/api/assets/${this.resolvedId}`)
          this.asset = { assetTag: res.data.assetTag || '', category: res.data.category || '', model: res.data.model || '', serialNumber: res.data.serialNumber || '', purchaseDate: res.data.purchaseDate ? res.data.purchaseDate.split('T')[0] : '', purchaseValue: res.data.purchaseValue ?? 0, status: NON_TERMINAL_FORM_STATUSES.includes(res.data.status) ? res.data.status : ASSET_STATUS.AVAILABLE, departmentId: res.data.departmentId || '', currentUserId: res.data.currentUserId || null, assignmentType: inferAssignmentType(res.data) }
        } catch (error) { console.error(error); this.$emit('error', 'Không tải được dữ liệu tài sản.') }
      },
      validateForm() {
        this.errors = {}
        if (!this.asset.assetTag) this.errors.assetTag = 'Asset Tag is required'
        if (!this.asset.category) this.errors.category = 'Category is required'
        if (!this.asset.serialNumber) this.errors.serialNumber = 'Serial number is required'
        if (!this.asset.purchaseDate) this.errors.purchaseDate = 'Purchase date is required'
        if (this.asset.purchaseValue === '' || this.asset.purchaseValue === null || this.asset.purchaseValue === undefined) this.errors.purchaseValue = 'Purchase value is required'
        if (this.asset.assignmentType === 'DEPARTMENT' && !this.asset.departmentId) this.errors.departmentId = 'Department is required for shared assets'
        return Object.keys(this.errors).length === 0
      },
      async updateAsset() {
        if (!this.validateForm()) { this.$emit('error', 'Vui lòng kiểm tra lại thông tin tài sản.'); return }
        this.loading = true
        try { await axios.put(`/api/assets/${this.resolvedId}`, this.asset); this.$emit('success', 'Cập nhật tài sản thành công.') }
        catch (error) { console.error(error); this.$emit('error', error?.response?.data?.message || 'Cập nhật tài sản thất bại.') }
        finally { this.loading = false }
      },
    },
  }
</script>

<style scoped>
  .asset-form-shell {
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  .asset-form {
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

  .form-group-full {
    grid-column: 1 / -1;
  }

  .form-group label {
    font-weight: 600;
    color: #374151;
  }

  .form-group input, .form-group select {
    border: 1px solid #d1d5db;
    border-radius: 10px;
    padding: 12px;
    font-size: 14px;
    outline: none;
  }

  .helper-text {
    color: #6b7280;
    font-size: 12px;
  }

  .error-text {
    color: #dc2626;
    font-size: 13px;
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

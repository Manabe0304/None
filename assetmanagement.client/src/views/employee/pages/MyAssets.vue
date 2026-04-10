<template>
  <div class="my-assets-page">
    <div class="page-header">
      <div>
        <h1>My Assets</h1>
        <p>View assigned assets, report issues, and request returns.</p>
      </div>
    </div>

    <div class="asset-grid" v-if="assets.length">
      <div v-for="asset in assets" :key="asset.assignmentId" class="asset-card">
        <div class="asset-card-header">
          <h3>{{ asset.assetTag }}</h3>
          <span class="status-pill" :class="statusClass(asset.status)">{{ formatStatus(asset.status) }}</span>
        </div>
        <div class="asset-meta">
          <p><strong>Model:</strong> {{ asset.model || '—' }}</p>
          <p><strong>Category:</strong> {{ asset.category || '—' }}</p>
          <p><strong>Serial:</strong> {{ asset.serialNumber || '—' }}</p>
          <p><strong>Assigned At:</strong> {{ formatDate(asset.assignedAt) }}</p>
        </div>
        <div class="asset-actions">
          <button class="btn btn-secondary" @click="openDetail(asset)">Detail</button>
          <button class="btn btn-warning" :disabled="isBlocked(asset)" :class="{ 'highlight-processed': isBlocked(asset) }" @click="openReport(asset, 'BROKEN')">
            {{ asset.hasOpenBrokenReport ? 'Reported Issue' : 'Report Issue' }}
          </button>
          <button class="btn btn-danger" :disabled="isBlocked(asset)" :class="{ 'highlight-processed': isBlocked(asset) }" @click="openReport(asset, 'LOST')">
            {{ asset.hasOpenBrokenReport ? 'Reported Lost' : 'Report Lost' }}
          </button>
          <button class="btn btn-primary" :disabled="isBlocked(asset) || !!asset.openReturnRequestStatus || isTerminalStatus(asset.status)" :class="{ 'highlight-processed': isBlocked(asset) || !!asset.openReturnRequestStatus }" @click="openReturn(asset)">
            {{ isBlocked(asset) || !!asset.openReturnRequestStatus ? 'Return Processed' : 'Return' }}
          </button>
        </div>
      </div>
    </div>

    <div v-else-if="loading" class="empty-state">Loading assets...</div>
    <div v-else class="empty-state">No assigned assets.</div>

    <AppModal v-model="showReportModal" :title="reportType === 'LOST' ? 'Report Lost Asset' : 'Report Asset Issue'" width="620px">
      <template #body>
        <div class="report-shell">
          <div class="report-summary">
            <p><strong>Asset:</strong> {{ selectedAsset?.assetTag }}</p>
            <p><strong>Type:</strong> {{ reportType }}</p>
          </div>
          <label class="form-label">Reason</label>
          <textarea v-model.trim="description" rows="5" placeholder="Describe the issue clearly..." />
        </div>
      </template>
      <template #footer>
        <button class="btn btn-secondary" :disabled="submitting" @click="closeReportModal">Cancel</button>
        <button class="btn" :class="reportType === 'LOST' ? 'btn-danger' : 'btn-primary'" :disabled="submitting" @click="submitReport">
          {{ submitting ? 'Submitting...' : reportType === 'LOST' ? 'Submit Lost Report' : 'Submit Report' }}
        </button>
      </template>
    </AppModal>

    <InitiateReturnModal v-if="showReturnModal" :assignment="selectedReturnAssignment" @close="showReturnModal = false" @success="handleReturnSuccess" />
    <AssetDetailModal v-if="showDetailModal" :asset-id="selectedDetailAssetId" @close="showDetailModal = false" />
  </div>
</template>

<script>
  import axios from 'axios'
  import AppModal from '@/components/common/AppModal.vue'
  import AssetDetailModal from '@/components/assets/AssetDetailModal.vue'
  import InitiateReturnModal from '@/features/returns/components/InitiateReturnModal.vue'
  import { formatAssetStatus, assetStatusClass, isTerminalAssetStatus } from '@/constants/assetStatus'
  import { showNotification } from '@/stores/notificationStore'

  const API_BASE = '/api'

  function getToken() {
    const token = localStorage.getItem('token')
    if (token) return token
    try {
      const user = JSON.parse(localStorage.getItem('user'))
      return user?.token || user?.accessToken || null
    } catch {
      return null
    }
  }

  export default {
    name: 'ViewMyAsset',
    components: { AppModal, AssetDetailModal, InitiateReturnModal },
    data() {
      return {
        assets: [], selectedAsset: null, selectedReturnAssignment: null, selectedDetailAssetId: null,
        description: '', reportType: 'BROKEN', loading: false, submitting: false,
        showReportModal: false, showReturnModal: false, showDetailModal: false,
      }
    },
    mounted() { this.loadAssets() },
    methods: {
      isBlocked(asset) {
        return asset.hasOpenBrokenReport || asset.status === 'LOST' || asset.status === 'BROKEN' || !!asset.openReturnRequestStatus;
      },
      isTerminalStatus(status) { return isTerminalAssetStatus(status) },
      async loadAssets() {
        try {
          this.loading = true
          const token = getToken()
          const res = await axios.get(`${API_BASE}/my-assets`, { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } })
          this.assets = res.data.data || []
        } catch (err) {
          console.error('Load assets error:', err)
          showNotification('Error', 'Failed to load your assets.', 'error')
        } finally { this.loading = false }
      },
      openReport(asset, type = 'BROKEN') { this.selectedAsset = asset; this.reportType = type; this.description = ''; this.showReportModal = true },
      closeReportModal() { if (this.submitting) return; this.showReportModal = false; this.selectedAsset = null; this.description = '' },
      openReturn(asset) {
        this.selectedReturnAssignment = { id: asset.assignmentId, assetId: asset.assetId, assetTag: asset.assetTag, assetName: asset.model || asset.assetTag, assetCategory: asset.category, assignedAt: asset.assignedAt, status: asset.status }
        this.showReturnModal = true
      },
      openDetail(asset) { this.selectedDetailAssetId = asset.assetId; this.showDetailModal = true },
      async submitReport() {
        if (!this.description.trim()) {
          showNotification('Error', 'Please enter reason.', 'error')
          return
        }
        try {
          this.submitting = true
          const token = getToken()
          await axios.post(`${API_BASE}/broken-reports`, { assetId: this.selectedAsset.assetId, description: this.description, issueType: this.reportType }, { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } })
          this.closeReportModal()
          await this.loadAssets()
          showNotification('Success', this.reportType === 'LOST' ? 'Lost asset report submitted.' : 'Issue report submitted.', 'success')
        } catch (err) {
          console.error(err)
          showNotification('Error', err?.response?.data?.message || 'Submit failed!', 'error')
        } finally { this.submitting = false }
      },
      async handleReturnSuccess() { this.showReturnModal = false; await this.loadAssets(); showNotification('Success', 'Return request submitted', 'success') },
      formatDate(date) { return date ? new Date(date).toLocaleString('vi-VN') : '' },
      formatStatus(status) { return formatAssetStatus(status) },
      statusClass(status) { return assetStatusClass(status) },
    },
  }
</script>

<style scoped>
  .my-assets-page {
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .page-header h1 {
    margin: 0 0 6px;
  }

  .page-header p {
    margin: 0;
    color: #6b7280;
  }

  .asset-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 16px;
  }

  .asset-card {
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    padding: 18px;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
  }

  .asset-card-header {
    display: flex;
    justify-content: space-between;
    gap: 10px;
    align-items: flex-start;
    margin-bottom: 12px;
  }

    .asset-card-header h3 {
      margin: 0;
    }

  .asset-meta p {
    margin: 6px 0;
    color: #334155;
  }

  .asset-actions {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 16px;
  }

  .status-pill {
    display: inline-flex;
    align-items: center;
    border-radius: 999px;
    padding: 6px 10px;
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

  .btn-warning {
    background: #f59e0b;
    color: #fff;
  }

  .btn-danger {
    background: #dc2626;
    color: #fff;
  }

  .empty-state {
    text-align: center;
    color: #6b7280;
    padding: 32px;
    background: #fff;
    border-radius: 14px;
  }

  .highlight-processed {
    background: #9ca3af !important;
    color: white !important;
    cursor: not-allowed;
    opacity: 0.7;
  }

  .report-shell {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .report-summary {
    padding: 12px 14px;
    border-radius: 12px;
    background: #f8fafc;
  }

    .report-summary p {
      margin: 4px 0;
    }

  .form-label {
    font-weight: 600;
    color: #374151;
  }

  textarea {
    border: 1px solid #d1d5db;
    border-radius: 10px;
    padding: 12px;
    font: inherit;
  }
</style>

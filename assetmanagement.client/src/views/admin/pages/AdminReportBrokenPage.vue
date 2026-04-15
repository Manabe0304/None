  <template>
  <div class="container py-4 admin-broken-page">
    <div class="page-header d-flex justify-content-between align-items-end gap-3 mb-4">
      <div>
        <h2>Issue Reports</h2>
        <p>Review broken and lost asset reports from employees.</p>
      </div>

      <button class="btn btn-primary btn-sm" @click="loadReports" :disabled="loading">
        {{ loading ? 'Loading...' : 'Refresh' }}
      </button>
    </div>

    <div class="toolbar mb-4">
      <input v-model="search"
             class="form-control"
             placeholder="Search by asset tag or reason..."
             @input="loadReports" />
    </div>

    <div class="table-wrapper table-responsive">
      <table class="table align-middle">
        <thead>
          <tr>
            <th>Asset</th>
            <th>Issue Type</th>
            <th>Description</th>
            <th>Status</th>
            <th>Created</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="r in reports" :key="r.id">
            <td>{{ r.assetTag }}</td>

            <td>
              <span class="type-pill" :class="r.issueType === 'LOST' ? 'type-lost' : 'type-broken'">
                {{ r.issueType }}
              </span>
            </td>

            <td>{{ r.description }}</td>

            <td>
              <span class="status-badge" :class="getStatusClass(r.status)">
                {{ r.status || '—' }}
              </span>
            </td>

            <td>{{ formatDate(r.createdAt) }}</td>

            <td>
              <div class="action-group d-flex gap-2" v-if="r.status === 'OPEN'">
                <button class="btn btn-outline-secondary btn-sm" @click="openDetail(r.assetId)">Detail</button>
                <button class="btn btn-success btn-sm" @click="acceptReport(r.id)">Approve</button>
                <button class="btn btn-danger btn-sm" @click="openRejectModal(r)">Reject</button>
              </div>

              <span v-else>{{ r.closureReason || '—' }}</span>
            </td>
          </tr>

          <tr v-if="!loading && !reports.length">
            <td colspan="6" class="empty">No reports found.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <AppModal v-model="showRejectModal" title="Reject issue report" width="520px">
      <template #body>
        <label class="reason-label">Reason</label>
        <textarea v-model.trim="rejectReason"
                  rows="4"
                  placeholder="Optional reject reason"
                  class="form-control" />
      </template>

      <template #footer>
        <button class="btn btn-outline-secondary btn-sm" @click="closeRejectModal">Cancel</button>
        <button class="btn btn-danger btn-sm" @click="rejectReport">Reject</button>
      </template>
    </AppModal>

    <AssetDetailModal v-if="showDetailModal"
                      :asset-id="selectedDetailAssetId"
                      @close="showDetailModal = false" />
  </div>
</template>

  <script>
    import axios from 'axios'
    import AppModal from '@/components/common/AppModal.vue'
    import AssetDetailModal from '@/components/assets/AssetDetailModal.vue'
    import { showNotification } from '@/stores/notificationStore'

    const API = '/api/admin/broken-reports'

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
      name: 'AdminReportBrokenPage',
      components: { AppModal, AssetDetailModal },
      data() {
        return { reports: [], search: '', loading: false, showRejectModal: false, selectedReport: null, rejectReason: '', showDetailModal: false, selectedDetailAssetId: null }
      },
      mounted() { this.loadReports() },
      methods: {
        openDetail(assetId) {
          this.selectedDetailAssetId = assetId;
          this.showDetailModal = true;
        },

        formatDate(value) {
          return value ? new Date(value).toLocaleString('vi-VN') : '—';
        },

        normalizeStatus(value) {
          return (value ?? '').toString().trim().toUpperCase();
        },

        getStatusClass(status) {
          const s = this.normalizeStatus(status);

          if (['OPEN', 'PENDING'].includes(s)) return 'status-open';
          if (['IN_PROGRESS', 'PROCESSING', 'UNDER_REVIEW'].includes(s)) return 'status-processing';
          if (['APPROVED', 'RESOLVED', 'COMPLETED'].includes(s)) return 'status-approved';
          if (['REJECTED', 'CANCELLED', 'CANCELED'].includes(s)) return 'status-rejected';

          return 'status-neutral';
        },

        async loadReports() {
          try {
            this.loading = true
            const token = getToken()
            if (!token) { this.reports = []; showNotification('Error', 'Missing token. Please login again.', 'error'); return }
            const res = await axios.get(API, { params: { search: this.search || '' }, headers: { Authorization: `Bearer ${token}` } })
            this.reports = res.data?.data || []
          } catch (error) {
            console.error('Load issue reports failed:', error)
            this.reports = []
            showNotification('Error', error?.response?.data?.message || 'Failed to load issue reports.', 'error')
          } finally { this.loading = false }
        },
        async acceptReport(reportId) {
          try {
            const token = getToken()
            if (!token) { showNotification('Error', 'Missing token. Please login again.', 'error'); return }
            await axios.post(`${API}/${reportId}/accept`, {}, { headers: { Authorization: `Bearer ${token}` } })
            await this.loadReports()
            showNotification('Success', 'Approved successfully', 'success')
          } catch (error) {
            console.error('Accept issue report failed:', error)
            showNotification('Error', error?.response?.data?.message || 'Approve failed.', 'error')
          }
        },
        openRejectModal(report) { this.selectedReport = report; this.rejectReason = ''; this.showRejectModal = true },
        closeRejectModal() { this.selectedReport = null; this.rejectReason = ''; this.showRejectModal = false },
        async rejectReport() {
          if (!this.selectedReport) return
          try {
            const token = getToken()
            if (!token) { showNotification('Error', 'Missing token. Please login again.', 'error'); return }
            await axios.post(`${API}/${this.selectedReport.id}/reject`, { reason: this.rejectReason }, { headers: { Authorization: `Bearer ${token}` } })
            await this.loadReports()
            showNotification('Success', 'Rejected successfully', 'success')
          } catch (error) {
            console.error('Reject issue report failed:', error)
            showNotification('Error', error?.response?.data?.message || 'Reject failed.', 'error')
          } finally { this.closeRejectModal() }
        },
      },
    }
  </script>

  <style scoped>
    .container {
      box-sizing: border-box;
      width: 100%;
      padding: 24px;
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: end;
      gap: 16px;
      margin-bottom: 20px;
    }

      .page-header h2 {
        margin: 0 0 6px;
      }

      .page-header p {
        margin: 0;
        color: #64748b;
      }

    .toolbar input,
    .toolbar .form-control {
      width: min(340px, 100%);
      padding: 10px 14px;
      border-radius: 8px;
      border: 1px solid #ddd;
      margin-bottom: 20px;
    }

    .table-wrapper,
    .table-responsive {
      overflow-x: auto;
      width: 100%;
    }

    table,
    .table {
      width: 100%;
      border-collapse: collapse;
      overflow: hidden;
      border-radius: 10px;
    }

    .action-group {
      display: flex;
      gap: 8px;
    }

    .detail-btn {
      background: #e5e7eb;
      color: #111827;
    }

    thead {
      background: #f5f7fa;
    }

    th, td {
      text-align: left;
      padding: 14px;
      border-top: 1px solid #eee;
      color: #444;
      vertical-align: top;
    }

    tbody tr:hover {
      background: #f9fbff;
    }

    .action-group {
      display: flex;
      gap: 8px;
    }

    .btn,
    button,
    .refresh-btn,
    .cancel-btn {
      padding: 8px 12px;
      border-radius: 8px;
      border: none;
      cursor: pointer;
      font-weight: 600;
    }

    .accept-btn, .refresh-btn {
      background: #16a34a;
      color: white;
    }

    .reject-btn {
      background: #dc2626;
      color: white;
    }

    .cancel-btn {
      background: #e5e7eb;
      color: #111827;
    }

    .type-pill {
      display: inline-flex;
      padding: 4px 10px;
      border-radius: 999px;
      font-size: 12px;
      font-weight: 700;
    }

    .type-broken {
      background: #fef3c7;
      color: #92400e;
    }

    .type-lost {
      background: #fee2e2;
      color: #b91c1c;
    }

    .reason-label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
    }

    textarea,
    .form-control {
      width: 100%;
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 12px;
      font: inherit;
    }

    .empty {
      text-align: center;
      color: #94a3b8;
    }
  </style>

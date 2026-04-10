<template>
  <div class="history-page">
    <div class="page-header">
      <div>
        <h1>Assignment History</h1>
        <p>Trace asset / employee assignment events over time.</p>
      </div>
      <button class="refresh-btn" @click="loadHistory" :disabled="loading">
        {{ loading ? "Loading..." : "Refresh" }}
      </button>
    </div>

    <div class="card">
      <div class="filter-grid">
        <div class="form-group">
          <label>Asset</label>
          <input v-model="filters.asset" type="text" placeholder="Search by asset tag or model..." />
        </div>

        <div class="form-group">
          <label>Employee</label>
          <input v-model="filters.employee" type="text" placeholder="Search employee..." />
        </div>

        <div class="form-group">
          <label>Department</label>
          <input v-model="filters.department" type="text" placeholder="Search department..." />
        </div>

        <div class="form-group">
          <label>Asset Type</label>
          <input v-model="filters.assetType" type="text" placeholder="Search asset type..." />
        </div>

        <div class="form-group">
          <label>From Date</label>
          <input v-model="filters.fromDate" type="date" />
        </div>

        <div class="form-group">
          <label>To Date</label>
          <input v-model="filters.toDate" type="date" />
        </div>
      </div>

      <div class="filter-actions">
        <button class="btn btn-primary" @click="loadHistory" :disabled="loading">Apply Filters</button>
        <button class="btn btn-secondary" @click="resetFilters" :disabled="loading">Reset</button>
      </div>
    </div>

    <div class="card">
      <div v-if="loading" class="empty-state">Loading assignment history...</div>

      <div v-else-if="history.length === 0" class="empty-state">
        No assignment history found.
      </div>

      <div v-else class="table-scroll">
        <table class="history-table">
          <thead>
            <tr>
              <th>Changed At</th>
              <th>Asset</th>
              <th>Employee</th>
              <th>Department</th>
              <th>Action</th>
              <th>Changed By</th>
              <th class="action-col">Detail</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="item in history" :key="item.id">
              <td>{{ formatDate(item.changedAt) }}</td>
              <td>{{ item.assetTag || "—" }}</td>
              <td>{{ item.employee || "—" }}</td>
              <td>{{ item.department || "—" }}</td>
              <td>
                <span class="badge" :class="badgeClass(item.action)">
                  {{ shortText(item.action, 18) }}
                </span>
              </td>
              <td>{{ item.changedBy || "—" }}</td>
              <td class="action-col">
                <button class="detail-btn" @click="openDetail(item)">
                  View Detail
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AssetHistoryDetailModal v-model="detailOpen"
                             :item="selectedItem" />
  </div>
</template>

<script setup>
  import { onMounted, reactive, ref } from "vue"
  import { getAssignmentHistory } from "@/services/assignmentService"
  import { showNotification } from "@/stores/notificationStore"
  import AssetHistoryDetailModal from "@/views/admin/components/AssetHistoryDetailModal.vue"

  const loading = ref(false)
  const history = ref([])
  const detailOpen = ref(false)
  const selectedItem = ref(null)

  const filters = reactive({
    asset: "",
    employee: "",
    department: "",
    assetType: "",
    fromDate: "",
    toDate: ""
  })

  function formatDate(dateValue) {
    if (!dateValue) return "N/A"
    const date = new Date(dateValue)
    if (Number.isNaN(date.getTime())) return "N/A"
    return date.toLocaleString("vi-VN")
  }

  function shortText(value, max = 18) {
    const text = String(value || "—")
    if (text.length <= max) return text
    return `${text.slice(0, max)}...`
  }

  function badgeClass(action) {
    switch (String(action || "").toUpperCase()) {
      case "ASSIGNED":
        return "badge-assigned"
      case "RETURNED":
        return "badge-returned"
      case "CANCELLED":
        return "badge-cancelled"
      default:
        return "badge-default"
    }
  }

  async function loadHistory() {
    try {
      loading.value = true
      history.value = await getAssignmentHistory({
        asset: filters.asset,
        employee: filters.employee,
        department: filters.department,
        assetType: filters.assetType,
        fromDate: filters.fromDate,
        toDate: filters.toDate
      })
    } catch (error) {
      history.value = []
      showNotification(
        "Error",
        error?.response?.data?.message || "Failed to fetch assignment history.",
        "error"
      )
    } finally {
      loading.value = false
    }
  }

  function resetFilters() {
    filters.asset = ""
    filters.employee = ""
    filters.department = ""
    filters.assetType = ""
    filters.fromDate = ""
    filters.toDate = ""
    loadHistory()
  }

  function openDetail(item) {
    selectedItem.value = item
    detailOpen.value = true
  }

  onMounted(loadHistory)
</script>

<style scoped>
  .history-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding: 24px;
  }

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: end;
    gap: 16px;
  }

    .page-header h1 {
      margin: 0;
      font-size: 28px;
      color: #1b2a41;
    }

    .page-header p {
      margin-top: 6px;
      color: #6b7280;
    }

  .card {
    background: #fff;
    border-radius: 14px;
    padding: 20px;
    border: 1px solid #e5e7eb;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
  }

  .filter-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
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

    .form-group input {
      height: 42px;
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 0 12px;
      outline: none;
    }

  .filter-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 16px;
  }

  .btn,
  .refresh-btn,
  .detail-btn {
    height: 40px;
    border: none;
    font-weight: 700;
    border-radius: 10px;
    padding: 0 16px;
    cursor: pointer;
  }

  .btn-primary,
  .refresh-btn {
    background: #2563eb;
    color: #fff;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }

  .detail-btn {
    background: #eff6ff;
    color: #1d4ed8;
    height: 34px;
    padding: 0 12px;
    font-size: 13px;
  }

  .table-scroll {
    overflow-x: auto;
  }

  .history-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
    min-width: 960px;
  }

    .history-table th,
    .history-table td {
      padding: 14px 12px;
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
      vertical-align: middle;
    }

    .history-table th {
      background: #f8fafc;
      color: #374151;
      font-weight: 700;
    }

  .ellipsis-text {
    max-width: 180px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .status-summary {
    color: #475569;
    font-weight: 600;
  }

  .badge {
    display: inline-flex;
    align-items: center;
    max-width: 160px;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .badge-assigned {
    background: #dbeafe;
    color: #1d4ed8;
  }

  .badge-returned {
    background: #dcfce7;
    color: #166534;
  }

  .badge-cancelled {
    background: #fee2e2;
    color: #b91c1c;
  }

  .badge-default {
    background: #e5e7eb;
    color: #374151;
  }

  .action-col {
    width: 120px;
  }

  .empty-state {
    text-align: center;
    color: #6b7280;
    padding: 20px;
  }

  @media (max-width: 900px) {
    .filter-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

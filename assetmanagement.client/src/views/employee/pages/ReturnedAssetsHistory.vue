<template>
  <div class="returned-history-page">
    <section class="history-card">
      <div class="history-header">
        <div>
          <h3>Returned Asset History</h3>
          <p>
            Review assets you have returned or assets collected from you by IT.
          </p>
        </div>
      </div>

      <div class="filters">
        <div class="field">
          <input v-model="searchKeyword"
                 type="text"
                 class="text-input"
                 placeholder="Search by asset tag, asset type, or reason..." />
        </div>

        <div class="field field-select">
          <select v-model="selectedStatus" class="select-input">
            <option value="">All statuses</option>
            <option value="PENDING">PENDING</option>
            <option value="APPROVED">APPROVED</option>
            <option value="REJECTED">REJECTED</option>
            <option value="COMPLETED">COMPLETED</option>
            <option value="COLLECTED">COLLECTED</option>
          </select>
        </div>

        <div class="field field-button">
          <button class="btn btn-secondary" @click="resetFilters">
            Reset Filters
          </button>
        </div>
      </div>

      <div v-if="loading" class="state-block">
        <div class="loader"></div>
        <p>Loading returned asset history...</p>
      </div>

      <div v-else-if="filteredItems.length === 0" class="state-block">
        <i class="fa-solid fa-box-open empty-icon"></i>
        <h5>No returned asset history found</h5>
        <p>You do not have any returned asset records yet.</p>
      </div>

      <div v-else class="table-shell">
        <table class="history-table">
          <thead>
            <tr>
              <th>Asset Tag</th>
              <th>Asset Type</th>
              <th>Return Initiated Date</th>
              <th>Return Reason</th>
              <th>Status</th>
              <th>Processed Date</th>
              <th class="align-right">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="item in filteredItems" :key="item.id">
              <td>{{ item.assetTag || "-" }}</td>
              <td>{{ item.assetType || "-" }}</td>
              <td>{{ formatDate(item.returnInitiatedDate) }}</td>
              <td>{{ item.returnReason || "-" }}</td>
              <td>
                <span class="status-chip" :class="getStatusClass(item.status)">
                  {{ item.status || "-" }}
                </span>
              </td>
              <td>{{ formatDate(item.processedDate) }}</td>
              <td class="align-right">
                <button class="btn btn-outline" @click="openDetail(item.id)">
                  View
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="errorMessage" class="message message-error">
        {{ errorMessage }}
      </div>
    </section>

    <Teleport to="body">
      <div v-if="showDetailModal"
           class="detail-modal-overlay"
           @click.self="closeDetailModal">
        <div class="detail-modal"
             role="dialog"
             aria-modal="true"
             aria-labelledby="returnHistoryDetailModalLabel">
          <div class="detail-modal-header">
            <h5 id="returnHistoryDetailModalLabel">Returned Asset Detail</h5>
            <button type="button"
                    class="close-button"
                    aria-label="Close"
                    @click="closeDetailModal">
              ×
            </button>
          </div>

          <div class="detail-modal-body">
            <div v-if="detailLoading" class="state-block state-block-small">
              <div class="loader"></div>
              <p>Loading detail...</p>
            </div>

            <div v-else-if="selectedDetail" class="detail-grid">
              <div class="detail-item">
                <label>Asset Tag</label>
                <div>{{ selectedDetail.assetTag || "-" }}</div>
              </div>

              <div class="detail-item">
                <label>Asset Type</label>
                <div>{{ selectedDetail.assetType || "-" }}</div>
              </div>

              <div class="detail-item">
                <label>Return Initiated Date</label>
                <div>{{ formatDate(selectedDetail.returnInitiatedDate) }}</div>
              </div>

              <div class="detail-item">
                <label>Processed Date</label>
                <div>{{ formatDate(selectedDetail.processedDate) }}</div>
              </div>

              <div class="detail-item">
                <label>Status</label>
                <div>
                  <span class="status-chip"
                        :class="getStatusClass(selectedDetail.status)">
                    {{ selectedDetail.status || "-" }}
                  </span>
                </div>
              </div>

              <div class="detail-item">
                <label>Handled By</label>
                <div>{{ selectedDetail.handledByName || "-" }}</div>
              </div>

              <div class="detail-item detail-item-full">
                <label>Return Reason</label>
                <div>{{ selectedDetail.returnReason || "-" }}</div>
              </div>

              <div class="detail-item detail-item-full">
                <label>Handling Notes</label>
                <div>{{ selectedDetail.handlingNotes || "-" }}</div>
              </div>

              <div class="detail-item detail-item-full">
                <label>Initial Reception Result</label>
                <div>{{ selectedDetail.initialReceptionResult || "-" }}</div>
              </div>

              <div class="detail-item">
                <label>Assignment ID</label>
                <div>{{ selectedDetail.assignmentId || "-" }}</div>
              </div>

              <div class="detail-item">
                <label>Assigned At</label>
                <div>{{ formatDate(selectedDetail.assignedAt) }}</div>
              </div>

              <div class="detail-item">
                <label>Returned At</label>
                <div>{{ formatDate(selectedDetail.returnedAt) }}</div>
              </div>
            </div>

            <div v-else class="empty-detail">No detail available.</div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
  import { computed, onBeforeUnmount, onMounted, ref } from "vue"
  import {
    getMyReturnedHistory,
    getMyReturnedHistoryDetail,
  } from "../services/employeeReturn.service"

  const items = ref([])
  const loading = ref(false)
  const errorMessage = ref("")
  const searchKeyword = ref("")
  const selectedStatus = ref("")

  const selectedDetail = ref(null)
  const detailLoading = ref(false)
  const showDetailModal = ref(false)

  onMounted(async () => {
    await loadData()
    document.addEventListener("keydown", handleEscape)
  })

  onBeforeUnmount(() => {
    document.removeEventListener("keydown", handleEscape)
    unlockBodyScroll()
  })

  const loadData = async () => {
    loading.value = true
    errorMessage.value = ""

    try {
      items.value = await getMyReturnedHistory()
    } catch (error) {
      errorMessage.value =
        error?.response?.data?.message || "Failed to load returned asset history"
    } finally {
      loading.value = false
    }
  }

  const resetFilters = () => {
    searchKeyword.value = ""
    selectedStatus.value = ""
  }

  const filteredItems = computed(() => {
    const keyword = searchKeyword.value.trim().toLowerCase()

    return items.value.filter((item) => {
      const matchesKeyword =
        !keyword ||
        item.assetTag?.toLowerCase().includes(keyword) ||
        item.assetType?.toLowerCase().includes(keyword) ||
        item.returnReason?.toLowerCase().includes(keyword)

      const matchesStatus =
        !selectedStatus.value || item.status === selectedStatus.value

      return matchesKeyword && matchesStatus
    })
  })

  const openDetail = async (id) => {
    detailLoading.value = true
    selectedDetail.value = null
    showDetailModal.value = true
    lockBodyScroll()

    try {
      selectedDetail.value = await getMyReturnedHistoryDetail(id)
    } catch (error) {
      errorMessage.value =
        error?.response?.data?.message || "Failed to load return record detail"
      closeDetailModal()
    } finally {
      detailLoading.value = false
    }
  }

  const closeDetailModal = () => {
    showDetailModal.value = false
    selectedDetail.value = null
    detailLoading.value = false
    unlockBodyScroll()
  }

  const handleEscape = (event) => {
    if (event.key === "Escape" && showDetailModal.value) {
      closeDetailModal()
    }
  }

  const lockBodyScroll = () => {
    document.body.style.overflow = "hidden"
  }

  const unlockBodyScroll = () => {
    document.body.style.overflow = ""
  }

  const formatDate = (value) => {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return "-"

    return date.toLocaleString()
  }

  const getStatusClass = (status) => {
    switch (status) {
      case "PENDING":
        return "status-warning"
      case "APPROVED":
        return "status-info"
      case "REJECTED":
        return "status-danger"
      case "COMPLETED":
        return "status-success"
      case "COLLECTED":
        return "status-primary"
      default:
        return "status-muted"
    }
  }
</script>

<style scoped>
  .returned-history-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .history-card {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
    padding: 24px;
  }

  .history-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 20px;
  }

    .history-header h3 {
      margin: 0 0 8px;
      font-size: 26px;
      font-weight: 700;
      color: #1f2937;
    }

    .history-header p {
      margin: 0;
      color: #6b7280;
      line-height: 1.5;
    }

  .filters {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 220px 180px;
    gap: 12px;
    margin-bottom: 20px;
  }

  .field {
    min-width: 0;
  }

  .text-input,
  .select-input {
    width: 100%;
    height: 44px;
    border: 1px solid #d1d5db;
    border-radius: 10px;
    padding: 0 14px;
    font-size: 14px;
    color: #111827;
    background: #ffffff;
    box-sizing: border-box;
  }

    .text-input:focus,
    .select-input:focus {
      outline: none;
      border-color: #2563eb;
      box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12);
    }

  .table-shell {
    width: 100%;
    overflow-x: auto;
  }

  .history-table {
    width: 100%;
    border-collapse: collapse;
    min-width: 900px;
  }

    .history-table thead th {
      background: #f8fafc;
      color: #374151;
      font-size: 14px;
      font-weight: 700;
      text-align: left;
      padding: 14px 16px;
      border-bottom: 1px solid #e5e7eb;
    }

    .history-table tbody td {
      padding: 14px 16px;
      border-bottom: 1px solid #e5e7eb;
      color: #111827;
      vertical-align: middle;
    }

    .history-table tbody tr:hover {
      background: #f9fafb;
    }

  .align-right {
    text-align: right !important;
  }

  .btn {
    border: none;
    border-radius: 10px;
    height: 42px;
    padding: 0 16px;
    font-weight: 600;
    cursor: pointer;
    transition: 0.2s ease;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }

  .btn-outline {
    background: #ffffff;
    color: #2563eb;
    border: 1px solid #93c5fd;
    height: 36px;
    padding: 0 14px;
  }

  .btn:hover,
  .close-button:hover {
    opacity: 0.92;
  }

  .status-chip {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 28px;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
    white-space: nowrap;
  }

  .status-warning {
    background: #fef3c7;
    color: #92400e;
  }

  .status-info {
    background: #dbeafe;
    color: #1d4ed8;
  }

  .status-danger {
    background: #fee2e2;
    color: #b91c1c;
  }

  .status-success {
    background: #dcfce7;
    color: #166534;
  }

  .status-primary {
    background: #e0e7ff;
    color: #4338ca;
  }

  .status-muted {
    background: #e5e7eb;
    color: #4b5563;
  }

  .state-block {
    min-height: 220px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 12px;
    text-align: center;
    color: #6b7280;
  }

  .state-block-small {
    min-height: 160px;
  }

  .empty-icon {
    font-size: 44px;
    color: #9ca3af;
  }

  .loader {
    width: 40px;
    height: 40px;
    border: 4px solid #dbeafe;
    border-top-color: #2563eb;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  .message {
    margin-top: 16px;
    border-radius: 10px;
    padding: 14px 16px;
    font-weight: 500;
  }

  .message-error {
    background: #fef2f2;
    border: 1px solid #fecaca;
    color: #b91c1c;
  }

  .detail-modal-overlay {
    position: fixed;
    inset: 0;
    z-index: 2000;
    background: rgba(15, 23, 42, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
  }

  .detail-modal {
    width: min(920px, 96vw);
    max-height: 90vh;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    background: #ffffff;
    border-radius: 18px;
    box-shadow: 0 24px 64px rgba(15, 23, 42, 0.25);
  }

  .detail-modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 18px 20px;
    border-bottom: 1px solid #e5e7eb;
  }

    .detail-modal-header h5 {
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      color: #111827;
    }

  .close-button {
    width: 40px;
    height: 40px;
    border: none;
    background: transparent;
    border-radius: 999px;
    color: #6b7280;
    font-size: 28px;
    line-height: 1;
    cursor: pointer;
  }

  .detail-modal-body {
    padding: 20px;
    overflow-y: auto;
  }

  .detail-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;
  }

  .detail-item {
    min-width: 0;
  }

  .detail-item-full {
    grid-column: 1 / -1;
  }

  .detail-item label {
    display: block;
    margin-bottom: 6px;
    font-size: 13px;
    font-weight: 700;
    color: #374151;
  }

  .detail-item div {
    color: #111827;
    line-height: 1.5;
    word-break: break-word;
  }

  .empty-detail {
    color: #6b7280;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  @media (max-width: 900px) {
    .filters {
      grid-template-columns: 1fr;
    }

    .field-button .btn {
      width: 100%;
    }

    .detail-grid {
      grid-template-columns: 1fr;
    }

    .detail-item-full {
      grid-column: auto;
    }
  }
</style>

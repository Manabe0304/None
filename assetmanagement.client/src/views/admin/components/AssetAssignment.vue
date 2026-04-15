<template>
  <div class="admin-page">
    <div class="page-header">
      <div>
        <h1>Asset Assignment</h1>
        <p>Assign available assets to requests approved by managers.</p>
      </div>
      <button class="refresh-btn" @click="loadData" :disabled="loading">
        {{ loading ? "Loading..." : "Refresh" }}
      </button>
    </div>

    <div class="card filter-card">
      <div class="filter-row">
        <div class="form-group">
          <label>Search Employee</label>
          <input v-model="filters.keyword"
                 type="text"
                 placeholder="Search by employee, department, asset type..." />
        </div>

        <div class="form-group">
          <label>Urgency</label>
          <select v-model="filters.urgency">
            <option value="">All</option>
            <option value="LOW">LOW</option>
            <option value="MEDIUM">MEDIUM</option>
            <option value="HIGH">HIGH</option>
            <option value="CRITICAL">CRITICAL</option>
          </select>
        </div>
      </div>
    </div>

    <div class="card">
      <div class="card-header">
        <h2>Approved Requests</h2>
        <span class="badge">{{ filteredRequests.length }} request(s)</span>
      </div>

      <div v-if="loading" class="empty-state">Loading approved requests...</div>

      <div v-else-if="filteredRequests.length === 0" class="empty-state">
        No approved requests available for assignment.
      </div>

      <div v-else class="request-list">
        <div v-for="request in filteredRequests"
             :key="request.id"
             class="request-item">
          <div class="request-info">
            <div class="request-title-row">
              <h3>{{ request.employeeName }}</h3>
              <span class="status-badge approved">APPROVED</span>
            </div>

            <div class="request-meta">
              <div style="display:flex; align-items:center; gap:8px;">
                <strong>Department:</strong>
                <input type="text" :value="request.departmentName || 'N/A'" disabled style="border: 1px solid #d1d5db; border-radius: 6px; padding: 4px 8px; background: #f3f4f6; color: #4b5563; font-weight: 500;" />
              </div>
              <span><strong>Asset Type:</strong> {{ request.assetType }}</span>
              <span><strong>Urgency:</strong> {{ request.urgency }}</span>
              <span><strong>Approved By:</strong> {{ request.approvedByManager || "N/A" }}</span>
              <span><strong>Approved At:</strong> {{ formatDate(request.approvedAt) }}</span>
            </div>

            <p class="request-reason">
              <strong>Reason:</strong> {{ request.reason }}
            </p>
            <p v-if="request.preferredModel" class="request-reason" style="margin-top: 4px; color: #1d4ed8;">
              <strong>Requested Model:</strong> {{ request.preferredModel }}
            </p>
          </div>

          <div class="request-action">
            <label style="margin-bottom: 4px;">Select Asset to Assign</label>
            
            <div v-if="getCompatibleAssets(request).length === 0" class="no-asset-warning">
              No matching AVAILABLE assets for type "{{ request.assetType }}"
            </div>
            
            <select v-else v-model="selectedAssets[request.id]" class="asset-select">
              <option value="">-- Choose an Available Asset --</option>
              <option v-for="asset in getCompatibleAssets(request)" :key="asset.id" :value="asset.id">
                [{{ asset.assetTag }}] {{ asset.category || asset.assetType }} - {{ asset.model || asset.assetName }}
              </option>
            </select>

            <div style="display: flex; gap: 8px;">
              <button class="assign-btn"
                      @click="assignAsset(request)"
                      :disabled="assigningRequestId === request.id || !selectedAssets[request.id]">
                {{ assigningRequestId === request.id ? "Assigning..." : "Assign" }}
              </button>
              <button class="reject-btn"
                      @click="rejectRequest(request)">
                Reject
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card help-card">
      <h3>Assignment Rules</h3>
      <ul>
        <li>Only requests with status <strong>APPROVED</strong> are shown here.</li>
        <li>Only assets with status <strong>AVAILABLE</strong> and matching the requested <strong>Asset Type</strong> can be assigned.</li>
        <li>After assignment: request becomes <strong>ASSIGNED</strong>, asset becomes <strong>IN_USE</strong>.</li>
      </ul>
    </div>
  </div>
</template>

<script setup>
  import { computed, onMounted, reactive, ref } from "vue"
  import { showNotification } from "@/stores/notificationStore"

  const loading = ref(false)
  const assigningRequestId = ref(null)

  const approvedRequests = ref([])
  const availableAssets = ref([])
  const selectedAssets = reactive({})

  const filters = reactive({
    keyword: "",
    urgency: ""
  })

  function getAccessToken() {
    return (
      localStorage.getItem("token") ||
      localStorage.getItem("access_token") ||
      null
    )
  }

  function decodeJwtPayload(token) {
    try {
      const payload = token.split(".")[1]
      const normalized = payload.replace(/-/g, "+").replace(/_/g, "/")
      const json = decodeURIComponent(
        atob(normalized)
          .split("")
          .map((c) => `%${("00" + c.charCodeAt(0).toString(16)).slice(-2)}`)
          .join("")
      )
      return JSON.parse(json)
    } catch {
      return null
    }
  }

  function getAssignedById() {
    const token = getAccessToken()
    if (!token) return null

    const payload = decodeJwtPayload(token)
    if (!payload) return null

    return (
      payload.nameid ||
      payload.sub ||
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
      null
    )
  }

  function buildAuthHeaders(extra = {}) {
    const token = getAccessToken()

    return {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...extra
    }
  }

  const filteredRequests = computed(() => {
    return approvedRequests.value.filter((request) => {
      const keyword = filters.keyword.trim().toLowerCase()

      const matchesKeyword =
        !keyword ||
        request.employeeName?.toLowerCase().includes(keyword) ||
        request.departmentName?.toLowerCase().includes(keyword) ||
        request.assetType?.toLowerCase().includes(keyword)

      const matchesUrgency =
        !filters.urgency || request.urgency === filters.urgency

      return matchesKeyword && matchesUrgency
    })
  })

  function formatDate(dateValue) {
    if (!dateValue) return "N/A"
    return new Date(dateValue).toLocaleString()
  }

  // --- HÀM MỚI: Lọc Asset cùng loại ---
  function getCompatibleAssets(request) {
    if (!request || !request.assetType) return [];
    const reqType = request.assetType.toLowerCase().trim();
    
    return availableAssets.value.filter(a => {
      const assetType = (a.category || a.assetType || "").toLowerCase().trim();
      return assetType === reqType;
    });
  }

  async function fetchApprovedRequests() {
    const response = await fetch(`/api/assignments/approved-requests`, {
      headers: buildAuthHeaders()
    })

    if (!response.ok) {
      throw new Error("Failed to fetch approved requests.")
    }

    const data = await response.json()
    approvedRequests.value = Array.isArray(data) ? data : data.items || []
    
    approvedRequests.value.forEach(req => {
      if (selectedAssets[req.id] === undefined) {
        selectedAssets[req.id] = ""
      }
    })
  }

  async function fetchAvailableAssets() {
    const response = await fetch(`/api/assignments/available-assets`, {
      headers: buildAuthHeaders()
    })

    if (!response.ok) {
      throw new Error("Failed to fetch available assets.")
    }

    const data = await response.json()
    availableAssets.value = Array.isArray(data) ? data : data.items || []
  }

  async function loadData() {
    try {
      loading.value = true
      await Promise.all([fetchApprovedRequests(), fetchAvailableAssets()])
    } catch (error) {
      console.error(error)
      showNotification("Error", error.message || "Unable to load assignment data.", "error")
    } finally {
      loading.value = false
    }
  }

  async function assignAsset(request) {
    const assetId = selectedAssets[request.id]
    
    if (!assetId) {
      showNotification("Error", "Please select an asset to assign.", "error")
      return
    }

    const assignedById = getAssignedById()

    if (!assignedById) {
      showNotification("Error", "Không xác định được admin đang đăng nhập.", "error")
      return
    }

    try {
      assigningRequestId.value = request.id

      const response = await fetch(`/api/assignments`, {
        method: "POST",
        headers: buildAuthHeaders(),
        body: JSON.stringify({
          requestId: request.id,
          assetId,
          userId: request.employeeId,
          assignedById
        })
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        throw new Error(errorData?.message || "Asset assignment failed.")
      }

      showNotification("Success", "Asset assigned successfully!", "success")
      await loadData()
    } catch (error) {
      console.error(error)
      showNotification("Error", error.message || "Unable to assign asset.", "error")
    } finally {
      assigningRequestId.value = null
    }
  }

  async function rejectRequest(request) {
    if (!confirm("Are you sure you want to reject this request?")) return;

    try {
      const response = await fetch(`/api/requests/${request.id}/reject`, {
        method: "POST",
        headers: buildAuthHeaders(),
        body: JSON.stringify({ reason: "Rejected by Admin" })
      })

      if (!response.ok) throw new Error("Rejection failed")

      showNotification("Success", "Request rejected.", "success")
      await loadData()
    } catch (err) {
      showNotification("Error", err.message || "Failed to reject request.", "error")
    }
  }

  onMounted(() => {
    loadData()
  })
</script>

<style scoped>
  .admin-page {
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
    background: #ffffff;
    border-radius: 14px;
    padding: 20px;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
    border: 1px solid #e5e7eb;
  }

  .card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

    .card-header h2 {
      margin: 0;
      font-size: 20px;
      color: #1f2937;
    }

  .filter-row {
    display: grid;
    grid-template-columns: 2fr 1fr;
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
      height: 42px;
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 0 12px;
      outline: none;
      font-size: 14px;
    }

      .form-group input:focus,
      .form-group select:focus {
        border-color: #1d4ed8;
      }

  .request-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .request-item {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 20px;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 18px;
    background: #f9fafb;
  }

  .request-title-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  }

    .request-title-row h3 {
      margin: 0;
      color: #111827;
      font-size: 18px;
    }

  .request-meta {
    display: flex;
    flex-wrap: wrap;
    gap: 10px 18px;
    margin-top: 10px;
    font-size: 14px;
    color: #4b5563;
  }

  .request-reason {
    margin-top: 12px;
    color: #374151;
    line-height: 1.5;
  }

  .request-action {
    display: flex;
    flex-direction: column;
    gap: 12px;
    justify-content: center;
  }

    .request-action label {
      font-weight: 600;
      color: #374151;
    }

    .asset-select {
      height: 42px;
      border: 1px solid #d1d5db;
      border-radius: 8px;
      padding: 0 12px;
      background: #ffffff;
      font-size: 14px;
      color: #374151;
      margin-bottom: 8px;
      width: 100%;
      cursor: pointer;
    }
    
    .asset-select:focus {
      border-color: #1d4ed8;
      outline: none;
    }

    .no-asset-warning {
      color: #dc2626;
      background: #fef2f2;
      border: 1px solid #fecaca;
      padding: 10px 12px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 500;
      margin-bottom: 8px;
    }

  /* ĐÃ SỬA: Bỏ flex:1 ra khỏi class chung này để nút Refresh về nguyên bản */
  .assign-btn,
  .reject-btn,
  .refresh-btn {
    height: 42px;
    border: none;
    border-radius: 10px;
    background: #1d4ed8;
    color: white;
    font-weight: 600;
    cursor: pointer;
    padding: 0 18px;
  }

  /* Cấp flex:1 riêng cho các nút trong vùng action để nó cân bằng */
  .assign-btn,
  .reject-btn {
    flex: 1;
  }

  .reject-btn {
    background: #dc2626;
  }

    .reject-btn:hover {
      background: #b91c1c;
    }

    .assign-btn:disabled,
    .reject-btn:disabled,
    .refresh-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

  .badge {
    background: #eff6ff;
    color: #1d4ed8;
    border-radius: 999px;
    padding: 6px 12px;
    font-size: 13px;
    font-weight: 700;
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    border-radius: 999px;
    padding: 6px 10px;
    font-size: 12px;
    font-weight: 700;
  }

    .status-badge.approved {
      background: #dcfce7;
      color: #166534;
    }

  .toast-notification {
    position: fixed;
    top: 24px;
    right: 24px;
    z-index: 9999;
    padding: 14px 20px;
    border-radius: 8px;
    font-weight: 600;
    font-size: 14px;
    box-shadow: 0 10px 25px rgba(0,0,0,0.15);
    color: white;
  }

    .toast-notification.error {
      background-color: #ef4444;
    }

    .toast-notification.success {
      background-color: #10b981;
    }

  .toast-fade-enter-active,
  .toast-fade-leave-active {
    transition: all 0.3s ease;
  }

  .toast-fade-enter-from,
  .toast-fade-leave-to {
    opacity: 0;
    transform: translateY(-20px);
  }

  .empty-state {
    text-align: center;
    padding: 28px 12px;
    color: #6b7280;
  }

  .help-card h3 {
    margin-top: 0;
    color: #1f2937;
  }

  .help-card ul {
    margin: 12px 0 0;
    padding-left: 18px;
    color: #4b5563;
    line-height: 1.7;
  }

  @media (max-width: 992px) {
    .request-item,
    .filter-row {
      grid-template-columns: 1fr;
    }
  }
</style>

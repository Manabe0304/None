<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h2>Current Asset Assignments</h2>
        <p class="subtitle">Track all assets currently being used by employees.</p>
      </div>
      <button class="refresh-btn" @click="loadAssignments" :disabled="loading">
        {{ loading ? "Loading..." : "Refresh" }}
      </button>
    </div>

    <div class="filter-card">
      <div class="filter-grid">
        <div class="form-group">
          <label>Employee</label>
          <input v-model="filters.employee" type="text" placeholder="Search employee name..." />
        </div>

        <div class="form-group">
          <label>Department</label>
          <input v-model="filters.department" type="text" placeholder="Search department..." />
        </div>

        <div class="form-group">
          <label>Asset Type</label>
          <input v-model="filters.assetType" type="text" placeholder="Search asset type..." />
        </div>
      </div>

      <div class="filter-actions">
        <button class="btn btn-primary" @click="loadAssignments" :disabled="loading">Apply Filters</button>
        <button class="btn btn-secondary" @click="resetFilters" :disabled="loading">Reset</button>
      </div>
    </div>

    <div class="table-card">
      <div v-if="loading" class="empty">Loading current assignments...</div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Asset Tag</th>
            <th>Asset Name</th>
            <th>Category</th>
            <th>Employee Name</th>
            <th>Department</th>
            <th>Assigned Date</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="assignment in assignments" :key="assignment.id">
            <td>{{ assignment.assetTag }}</td>
            <td>{{ assignment.assetName || "—" }}</td>
            <td>{{ assignment.category || "—" }}</td>
            <td>{{ assignment.employeeName }}</td>
            <td>{{ assignment.department || "—" }}</td>
            <td>{{ formatDate(assignment.assignedAt) }}</td>
            <td>
              <span class="badge badge-active">{{ assignment.status }}</span>
            </td>
          </tr>
          <tr v-if="!loading && assignments.length === 0">
            <td colspan="7" class="empty">No active assignments found.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
  import { onMounted, reactive, ref } from "vue"
  import { getCurrentAssignments } from "@/services/assignmentService"
  import { showNotification } from "@/stores/notificationStore"

  const loading = ref(false)
  const assignments = ref([])

  const filters = reactive({
    employee: "",
    department: "",
    assetType: ""
  })

  function formatDate(dateStr) {
    if (!dateStr) return "N/A"
    return new Date(dateStr).toLocaleString("vi-VN")
  }

  async function loadAssignments() {
    try {
      loading.value = true
      assignments.value = await getCurrentAssignments({
        employee: filters.employee,
        department: filters.department,
        assetType: filters.assetType
      })
    } catch (error) {
      assignments.value = []
      showNotification(
        "Error",
        error?.response?.data?.message || "Failed to load current assignments.",
        "error"
      )
    } finally {
      loading.value = false
    }
  }

  function resetFilters() {
    filters.employee = ""
    filters.department = ""
    filters.assetType = ""
    loadAssignments()
  }

  onMounted(loadAssignments)
</script>

<style scoped>
  .page {
    padding: 24px;
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .page-header {
    display: flex;
    align-items: end;
    justify-content: space-between;
    gap: 16px;
  }

  .page h2 {
    font-size: 22px;
    font-weight: 600;
    margin-bottom: 6px;
  }

  .subtitle {
    color: #6b7280;
  }

  .filter-card,
  .table-card {
    background: white;
    border-radius: 10px;
    padding: 20px;
    overflow: hidden;
    box-shadow: 0 1px 4px rgba(0,0,0,0.1);
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
      border-radius: 8px;
      padding: 0 12px;
    }

  .filter-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 16px;
  }

  .btn,
  .refresh-btn {
    height: 40px;
    border: none;
    font-weight: bold;
    border-radius: 8px;
    padding: 0 16px;
    cursor: pointer;
  }

  .btn-primary,
  .refresh-btn {
    background: #2563eb;
    color: white;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }

  .data-table {
    width: 100%;
    border-collapse: collapse;
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

  .empty {
    text-align: center;
    color: #9ca3af;
    padding: 32px !important;
  }

  .badge {
    display: inline-flex;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
  }

  .badge-active {
    background: #dcfce7;
    color: #166534;
  }

  @media (max-width: 900px) {
    .filter-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

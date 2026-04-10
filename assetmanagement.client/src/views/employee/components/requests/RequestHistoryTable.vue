<template>
  <div class="history-card">
    <div class="card-header">
      <h2>Request History</h2>
      <p>Track the progress of your submitted requests.</p>
    </div>

    <div v-if="loading" class="state-box">
      Loading request history...
    </div>

    <div v-else-if="!requests.length" class="state-box empty">
      No request history found.
    </div>

    <div v-else class="table-wrapper">
      <table class="history-table">
        <thead>
          <tr>
            <th>Requested Device Type</th>
            <th>Status</th>
            <th>Rejection Reason</th>
            <th>Assignment Information</th>
            <th>Date</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="item in requests" :key="item.id">
            <td>{{ item.requestedDeviceType }}</td>

            <td>
              <span class="status-badge" :class="statusClass(item.currentStatus)">
                {{ item.currentStatus }}
              </span>
            </td>

            <td>{{ item.rejectionReason || "-" }}</td>

            <td class="reason-cell">
              <template v-if="item.assignment">
                <div><strong>Asset Tag:</strong> {{ item.assignment.assetTag || "-" }}</div>
                <div><strong>Category:</strong> {{ item.assignment.assetCategory || "-" }}</div>
                <div><strong>Assigned At:</strong> {{ formatDateTime(item.assignment.assignmentDate || item.assignment.assignedAt) }}</div>
                <div><strong>Status:</strong> {{ item.assignment.status || "-" }}</div>
              </template>
              <span v-else>-</span>
            </td>

            <td>{{ formatDate(item.requestDate) }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
  </div>
</template>

<script setup>
  import { REQUEST_STATUS } from "../../constants/request"

  defineProps({
    requests: {
      type: Array,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    },
    errorMessage: {
      type: String,
      default: ""
    }
  })

  function formatDate(dateString) {
    if (!dateString) return "-"
    return new Intl.DateTimeFormat("en-GB").format(new Date(dateString))
  }

  function formatDateTime(dateString) {
    if (!dateString) return "-"
    return new Intl.DateTimeFormat("en-GB", {
      dateStyle: "short",
      timeStyle: "short"
    }).format(new Date(dateString))
  }

  function statusClass(status) {
    if (status === REQUEST_STATUS.PENDING) return "pending"
    if (status === REQUEST_STATUS.APPROVED || status === REQUEST_STATUS.ASSIGNED) return "approved"
    if (status === REQUEST_STATUS.REJECTED) return "rejected"
    return ""
  }
</script>

<style scoped>
  .history-card {
    background: #ffffff;
    border-radius: 14px;
    padding: 24px;
    box-shadow: 0 4px 16px rgba(15, 23, 42, 0.08);
  }

  .card-header {
    margin-bottom: 20px;
  }

    .card-header h2 {
      margin: 0 0 8px;
      color: #0f2f5f;
      font-size: 32px;
      font-weight: 700;
    }

    .card-header p {
      margin: 0;
      color: #64748b;
    }

  .table-wrapper {
    overflow-x: auto;
  }

  .history-table {
    width: 100%;
    min-width: 300px;
    border-collapse: collapse;
  }

    .history-table thead th {
      text-align: left;
      background: #f8fafc;
      color: #0f172a;
      padding: 14px 12px;
      border-bottom: 1px solid #e2e8f0;
      font-size: 14px;
    }

    .history-table tbody td {
      padding: 14px 12px;
      border-bottom: 1px solid #eef2f7;
      font-size: 14px;
      color: #334155;
      vertical-align: middle;
    }

    .history-table td:last-child {
      white-space: nowrap;
      width: 100px; /* Cố định độ rộng cho cột ngày */
    }

  .reason-cell {
    max-width: 260px;
    white-space: normal;
    word-break: break-word;
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 90px;
    padding: 6px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
  }

    .status-badge.pending {
      background: #fef3c7;
      color: #b45309;
    }

    .status-badge.approved {
      background: #dcfce7;
      color: #15803d;
    }

    .status-badge.rejected {
      background: #fee2e2;
      color: #b91c1c;
    }

  .state-box {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 180px;
    border: 1px dashed #cbd5e1;
    border-radius: 12px;
    color: #64748b;
    font-weight: 600;
  }

    .state-box.empty {
      background: #f8fafc;
    }

  .error-text {
    margin-top: 14px;
    color: #dc2626;
    font-weight: 600;
  }
</style>

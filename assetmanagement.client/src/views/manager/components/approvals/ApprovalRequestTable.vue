<template>
  <div class="approval-card">
    <div class="card-header">
      <h2>Approval Requests</h2>
    </div>

    <div v-if="loading" class="state-box">
      Loading requests...
    </div>

    <div v-else-if="!requests.length" class="state-box empty">
      No pending requests found.
    </div>

    <div v-else class="table-wrapper">
      <table class="approval-table">
        <thead>
          <tr>
            <th>Employee Name</th>
            <th>Department</th>
            <th>Asset</th>
            <th>Reason</th>
            <th>Urgency</th>
            <th>Date</th>
            <th class="col-actions">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in requests" :key="item.id">
            <td>
              <strong>{{ item.employeeName }}</strong>
            </td>
            <td>
              <span style="background: #f3f4f6; color: #4b5563; padding: 4px 8px; border-radius: 6px; font-size: 12px; font-weight: 600;">
                {{ item.department }}
              </span>
            </td>
            <td>{{ item.assetName }}</td>
            <td class="reason-cell">{{ item.reason }}</td>
            <td>
              <span class="urgency-badge" :class="urgencyClass(item.urgency)">
                {{ item.urgency }}
              </span>
            </td>
            <td>{{ formatDate(item.requestDate) }}</td>
            <td class="action-group">
              <button
                class="btn btn-secondary"
                type="button"
                @click="$emit('viewDetail', item)"
              >
                Detail
              </button>
              
              <button
                class="btn btn-approve"
                type="button"
                :disabled="actionLoadingId === item.id"
                @click="$emit('approve', item.id)"
              >
                {{ actionLoadingId === item.id ? 'Processing...' : 'Approve' }}
              </button>

              <button
                class="btn btn-reject"
                type="button"
                :disabled="actionLoadingId === item.id"
                @click="$emit('reject', item.id)"
              >
                {{ actionLoadingId === item.id ? 'Processing...' : 'Reject' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <p v-if="errorMessage" class="page-error">{{ errorMessage }}</p>
  </div>
</template>

<script setup>
import { APPROVAL_URGENCY } from '../../types/approval'

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
    default: ''
  },
  actionLoadingId: {
    type: String,
    default: null
  }
})

defineEmits(['approve', 'reject', 'viewDetail'])

function formatDate(dateString) {
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('en-GB').format(date)
}

function urgencyClass(urgency) {
  switch (urgency) {
    case APPROVAL_URGENCY.HIGH:
      return 'high'
    case APPROVAL_URGENCY.MEDIUM:
      return 'medium'
    default:
      return 'low'
  }
}
</script>

<style scoped>
.approval-card {
  background: #ffffff;
  border-radius: 14px;
  padding: 24px;
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.08);
}

.card-header {
  margin-bottom: 18px;
}

.card-header h2 {
  margin: 0;
  color: #0f2f5f;
  font-size: 28px;
  font-weight: 700;
}

.table-wrapper {
  overflow-x: auto;
}

.approval-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 980px;
}

.approval-table thead th {
  background: #f8fafc;
  color: #0f172a;
  text-align: left;
  padding: 14px 12px;
  border-bottom: 1px solid #e2e8f0;
  font-size: 14px;
}

.approval-table tbody td {
  padding: 14px 12px;
  border-bottom: 1px solid #eef2f7;
  vertical-align: middle;
  color: #334155;
  font-size: 14px;
}

.reason-cell {
  max-width: 280px;
  white-space: normal;
  word-break: break-word;
}

.col-actions {
  width: 220px;
}

.action-group {
  display: flex;
  gap: 10px;
}

.btn {
  border: none;
  border-radius: 8px;
  padding: 10px 16px;
  font-weight: 700;
  cursor: pointer;
  min-width: 92px;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.btn-approve {
  background: #22c55e;
  color: white;
}

.btn-reject {
  background: #e74c3c;
  color: white;
}

.btn-secondary {
  background: #e5e7eb;
  color: #111827;
}

.urgency-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 80px;
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.urgency-badge.high {
  background: #fee2e2;
  color: #b91c1c;
}

.urgency-badge.medium {
  background: #fef3c7;
  color: #b45309;
}

.urgency-badge.low {
  background: #dcfce7;
  color: #15803d;
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

.page-error {
  margin-top: 14px;
  color: #dc2626;
  font-weight: 600;
}
</style>

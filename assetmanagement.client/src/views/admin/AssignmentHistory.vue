<template>
  <div class="page">
    <h2>Asset Assignment History</h2>
    <p class="subtitle">Full historical record of all asset assignments.</p>

    <div class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Asset Tag</th>
            <th>Category</th>
            <th>Employee Name</th>
            <th>Assigned Date</th>
            <th>Return Date</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in history" :key="a.id">
            <td>{{ a.assetTag }}</td>
            <td>{{ a.category }}</td>
            <td>{{ a.employeeName }}</td>
            <td>{{ formatDate(a.assignedAt) }}</td>
            <td>{{ a.returnedAt ? formatDate(a.returnedAt) : '—' }}</td>
            <td>
              <span :class="['badge', a.status === 'ACTIVE' ? 'badge-active' : 'badge-returned']">
                {{ a.status }}
              </span>
            </td>
          </tr>
          <tr v-if="history.length === 0">
            <td colspan="6" class="empty">No assignment history found.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import axios from "axios"

export default {
  name: "AssignmentHistory",
  data() {
    return { history: [] }
  },
  async mounted() {
    const res = await axios.get("/api/assignments/history")
    this.history = res.data
  },
  methods: {
    formatDate(dateStr) {
      return new Date(dateStr).toLocaleDateString("vi-VN")
    }
  }
}
</script>

<style scoped>
.page { padding: 24px; }
.page h2 { font-size: 22px; font-weight: 600; margin-bottom: 6px; }
.subtitle { color: #6b7280; margin-bottom: 20px; }
.table-card {
  background: white;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
}
.data-table { width: 100%; border-collapse: collapse; }
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
.empty { text-align: center; color: #9ca3af; padding: 32px !important; }
.badge {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}
.badge-active   { background: #d1fae5; color: #065f46; }
.badge-returned { background: #e5e7eb; color: #374151; }
</style>

<template>
  <div class="page">
    <h2>Current Asset Assignments</h2>
    <p class="subtitle">All active assignments currently in use.</p>

    <div class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Asset Tag</th>
            <th>Category</th>
            <th>Employee Name</th>
            <th>Department</th>
            <th>Assigned Date</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in assignments" :key="a.id">
            <td>{{ a.assetTag }}</td>
            <td>{{ a.category }}</td>
            <td>{{ a.employeeName }}</td>
            <td>{{ a.department }}</td>
            <td>{{ formatDate(a.assignedAt) }}</td>
          </tr>
          <tr v-if="assignments.length === 0">
            <td colspan="5" class="empty">No active assignments found.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import axios from "axios"

export default {
  name: "AssignmentCurrent",
  data() {
    return { assignments: [] }
  },
  async mounted() {
    const res = await axios.get("/api/assignments/current")
    this.assignments = res.data
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
</style>

<template>
  <div class="overview-page">
    <div class="page-header">
      <div>
        <h1>Dashboard</h1>
        <p>Overview of users, assets, requests, and assignments.</p>
      </div>
    </div>

    <div class="stats-grid">
      <div class="stat-card">
        <h3>Total Users</h3>
        <p>{{ stats.totalUsers }}</p>
      </div>

      <div class="stat-card">
        <h3>Total Assets</h3>
        <p>{{ stats.totalAssets }}</p>
      </div>

      <div class="stat-card">
        <h3>Approved Requests</h3>
        <p>{{ stats.approvedRequests }}</p>
      </div>

      <div class="stat-card">
        <h3>Active Assignments</h3>
        <p>{{ stats.activeAssignments }}</p>
      </div>
    </div>

    <div class="info-card">
      <h2>Admin Workflow</h2>
      <p>
        Employee submits request → Manager approves request → Admin assigns available asset → Asset status changes to IN_USE.
      </p>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive } from "vue"

const stats = reactive({
  totalUsers: 0,
  totalAssets: 0,
  approvedRequests: 0,
  activeAssignments: 0
})

async function loadStats() {
  try {
    const response = await fetch("/api/admin/dashboard")
    if (!response.ok) return

    const data = await response.json()
    stats.totalUsers = data.totalUsers || 0
    stats.totalAssets = data.totalAssets || 0
    stats.approvedRequests = data.approvedRequests || 0
    stats.activeAssignments = data.activeAssignments || 0
  } catch (error) {
    console.error(error)
  }
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
  .overview-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
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

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
  }

  .stat-card,
  .info-card {
    background: #fff;
    border-radius: 14px;
    padding: 22px;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
    border: 1px solid #e5e7eb;
  }

    .stat-card h3 {
      margin: 0 0 12px;
      font-size: 16px;
      color: #4b5563;
    }

    .stat-card p {
      margin: 0;
      font-size: 32px;
      font-weight: 700;
      color: #1d4ed8;
    }

    .info-card h2 {
      margin-top: 0;
      color: #1f2937;
    }

    .info-card p {
      color: #4b5563;
      line-height: 1.6;
    }

  @media (max-width: 992px) {
    .stats-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }
</style>

<template>
  <div class="dashboard-page">
    <section class="kpi-grid">
      <article class="kpi-card">
        <span class="kpi-label">Total Assets</span>
        <strong class="kpi-value">{{ kpi.totalAssets ?? 0 }}</strong>
      </article>

      <article class="kpi-card">
        <span class="kpi-label">Available Assets</span>
        <strong class="kpi-value">{{ kpi.availableAssets ?? 0 }}</strong>
      </article>

      <article class="kpi-card">
        <span class="kpi-label">Under Maintenance</span>
        <strong class="kpi-value">{{ kpi.maintenanceAssets ?? 0 }}</strong>
      </article>

      <article class="kpi-card">
        <span class="kpi-label">Pending Requests</span>
        <strong class="kpi-value">{{ kpi.pendingRequests ?? 0 }}</strong>
      </article>
    </section>

    <section class="chart-grid chart-grid--top">
      <article class="panel chart-panel">
        <div class="panel-header">
          <h3>Asset Status</h3>
        </div>
        <div class="chart-body chart-body--medium">
          <canvas ref="pieCanvas"></canvas>
        </div>
      </article>

      <article class="panel chart-panel">
        <div class="panel-header">
          <h3>Repair Spending / Month</h3>
        </div>
        <div class="chart-body chart-body--medium">
          <canvas ref="barCanvas"></canvas>
        </div>
      </article>
    </section>

    <section class="panel chart-panel">
      <div class="panel-header">
        <h3>Asset Activity Over Time</h3>
      </div>
      <div class="chart-body chart-body--large">
        <canvas ref="lineCanvas"></canvas>
      </div>
    </section>

    <section class="table-grid">
      <article class="panel">
        <div class="panel-header">
          <h3>Assets by Department</h3>
        </div>

        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Department</th>
                <th>Assets</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in departmentAssets" :key="d.department">
                <td>{{ d.department }}</td>
                <td>{{ d.count }}</td>
              </tr>
              <tr v-if="!departmentAssets.length">
                <td colspan="2" class="empty-cell">No data</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <article class="panel">
        <div class="panel-header">
          <h3>Top Used Assets</h3>
        </div>

        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Asset</th>
                <th>Usage</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in topAssets" :key="a.assetName">
                <td>{{ a.assetName }}</td>
                <td>{{ a.usage }}</td>
              </tr>
              <tr v-if="!topAssets.length">
                <td colspan="2" class="empty-cell">No data</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>
    </section>
  </div>
</template>

<script setup>
  import { nextTick, onActivated, onBeforeUnmount, onDeactivated, onMounted, ref } from "vue"
  import { Chart } from "chart.js/auto"

  const pieCanvas = ref(null)
  const barCanvas = ref(null)
  const lineCanvas = ref(null)

  const kpi = ref({})
  const departmentAssets = ref([])
  const topAssets = ref([])

  const pieChartData = ref([])
  const barChartData = ref([])
  const lineChartData = ref([])

  const charts = {
    pie: null,
    bar: null,
    line: null
  }

  let loadSequence = 0

  async function fetchJson(url) {
    const response = await fetch(url)
    if (!response.ok) {
      throw new Error(`Request failed: ${url}`)
    }

    return await response.json()
  }

  function destroyChart(key) {
    if (charts[key]) {
      charts[key].destroy()
      charts[key] = null
    }
  }

  function destroyCharts() {
    destroyChart("pie")
    destroyChart("bar")
    destroyChart("line")
  }

  function renderPieChart() {
    destroyChart("pie")
    if (!pieCanvas.value) return

    charts.pie = new Chart(pieCanvas.value, {
      type: "pie",
      data: {
        labels: pieChartData.value.map((x) => x.status),
        datasets: [
          {
            data: pieChartData.value.map((x) => x.count)
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: "top",
            labels: {
              boxWidth: 14
            }
          }
        }
      }
    })
  }

  function renderBarChart() {
    destroyChart("bar")
    if (!barCanvas.value) return

    charts.bar = new Chart(barCanvas.value, {
      type: "bar",
      data: {
        labels: barChartData.value.map((x) => `${x.month}/${x.year}`),
        datasets: [
          {
            label: "Repair Cost",
            data: barChartData.value.map((x) => x.total)
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    })
  }

  function renderLineChart() {
    destroyChart("line")
    if (!lineCanvas.value) return

    charts.line = new Chart(lineCanvas.value, {
      type: "line",
      data: {
        labels: lineChartData.value.map((x) => `${x.month}/${x.year}`),
        datasets: [
          {
            label: "Asset Activities",
            data: lineChartData.value.map((x) => x.count),
            tension: 0.35,
            fill: false
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            precision: 0
          }
        }
      }
    })
  }

  function renderCharts() {
    renderPieChart()
    renderBarChart()
    renderLineChart()
  }

  async function loadDashboardData() {
    const sequence = ++loadSequence

    const [kpiData, departmentData, topAssetData, pieData, barData, lineData] = await Promise.all([
      fetchJson("/api/dashboard/kpi"),
      fetchJson("/api/dashboard/assets-by-department"),
      fetchJson("/api/dashboard/top-assets"),
      fetchJson("/api/dashboard/asset-status"),
      fetchJson("/api/dashboard/repair-spending"),
      fetchJson("/api/dashboard/activity-trend")
    ])

    if (sequence !== loadSequence) {
      return null
    }

    kpi.value = kpiData ?? {}
    departmentAssets.value = Array.isArray(departmentData) ? departmentData : []
    topAssets.value = Array.isArray(topAssetData) ? topAssetData : []
    pieChartData.value = Array.isArray(pieData) ? pieData : []
    barChartData.value = Array.isArray(barData) ? barData : []
    lineChartData.value = Array.isArray(lineData) ? lineData : []

    return sequence
  }

  async function reloadDashboard() {
    const sequence = await loadDashboardData()
    if (!sequence) return

    await nextTick()

    if (sequence !== loadSequence) {
      return
    }

    renderCharts()
  }

  function cleanupDashboard() {
    loadSequence += 1
    destroyCharts()
  }

  onMounted(reloadDashboard)
  onActivated(reloadDashboard)
  onBeforeUnmount(cleanupDashboard)
  onDeactivated(cleanupDashboard)
</script>

<style scoped>
  .dashboard-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .kpi-grid {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 20px;
  }

  .kpi-card,
  .panel {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
  }

  .kpi-card {
    padding: 20px;
  }

  .kpi-label {
    display: block;
    font-size: 14px;
    color: #6b7280;
    margin-bottom: 10px;
  }

  .kpi-value {
    font-size: 30px;
    font-weight: 700;
    color: #111827;
  }

  .chart-grid--top,
  .table-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 20px;
  }

  .panel {
    padding: 20px;
    min-width: 0;
  }

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

    .panel-header h3 {
      font-size: 18px;
      color: #111827;
      margin: 0;
    }

  .chart-body {
    position: relative;
    width: 100%;
  }

  .chart-body--medium {
    height: 320px;
  }

  .chart-body--large {
    height: 360px;
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .data-table {
    width: 100%;
    border-collapse: collapse;
  }

    .data-table thead th {
      background: #f9fafb;
      color: #374151;
      font-size: 13px;
      font-weight: 600;
      padding: 12px 14px;
      text-align: left;
    }

    .data-table tbody td {
      color: #111827;
      font-size: 14px;
      padding: 12px 14px;
      border-top: 1px solid #f1f5f9;
    }

  .empty-cell {
    text-align: center;
    color: #9ca3af;
  }

  @media (max-width: 1024px) {
    .kpi-grid,
    .chart-grid--top,
    .table-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

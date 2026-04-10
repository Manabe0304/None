<template>
  <div class="page returns-page">
    <h2>Return Requests</h2>
    <p class="subtitle">Manage return requests: receive asset and inspect.</p>

    <div class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Asset Tag</th>
            <th>Requested By</th>
            <th>Status</th>
            <th>Requested At</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in returns" :key="r.id">
            <td>{{ r.assetTag }}</td>
            <td>{{ r.requestedBy }}</td>
            <td>{{ r.status }}</td>
            <td>{{ formatDate(r.createdAt) }}</td>
            <td>
              <button v-if="r.status === 'REQUESTED'" @click="receive(r)" class="btn">Receive</button>
              <div v-else-if="r.status === 'PENDING_INSPECTION'" class="inspect">
                <select v-model="r._selectedCondition">
                  <option value="GOOD">GOOD</option>
                  <option value="FAIR">FAIR</option>
                  <option value="DAMAGED">DAMAGED</option>
                  <option value="MISSING_ACCESSORIES">MISSING_ACCESSORIES</option>
                  <option value="BROKEN">BROKEN</option>
                </select>
                <button @click="inspect(r)" class="btn">Inspect</button>
              </div>
              <span v-else>—</span>
            </td>
          </tr>
          <tr v-if="returns.length === 0">
            <td colspan="5" class="empty">No return requests.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import api from '@/services/axiosConfig'
export default {
  name: 'AdminReturnsPage',
  data() {
    return { returns: [] }
  },
  async mounted() {
    await this.fetch()
  },
  methods: {
    formatDate(d) {
      try { return new Date(d).toLocaleString('vi-VN') } catch { return '' }
    },
    async fetch() {
      try {
        const res = await api.get('/returns')
        this.returns = res.data.map(r => ({ ...r, _selectedCondition: 'GOOD' }))
      } catch (e) {
        console.error('Failed to load returns', e)
        this.returns = []
      }
    },
    async receive(r) {
      if (!confirm('Mark asset as received?')) return
      try {
        await api.post(`/returns/${r.id}/receive`, { physicallyReceived: true })
        alert('Asset received')
        await this.fetch()
      } catch (e) {
        alert(e?.response?.data?.message || 'Failed')
      }
    },
    async inspect(r) {
      if (!confirm('Confirm inspection and set asset condition?')) return
      try {
        await api.post(`/returns/${r.id}/inspect`, { handbackCondition: r._selectedCondition })
        alert('Inspected')
        await this.fetch()
      } catch (e) {
        alert(e?.response?.data?.message || 'Failed')
      }
    }
  }
}
</script>

<style scoped>
.page { padding: 24px }
.table-card { background: white; border-radius: 8px; overflow: hidden }
.data-table { width:100%; border-collapse: collapse }
.data-table th{ background:#f3f4f6; padding:12px; text-align:left }
.data-table td{ padding:12px; border-bottom:1px solid #eee }
.btn{ background:#0f62ff; color:white; border:none; padding:6px 10px; border-radius:6px }
.empty{ text-align:center; color:#9ca3af; padding:24px }
</style>

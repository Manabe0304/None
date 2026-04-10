<template>
  <AppModal v-model="visible" title="Asset Detail" width="900px">
    <template #body>
      <div v-if="loading">Loading detail...</div>
      <div v-else-if="!detail">No detail found.</div>
      <template v-else>
        <div class="asset-grid">
          <div><strong>Asset Tag:</strong> {{ detail.assetTag }}</div>
          <div><strong>Category:</strong> {{ detail.category || '—' }}</div>
          <div><strong>Model:</strong> {{ detail.model || '—' }}</div>
          <div><strong>Serial:</strong> {{ detail.serialNumber || '—' }}</div>
          <div><strong>Status:</strong> {{ detail.status }}</div>
          <div><strong>Department:</strong> {{ detail.department || '—' }}</div>
          <div><strong>Current User:</strong> {{ detail.currentUser || '—' }}</div>
          <div v-if="detail.isBeyondRepair"><strong>Beyond Repair:</strong> Yes</div>
          <div v-if="detail.isBeyondRepair"><strong>Reason:</strong> {{ detail.beyondRepairReason || '—' }}</div>
        </div>

        <div v-if="detail.liquidation" class="liquidation-box">
          <h4 class="section-title">Liquidation</h4>
          <div class="asset-grid">
            <div><strong>Reason:</strong> {{ detail.liquidation.reason || '—' }}</div>
            <div><strong>Date:</strong> {{ detail.liquidation.liquidationDate || '—' }}</div>
            <div><strong>Disposal Method:</strong> {{ detail.liquidation.disposalMethod || '—' }}</div>
            <div><strong>Created By:</strong> {{ detail.liquidation.createdBy || '—' }}</div>
            <div class="full-row"><strong>Notes:</strong> {{ detail.liquidation.notes || '—' }}</div>
          </div>
        </div>

        <h4 class="section-title">Lifecycle History</h4>
        <div class="timeline" v-if="detail.history?.length">
          <div class="timeline-item" v-for="item in detail.history" :key="`${item.action}-${item.changedAt}-${item.note}`">
            <div class="timeline-head">
              <strong>{{ item.action }}</strong>
              <span>{{ formatDate(item.changedAt) }}</span>
            </div>
            <div class="timeline-meta">
              <span>{{ item.previousStatus || '—' }}</span>
              <span>→</span>
              <span>{{ item.newStatus || '—' }}</span>
            </div>
            <div class="timeline-note">{{ item.note || '—' }}</div>
            <div class="timeline-by">By: {{ item.changedBy || 'System' }}</div>
          </div>
        </div>
        <div v-else>No history yet.</div>
      </template>
    </template>

    <template #footer>
      <button class="close-btn" @click="visible = false">Close</button>
    </template>
  </AppModal>
</template>

<script>
  import api from "@/services/axiosConfig"
  import AppModal from "@/components/common/AppModal.vue"

  export default {
    name: "AssetDetailModal",
    components: { AppModal },
    props: {
      assetId: {
        type: String,
        required: true,
      },
    },
    emits: ["close"],
    data() {
      return {
        visible: true,
        loading: false,
        detail: null,
      }
    },
    watch: {
      visible(value) {
        if (!value) this.$emit("close")
      },
      assetId: {
        immediate: false,
        async handler() {
          if (this.assetId) {
            await this.loadDetail()
          }
        },
      },
    },
    async mounted() {
      await this.loadDetail()
    },
    methods: {
      async loadDetail() {
        if (!this.assetId) {
          this.detail = null
          return
        }

        try {
          this.loading = true
          this.detail = null

          const res = await api.get(`/assets/${this.assetId}/detail`)
          this.detail = res.data?.data ?? res.data ?? null
        } catch (error) {
          console.error("Load asset detail failed", error)
          this.detail = null
        } finally {
          this.loading = false
        }
      },
      formatDate(value) {
        if (!value) return ""
        return new Date(value).toLocaleString("vi-VN")
      },
    },
  }
</script>

<style scoped>
  .asset-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
    margin-bottom: 20px;
  }

  .full-row {
    grid-column: 1 / -1;
  }

  .section-title {
    margin: 18px 0 12px;
  }

  .liquidation-box {
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 12px;
    background: #fff7ed;
  }

  .timeline {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .timeline-item {
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    padding: 12px;
    background: #f8fafc;
  }

  .timeline-head {
    display: flex;
    justify-content: space-between;
    gap: 12px;
  }

  .timeline-meta,
  .timeline-by {
    margin-top: 6px;
    color: #64748b;
    font-size: 14px;
  }

  .timeline-note {
    margin-top: 8px;
    white-space: pre-wrap;
  }

  .close-btn {
    border: none;
    border-radius: 8px;
    padding: 8px 12px;
    background: #e5e7eb;
    cursor: pointer;
  }
</style>

<template>
  <AppModal v-model="visible" title="Request Detail" width="600px">
    <template #body>
      <div v-if="request" class="detail-container">
        <div class="detail-row">
          <span class="label">Employee:</span>
          <span>{{ request.employeeName }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Department:</span>
          <span>{{ request.department || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Asset Requested:</span>
          <span>{{ request.assetName || request.assetType || 'N/A' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Urgency:</span>
          <span class="urgency" :class="request.urgency?.toLowerCase()">{{ request.urgency }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Reason:</span>
          <p class="reason-text">{{ request.reason }}</p>
        </div>
        <div class="detail-row">
          <span class="label">Date:</span>
          <span>{{ formatDate(request.requestDate) }}</span>
        </div>
      </div>
      <div v-else class="empty">No data</div>
    </template>
    <template #footer>
      <button class="btn btn-secondary" @click="close">Close</button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'

const props = defineProps({
  request: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close'])

const visible = ref(true)

watch(visible, (val) => {
  if (!val) {
    emit('close')
  }
})

function close() {
  visible.value = false
}

function formatDate(dateString) {
  if (!dateString) return '-'
  return new Intl.DateTimeFormat('en-GB').format(new Date(dateString))
}
</script>

<style scoped>
.detail-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.label {
  font-weight: 600;
  color: #475569;
  font-size: 14px;
}

.reason-text {
  margin: 0;
  padding: 10px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  color: #334155;
  white-space: pre-wrap;
}

.urgency {
  display: inline-flex;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 700;
  width: fit-content;
}

.urgency.high {
  background: #fee2e2;
  color: #b91c1c;
}
.urgency.medium {
  background: #fef3c7;
  color: #b45309;
}
.urgency.low {
  background: #dcfce7;
  color: #15803d;
}

.btn {
  padding: 8px 16px;
  border-radius: 8px;
  border: none;
  font-weight: 600;
  cursor: pointer;
}

.btn-secondary {
  background: #e2e8f0;
  color: #475569;
}
</style>

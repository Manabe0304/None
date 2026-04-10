<template>
  <AppModal :model-value="modelValue"
            title="Assignment History Detail"
            width="860px"
            @update:modelValue="emit('update:modelValue', $event)">
    <template #body>
      <div v-if="item" class="detail-grid">
        <div class="detail-item">
          <span class="label">Changed At</span>
          <span class="value">{{ formatDate(item.changedAt) }}</span>
        </div>

        <div class="detail-item">
          <span class="label">Asset</span>
          <span class="value">{{ item.assetTag || "—" }}</span>
        </div>

        <div class="detail-item">
          <span class="label">Asset Type</span>
          <span class="value">{{ item.assetType || "—" }}</span>
        </div>

        <div class="detail-item">
          <span class="label">Employee</span>
          <span class="value">{{ item.employee || "—" }}</span>
        </div>

        <div class="detail-item">
          <span class="label">Department</span>
          <span class="value">{{ item.department || "—" }}</span>
        </div>

        <div class="detail-item">
          <span class="label">Changed By</span>
          <span class="value">{{ item.changedBy || "—" }}</span>
        </div>
      </div>

      <div v-if="item" class="detail-block">
        <h4>Action</h4>
        <p>{{ item.action || "—" }}</p>
      </div>

      <div v-if="item" class="detail-grid detail-grid-status">
        <div class="detail-item">
          <span class="label">Previous Status</span>
          <span class="value">{{ item.previousStatus || "—" }}</span>
        </div>
        <div class="detail-item">
          <span class="label">New Status</span>
          <span class="value">{{ item.newStatus || "—" }}</span>
        </div>
      </div>

      <div v-if="item" class="detail-block">
        <h4>Note</h4>
        <p>{{ item.note || "—" }}</p>
      </div>
    </template>

    <template #footer>
      <button class="close-btn" @click="emit('update:modelValue', false)">Close</button>
    </template>
  </AppModal>
</template>

<script setup>
import AppModal from "@/components/common/AppModal.vue"

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  item: { type: Object, default: null }
})

const emit = defineEmits(["update:modelValue"])

function formatDate(value) {
  if (!value) return "N/A"
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return "N/A"
  return date.toLocaleString("vi-VN")
}
</script>

<style scoped>
  .detail-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 14px;
  }

  .detail-grid-status {
    margin-top: 18px;
  }

  .detail-item {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 14px;
    background: #f8fafc;
    border-radius: 12px;
  }

  .label {
    font-size: 12px;
    font-weight: 700;
    color: #64748b;
    text-transform: uppercase;
  }

  .value {
    color: #0f172a;
    word-break: break-word;
  }

  .detail-block {
    margin-top: 18px;
    padding: 16px;
    border: 1px solid #e2e8f0;
    border-radius: 12px;
  }

    .detail-block h4 {
      margin: 0 0 10px;
      font-size: 14px;
      font-weight: 700;
      color: #0f172a;
    }

    .detail-block p {
      margin: 0;
      color: #334155;
      line-height: 1.6;
      white-space: pre-wrap;
    }

  .close-btn {
    border: none;
    background: #e2e8f0;
    color: #0f172a;
    border-radius: 10px;
    padding: 10px 16px;
    font-weight: 700;
    cursor: pointer;
  }

  @media (max-width: 768px) {
    .detail-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

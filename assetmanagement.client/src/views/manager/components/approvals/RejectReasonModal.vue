<template>
  <div v-if="modelValue" class="modal-overlay" @click.self="handleClose">
    <div class="modal-container">
      <div class="modal-header">
        <h3>Reject Request</h3>
        <button class="icon-close" type="button" @click="handleClose">×</button>
      </div>

      <div class="modal-body">
        <p class="modal-description">
          Please enter the reason for rejecting this request.
        </p>

        <label class="form-label" for="reject-reason">Reject reason</label>
        <textarea
          id="reject-reason"
          v-model.trim="localReason"
          class="form-textarea"
          rows="5"
          placeholder="Enter reject reason..."
          maxlength="500"
        />

        <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
      </div>

      <div class="modal-footer">
        <button class="btn btn-secondary" type="button" @click="handleClose">
          Cancel
        </button>
        <button
          class="btn btn-danger"
          type="button"
          :disabled="submitting"
          @click="handleSubmit"
        >
          {{ submitting ? 'Rejecting...' : 'Confirm Reject' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'submit'])

const localReason = ref('')
const errorMessage = ref('')

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      localReason.value = ''
      errorMessage.value = ''
    }
  }
)

function handleClose() {
  emit('update:modelValue', false)
}

function handleSubmit() {
  if (!localReason.value) {
    errorMessage.value = 'Reject reason is required.'
    return
  }

  errorMessage.value = ''
  emit('submit', localReason.value)
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(13, 27, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  padding: 16px;
}

.modal-container {
  width: 100%;
  max-width: 520px;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.18);
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 20px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  margin: 0;
  font-size: 22px;
  color: #0f2f5f;
}

.icon-close {
  border: none;
  background: transparent;
  font-size: 26px;
  cursor: pointer;
  color: #475569;
}

.modal-body {
  padding: 20px;
}

.modal-description {
  margin: 0 0 14px;
  color: #475569;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #0f172a;
}

.form-textarea {
  width: 100%;
  resize: vertical;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 12px 14px;
  font-size: 14px;
  outline: none;
  transition: 0.2s ease;
  box-sizing: border-box;
}

.form-textarea:focus {
  border-color: #0f3d75;
  box-shadow: 0 0 0 3px rgba(15, 61, 117, 0.12);
}

.error-text {
  margin-top: 10px;
  color: #dc2626;
  font-size: 14px;
  font-weight: 500;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 16px 20px 20px;
}

.btn {
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  font-weight: 700;
  cursor: pointer;
  min-width: 120px;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.btn-secondary {
  background: #e2e8f0;
  color: #1e293b;
}

.btn-danger {
  background: #e74c3c;
  color: #fff;
}
</style>
<template>
  <AppModal v-model="visible" title="Initiate Return" width="560px">
    <template #body>
      <div class="summary-box">
        <p><strong>Asset:</strong> {{ assignment?.assetName || '-' }}</p>
        <p><strong>Asset Tag:</strong> {{ assignment?.assetTag || '-' }}</p>
        <p><strong>Assignment ID:</strong> {{ assignment?.id || '-' }}</p>
      </div>

      <div class="field">
        <label>Return Reason <span class="required">*</span></label>
        <textarea v-model="reason" rows="4" placeholder="Enter reason for returning this asset" />
      </div>

      <div class="field">
        <label>Notes</label>
        <textarea v-model="notes" rows="3" placeholder="Additional notes (optional)" />
      </div>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    </template>

    <template #footer>
      <button class="secondary-btn" @click="close">Cancel</button>
      <button class="primary-btn" :disabled="submitting" @click="submitForm">
        {{ submitting ? 'Submitting...' : 'Submit Return Request' }}
      </button>
    </template>
  </AppModal>
</template>

<script>
  import * as svc from "../services"
  import AppModal from "@/components/common/AppModal.vue"

  export default {
    name: "InitiateReturnModal",
    components: { AppModal },
    props: {
      assignment: {
        type: Object,
        default: null,
      },
    },
    emits: ["close", "success"],
    data() {
      return {
        visible: true,
        reason: "",
        notes: "",
        submitting: false,
        errorMessage: "",
      }
    },
    watch: {
      visible(value) {
        if (!value) this.$emit("close")
      },
    },
    methods: {
      close() {
        this.visible = false
      },
      async submitForm() {
        this.errorMessage = ""

        if (!this.assignment?.id || !this.assignment?.assetId) {
          this.errorMessage = "Invalid assignment selected."
          return
        }

        if (!this.reason.trim()) {
          this.errorMessage = "Return reason is required."
          return
        }

        this.submitting = true

        try {
          await svc.initiateReturn({
            assignmentId: this.assignment.id,
            assetId: this.assignment.assetId,
            reason: this.reason.trim(),
            notes: this.notes.trim(),
          })

          this.$emit("success")
        } catch (error) {
          console.error("Failed to initiate return", error)
          this.errorMessage = error?.response?.data?.message || error?.message || "Failed to create return request."
        } finally {
          this.submitting = false
        }
      },
    },
  }
</script>

<style scoped>
  .summary-box {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    padding: 12px;
    margin-bottom: 14px;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin-bottom: 14px;
  }

    .field textarea {
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 10px 12px;
      font: inherit;
    }

  .required,
  .error-text {
    color: #dc2626;
  }

  .primary-btn,
  .secondary-btn {
    border: none;
    border-radius: 8px;
    padding: 8px 12px;
    cursor: pointer;
    font-weight: 600;
  }

  .primary-btn {
    background: #0f766e;
    color: #fff;
  }

  .secondary-btn {
    background: #e5e7eb;
    color: #111827;
  }
</style>

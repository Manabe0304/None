<template>
  <AppModal v-model="visible" :title="title" width="560px">
    <template #body>
      <div class="summary-box" v-if="asset">
        <p><strong>Asset:</strong> {{ asset.model || asset.assetTag }}</p>
        <p><strong>Tag:</strong> {{ asset.assetTag }}</p>
        <p><strong>Status:</strong> {{ asset.status }}</p>
      </div>

      <div class="form-group">
        <label>Reason <span class="required">*</span></label>
        <textarea v-model.trim="form.reason"
                  rows="4"
                  placeholder="Enter reason..." />
        <p v-if="error" class="error-text">{{ error }}</p>
      </div>

      <template v-if="mode === 'liquidate'">
        <div class="form-group">
          <label>Liquidation Date <span class="required">*</span></label>
          <input v-model="form.liquidationDate" type="date" />
        </div>

        <div class="form-group">
          <label>Disposal Method</label>
          <input v-model.trim="form.disposalMethod"
                 type="text"
                 placeholder="Sold / Recycled / Destroyed..." />
        </div>

        <div class="form-group">
          <label>Notes</label>
          <textarea v-model.trim="form.notes"
                    rows="3"
                    placeholder="Optional notes..." />
        </div>
      </template>
    </template>

    <template #footer>
      <button class="btn btn-secondary" @click="close">Cancel</button>
      <button class="btn btn-primary" @click="submit">
        {{ mode === "beyondRepair" ? "Confirm" : "Liquidate" }}
      </button>
    </template>
  </AppModal>
</template>

<script>
import AppModal from "@/components/common/AppModal.vue"

export default {
  name: "AssetLifecycleModal",
  components: { AppModal },
  props: {
    modelValue: Boolean,
    asset: Object,
    mode: {
      type: String,
      default: "beyondRepair",
    },
  },
  emits: ["update:modelValue", "submit"],
  data() {
    return {
      visible: this.modelValue,
      error: "",
      form: {
        reason: "",
        liquidationDate: new Date().toISOString().slice(0, 10),
        disposalMethod: "",
        notes: "",
      },
    }
  },
  computed: {
    title() {
      return this.mode === "beyondRepair"
        ? "Mark Asset as Beyond Repair"
        : "Liquidate Asset"
    },
  },
  watch: {
    modelValue(val) {
      this.visible = val
      if (val) {
        this.error = ""
        this.form = {
          reason: "",
          liquidationDate: new Date().toISOString().slice(0, 10),
          disposalMethod: "",
          notes: "",
        }
      }
    },
    visible(val) {
      this.$emit("update:modelValue", val)
    },
  },
  methods: {
    close() {
      this.visible = false
    },
    submit() {
      this.error = ""

      if (!this.form.reason) {
        this.error = "Reason is required."
        return
      }

      if (this.mode === "liquidate" && !this.form.liquidationDate) {
        this.error = "Liquidation date is required."
        return
      }

      this.$emit("submit", { ...this.form })
    },
  },
}
</script>

<style scoped>
  .summary-box {
    background: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 10px;
    padding: 12px;
    margin-bottom: 14px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 14px;
  }

    .form-group label {
      font-weight: 600;
      color: #374151;
    }

    .form-group input,
    .form-group textarea {
      border: 1px solid #d1d5db;
      border-radius: 10px;
      padding: 12px;
      font: inherit;
    }

  .required {
    color: #dc2626;
  }

  .error-text {
    color: #dc2626;
    font-size: 13px;
  }

  .btn {
    border: none;
    border-radius: 10px;
    padding: 10px 14px;
    font-weight: 600;
    cursor: pointer;
  }

  .btn-primary {
    background: #1d4ed8;
    color: #fff;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #111827;
  }
</style>

<template>
  <div class="request-card">
    <div class="card-header">
      <h2>Send Request</h2>
      <p>Submit a new equipment request for review.</p>
    </div>

    <form class="request-form" @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="assetType">Asset Type</label>
        <select id="assetType" v-model="form.assetType" class="form-select" @change="handleAssetTypeChange">
          <option value="">Select asset type</option>
          <option value="LAPTOP">LAPTOP</option>
          <option value="MONITOR">MONITOR</option>
          <option value="KEYBOARD">KEYBOARD</option>
          <option value="MOUSE">MOUSE</option>
          <option value="HEADSET">HEADSET</option>
          <option value="DOCKING_STATION">DOCKING STATION</option>
          <option value="PRINTER">PRINTER</option>
        </select>
        <p v-if="errors.assetType" class="error-text">{{ errors.assetType }}</p>
      </div>

      <div class="form-group">
        <label for="preferredModel">Product Name</label>
        <select id="preferredModel"
                v-model="form.preferredModel"
                class="form-select"
                :disabled="!form.assetType || modelLoading">
          <option value="">Select product name</option>
          <option v-for="item in availableModels" :key="item.assetTag || item.AssetTag || item.model || item.Model || item" :value="item.model || item.Model || item">
            {{ item.model || item.Model || item }}<template v-if="item.assetTag || item.AssetTag"> - {{ item.assetTag || item.AssetTag }}</template>
          </option>
        </select>
        <p v-if="errors.preferredModel" class="error-text">{{ errors.preferredModel }}</p>
      </div>

      <div class="form-group">
        <label for="reason">Reason</label>
        <textarea id="reason"
                  v-model.trim="form.reason"
                  class="form-textarea"
                  rows="5"
                  placeholder="Explain why you need this asset" />
        <p v-if="errors.reason" class="error-text">{{ errors.reason }}</p>
      </div>

      <div class="form-group">
        <label for="urgency">Urgency</label>
        <select id="urgency" v-model="form.urgency" class="form-select">
          <option value="">Select urgency</option>
          <option value="LOW">LOW</option>
          <option value="MEDIUM">MEDIUM</option>
          <option value="HIGH">HIGH</option>
          <option value="CRITICAL">CRITICAL</option>
        </select>
        <p v-if="errors.urgency" class="error-text">{{ errors.urgency }}</p>
      </div>

      <div class="form-actions">
        <button class="btn-submit" type="submit" :disabled="submitting">
          {{ submitting ? "Submitting..." : "Submit Request" }}
        </button>
      </div>

      <p v-if="successMessage" class="success-text">{{ successMessage }}</p>
      <p v-if="serverError" class="error-text">{{ serverError }}</p>
    </form>
  </div>
</template>

<script setup>
  import { reactive, ref } from "vue"
  import { getAvailableModelsByType, submitEmployeeRequest } from "../../services/employeeRequest.service"

  const emit = defineEmits(["submitted"])

  const form = reactive({
    assetType: "",
    preferredModel: "",
    reason: "",
    urgency: "",
  })

  const errors = reactive({
    assetType: "",
    preferredModel: "",
    reason: "",
    urgency: "",
  })

  const submitting = ref(false)
  const successMessage = ref("")
  const serverError = ref("")
  const availableModels = ref([])
  const modelLoading = ref(false)

  async function handleAssetTypeChange() {
    form.preferredModel = ""
    availableModels.value = []

    if (!form.assetType) return

    try {
      modelLoading.value = true
      availableModels.value = await getAvailableModelsByType(form.assetType)
    } catch (error) {
      console.error("Load models failed:", error)
      availableModels.value = []
    } finally {
      modelLoading.value = false
    }
  }

  function validateForm() {
    let isValid = true

    errors.assetType = ""
    errors.preferredModel = ""
    errors.reason = ""
    errors.urgency = ""
    serverError.value = ""

    if (!form.assetType) {
      errors.assetType = "Asset type is required."
      isValid = false
    }

    if (!form.preferredModel) {
      errors.preferredModel = "Product name is required."
      isValid = false
    }

    if (!form.reason) {
      errors.reason = "Reason is required."
      isValid = false
    } else if (form.reason.trim().length < 10) {
      errors.reason = "Reason must be at least 10 characters."
      isValid = false
    }

    if (!form.urgency) {
      errors.urgency = "Urgency is required."
      isValid = false
    }

    return isValid
  }

  async function handleSubmit() {
    if (!validateForm()) return

    submitting.value = true
    successMessage.value = ""
    serverError.value = ""

    try {
      const created = await submitEmployeeRequest({
        assetType: form.assetType,
        preferredModel: form.preferredModel,
        reason: form.reason,
        urgency: form.urgency,
      })

      successMessage.value = `Request submitted successfully. Status is now ${created?.status || "PENDING"}.`

      form.assetType = ""
      form.preferredModel = ""
      form.reason = ""
      form.urgency = ""
      availableModels.value = []

      emit("submitted", created)
    } catch (error) {
      console.error("Submit request failed:", error)
      serverError.value = error?.response?.data?.message || "Failed to submit request."
    } finally {
      submitting.value = false
    }
  }
</script>

<style scoped>
  .request-card {
    background: #ffffff;
    border-radius: 14px;
    padding: 24px;
    box-shadow: 0 4px 16px rgba(15, 23, 42, 0.08);
  }

  .card-header {
    margin-bottom: 20px;
  }

    .card-header h2 {
      margin: 0 0 8px;
      color: #0f2f5f;
      font-size: 32px;
      font-weight: 700;
    }

    .card-header p {
      margin: 0;
      color: #64748b;
    }

  .request-form {
    display: flex;
    flex-direction: column;
    gap: 18px;
  }

  .form-group label {
    display: block;
    margin-bottom: 8px;
    font-weight: 700;
    color: #0f172a;
  }

  .form-input,
  .form-textarea,
  .form-select {
    width: 100%;
    box-sizing: border-box;
    border: 1px solid #cbd5e1;
    border-radius: 10px;
    padding: 12px 14px;
    font-size: 14px;
    outline: none;
    transition: 0.2s ease;
  }

    .form-input:focus,
    .form-textarea:focus,
    .form-select:focus {
      border-color: #0f3d75;
      box-shadow: 0 0 0 3px rgba(15, 61, 117, 0.12);
    }

  .form-textarea {
    resize: vertical;
  }

  .form-actions {
    display: flex;
    justify-content: flex-start;
  }

  .btn-submit {
    border: none;
    border-radius: 10px;
    background: #22c55e;
    color: white;
    font-weight: 700;
    padding: 12px 20px;
    cursor: pointer;
    min-width: 160px;
  }

    .btn-submit:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

  .error-text {
    margin-top: 8px;
    color: #dc2626;
    font-size: 14px;
    font-weight: 500;
  }

  .success-text {
    color: #15803d;
    font-size: 14px;
    font-weight: 600;
  }
</style>

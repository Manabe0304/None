<template>
  <div class="page">
    <h2>Assign Asset to Employee</h2>
    <p class="subtitle">Select an approved request and an available asset to create an assignment.</p>

    <div class="form-card">
      <!-- Step 1: Select Request -->
      <div class="form-group">
        <label>Approved Request</label>
        <select v-model="selectedRequest">
          <option value="">-- Select a request --</option>
          <option v-for="req in requests" :key="req.id" :value="req">
            {{ req.employeeName }} — {{ req.assetType }} ({{ req.reason }})
          </option>
        </select>
        <p v-if="requests.length === 0" class="hint">No approved requests available.</p>
      </div>

      <!-- Step 2: Select Asset -->
      <div class="form-group">
        <label>Available Asset</label>
        <select v-model="selectedAssetId">
          <option value="">-- Select an asset --</option>
          <option v-for="asset in assets" :key="asset.id" :value="asset.id">
            {{ asset.assetTag }} — {{ asset.category }}
          </option>
        </select>
        <p v-if="assets.length === 0" class="hint">No available assets.</p>
      </div>

      <button class="btn-assign" :disabled="!selectedRequest || !selectedAssetId || loading" @click="assign">
        {{ loading ? "Assigning..." : "Assign Asset" }}
      </button>

      <p v-if="successMsg" class="success">{{ successMsg }}</p>
      <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
    </div>
  </div>
</template>

<script>
import axios from "axios"

export default {
  name: "AssignAsset",
  data() {
    return {
      requests: [],
      assets: [],
      selectedRequest: "",
      selectedAssetId: "",
      loading: false,
      successMsg: "",
      errorMsg: ""
    }
  },
  async mounted() {
    await Promise.all([this.loadRequests(), this.loadAssets()])
  },
  methods: {
    async loadRequests() {
      const res = await axios.get("/api/assignments/approved-requests")
      this.requests = res.data
    },
    async loadAssets() {
      const res = await axios.get("/api/assignments/available-assets")
      this.assets = res.data
    },
    async assign() {
      this.successMsg = ""
      this.errorMsg = ""
      this.loading = true
      try {
        await axios.post("/api/assignments", {
          assetId: this.selectedAssetId,
          userId: this.selectedRequest.employeeId,
          requestId: this.selectedRequest.id,
          assignedById: null
        })
        this.successMsg = "Asset assigned successfully!"
        this.selectedRequest = ""
        this.selectedAssetId = ""
        await Promise.all([this.loadRequests(), this.loadAssets()])
      } catch (e) {
        this.errorMsg = e.response?.data?.message || "Assignment failed. Please try again."
      } finally {
        this.loading = false
      }
    }
  }
}
</script>

<style scoped>
.page {
  padding: 24px;
}
.page h2 {
  font-size: 22px;
  font-weight: 600;
  margin-bottom: 6px;
}
.subtitle {
  color: #6b7280;
  margin-bottom: 24px;
}
.form-card {
  background: white;
  padding: 28px;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
  max-width: 560px;
}
.form-group {
  margin-bottom: 20px;
}
.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 8px;
  color: #374151;
}
.form-group select {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}
.hint {
  color: #9ca3af;
  font-size: 13px;
  margin-top: 4px;
}
.btn-assign {
  background: #10b981;
  color: white;
  border: none;
  padding: 10px 24px;
  border-radius: 6px;
  font-size: 15px;
  cursor: pointer;
  width: 100%;
}
.btn-assign:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.success {
  color: #10b981;
  margin-top: 14px;
  font-weight: 500;
}
.error {
  color: #ef4444;
  margin-top: 14px;
}
</style>

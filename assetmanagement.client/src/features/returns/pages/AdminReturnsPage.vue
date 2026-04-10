<template>
  <section class="admin-returns-page">
    <div class="page-header">
      <h2>Returns Management</h2>
      <p class="muted">Manage pending handbacks and post-return inspections.</p>
    </div>

    <div class="tabs">
      <button class="tab-btn"
              :class="{ active: activeTab === 'pending' }"
              @click="activeTab = 'pending'">
        Pending Returns
      </button>
      <button class="tab-btn"
              :class="{ active: activeTab === 'inspection' }"
              @click="activeTab = 'inspection'">
        Pending Inspection
      </button>
      <button class="tab-btn"
              :class="{ active: activeTab === 'history' }"
              @click="activeTab = 'history'">
        Processed History
      </button>
    </div>

    <div v-if="loading" class="card">
      <p>Loading returns data...</p>
    </div>

    <template v-else>
      <div v-if="activeTab === 'pending'" class="card">
        <h3>Pending Returns</h3>

        <div v-if="pendingReturns.length === 0" class="empty">
          No pending returns found.
        </div>

        <table v-else class="table">
          <thead>
            <tr>
              <th>Asset</th>
              <th>Employee</th>
              <th>Reason</th>
              <th>Initiated By</th>
              <th>Initiated At</th>
              <th>Status</th>
              <th style="width: 140px">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in pendingReturns" :key="item.id">
              <td>
                <div class="strong">{{ item.assetName || item.assetTag || item.assetId }}</div>
                <div class="muted small">{{ item.assetTag || "-" }}</div>
              </td>
              <td>{{ item.targetUserName || item.targetUserId || "-" }}</td>
              <td>{{ item.reason || "-" }}</td>
              <td>{{ item.requestedByName || item.requestedByUserId || "-" }}</td>
              <td>{{ formatDate(item.initiatedAt || item.createdAt) }}</td>
              <td>
                <span class="status-badge">{{ item.status }}</span>
              </td>
              <td style="display: flex; gap: 8px;">
                <button class="secondary-btn" @click="openDetail(item.assetId)">Detail</button>
                <button class="primary-btn" @click="openProcessModal(item)">
                  Process
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="activeTab === 'inspection'" class="card">
        <h3>Pending Inspection</h3>

        <div v-if="pendingInspection.length === 0" class="empty">
          No assets pending inspection.
        </div>

        <table v-else class="table">
          <thead>
            <tr>
              <th>Asset</th>
              <th>Employee</th>
              <th>Reason</th>
              <th>Handback Condition</th>
              <th>Status</th>
              <th style="width: 140px">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in pendingInspection" :key="item.id">
              <td>
                <div class="strong">{{ item.assetName || item.assetTag || item.assetId }}</div>
                <div class="muted small">{{ item.assetTag || "-" }}</div>
              </td>
              <td>{{ item.targetUserName || item.targetUserId || "-" }}</td>
              <td>{{ item.reason || "-" }}</td>
              <td>{{ item.handbackCondition || "-" }}</td>
              <td>
                <span class="status-badge">{{ item.status }}</span>
              </td>
              <td style="display: flex; gap: 8px;">
                <button class="secondary-btn" @click="openDetail(item.assetId)">Detail</button>
                <button class="primary-btn" @click="openInspectionModal(item)">
                  Inspect
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="activeTab === 'history'" class="card">
        <h3>Processed Returns</h3>

        <div v-if="processedReturns.length === 0" class="empty">
          No processed return history yet.
        </div>

        <table v-else class="table">
          <thead>
            <tr>
              <th>Asset</th>
              <th>Employee</th>
              <th>Final Result</th>
              <th>Inspected At</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in processedReturns" :key="item.id">
              <td>
                <div class="strong">{{ item.assetName || item.assetTag || item.assetId }}</div>
                <div class="muted small">{{ item.assetTag || "-" }}</div>
              </td>
              <td>{{ item.targetUserName || item.targetUserId || "-" }}</td>
              <td>{{ item.inspectionResult || "-" }}</td>
              <td>{{ formatDate(item.inspectedAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div v-if="showProcessModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Process Handback</h3>
          <button class="icon-btn" @click="closeProcessModal">×</button>
        </div>

        <div class="modal-body">
          <div class="summary-box">
            <p><strong>Asset:</strong> {{ selectedItem?.assetName || selectedItem?.assetTag || selectedItem?.assetId }}</p>
            <p><strong>Employee:</strong> {{ selectedItem?.targetUserName || selectedItem?.targetUserId || "-" }}</p>
            <p><strong>Reason:</strong> {{ selectedItem?.reason || "-" }}</p>
          </div>

          <label class="field-checkbox">
            <input v-model="processForm.physicallyReceived" type="checkbox" />
            <span>Device physically received</span>
          </label>

          <div class="field">
            <label>Return Date</label>
            <input v-model="processForm.returnDate" type="date" />
          </div>

          <div class="field">
            <label>Handback Condition</label>
            <select v-model="processForm.handbackCondition">
              <option value="">Select condition</option>
              <option value="GOOD">GOOD</option>
              <option value="FAIR">FAIR</option>
              <option value="DAMAGED">DAMAGED</option>
              <option value="MISSING_ACCESSORIES">MISSING_ACCESSORIES</option>
              <option value="BROKEN">BROKEN</option>
            </select>
          </div>

          <div class="field">
            <label>Notes</label>
            <textarea v-model="processForm.handbackNotes"
                      rows="4"
                      placeholder="Add notes about handback condition..." />
          </div>

          <p v-if="processError" class="error-text">{{ processError }}</p>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="closeProcessModal">Cancel</button>
          <button class="primary-btn" @click="submitProcessHandback">Confirm Handback</button>
        </div>
      </div>
    </div>

    <div v-if="showInspectionModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Inspect Returned Asset</h3>
          <button class="icon-btn" @click="closeInspectionModal">×</button>
        </div>

        <div class="modal-body">
          <div class="summary-box">
            <p><strong>Asset:</strong> {{ selectedInspection?.assetName || selectedInspection?.assetTag || selectedInspection?.assetId }}</p>
            <p><strong>Employee:</strong> {{ selectedInspection?.targetUserName || selectedInspection?.targetUserId || "-" }}</p>
            <p><strong>Reason:</strong> {{ selectedInspection?.reason || "-" }}</p>
            <p><strong>Handback Condition:</strong> {{ selectedInspection?.handbackCondition || "-" }}</p>
          </div>

          <div class="field">
            <label>Inspection Notes</label>
            <textarea v-model="inspectionForm.inspectionNotes"
                      rows="4"
                      placeholder="Describe findings from inspection..." />
          </div>

          <div class="field">
            <label>Accessories Notes</label>
            <textarea v-model="inspectionForm.accessoriesNotes"
                      rows="3"
                      placeholder="Missing charger, bag, mouse, accessories, etc." />
          </div>

          <div class="field">
            <label>Next Action</label>
            <select v-model="inspectionForm.inspectionResult">
              <option value="">Select result</option>
              <option value="AVAILABLE">AVAILABLE</option>
              <option value="MAINTENANCE">MAINTENANCE</option>
              <option value="BROKEN">BROKEN</option>
            </select>
          </div>

          <p v-if="inspectionError" class="error-text">{{ inspectionError }}</p>
        </div>

        <div class="modal-footer modal-footer-spread">
          <button class="secondary-btn danger-btn" @click="openBeyondRepairFromInspection">
            Mark as Beyond Repair
          </button>
          <div class="modal-actions-right">
            <button class="secondary-btn" @click="saveDraft">Save Draft</button>
            <button class="primary-btn" @click="submitInspection">Complete Inspection</button>
          </div>
        </div>
      </div>
    </div>

    <AssetLifecycleModal v-if="showLifecycleModal"
                         v-model="showLifecycleModal"
                         :asset="selectedLifecycleAsset"
                         mode="beyondRepair"
                         @submit="handleBeyondRepairSubmit" />

    <AssetDetailModal v-if="showDetailModal" :asset-id="selectedDetailAssetId" @close="showDetailModal = false" />
  </section>
</template>

<script>
  import * as svc from "../services"
  import AssetLifecycleModal from "@/components/assets/AssetLifecycleModal.vue"
  import AssetDetailModal from "@/components/assets/AssetDetailModal.vue"
  import { markBeyondRepair } from "@/services/assetLifecycleService"

  export default {
    name: "AdminReturnsPage",
    components: {
      AssetLifecycleModal,
      AssetDetailModal,
    },
    data() {
      return {
        activeTab: "pending",
        loading: false,
        pendingReturns: [],
        pendingInspection: [],
        processedReturns: [],
        showProcessModal: false,
        selectedItem: null,
        processForm: {
          physicallyReceived: false,
          returnDate: "",
          handbackCondition: "",
          handbackNotes: "",
        },
        processError: "",
        showInspectionModal: false,
        selectedInspection: null,
        inspectionForm: {
          inspectionNotes: "",
          accessoriesNotes: "",
          inspectionResult: "",
        },
        inspectionError: "",
        showLifecycleModal: false,
        selectedLifecycleAsset: null,
        showDetailModal: false,
        selectedDetailAssetId: null,
      }
    },
    async mounted() {
      await this.refresh()
    },
    methods: {
      openDetail(assetId) {
        this.selectedDetailAssetId = assetId
        this.showDetailModal = true
      },
      formatDate(value) {
        if (!value) return "-"
        try {
          return new Date(value).toLocaleString("vi-VN")
        } catch (e) {
          return value
        }
      },

      async refresh() {
        this.loading = true
        try {
          const [pending, inspection, processed] = await Promise.all([
            svc.getPendingReturns ? svc.getPendingReturns() : [],
            svc.getPendingInspectionReturns ? svc.getPendingInspectionReturns() : [],
            svc.getProcessedReturns ? svc.getProcessedReturns() : [],
          ])

          this.pendingReturns = Array.isArray(pending) ? pending : []
          this.pendingInspection = Array.isArray(inspection) ? inspection : []
          this.processedReturns = Array.isArray(processed) ? processed : []
        } catch (error) {
          console.error("Failed to load returns data", error)
          this.pendingReturns = []
          this.pendingInspection = []
          this.processedReturns = []
        } finally {
          this.loading = false
        }
      },

      openProcessModal(item) {
        this.selectedItem = item
        this.processForm = {
          physicallyReceived: false,
          returnDate: new Date().toISOString().slice(0, 10),
          handbackCondition: "",
          handbackNotes: "",
        }
        this.processError = ""
        this.showProcessModal = true
      },

      closeProcessModal() {
        this.showProcessModal = false
        this.selectedItem = null
        this.processError = ""
      },

      async submitProcessHandback() {
        this.processError = ""

        if (!this.processForm.physicallyReceived) {
          this.processError = "Please confirm the device has been physically received."
          return
        }

        if (!this.selectedItem?.id) {
          this.processError = "Invalid return request."
          return
        }

        try {
          if (svc.processHandback) {
            await svc.processHandback({
              returnRequestId: this.selectedItem.id,
              physicallyReceived: this.processForm.physicallyReceived,
              returnDate: this.processForm.returnDate,
              handbackCondition: this.processForm.handbackCondition,
              handbackNotes: this.processForm.handbackNotes,
              processedByUserId: "admin-1",
            })
          }

          this.closeProcessModal()
          await this.refresh()
        } catch (error) {
          console.error("Process handback failed", error)
          this.processError =
            error?.response?.data?.message || error?.message || "Failed to process handback."
        }
      },

      openInspectionModal(item) {
        this.selectedInspection = item
        this.inspectionForm = {
          inspectionNotes: item?.inspectionNotes || "",
          accessoriesNotes: item?.accessoriesNotes || "",
          inspectionResult: item?.inspectionResult || "",
        }
        this.inspectionError = ""
        this.showInspectionModal = true
      },

      closeInspectionModal() {
        this.showInspectionModal = false
        this.selectedInspection = null
        this.inspectionError = ""
      },

      openBeyondRepairFromInspection() {
        this.inspectionError = ""

        if (!this.selectedInspection?.assetId) {
          this.inspectionError = "Invalid asset."
          return
        }

        this.selectedLifecycleAsset = {
          id: this.selectedInspection.assetId,
          assetTag: this.selectedInspection.assetTag,
          model: this.selectedInspection.assetName,
          status: "UNDER_MAINTENANCE",
        }
        this.showLifecycleModal = true
      },

      async handleBeyondRepairSubmit(form) {
        try {
          if (!this.selectedLifecycleAsset?.id) return

          await markBeyondRepair(this.selectedLifecycleAsset.id, form.reason)

          this.showLifecycleModal = false
          this.closeInspectionModal()
          await this.refresh()
        } catch (error) {
          console.error("Mark beyond repair failed", error)
          this.inspectionError =
            error?.response?.data?.message || error?.message || "Failed to mark asset as beyond repair."
        }
      },

      async saveDraft() {
        this.inspectionError = ""

        if (!this.selectedInspection?.id) {
          this.inspectionError = "Invalid inspection request."
          return
        }

        try {
          if (svc.saveInspectionDraft) {
            await svc.saveInspectionDraft({
              returnRequestId: this.selectedInspection.id,
              inspectionNotes: this.inspectionForm.inspectionNotes,
              accessoriesNotes: this.inspectionForm.accessoriesNotes,
            })
          }

          this.closeInspectionModal()
          await this.refresh()
        } catch (error) {
          console.error("Save inspection draft failed", error)
          this.inspectionError = error?.message || "Failed to save inspection draft."
        }
      },

      async submitInspection() {
        this.inspectionError = ""

        if (!this.inspectionForm.inspectionResult) {
          this.inspectionError = "Please select the next action."
          return
        }

        if (!this.selectedInspection?.id) {
          this.inspectionError = "Invalid inspection request."
          return
        }

        try {
          if (svc.completeInspection) {
            await svc.completeInspection({
              returnRequestId: this.selectedInspection.id,
              inspectionNotes: this.inspectionForm.inspectionNotes,
              accessoriesNotes: this.inspectionForm.accessoriesNotes,
              inspectionResult: this.inspectionForm.inspectionResult,
              inspectedByUserId: "admin-1",
            })
          }

          this.closeInspectionModal()
          await this.refresh()
        } catch (error) {
          console.error("Complete inspection failed", error)
          this.inspectionError =
            error?.response?.data?.message || error?.message || "Failed to complete inspection."
        }
      },
    },
  }
</script>

<style scoped>
  .admin-returns-page {
    box-sizing: border-box;
    width: 100%;
    padding: 16px;
  }

  .page-header {
    margin-bottom: 16px;
  }

  .tabs {
    display: flex;
    gap: 12px;
    margin-bottom: 16px;
  }

  .tab-btn {
    border: 1px solid #d1d5db;
    background: #fff;
    color: #111827;
    padding: 8px 14px;
    border-radius: 8px;
    cursor: pointer;
  }

    .tab-btn.active {
      background: #0f2f5f;
      color: #fff;
      border-color: #0f2f5f;
    }

  .card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    margin-bottom: 16px;
    overflow-x: auto;
  }

  .table {
    width: 100%;
    border-collapse: collapse;
  }

    .table th,
    .table td {
      padding: 10px;
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
      vertical-align: top;
    }

  .strong {
    font-weight: 600;
  }

  .small {
    font-size: 12px;
  }

  .muted {
    color: #6b7280;
  }

  .empty {
    color: #6b7280;
    padding: 8px 0;
  }

  .status-badge {
    display: inline-block;
    background: #eef2ff;
    color: #3730a3;
    border-radius: 999px;
    padding: 4px 10px;
    font-size: 12px;
    font-weight: 600;
  }

  .primary-btn,
  .secondary-btn,
  .icon-btn {
    border: none;
    cursor: pointer;
    border-radius: 8px;
  }

  .primary-btn {
    background: #0f2f5f;
    color: #fff;
    padding: 8px 12px;
  }

  .secondary-btn {
    background: #e5e7eb;
    color: #111827;
    padding: 8px 12px;
  }

  .danger-btn {
    background: #b45309;
    color: #fff;
  }

  .icon-btn {
    background: transparent;
    color: #111827;
    font-size: 20px;
    line-height: 1;
  }

  .modal-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(17, 24, 39, 0.35);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 999;
  }

  .modal-card {
    width: 100%;
    max-width: 560px;
    background: #fff;
    border-radius: 14px;
    overflow: hidden;
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.18);
  }

  .modal-header,
  .modal-footer {
    padding: 14px 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .modal-footer-spread {
    gap: 12px;
  }

  .modal-actions-right {
    display: flex;
    gap: 10px;
  }

  .modal-body {
    padding: 16px;
  }

  .summary-box {
    background: #f9fafb;
    border: 1px solid #e5e7eb;
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

    .field input,
    .field select,
    .field textarea {
      border: 1px solid #d1d5db;
      border-radius: 8px;
      padding: 10px;
      font: inherit;
    }

  .field-checkbox {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 14px;
  }

  .error-text {
    color: #dc2626;
    font-size: 13px;
  }
</style>

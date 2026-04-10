<template>
  <section class="employee-returns-page">
    <h2>My Assigned Assets</h2>

    <p v-if="loading">Loading...</p>

    <div v-else class="content-stack">
      <div class="full-section">
        <EmployeeAssignedAssetsList :assignments="assignments"
                                    @initiate="openInitiateModal" />
      </div>

      <div class="full-section request-section">
        <div class="card">
          <h3>Your Return Requests</h3>

          <div v-if="returns.length === 0" class="empty">
            No return requests.
          </div>

          <table v-else class="table">
            <thead>
              <tr>
                <th>Asset</th>
                <th>Status</th>
                <th>Reason</th>
                <th>Requested At</th>
                <th>Handback Condition</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in returns" :key="r.id">
                <td>
                  <div class="tag">{{ r.assetTag || r.assetId }}</div>
                </td>
                <td>
                  <span class="status-chip">{{ r.status }}</span>
                </td>
                <td>{{ r.reason || "-" }}</td>
                <td>{{ formatDate(r.createdAt || r.initiatedAt) }}</td>
                <td>{{ r.handbackCondition || "-" }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <InitiateReturnModal v-if="showModal"
                         :assignment="selectedAssignment"
                         @close="showModal = false"
                         @success="onInitiateSuccess" />
  </section>
</template>

<script>
  import EmployeeAssignedAssetsList from "../components/EmployeeAssignedAssetsList.vue"
  import InitiateReturnModal from "../components/InitiateReturnModal.vue"
  import * as svc from "../services"

  export default {
    name: "EmployeeReturnsPage",
    components: {
      EmployeeAssignedAssetsList,
      InitiateReturnModal,
    },
    data() {
      return {
        assignments: [],
        returns: [],
        loading: false,
        showModal: false,
        selectedAssignment: null,
      }
    },
    async mounted() {
      await this.refresh()
    },
    methods: {
      formatDate(d) {
        try {
          return new Date(d).toLocaleString("vi-VN")
        } catch {
          return ""
        }
      },
      async refresh() {
        this.loading = true
        try {
          const [assignRes, returnRes] = await Promise.all([
            svc.getEmployeeAssignedAssets(),
            svc.getReturnsForUser(),
          ])

          const myAssignments = Array.isArray(assignRes) ? assignRes : []
          const myReturns = Array.isArray(returnRes) ? returnRes : []

          const returnSet = new Set()
          myReturns.forEach((r) => {
            if (r.assignmentId) returnSet.add(r.assignmentId)
          })

          this.assignments = myAssignments.map((a) => ({
            id: a.id,
            assetId: a.assetId,
            assetTag: a.assetTag || a.assetTagName || a.tag,
            assetName: a.assetName || a.name,
            assetCategory: a.category || a.assetCategory,
            assignedAt: a.assignedAt,
            status: a.status || a.assignmentStatus || "ACTIVE",
            requested: returnSet.has(a.id),
          }))

          this.returns = myReturns
        } catch (e) {
          console.error("Failed to load employee returns data", e)
          this.assignments = []
          this.returns = []
        } finally {
          this.loading = false
        }
      },
      openInitiateModal(assignment) {
        this.selectedAssignment = assignment
        this.showModal = true
      },
      async onInitiateSuccess() {
        this.showModal = false
        await this.refresh()
        alert("Return request submitted")
      },
    },
  }
</script>

<style scoped>
  .employee-returns-page {
    width: 100%;
    padding: 16px;
  }

  .content-stack {
    display: flex;
    flex-direction: column;
    gap: 24px;
  }

  .full-section {
    width: 100%;
  }

  .request-section .card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
  }

  .empty {
    color: #6b7280;
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

  .tag {
    font-weight: 600;
  }

  .status-chip {
    display: inline-block;
    background: #ecfdf5;
    color: #065f46;
    border-radius: 999px;
    padding: 4px 10px;
    font-size: 12px;
    font-weight: 600;
  }
</style>

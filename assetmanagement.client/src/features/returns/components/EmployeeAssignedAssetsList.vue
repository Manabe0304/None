<template>
  <div class="card">
    <h3 class="section-title">Assigned Assets</h3>

    <div v-if="!assignments || assignments.length === 0" class="empty">
      No active assignments.
    </div>

    <table v-else class="table">
      <thead>
        <tr>
          <th>Asset</th>
          <th>Category</th>
          <th>Assigned At</th>
          <th>Status</th>
          <th style="width: 160px;">Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="assignment in assignments" :key="assignment.id">
          <td>
            <div class="asset-name">{{ assignment.assetName || "-" }}</div>
            <div class="asset-tag">{{ assignment.assetTag || "-" }}</div>
          </td>
          <td>{{ assignment.assetCategory || "-" }}</td>
          <td>{{ formatDate(assignment.assignedAt) }}</td>
          <td>
            <span class="status-chip">
              {{ assignment.status || "ACTIVE" }}
            </span>
          </td>
          <td>
            <button class="primary-btn"
                    :disabled="assignment.requested"
                    @click="$emit('initiate', assignment)">
              {{ assignment.requested ? "Already Requested" : "Initiate Return" }}
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
  export default {
    name: "EmployeeAssignedAssetsList",
    props: {
      assignments: {
        type: Array,
        default: () => [],
      },
    },
    emits: ["initiate"],
    methods: {
      formatDate(value) {
        if (!value) return "-"
        try {
          return new Date(value).toLocaleString("vi-VN")
        } catch {
          return value
        }
      },
    },
  }
</script>

<style scoped>
  .card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
  }

  .section-title {
    margin: 0 0 12px;
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

  .asset-name {
    font-weight: 600;
  }

  .asset-tag {
    font-size: 12px;
    color: #6b7280;
  }

  .status-chip {
    display: inline-block;
    background: #eef2ff;
    color: #3730a3;
    border-radius: 999px;
    padding: 4px 10px;
    font-size: 12px;
    font-weight: 600;
  }

  .primary-btn {
    background: #0f766e;
    color: #fff;
    border: none;
    border-radius: 8px;
    padding: 8px 12px;
    cursor: pointer;
  }

    .primary-btn:disabled {
      background: #9ca3af;
      cursor: not-allowed;
    }
</style>

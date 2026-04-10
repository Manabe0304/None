<template>
  <div class="audit-logs-page">
    <div class="page-header">
      <div>
        <h2>Audit Logs</h2>
        <p class="page-subtitle">Track user activities</p>
      </div>

      <button class="refresh-btn" @click="fetchLogs" :disabled="isLoading">
        {{ isLoading ? "Loading..." : "Refresh" }}
      </button>
    </div>

    <div v-if="errorMessage" class="error-banner">
      <div>
        <strong>Cannot load audit logs.</strong>
        <p>{{ errorMessage }}</p>
      </div>

      <button class="retry-btn" @click="fetchLogs" :disabled="isLoading">
        Try again
      </button>
    </div>

    <div class="table-wrapper">
      <table class="audit-table">
        <thead>
          <tr>
            <th>Time</th>
            <th>User</th>
            <th>Department</th>
            <th>Action</th>
            <th>Target</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="log in logs" :key="log.id">
            <td>
              <div class="time-cell">
                <div class="time-main">{{ formatDate(log.createdAt) }}</div>
                <div class="time-relative">{{ formatRelativeTime(log.createdAt) }}</div>
              </div>
            </td>

            <td>{{ getUsername(log) }}</td>
            <td>{{ getDepartment(log) }}</td>

            <td>
              <span class="action-badge" :class="getActionClass(log.action)">
                {{ getAction(log) }}
              </span>
            </td>

            <td>{{ getTargetName(log) }}</td>
          </tr>

          <tr v-if="!isLoading && !errorMessage && logs.length === 0">
            <td colspan="5" class="empty-state">No audit logs found</td>
          </tr>

          <tr v-if="isLoading && logs.length === 0">
            <td colspan="5" class="empty-state">Loading audit logs...</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
  import { onMounted, onBeforeUnmount, ref } from "vue";
  import auditLogService from "@/services/auditLogService";

  const logs = ref([]);
  const isLoading = ref(false);
  const errorMessage = ref("");
  const now = ref(Date.now());

  let clockIntervalId = null;

  const normalizeLog = (log) => ({
    id:
      log?.id ??
      `${log?.createdAt ?? ""}-${log?.action ?? ""}-${log?.targetName ?? ""}`,
    createdAt: log?.createdAt ?? null,
    username:
      log?.username ??
      log?.userName ??
      log?.actorEmail ??
      log?.email ??
      "",
    department:
      log?.department ??
      log?.departmentName ??
      log?.actorDepartment ??
      "",
    action: log?.action ?? "",
    targetName:
      log?.targetName ??
      log?.entityName ??
      log?.objectName ??
      log?.targetEmail ??
      "",
  });

  const normalizeLogs = (data) => {
    if (!Array.isArray(data)) return [];
    return data.map(normalizeLog);
  };

  const isSameLogs = (oldLogs, newLogs) => {
    if (oldLogs.length !== newLogs.length) return false;

    for (let i = 0; i < oldLogs.length; i++) {
      const oldItem = oldLogs[i];
      const newItem = newLogs[i];

      if (
        oldItem.id !== newItem.id ||
        oldItem.createdAt !== newItem.createdAt ||
        oldItem.username !== newItem.username ||
        oldItem.department !== newItem.department ||
        oldItem.action !== newItem.action ||
        oldItem.targetName !== newItem.targetName
      ) {
        return false;
      }
    }

    return true;
  };

  const getErrorMessage = (error) => {
    const status = error?.response?.status;
    const apiMessage =
      error?.response?.data?.message ||
      error?.response?.data?.title ||
      error?.message;

    if (status === 401) {
      return "Your session has expired or you are not authenticated.";
    }

    if (status === 403) {
      return "You do not have permission to view audit logs.";
    }

    if (status === 404) {
      return "Audit log endpoint was not found.";
    }

    if (status >= 500) {
      return "The server encountered an error while loading audit logs.";
    }

    return apiMessage || "An unexpected error occurred while fetching audit logs.";
  };

  const fetchLogs = async () => {
    if (isLoading.value) return;

    try {
      isLoading.value = true;
      errorMessage.value = "";

      const response = await auditLogService.getAll();
      const payload = Array.isArray(response) ? response : response?.data;
      const newData = normalizeLogs(payload);

      if (!isSameLogs(logs.value, newData)) {
        logs.value = newData;
      }
    } catch (error) {
      console.error("Error fetching audit logs:", error);
      errorMessage.value = getErrorMessage(error);
      logs.value = [];
    } finally {
      isLoading.value = false;
    }
  };

  const formatDate = (date) => {
    if (!date) return "Unknown";

    const d = new Date(date);
    if (Number.isNaN(d.getTime())) return "Unknown";

    return d.toLocaleString("vi-VN");
  };

  const formatRelativeTime = (date) => {
    if (!date) return "Unknown";

    const d = new Date(date);
    if (Number.isNaN(d.getTime())) return "Unknown";

    const diffMs = now.value - d.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);

    if (diffSeconds < 5) return "Just now";
    if (diffSeconds < 60) return `${diffSeconds} seconds ago`;

    const diffMinutes = Math.floor(diffSeconds / 60);
    if (diffMinutes < 60) return `${diffMinutes} minutes ago`;

    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 30) return `${diffDays} days ago`;

    const diffMonths = Math.floor(diffDays / 30);
    if (diffMonths < 12) return `${diffMonths} months ago`;

    const diffYears = Math.floor(diffMonths / 12);
    return `${diffYears} years ago`;
  };

  const getUsername = (log) => log?.username?.trim() || "Unknown";
  const getDepartment = (log) => log?.department?.trim() || "Unknown";
  const getTargetName = (log) => log?.targetName?.trim() || "Unknown";

  const prettifyAction = (value) => {
    if (!value || typeof value !== "string") return "unknown";

    const raw = value.trim();

    const normalized = raw
      .replace(/([a-z])([A-Z])/g, "$1 $2")
      .replace(/[_-]+/g, " ")
      .replace(/\s+/g, " ")
      .trim()
      .toLowerCase();

    if (normalized.startsWith("get ")) return normalized.replace(/^get\s+/, "view ");
    if (normalized.startsWith("post ")) return normalized.replace(/^post\s+/, "create ");
    if (normalized.startsWith("put ")) return normalized.replace(/^put\s+/, "update ");
    if (normalized.startsWith("patch ")) return normalized.replace(/^patch\s+/, "update ");
    if (normalized.startsWith("delete ")) return normalized;

    return normalized;
  };

  const getAction = (log) => prettifyAction(log?.action);

  const getActionClass = (action) => {
    const normalized = prettifyAction(action);

    if (normalized.startsWith("create")) return "action-create";
    if (normalized.startsWith("update")) return "action-update";
    if (normalized.startsWith("delete")) return "action-delete";
    if (normalized.startsWith("approve")) return "action-approve";
    if (normalized.startsWith("reject")) return "action-reject";
    if (normalized.startsWith("view")) return "action-view";

    return "";
  };

  onMounted(() => {
    fetchLogs();

    clockIntervalId = setInterval(() => {
      now.value = Date.now();
    }, 1000);
  });

  onBeforeUnmount(() => {
    if (clockIntervalId) clearInterval(clockIntervalId);
  });
</script>

<style scoped>
  .audit-logs-page {
    padding: 24px;
    background: #f8fafc;
  }

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 16px;
  }

    .page-header h2 {
      margin: 0;
      font-size: 26px;
      font-weight: 700;
    }

  .page-subtitle {
    margin-top: 6px;
    color: #64748b;
    font-size: 14px;
  }

  .refresh-btn,
  .retry-btn {
    background: #162233;
    color: #fff;
    padding: 8px 14px;
    border-radius: 6px;
    border: none;
    cursor: pointer;
  }

    .refresh-btn:disabled,
    .retry-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

  .error-banner {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;
    margin-bottom: 16px;
    padding: 14px 16px;
    border: 1px solid #fecaca;
    background: #fef2f2;
    border-radius: 10px;
    color: #991b1b;
  }

    .error-banner p {
      margin: 4px 0 0;
    }

  .table-wrapper {
    background: #fff;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 1px 2px rgba(15, 23, 42, 0.05);
  }

  .audit-table {
    width: 100%;
    border-collapse: collapse;
  }

    .audit-table th,
    .audit-table td {
      padding: 12px;
      border-bottom: 1px solid #e2e8f0;
      text-align: left;
      vertical-align: middle;
    }

    .audit-table th {
      background: #f1f5f9;
      font-weight: 600;
    }

    .audit-table tbody tr:hover {
      background: #f8fafc;
    }

  .time-cell {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .time-main {
    font-weight: 500;
    color: #0f172a;
  }

  .time-relative {
    font-size: 12px;
    color: #64748b;
  }

  .action-badge {
    display: inline-block;
    padding: 4px 10px;
    border-radius: 999px;
    background: #e2e8f0;
    color: #0f172a;
    font-size: 13px;
    line-height: 1.4;
  }

  .action-create {
    background: #dcfce7;
    color: #166534;
  }

  .action-update {
    background: #dbeafe;
    color: #1d4ed8;
  }

  .action-delete,
  .action-reject {
    background: #fee2e2;
    color: #b91c1c;
  }

  .action-approve {
    background: #ecfccb;
    color: #3f6212;
  }

  .action-view {
    background: #f1f5f9;
    color: #475569;
  }

  .empty-state {
    padding: 20px;
    text-align: center;
    color: #64748b;
  }
</style>

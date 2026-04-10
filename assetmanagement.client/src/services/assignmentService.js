import api from "@/services/axiosConfig"

function removeEmptyParams(params = {}) {
  return Object.fromEntries(
    Object.entries(params).filter(([, value]) => {
      if (value === undefined || value === null) return false
      if (typeof value === "string" && value.trim() === "") return false
      return true
    })
  )
}

function normalizeCurrentAssignment(item = {}) {
  return {
    id: item.id ?? item.Id,
    assetTag: item.assetTag ?? item.AssetTag ?? "",
    assetName: item.assetName ?? item.AssetName ?? "",
    category: item.category ?? item.Category ?? "",
    employeeName: item.employeeName ?? item.EmployeeName ?? "",
    department: item.department ?? item.Department ?? "",
    assignedAt: item.assignedAt ?? item.AssignedAt ?? null,
    status: item.status ?? item.Status ?? ""
  }
}

function normalizeHistoryItem(item = {}) {
  return {
    id: item.id ?? item.Id,
    changedAt: item.changedAt ?? item.ChangedAt ?? item.timestamp ?? item.Timestamp ?? null,
    assetTag: item.assetTag ?? item.AssetTag ?? item.asset ?? item.Asset ?? "",
    assetName: item.assetName ?? item.AssetName ?? "",
    assetType: item.assetType ?? item.AssetType ?? item.category ?? item.Category ?? "",
    employee: item.employee ?? item.Employee ?? item.employeeName ?? item.EmployeeName ?? "",
    department: item.department ?? item.Department ?? "",
    action: item.action ?? item.Action ?? "",
    previousStatus: item.previousStatus ?? item.PreviousStatus ?? "",
    newStatus: item.newStatus ?? item.NewStatus ?? "",
    changedBy: item.changedBy ?? item.ChangedBy ?? "",
    note: item.note ?? item.Note ?? "",
    raw: item
  }
}

export async function getCurrentAssignments(params = {}) {
  const response = await api.get("/assignments/current", {
    params: removeEmptyParams(params)
  })

  const data = Array.isArray(response.data) ? response.data : []
  return data.map(normalizeCurrentAssignment)
}

export async function getAssignmentHistory(params = {}) {
  const response = await api.get("/assignments/history", {
    params: removeEmptyParams(params)
  })

  const data = Array.isArray(response.data) ? response.data : []
  return data.map(normalizeHistoryItem)
}

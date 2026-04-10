import axios from "axios"

function getAccessToken() {
  // prefer new key 'access_token' but fall back to legacy 'token'
  const token = localStorage.getItem("access_token") || localStorage.getItem("token")
  if (token) return token

  try {
    const user = JSON.parse(localStorage.getItem("user"))
    return user?.token || user?.accessToken || null
  } catch {
    return null
  }
}

const axiosClient = axios.create()

axiosClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

function unwrapResponse(res) {
  const payload = res?.data
  if (payload && typeof payload === "object" && "data" in payload) {
    return payload.data
  }
  return payload
}

function normalizeAssignment(item) {
  return {
    id: item.id || item.assignmentId,
    assignmentId: item.id || item.assignmentId,
    assetId: item.assetId,
    assetTag: item.assetTag || item.tag || "",
    assetName: item.assetName || item.name || "",
    assetCategory: item.assetCategory || item.category || "",
    assignedAt: item.assignedAt,
    returnedAt: item.returnedAt || null,
    status: item.status || "ACTIVE",
    currentAssignmentId: item.id || item.assignmentId,
  }
}

function normalizeReturnRequest(item) {
  return {
    id: item.id,
    assignmentId: item.assignmentId,
    assetId: item.assetId,
    assetTag: item.assetTag || item.asset?.assetTag || "",
    assetName: item.assetName || item.asset?.name || "",
    requestedByUserId: item.requestedById || item.requestedByUserId,
    requestedByName:
      item.requestedByName ||
      item.requestedBy?.fullName ||
      item.requestedBy?.name ||
      "",
    targetUserId:
      item.targetUserId ||
      item.employeeId ||
      item.assignment?.userId ||
      item.assignment?.user?.id,
    targetUserName:
      item.targetUserName ||
      item.employeeName ||
      item.assignment?.user?.fullName ||
      item.assignment?.user?.name ||
      "",
    reason: item.reason || "",
    notes: item.notes || "",
    status: item.status || "REQUESTED",
    initiatedAt:
      item.initiatedAt ||
      item.createdAt ||
      item.creationTime ||
      item.createdDate,
    createdAt:
      item.createdAt ||
      item.initiatedAt ||
      item.creationTime ||
      item.createdDate,
    receivedAt: item.receivedAt || null,
    inspectedAt: item.inspectedAt || null,
    handbackCondition: item.handbackCondition || null,
    handbackNotes: item.handbackNotes || "",
    inspectionNotes: item.inspectionNotes || "",
    accessoriesNotes: item.accessoriesNotes || "",
    inspectionResult: item.inspectionResult || "",
  }
}

export async function getEmployeeAssignedAssets() {
  const res = await axiosClient.get("/api/assignments/my")
  const data = unwrapResponse(res)

  if (!Array.isArray(data)) return []

  return data
    .map(normalizeAssignment)
    .filter((item) =>
      ["ASSIGNED", "ACTIVE"].includes((item.status || "").toUpperCase())
    )
}

export async function getReturnsForUser() {
  const res = await axiosClient.get("/api/returns/my")
  const data = unwrapResponse(res)

  if (!Array.isArray(data)) return []

  return data.map(normalizeReturnRequest)
}

export async function initiateReturn(payload) {
  const body = {
    assignmentId: payload.assignmentId,
    assetId: payload.assetId,
    reason: payload.reason,
    notes: payload.notes || "",
  }

  const res = await axiosClient.post("/api/returns", body)
  return normalizeReturnRequest(unwrapResponse(res))
}

export async function getAllReturns() {
  const res = await axiosClient.get("/api/returns")
  const data = unwrapResponse(res)

  if (!Array.isArray(data)) return []

  return data.map(normalizeReturnRequest)
}

export async function getPendingReturns() {
  const items = await getAllReturns()
  return items.filter((item) => item.status === "REQUESTED")
}

export async function getPendingInspectionReturns() {
  const items = await getAllReturns()
  return items.filter((item) => item.status === "PENDING_INSPECTION")
}

export async function getProcessedReturns() {
  const items = await getAllReturns()
  return items.filter((item) => item.status === "INSPECTED")
}

export async function processHandback(payload) {
  const body = {
    receivedAt: payload.returnDate || new Date().toISOString(),
    handbackCondition: payload.handbackCondition || null,
    notes: payload.handbackNotes || "",
    physicallyReceived: payload.physicallyReceived,
  }

  const res = await axiosClient.post(
    `/api/returns/${payload.returnRequestId}/receive`,
    body
  )

  return normalizeReturnRequest(unwrapResponse(res))
}

export async function saveInspectionDraft(payload) {
  return {
    returnRequestId: payload.returnRequestId,
    inspectionNotes: payload.inspectionNotes || "",
    accessoriesNotes: payload.accessoriesNotes || "",
  }
}

export async function completeInspection(payload) {
  const body = {
    inspectionNotes: payload.inspectionNotes || "",
    accessoriesNotes: payload.accessoriesNotes || "",
    inspectionResult: payload.inspectionResult,
  }

  const res = await axiosClient.post(
    `/api/returns/${payload.returnRequestId}/inspect`,
    body
  )

  return normalizeReturnRequest(unwrapResponse(res))
}

export default {
  getEmployeeAssignedAssets,
  getReturnsForUser,
  initiateReturn,
  getAllReturns,
  getPendingReturns,
  getPendingInspectionReturns,
  getProcessedReturns,
  processHandback,
  saveInspectionDraft,
  completeInspection,
}

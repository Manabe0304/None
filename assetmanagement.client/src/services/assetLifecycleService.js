import api from "@/services/axiosConfig"

export async function markBeyondRepair(assetId, reason) {
  const res = await api.post(`/assets/${assetId}/mark-beyond-repair`, {
    reason,
  })
  return res.data
}

export async function liquidateAsset(assetId, payload) {
  const res = await api.post(`/assets/${assetId}/liquidate`, {
    reason: payload.reason,
    liquidationDate: payload.liquidationDate,
    disposalMethod: payload.disposalMethod,
    notes: payload.notes || "",
  })
  return res.data
}

export const ASSET_STATUS = Object.freeze({
  AVAILABLE: "AVAILABLE",
  IN_USE: "IN_USE",
  IN_USE_SHARED: "IN_USE_SHARED",
  REPORTED_BROKEN: "REPORTED_BROKEN",
  BROKEN: "BROKEN",
  UNDER_MAINTENANCE: "UNDER_MAINTENANCE",
  LOST: "LOST",
  BEYOND_REPAIR: "BEYOND_REPAIR",
  LIQUIDATED: "LIQUIDATED",
})

export const NON_TERMINAL_FORM_STATUSES = [
  ASSET_STATUS.AVAILABLE,
  ASSET_STATUS.IN_USE,
  ASSET_STATUS.IN_USE_SHARED,
  ASSET_STATUS.BROKEN,
  ASSET_STATUS.UNDER_MAINTENANCE,
  ASSET_STATUS.LOST,
]

export function formatAssetStatus(status) {
  return String(status || ASSET_STATUS.AVAILABLE).replaceAll("_", " ")
}

export function assetStatusClass(status) {
  const normalized = String(status || ASSET_STATUS.AVAILABLE).toUpperCase()

  if (normalized === ASSET_STATUS.AVAILABLE) return "status-available"
  if (normalized === ASSET_STATUS.IN_USE) return "status-in-use"
  if (normalized === ASSET_STATUS.IN_USE_SHARED) return "status-shared"
  if (normalized === ASSET_STATUS.REPORTED_BROKEN) return "status-reported"
  if (normalized === ASSET_STATUS.BROKEN) return "status-broken"
  if (normalized === ASSET_STATUS.UNDER_MAINTENANCE) return "status-maintenance"
  if (normalized === ASSET_STATUS.LOST) return "status-lost"
  if (normalized === ASSET_STATUS.BEYOND_REPAIR) return "status-beyond-repair"
  if (normalized === ASSET_STATUS.LIQUIDATED) return "status-liquidated"

  return "status-other"
}

export function isTerminalAssetStatus(status) {
  const normalized = String(status || "").toUpperCase()
  return [ASSET_STATUS.BEYOND_REPAIR, ASSET_STATUS.LIQUIDATED].includes(normalized)
}

export function canMarkBeyondRepair(asset) {
  return String(asset?.status || "").toUpperCase() === ASSET_STATUS.UNDER_MAINTENANCE
}

export function canLiquidate(asset) {
  return String(asset?.status || "").toUpperCase() === ASSET_STATUS.BEYOND_REPAIR
}

export function inferAssignmentType(asset = {}) {
  if (String(asset?.status || "").toUpperCase() === ASSET_STATUS.IN_USE_SHARED) {
    return "DEPARTMENT"
  }

  if (asset?.currentUserId || asset?.currentUserName) {
    return "PERSONAL"
  }

  return "UNASSIGNED"
}

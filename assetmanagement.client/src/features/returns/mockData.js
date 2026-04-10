import {
  ASSIGNMENT_STATUS,
  ASSET_FINAL_STATUS
} from "./types"

export const mockUsers = [
  {
    id: "admin-1",
    fullName: "System Admin",
    role: "ADMIN",
    email: "admin@company.com",
  },
  {
    id: "emp-1",
    fullName: "Nguyen Quoc Bob",
    role: "EMPLOYEE",
    email: "bob@company.com",
  },
]

export const mockAssets = [
  {
    id: "asset-1",
    assetTag: "LT-001",
    name: "Dell Latitude 5420",
    status: "IN_USE",
    assignedToUserId: "emp-1",
    currentAssignmentId: "assign-1",
  },
  {
    id: "asset-2",
    assetTag: "MON-002",
    name: "Dell Monitor 24 inch",
    status: "IN_USE",
    assignedToUserId: "emp-2",
    currentAssignmentId: "assign-2",
  },
  {
    id: "asset-3",
    assetTag: "KB-003",
    name: "Mechanical Keyboard",
    status: ASSET_FINAL_STATUS.AVAILABLE,
    assignedToUserId: null,
    currentAssignmentId: null,
  },
]

export const mockAssignments = [
  {
    id: "assign-1",
    assetId: "asset-1",
    userId: "emp-1",
    status: ASSIGNMENT_STATUS.ACTIVE,
    assignedAt: "2026-03-01T08:00:00.000Z",
    closedAt: null,
  },
  {
    id: "assign-2",
    assetId: "asset-2",
    userId: "emp-2",
    status: ASSIGNMENT_STATUS.ACTIVE,
    assignedAt: "2026-03-02T09:00:00.000Z",
    closedAt: null,
  },
]

export const mockReturnRequests = []


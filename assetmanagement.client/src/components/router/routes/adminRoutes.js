import AdminDashboard from "@/views/admin/AdminDashboard.vue"
import AssetAssignmentPage from "@/views/admin/pages/AssetAssignmentPage.vue"
import AssetHistoryPage from "@/views/admin/pages/AssetHistoryPage.vue"
import CurrentAssignmentsPage from "@/views/admin/pages/CurrentAssignmentsPage.vue"
import DepartmentPage from "@/views/admin/pages/DepartmentPage.vue"
import ManageUsersPage from "@/views/admin/pages/ManageUsersPage.vue"
import SettingsPage from "@/views/admin/pages/SettingsPage.vue"
import ReportBroken from "@/views/admin/pages/AdminReportBrokenPage.vue"

import AdminReturnsPage from "@/features/returns/pages/AdminReturnsPage.vue"

import ManageAsset from "@/components/AssetManage/AssetListManage.vue"
import UserManage from "@/views/admin/UserManage.vue"
import UserCreate from "@/views/admin/UserCreate.vue"
import UserEdit from "@/views/admin/UserEdit.vue"
import UserDetail from "@/views/admin/UserDetail.vue"

import AuditLogs from "@/views/admin/pages/AuditLogs.vue"

const adminRoutes = [
  {
    path: "/admin",
    meta: { role: "ADMIN", layout: "default" },
    children: [
      {
        path: "dashboard",
        name: "AdminDashboard",
        component: AdminDashboard
      },
      {
        path: "users",
        component: ManageUsersPage,
        children: [
          { path: "", name: "AdminManageUsers", component: UserManage },
          { path: "create", name: "AdminUserCreate", component: UserCreate },
          { path: ":id", name: "AdminUserDetail", component: UserDetail, props: true },
          { path: ":id/edit", name: "AdminUserEdit", component: UserEdit, props: true }
        ],
      },
      {
        path: "assets",
        name: "AdminManageAssets",
        component: ManageAsset
      },
      {
        path: "assignments",
        name: "AdminAssetAssignment",
        component: AssetAssignmentPage
      },
      {
        path: "current-assignments",
        name: "AdminCurrentAssignments",
        component: CurrentAssignmentsPage
      },
      {
        path: "history",
        name: "AdminAssetHistory",
        component: AssetHistoryPage
      },
      {
        path: "departments",
        name: "AdminDepartments",
        component: DepartmentPage
      },
      {
        path: "report-broken",
        name: "AdminManageReport",
        component: ReportBroken,
      },
      {
        path: "dashboard/returns",
        name: "AdminReturns",
        component: AdminReturnsPage
      },

      {
        path: "audit-logs",
        name: "AdminAuditLogs",
        component: AuditLogs
      },

      {
        path: "settings",
        name: "AdminSettings",
        component: SettingsPage
      }
    ]
  }
]

export default adminRoutes

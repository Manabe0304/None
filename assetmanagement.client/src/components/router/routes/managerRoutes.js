import ManagerApprovalsPage from "@/views/manager/pages/ManagerApprovalsPage.vue"
import ManagerMaintainceReport from "@/views/manager/pages/MaintanceRecordPage.vue"
import ManagerManageAsset from "@/components/AssetManage/AssetListManage.vue"
import ManagerVendor from "@/views/manager/pages/ManagerVendorPage.vue"
const managerRoutes = [
  {
    path: "/manager",
    meta: { role: "MANAGER", layout: "default" },
    children: [
      {
        path: "approvals",
        name: "manager-approvals",
        component: ManagerApprovalsPage
      },
      {
        path: "vendor",
        name: "manager-vendor",
        component: ManagerVendor
      },
      {
        path: "maintance-record",
        name: "manager-maintancerecord",
        component: ManagerMaintainceReport
      },
      {
        path: "assets",
        name: "manager-assets",
        component: ManagerManageAsset
      }
    ]
  }
]

export default managerRoutes

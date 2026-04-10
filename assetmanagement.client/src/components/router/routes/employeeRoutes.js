import MyAssets from "@/views/employee/pages/MyAssets.vue"
import SendRequest from "@/views/employee/pages/SendRequest.vue"
import RequestHistory from "@/views/employee/pages/RequestHistory.vue"
import ReturnedAssetsHistory from "@/views/employee/pages/ReturnedAssetsHistory.vue"


const employeeRoutes = [
  {
    path: "/employee",
    meta: { role: "EMPLOYEE", layout: "default" },
    children: [
      {
        path: "assets",
        name: "employee-assets",
        component: MyAssets,
      },
      {
        path: "request",
        name: "employee-request",
        component: SendRequest,
      },
      {
        path: "history",
        name: "employee-history",
        component: RequestHistory
      },
      {
        path: "returned-history",
        name: "employee-returned-history",
        component: ReturnedAssetsHistory
      }
    ]
  }
]

export default employeeRoutes

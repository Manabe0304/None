const sidebarMenu = {
  admin: [
    {
      text: "Dashboard",
      to: "/admin/dashboard",
      icon: "fa-solid fa-chart-line",
    },
    {
      text: "Manage Users",
      to: "/admin/users",
      icon: "fa-solid fa-users",
    },
    {
      text: "Manage Assets",
      to: "/admin/assets",
      icon: "fa-solid fa-computer",
    },
    {
      text: "Assignments",
      to: "/admin/assignments",
      icon: "fa-solid fa-share-from-square",
    },
    {
      text: "Current Assignments",
      to: "/admin/current-assignments",
      icon: "fa-solid fa-list-check",
    },
    {
      text: "Asset History",
      to: "/admin/history",
      icon: "fa-solid fa-clock-rotate-left",
    },
    {
      text: "Departments",
      to: "/admin/departments",
      icon: "fa-solid fa-building",
    },
    {
      text: "Report Broken",
      to: "/admin/report-broken",
      icon: "fa-solid fa-triangle-exclamation",
    },
    {
      text: "Returns",
      to: "/admin/dashboard/returns",
      icon: "fa-solid fa-rotate-left",
    },

    {
      text: "Audit Logs",
      to: "/admin/audit-logs",
      icon: "fa-solid fa-file-lines",
    },

    {
      text: "Settings",
      to: "/admin/settings",
      icon: "fa-solid fa-gear",
    }
  ],

  manager: [
    {
      text: "Approvals",
      to: "/manager/approvals",
      icon: "fa-solid fa-circle-check",
    },
    {
      text: "Vendor",
      to: "/manager/vendor",
      icon: "fa-solid fa-hammer",
    },
    {
      text: "Maintance Record",
      to: "/manager/maintance-record",
      icon: "fa-solid fa-circle-check",
    },
    {
      text: "Assets",
      to: "/manager/assets",
      icon: "fa-solid fa-computer",
    },
  ],

  employee: [
    {
      text: "My Asset",
      to: "/employee/assets",
      icon: "fa-solid fa-paper-plane",
    },
    {
      text: "Submit Request",
      to: "/employee/request",
      icon: "fa-solid fa-paper-plane",
    },
    {
      text: "Request History",
      to: "/employee/history",
      icon: "fa-solid fa-clock-rotate-left",
    },
    {
      text: "Return Assets",
      to: "/employee/returned-history",
      icon: "fa-solid fa-rotate-left",
    },
  ],
}

export default sidebarMenu

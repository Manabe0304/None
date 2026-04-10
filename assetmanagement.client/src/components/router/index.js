/* eslint-disable no-useless-assignment */
import { createRouter, createWebHistory } from "vue-router";

import AssetList from "../AssetManage/AssetListManage.vue";
import AssetCreate from "../AssetManage/AssetCreate.vue";
import AssetEdit from "../AssetManage/AssetEdit.vue";
import AssetDeleted from "../AssetManage/AssetDeleted.vue";
import Login from "../AppLogin.vue";

import adminRoutes from "./routes/adminRoutes";
import managerRoutes from "./routes/managerRoutes";
import employeeRoutes from "./routes/employeeRoutes";

import ForgotPassword from "@/views/auth/ForgotPassword.vue"
import CreateNewPassword from "@/views/auth/CreateNewPassword.vue"

const routes = [
  { path: "/", redirect: "/login" },
  { path: "/login", component: Login },
  { path: "/forgot-password", component: ForgotPassword },
  { path: "/reset-password", component: CreateNewPassword },
  ...adminRoutes,
  ...managerRoutes,
  ...employeeRoutes,
  { path: "/assets", component: AssetList },
  { path: "/assets/create", component: AssetCreate },
  { path: "/assets/edit/:id", component: AssetEdit, props: true },
  { path: "/assetsdeleted", component: AssetDeleted },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to) => {
  const token = localStorage.getItem("token");
  let user = null;

  try {
    const userRaw = localStorage.getItem("user");
    user = userRaw ? JSON.parse(userRaw) : null;
  } catch {
    user = null;
  }

  const requiredRole = to.matched.find((record) => record.meta.role)?.meta.role;
  const isLoginPage = to.path === "/login";

  if (requiredRole && !token) {
    if (import.meta.env.DEV) {
      const mockUser = {
        id: "dev-1",
        role: requiredRole,
        fullName: `Dev ${requiredRole}`,
        departmentName: "IT Department",
      };

      localStorage.setItem("token", "dev-token");
      localStorage.setItem("user", JSON.stringify(mockUser));
      return true;
    }

    return "/login";
  }

  if (isLoginPage && token) {
    const roleRedirect = {
      ADMIN: "/admin/dashboard",
      MANAGER: "/manager/approvals",
      EMPLOYEE: "/employee/assets",
    };

    const redirectPath = user ? roleRedirect[user.role] : null;

    if (!redirectPath) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      return "/login";
    }

    return redirectPath;
  }

  if (requiredRole && user?.role !== requiredRole) {
    return "/login";
  }

  return true;
});

export default router;

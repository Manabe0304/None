<template>
  <aside class="sidebar" :class="`sidebar-${role}`">
    <h2>{{ panelTitle }}</h2>

    <nav class="sidebar-menu">
      <RouterLink v-for="item in items"
                  :key="item.text"
                  :to="item.to"
                  class="sidebar-item"
                  :class="{ active: isActive(item) }">
        <i :class="item.icon"></i>
        <span>{{ item.text }}</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<script setup>
  import { computed } from "vue"
  import { useRoute } from "vue-router"
  import sidebarMenu from "../../config/sidebarMenu"

  const route = useRoute()

  const role = computed(() => {
    const path = route.path

    if (path.startsWith("/admin")) return "admin"
    if (path.startsWith("/manager")) return "manager"
    if (path.startsWith("/employee")) return "employee"

    return "manager"
  })

  const panelTitle = computed(() => {
    return `${role.value.charAt(0).toUpperCase()}${role.value.slice(1)} Panel`
  })

  const items = computed(() => sidebarMenu[role.value] || [])

  function isActive(item) {
    if (route.path === item.to) return true
    if (route.path.startsWith(`${item.to}/`)) return true

    if (item.to === "/admin/dashboard/users") {
      return route.path.startsWith("/admin/dashboard/users")
    }

    if (item.to === "/admin/dashboard/assets") {
      return route.path.startsWith("/admin/dashboard/assets")
    }

    if (item.to === "/admin/dashboard/assignments") {
      return route.path.startsWith("/admin/dashboard/assignments")
    }

    return false
  }
</script>

<style scoped>
  .sidebar {
    width: var(--app-sidebar-width, 260px);
    min-width: var(--app-sidebar-width, 260px);
    height: 100%;
    overflow-y: auto;
    padding: 20px 16px 24px;
    box-sizing: border-box;
    color: white;
    border-right: 1px solid rgba(255, 255, 255, 0.12);
    box-shadow: 10px 0 30px rgba(15, 23, 42, 0.08);
  }

  .sidebar-admin {
    background: #162233;
  }

  .sidebar-manager {
    background: #092850;
  }

  .sidebar-employee {
    background: #064336;
  }

  .sidebar h2 {
    margin: 0 0 18px;
    font-size: 20px;
  }

  .sidebar-menu {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .sidebar-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 11px 12px;
    border: 1px solid rgba(255, 255, 255, 0.16);
    border-radius: 10px;
    color: white;
    text-decoration: none;
    transition: transform 0.2s ease, background 0.2s ease, border-color 0.2s ease;
  }

    .sidebar-item:hover {
      background: rgba(255, 255, 255, 0.12);
      border-color: rgba(255, 255, 255, 0.28);
      transform: translateX(4px);
    }

    .sidebar-item.active {
      background: rgba(255, 255, 255, 0.18);
      border-color: rgba(255, 255, 255, 0.35);
      transform: translateX(4px);
    }

  .sidebar::-webkit-scrollbar {
    width: 8px;
  }

  .sidebar::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.25);
    border-radius: 999px;
  }

  @media (max-width: 768px) {
    .sidebar {
      padding: 16px 12px 20px;
    }

      .sidebar h2 {
        font-size: 18px;
      }

    .sidebar-item {
      padding: 10px;
    }
  }
</style>

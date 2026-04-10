<template>
  <div v-if="isLayoutRequired" :class="['app-shell', themeClass]">
    <AppHeader />

    <div class="app-body">
      <AppSidebar />

      <main class="app-content">
        <div class="app-content-scroll">
          <section class="app-page">
            <router-view v-slot="{ Component, route }">
              <transition name="page-fade-slide" mode="out-in">
                <component :is="Component" :key="route.fullPath" />
              </transition>
            </router-view>
          </section>

          <AppFooter />
        </div>
      </main>
    </div>

    <AppNotificationHost />
  </div>

  <div v-else class="empty-layout">
    <router-view v-slot="{ Component, route }">
      <transition name="page-fade-slide" mode="out-in">
        <component :is="Component" :key="route.fullPath" />
      </transition>
    </router-view>

    <AppNotificationHost />
  </div>
</template>

<script setup>
  import { computed, onMounted } from "vue"
  import { useRoute } from "vue-router"
  import AppHeader from "./components/layout/AppHeader.vue"
  import AppSidebar from "./components/layout/AppSidebar.vue"
  import AppFooter from "./components/layout/AppFooter.vue"
  import AppNotificationHost from "./components/common/AppNotificationHost.vue"
  import { patchWindowAlert } from "./stores/notificationStore"

  const route = useRoute()

  const isLayoutRequired = computed(() => route.meta.layout === "default")

  const themeClass = computed(() => {
    if (route.path.startsWith("/admin")) return "theme-admin"
    if (route.path.startsWith("/manager")) return "theme-manager"
    if (route.path.startsWith("/employee")) return "theme-employee"
    return "theme-manager"
  })

  onMounted(() => {
    patchWindowAlert()
  })
</script>

<style>
  body {
    margin: 0;
  }

  .app-shell {
    --app-header-height: 72px;
    --app-sidebar-width: 260px;
    min-height: 100vh;
    height: 100vh;
    display: flex;
    flex-direction: column;
    background: #f0f2f5;
    overflow: hidden;
  }

    .app-shell.theme-admin .header,
    .app-shell.theme-admin .footer {
      background: #1f2d3d;
    }

    .app-shell.theme-manager .header,
    .app-shell.theme-manager .footer {
      background: #0c3463;
    }

    .app-shell.theme-employee .header,
    .app-shell.theme-employee .footer {
      background: #085747;
    }

  .app-body {
    flex: 1;
    min-height: 0;
    display: flex;
    overflow: hidden;
  }

  .app-content {
    flex: 1;
    min-width: 0;
    min-height: 0;
    overflow: hidden;
    background: #f0f2f5;
  }

  .app-content-scroll {
    height: 100%;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
  }

  .app-page {
    flex: 1 0 auto;
    padding: 20px;
    box-sizing: border-box;
  }

  .empty-layout {
    min-height: 100vh;
  }

  .page-fade-slide-enter-active,
  .page-fade-slide-leave-active {
    transition: opacity 0.22s ease, transform 0.22s ease;
  }

  .page-fade-slide-enter-from {
    opacity: 0;
    transform: translateY(10px);
  }

  .page-fade-slide-leave-to {
    opacity: 0;
    transform: translateY(-8px);
  }

  .app-content-scroll::-webkit-scrollbar {
    width: 10px;
  }

  .app-content-scroll::-webkit-scrollbar-thumb {
    background: rgba(100, 116, 139, 0.35);
    border-radius: 999px;
  }

  @media (max-width: 1024px) {
    .app-shell {
      --app-sidebar-width: 220px;
    }
  }

  @media (max-width: 768px) {
    .app-shell {
      --app-header-height: 64px;
      --app-sidebar-width: 200px;
    }

    .app-page {
      padding: 16px;
    }
  }
</style>

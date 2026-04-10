<template>
  <section class="manager-dashboard-content">
    <div v-if="$route.path === '/manager/dashboard'" class="welcome-card">
      <h2>Manager Dashboard</h2>
      <p>Select a feature from the sidebar to continue.</p>
    </div>

    <router-view v-else />
  </section>
</template>

<script setup>
</script>

<style scoped>
  .manager-dashboard-content {
    width: 100%;
  }

  .welcome-card {
    background: #ffffff;
    border-radius: 14px;
    padding: 24px;
    box-shadow: 0 4px 16px rgba(15, 23, 42, 0.08);
  }

    .welcome-card h2 {
      margin: 0 0 12px;
      color: #0f2f5f;
      font-size: 28px;
      font-weight: 700;
    }

    .welcome-card p {
      margin: 0;
      color: #64748b;
      font-size: 15px;
    }
</style>

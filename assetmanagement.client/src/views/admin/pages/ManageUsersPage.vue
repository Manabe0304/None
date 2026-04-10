<template>
  <div class="manage-users-page">
    <div class="page-header">
      <div>
        <h1>Manage Users</h1>
        <p>Manage user accounts, permissions, and department assignments.</p>
      </div>
    </div>

    <router-view />
  </div>
</template>

<script setup>
</script>

<style scoped>
  .manage-users-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .page-header h1 {
    margin: 0 0 6px;
    font-size: 28px;
    font-weight: 700;
    color: #1b2a41;
  }

  .page-header p {
    margin: 0;
    color: #6b7280;
  }
</style>

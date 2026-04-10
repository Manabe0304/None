<template>
  <div class="simple-page">
    <h1>Manage Assets</h1>
    <p>Asset inventory, status tracking, and lifecycle administration.</p>

    <div class="card">
      <p>
        Tại đây bạn có thể tích hợp danh sách tài sản, bộ lọc theo trạng thái, loại thiết bị, và thao tác CRUD tài sản.
      </p>
    </div>
  </div>
</template>

<script setup>
</script>

<style scoped>
  .simple-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

    .simple-page h1 {
      margin: 0;
      font-size: 28px;
      color: #1b2a41;
    }

    .simple-page p {
      margin: 0;
      color: #6b7280;
    }

  .card {
    background: #fff;
    border-radius: 14px;
    padding: 20px;
    border: 1px solid #e5e7eb;
    box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
    color: #374151;
  }
</style>

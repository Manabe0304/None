<script setup>
import { useAuthStore } from '../../stores/authStore'

const authStore = useAuthStore()
</script>

<template>
  <div class="dashboard">
    <h2>Manager Dashboard</h2>
    <p>Xin chào, <strong>{{ authStore.userFullName }}</strong>!</p>

    <div class="stats-grid">
      <div class="stat-card">
        <h3>Phê duyệt yêu cầu</h3>
        <p>Xem xét và phê duyệt các yêu cầu tài sản từ nhân viên.</p>
      </div>
      <div class="stat-card">
        <h3>Quản lý tài sản</h3>
        <p>Theo dõi tài sản thuộc phòng ban của bạn.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
  margin-top: 20px;
}

.stat-card {
  background: #fff;
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  padding: 20px;
}

.stat-card h3 {
  margin: 0 0 8px 0;
  color: #0c3463;
}

.stat-card p {
  margin: 0;
  color: #666;
  font-size: 14px;
}
</style>

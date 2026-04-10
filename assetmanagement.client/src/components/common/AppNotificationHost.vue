<template>
  <div class="toast-stack">
    <transition-group name="toast-slide" tag="div">
      <div v-for="item in notifications"
           :key="item.id"
           class="toast-item"
           :class="[`toast-${item.type}`]">
        <div class="toast-content">
          <div class="toast-title-row">
            <strong>{{ item.title }}</strong>
            <button class="toast-close" @click="removeNotification(item.id)">
              <i class="fa-solid fa-xmark"></i>
            </button>
          </div>
          <p>{{ item.message }}</p>
        </div>
      </div>
    </transition-group>
  </div>
</template>

<script setup>
import { useNotificationStore } from "@/stores/notificationStore"

const { notifications, removeNotification } = useNotificationStore()
</script>

<style scoped>
  .toast-stack {
    position: fixed;
    top: calc(var(--app-header-height, 72px) + 16px);
    right: 16px;
    z-index: 1400;
    display: flex;
    flex-direction: column;
    gap: 12px;
    width: min(360px, calc(100vw - 32px));
  }

  .toast-item {
    border-radius: 14px;
    box-shadow: 0 18px 40px rgba(15, 23, 42, 0.18);
    overflow: hidden;
    backdrop-filter: blur(10px);
  }

  .toast-content {
    padding: 14px 16px;
    background: #fff;
    border-left: 4px solid transparent;
  }

  .toast-title-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 4px;
  }

    .toast-title-row strong {
      font-size: 14px;
      font-weight: 700;
      color: #111827;
    }

  .toast-content p {
    margin: 0;
    color: #4b5563;
    line-height: 1.45;
  }

  .toast-close {
    border: none;
    background: transparent;
    color: #64748b;
    cursor: pointer;
    font-size: 14px;
  }

  .toast-success .toast-content {
    border-left-color: #16a34a;
  }

  .toast-error .toast-content {
    border-left-color: #dc2626;
  }

  .toast-warning .toast-content {
    border-left-color: #d97706;
  }

  .toast-info .toast-content {
    border-left-color: #2563eb;
  }

  .toast-slide-enter-active,
  .toast-slide-leave-active {
    transition: all 0.22s ease;
  }

  .toast-slide-enter-from,
  .toast-slide-leave-to {
    opacity: 0;
    transform: translateX(18px) translateY(-8px);
  }
</style>

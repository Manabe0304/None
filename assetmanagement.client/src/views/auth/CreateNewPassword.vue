<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Create New Password</h2>
      <p>{{ email }}</p>

      <div class="input-group">
        <input v-model="newPassword"
               :type="showPassword ? 'text' : 'password'"
               placeholder="New password" />
        <span class="toggle-password" @click="showPassword = !showPassword">
          <i :class="showPassword ? 'bi bi-eye-fill' : 'bi bi-eye-slash-fill'"></i>
        </span>
      </div>

      <div class="input-group">
        <input v-model="confirmPassword"
               :type="showConfirmPassword ? 'text' : 'password'"
               placeholder="Confirm password" />
        <span class="toggle-password" @click="showConfirmPassword = !showConfirmPassword">
          <i :class="showConfirmPassword ? 'bi bi-eye-fill' : 'bi bi-eye-slash-fill'"></i>
        </span>
      </div>

      <button :disabled="loading" @click="submit">
        {{ loading ? 'Saving...' : 'Save Password' }}
      </button>

      <p v-if="error" class="error">{{ error }}</p>
      <p v-if="success" class="success">{{ success }}</p>
    </div>
  </div>
</template>

<script setup>
import axios from 'axios'
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const email = computed(() => route.query.email || '')
const newPassword = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const loading = ref(false)
const error = ref('')
const success = ref('')

async function submit() {
  error.value = ''
  success.value = ''

  if (!email.value) {
    error.value = 'Missing email'
    return
  }

  if (!newPassword.value || !confirmPassword.value) {
    error.value = 'Please enter both password fields'
    return
  }

  try {
    loading.value = true
    await axios.post('https://localhost:7172/api/v1/auth/reset-password', {
      email: email.value,
      newPassword: newPassword.value,
      confirmPassword: confirmPassword.value,
    })
    success.value = 'Password updated successfully. Redirecting to login...'
    setTimeout(() => router.push('/login'), 1200)
  } catch (e) {
    error.value = e?.response?.data?.message || 'Failed to reset password'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
  html, body {
    margin: 0;
    padding: 0;
    height: 100%;
    width: 100%;
    overflow: hidden; /* Khóa hoàn toàn cuộn ngang và dọc */
  }

  .auth-page {
    height: 100vh;
    width: 100vw;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px; 
    background: linear-gradient(rgba(7, 38, 56, 0.45), rgba(7, 38, 56, 0.45)), url('https://media.licdn.com/dms/image/v2/D4E12AQE9hO4flh5ZaA/article-cover_image-shrink_720_1280/article-cover_image-shrink_720_1280/0/1699875224574?e=2147483647&v=beta&t=fs8Hsak9ZvWghIDRWFzzFPDyOHgPSb7l6jf3E_WWfhE');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    overflow: hidden;
    box-sizing: border-box;
  }

  .auth-card {
    width: 100%;
    max-width: 460px;
    background: rgba(255, 255, 255, 0.96);
    border-radius: 20px;
    box-shadow: 0 20px 45px rgba(0, 0, 0, 0.18);
    padding: 36px 32px;
    display: flex;
    flex-direction: column;
    gap: 14px;
    backdrop-filter: blur(4px);
  }

    .auth-card h2 {
      margin: 0;
      text-align: center;
      font-size: 2rem;
      font-weight: 800;
      color: #1e293b;
    }

    .auth-card > p {
      margin: 0 0 4px;
      text-align: center;
      color: #475569;
      font-size: 0.95rem;
      word-break: break-word;
    }

    .auth-card input {
      width: 100%;
      height: 48px;
      border: 1px solid #d7dee7;
      border-radius: 10px;
      padding: 0 14px;
      padding-right: 45px !important;
      font-size: 15px;
      color: #1f2937;
      background: #fffef0;
      outline: none;
      transition: all 0.2s ease;
      box-sizing: border-box;
    }

      .auth-card input:focus {
        border-color: #34c38f;
        box-shadow: 0 0 0 4px rgba(52, 195, 143, 0.15);
        background: #ffffff;
      }

      .auth-card input::placeholder {
        color: #94a3b8;
      }

    .auth-card button {
      width: 100%;
      height: 50px;
      border: none;
      border-radius: 10px;
      background: #39c98a;
      color: white;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s ease;
      margin-top: 4px;
    }

      .auth-card button:hover:not(:disabled) {
        background: #28b97a;
        transform: translateY(-1px);
      }

      .auth-card button:disabled {
        opacity: 0.7;
        cursor: not-allowed;
      }

  .input-group {
    position: relative;
    width: 100%;
  }

  .toggle-password {
    position: absolute;
    right: 14px;
    top: 50%;
    transform: translateY(-50%);
    cursor: pointer;
    color: #94a3b8;
    transition: color 0.2s;
    display: flex;
    align-items: center;
    z-index: 10;
  }

    .toggle-password:hover {
      color: #34c38f;
    }
  .error,
  .success {
    margin: 0;
    padding: 12px 14px;
    border-radius: 10px;
    font-size: 0.95rem;
    text-align: center;
  }

  .error {
    background: #fef2f2;
    color: #dc2626;
    border: 1px solid #fecaca;
  }

  .success {
    background: #ecfdf5;
    color: #059669;
    border: 1px solid #a7f3d0;
  }

  @media (max-width: 576px) {
    .auth-page {
      padding: 16px;
    }

    .auth-card {
      padding: 28px 20px;
      border-radius: 16px;
    }

      .auth-card h2 {
        font-size: 1.7rem;
      }
  }
</style>

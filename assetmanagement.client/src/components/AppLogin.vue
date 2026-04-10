<template>
  <div class="login-page">

    <div class="login-card">
      <h1>Login</h1>

      <form @submit.prevent="handleLogin">

        <div class="form-group">
          <label>Email</label>
          <input v-model="email"
                 type="text"
                 placeholder="Enter email"
                 required />
        </div>

        <div class="form-group">
          <label>Password</label>
          <div class="password-wrapper">
            <input v-model="password"
                   :type="showPassword ? 'text' : 'password'"
                   placeholder="Enter password"
                   required />
            <span class="toggle-password" @click="showPassword = !showPassword">
              <i :class="showPassword ? 'bi bi-eye-fill' : 'bi bi-eye-slash-fill'"></i>
            </span>
          </div>
        </div>

        <button type="submit" :disabled="loading">
          {{ loading ? "Logging in..." : "Login" }}
        </button>

      </form>

      <div class="auth-links">
        <router-link to="/forgot-password">Forgot Password?</router-link>
      </div>

      <p v-if="errorMessage" class="error">
        {{ errorMessage }}
      </p>

    </div>

  </div>
</template>

<script>
  import axios from "axios"
  import { ref } from "vue"
  import { useRouter } from "vue-router"
  import { useAuthStore } from "../stores/auth" 

  export default {

    name: "Login",

    setup() {

      const router = useRouter()

      const email = ref("")
      const password = ref("")
      const loading = ref(false)
      const errorMessage = ref("")
      const authStore = useAuthStore()

      // Thêm dòng này
      const showPassword = ref(false)

      const handleLogin = async () => {

        loading.value = true
        errorMessage.value = ""

        try {

          const res = await axios.post(
            "https://localhost:7172/api/v1/auth",
            {
              email: email.value,
              password: password.value
            },
            {
              headers: {
                "X-Api-Key": "123456"
              }
            }
          )

          const user = res.data.data

          authStore.login(user)

          if (user.role === "ADMIN") {
            router.push("/admin/dashboard")
          }
          else if (user.role === "MANAGER") {
            router.push("/manager/approvals")
          }
          else {
            router.push("/employee/assets")
          }

        }
        catch (error) {

          errorMessage.value = "Wrong email or password"

        }
        finally {

          loading.value = false

        }

      }

      return {
        email,
        password,
        loading,
        errorMessage,
        handleLogin,
        showPassword
      }

    }

  }
</script>

<style scoped>
  .login-page {
    height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url('https://www.internalauditing.com/wp-content/uploads/2020/08/Asset-Management-ISO-55001-Internal-Auditing-Pros-of-America.jpg');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
  }

  .login-card {
    width: 100%;
    max-width: 380px;
    padding: 40px;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.95);
    box-shadow: 0 10px 25px rgba(0,0,0,0.2);
    box-sizing: border-box; 
  }

    .login-card h1 {
      text-align: center;
      margin-bottom: 30px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #333;
      font-weight: 700;
    }

  .form-group {
    margin-bottom: 20px;
  }

    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #444;
    }

    .form-group input {
      width: 100%;
      padding: 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      box-sizing: border-box;
      font-size: 14px;
      transition: border-color 0.3s;
    }

      .form-group input:focus {
        outline: none;
        border-color: #42b983;
      }

  button {
    width: 100%;
    padding: 12px;
    background: #42b983;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: bold;
    font-size: 16px;
    box-sizing: border-box;
    transition: background 0.3s;
  }

    button:hover {
      background: #369a6d;
    }

    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

  .error {
    margin-top: 15px;
    color: #d63031;
    text-align: center;
    font-size: 14px;
    font-weight: 500;
  }

  .password-wrapper {
    position: relative;
    display: flex;
    align-items: center;
  }

    .password-wrapper input {
      padding-right: 40px;
    }

  .toggle-password {
    position: absolute;
    right: 12px;
    cursor: pointer;
    color: #666;
    font-size: 1.2rem;
    display: flex;
    transition: color 0.2s ease;
  }

    .toggle-password:hover {
      color: #42b983;
    }

  .auth-links {
    margin-top: 12px;
    text-align: right;
  }

    .auth-links a {
      color: #0f766e;
      text-decoration: none;
      font-weight: 600;
    }
</style>

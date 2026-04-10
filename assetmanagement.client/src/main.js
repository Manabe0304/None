import { createPinia } from "pinia"
import { createApp } from "vue"
import App from "./App.vue"

import router from "./components/router"

import 'bootstrap-icons/font/bootstrap-icons.css'
import '@fortawesome/fontawesome-free/css/all.min.css'

const app = createApp(App)


const pinia = createPinia()           // 👈 tạo pinia

app.use(pinia)
app.use(router)

app.mount("#app")

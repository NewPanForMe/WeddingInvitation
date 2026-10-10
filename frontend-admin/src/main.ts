import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { setUnauthorizedHandler } from './api/http'
import { useAuthStore } from './stores/auth'
import './styles/index.css'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// 401 时同步清空内存中的登录态（localStorage 与跳转由 http 拦截器处理）
setUnauthorizedHandler(() => {
  useAuthStore().clear()
})

app.mount('#app')

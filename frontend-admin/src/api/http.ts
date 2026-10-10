import axios, { AxiosError, type AxiosInstance, type AxiosResponse } from 'axios'
import { createDiscreteApi } from 'naive-ui'
import type { ApiError } from '@/types'

/** localStorage 中登录态的存储键 */
export const TOKEN_KEY = 'wedding_admin_token'
export const USERNAME_KEY = 'wedding_admin_username'
export const EXPIRES_AT_KEY = 'wedding_admin_expires_at'

/** 脱离组件树的全局提示（可在拦截器中使用） */
const { message: globalMessage } = createDiscreteApi(['message'])

/** 401 时由 main.ts 注入：清空 Pinia 中的登录态 */
let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(handler: () => void) {
  unauthorizedHandler = handler
}

/** 应用基路径（与 vite base / router history 保持一致） */
const APP_BASE = import.meta.env.BASE_URL
const SITE_BASE = APP_BASE.replace(/\/admin\/?$/, '')

function redirectToLogin() {
  const loginPath = `${APP_BASE}login`
  if (window.location.pathname !== loginPath && !window.location.pathname.endsWith('/login')) {
    window.location.replace(loginPath)
  }
}

function clearAuthStorage() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USERNAME_KEY)
  localStorage.removeItem(EXPIRES_AT_KEY)
}

const http: AxiosInstance = axios.create({
  baseURL: `${SITE_BASE}/api`,
  timeout: 30000
})

/* 请求拦截：注入 Authorization: Bearer <token> */
http.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

/* 响应拦截：统一错误提示；401 清空登录态并跳转登录页 */
http.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError<ApiError>) => {
    const status = error.response?.status
    const data = error.response?.data
    const url = error.config?.url ?? ''
    const isLoginRequest = url.includes('/admin/login')

    let text = data?.message || (error.code === 'ECONNABORTED' ? '请求超时，请稍后重试' : '') || error.message || '请求失败'

    if (status === 401 && !isLoginRequest) {
      clearAuthStorage()
      unauthorizedHandler?.()
      text = data?.message || '登录已失效，请重新登录'
      globalMessage.error(text)
      redirectToLogin()
      return Promise.reject(error)
    }

    globalMessage.error(text)
    return Promise.reject(error)
  }
)

export default http

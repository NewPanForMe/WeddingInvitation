import axios, { AxiosError, type AxiosResponse } from 'axios'
import type { ApiErrorBody } from '@/types'

/**
 * 统一 axios 实例。
 * baseURL 跟随站点基路径：应用部署在 /Wedding/i，接口仍在 /Wedding/api。
 * 生产环境靠同源部署（dist 放到站点根目录，后端同域）。
 */
const APP_BASE = import.meta.env.BASE_URL
const SITE_BASE = APP_BASE.replace(/\/i\/?$/, '')

const http = axios.create({
  baseURL: SITE_BASE.replace(/\/$/, ''),
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
})

/** 判断响应体是否为后端统一错误结构 { code, message } */
function isApiErrorBody(data: unknown): data is ApiErrorBody {
  if (typeof data !== 'object' || data === null) return false
  const body = data as Record<string, unknown>
  return typeof body.code === 'number' && typeof body.message === 'string'
}

/** 无 response 时的兜底文案（网络断开、超时、跨域等） */
function fallbackMessage(error: AxiosError): string {
  if (error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT') {
    return '网络请求超时，请稍后重试'
  }
  if (error.code === 'ERR_NETWORK') {
    return '网络连接失败，请检查网络后重试'
  }
  return error.message || '请求失败，请稍后重试'
}

http.interceptors.response.use(
  // 成功：直接把响应体透出，调用方拿到的是契约里的 JSON 结构
  (response: AxiosResponse) => response.data,
  // 失败：把 { code, message } 统一转成 Error(message)，业务层只处理 message
  (error: AxiosError) => {
    const body: unknown = error.response?.data
    if (isApiErrorBody(body)) {
      const wrapped = new Error(body.message)
      wrapped.name = 'ApiError'
      ;(wrapped as Error & { code?: number }).code = body.code
      return Promise.reject(wrapped)
    }
    return Promise.reject(new Error(fallbackMessage(error)))
  }
)

export default http

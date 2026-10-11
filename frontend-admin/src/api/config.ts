import http from './http'
import type { WeddingConfig, WeddingConfigPayload } from '@/types'

/** GET /api/admin/config */
export async function fetchConfig(): Promise<WeddingConfig> {
  const { data } = await http.get<WeddingConfig>('/admin/config')
  return data
}

/** POST /api/admin/config —— 仅提交文字地址，无经纬度字段；POST 避免 IIS/WebDAV 拦截更新请求。 */
export async function updateConfig(payload: WeddingConfigPayload): Promise<WeddingConfig> {
  const { data } = await http.post<WeddingConfig>('/admin/config', payload)
  return data
}

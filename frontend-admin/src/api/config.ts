import http from './http'
import type { WeddingConfig, WeddingConfigPayload } from '@/types'

/** GET /api/admin/config */
export async function fetchConfig(): Promise<WeddingConfig> {
  const { data } = await http.get<WeddingConfig>('/admin/config')
  return data
}

/** PUT /api/admin/config —— 仅提交文字地址，无经纬度字段 */
export async function updateConfig(payload: WeddingConfigPayload): Promise<WeddingConfig> {
  const { data } = await http.put<WeddingConfig>('/admin/config', payload)
  return data
}

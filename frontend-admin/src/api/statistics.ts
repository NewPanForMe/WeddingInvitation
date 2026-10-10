import http from './http'
import type { StatisticsDto } from '@/types'

/** GET /api/admin/statistics */
export async function fetchStatistics(): Promise<StatisticsDto> {
  const { data } = await http.get<StatisticsDto>('/admin/statistics')
  return data
}

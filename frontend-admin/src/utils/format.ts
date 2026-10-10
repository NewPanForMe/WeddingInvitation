/**
 * 时间展示统一走北京时间（Asia/Shanghai）。
 * 后端返回的是带时区的 ISO 字符串（如 2026-10-01T04:00:00+00:00）。
 */

const dateTimeFormatter = new Intl.DateTimeFormat('zh-CN', {
  timeZone: 'Asia/Shanghai',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false
})

const dateFormatter = new Intl.DateTimeFormat('zh-CN', {
  timeZone: 'Asia/Shanghai',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit'
})

/** 格式化为 YYYY-MM-DD HH:mm（北京时间） */
export function formatDateTime(value?: string | number | Date | null): string {
  if (value === null || value === undefined || value === '') return '-'
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  const parts = dateTimeFormatter.formatToParts(date)
  const pick = (type: Intl.DateTimeFormatPartTypes) => parts.find((p) => p.type === type)?.value ?? ''
  return `${pick('year')}-${pick('month')}-${pick('day')} ${pick('hour')}:${pick('minute')}`
}

/** 格式化为 YYYY-MM-DD（北京时间） */
export function formatDate(value?: string | number | Date | null): string {
  if (value === null || value === undefined || value === '') return '-'
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  const parts = dateFormatter.formatToParts(date)
  const pick = (type: Intl.DateTimeFormatPartTypes) => parts.find((p) => p.type === type)?.value ?? ''
  return `${pick('year')}-${pick('month')}-${pick('day')}`
}

/** ISO 字符串 → n-date-picker 需要的毫秒时间戳（datetime 类型） */
export function isoToTimestamp(value?: string | null): number | null {
  if (!value) return null
  const time = new Date(value).getTime()
  return Number.isNaN(time) ? null : time
}

/** 毫秒时间戳 → 带时区的 ISO 字符串（提交给后端） */
export function timestampToIso(value?: number | null): string {
  if (value === null || value === undefined) return ''
  return new Date(value).toISOString()
}

/** 百分比展示：0.667 → 66.7% */
export function formatPercent(rate?: number | null, digits = 1): string {
  if (rate === null || rate === undefined || Number.isNaN(rate)) return '0%'
  return `${(rate * 100).toFixed(digits)}%`
}

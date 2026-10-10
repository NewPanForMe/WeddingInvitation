/** 后端接口契约对应的类型定义（已冻结，勿随意改动） */

/** 宾客 RSVP 状态 */
export type GuestStatus = 'pending' | 'attending' | 'declined'

export interface GuestDto {
  id: number
  name: string
  salutation: string | null
  phone: string | null
  relation: string | null
  tableNo: string | null
  maxGuests: number | null
  status: GuestStatus
  token: string
  rsvpSubmitted: boolean
  attend: boolean | null
  guestCount: number | null
  remark: string | null
  rsvpUpdatedAt: string | null
  createdAt: string
}

export interface GuestUpsertPayload {
  name: string
  salutation?: string | null
  phone?: string | null
  relation?: string | null
  tableNo?: string | null
  maxGuests?: number | null
}

export interface GuestListQuery {
  keyword?: string
  status?: GuestStatus | ''
  page?: number
  pageSize?: number
}

export interface GuestListResult {
  total: number
  page: number
  pageSize: number
  items: GuestDto[]
}

export interface GuestImportResult {
  total: number
  imported: number
  skipped: number
  errors: string[]
}

export interface GuestLinkResult {
  token: string
  url: string
}

export interface PhotoDto {
  id: number
  url: string
  title: string | null
  sortOrder: number
  enabled: boolean
  createdAt: string
}

export interface PhotoUpdatePayload {
  title?: string | null
  sortOrder?: number
  enabled?: boolean
}

export interface WeddingConfig {
  groomName: string
  brideName: string
  weddingTime: string
  venue: string
  /** 详细地址（仅文字地址，禁止经纬度字段） */
  address: string
  coverUrl: string
  rsvpDeadline: string
}

export type WeddingConfigPayload = WeddingConfig

export interface StatisticsDto {
  totalGuests: number
  responded: number
  attending: number
  declined: number
  pending: number
  expectedAttendees: number
  /** 0 ~ 1 */
  responseRate: number
}

/** 登录相关 */
export interface LoginPayload {
  username: string
  password: string
}

export interface LoginResult {
  token: string
  expiresAt: string
  username: string
}

export interface ChangePasswordPayload {
  oldPassword: string
  newPassword: string
}

export interface SimpleResult {
  success: boolean
  message?: string
}

/** 统一错误结构 */
export interface ApiError {
  code: number
  message: string
}

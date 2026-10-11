/**
 * 与后端接口契约一一对应的类型定义。
 *
 * 契约来源（已冻结）：
 *  GET  /api/invite/{token} -> InviteResponse（token 无效返回 404 + ApiErrorBody）
 *  POST /api/rsvp           -> RsvpSubmitResponse（超时返回 400 + ApiErrorBody）
 */

/** 统一错误结构：{ "code": 404, "message": "邀请链接无效" } */
export interface ApiErrorBody {
  code: number
  message: string
}

/** 宾客信息 */
export interface Guest {
  /** 宾客姓名 */
  name: string
  /** 称呼，如「亲爱的张三」 */
  salutation: string
  /** 桌号，如「A区 8号桌」 */
  tableNo: string
  /** 历史兼容字段，RSVP 出席人数不再使用该值作为上限 */
  maxGuests: number
}

/** 婚礼配置（新人、时间、地点、封面） */
export interface InviteConfig {
  groomName: string
  brideName: string
  /** ISO 8601 带时区，如 2026-10-01T04:00:00+00:00 */
  weddingTime: string
  /** 场所名，如「XX酒店 3楼宴会厅」 */
  venue: string
  /** 详细地址，如「XX市XX区XX路88号」 */
  address: string
  /** 封面图，相对路径如 /Wedding/upload/cover.jpg */
  coverUrl: string
  /** 回复截止时间（ISO 8601 带时区） */
  rsvpDeadline: string
}

/** 照片墙条目 */
export interface InvitePhoto {
  id: number
  /** 相对路径如 /Wedding/upload/1.jpg */
  url: string
  title: string
  sortOrder: number
}

/** 已有 RSVP 回复（未回复时各字段为 null） */
export interface RsvpRecord {
  submitted: boolean
  attend: boolean | null
  guestCount: number | null
  remark: string | null
  updatedAt: string | null
}

/** GET /api/invite/{token} 的 200 响应体 */
export interface InviteResponse {
  guest: Guest
  config: InviteConfig
  photos: InvitePhoto[]
  rsvp: RsvpRecord
  /** 是否仍在可回复时间窗内 */
  rsvpOpen: boolean
}

/** POST /api/rsvp 请求体 */
export interface RsvpSubmitPayload {
  token: string
  attend: boolean
  guestCount: number
  remark: string
}

/** POST /api/rsvp 的 200 响应体 */
export interface RsvpSubmitResponse {
  success: boolean
  message: string
  /** attending | declined */
  status: RsvpStatus
}

/** 回复状态 */
export type RsvpStatus = 'attending' | 'declined' | string

/** 页面加载状态 */
export type LoadState = 'idle' | 'loading' | 'success' | 'error'

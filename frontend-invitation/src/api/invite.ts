import http from './http'
import type { InviteResponse, RsvpSubmitPayload, RsvpSubmitResponse } from '@/types'

/** 获取请柬数据：token 无效时 http 层会抛出 Error('邀请链接无效') */
export function getInvite(token: string): Promise<InviteResponse> {
  return http.get<unknown, InviteResponse>(`/api/invite/${encodeURIComponent(token)}`, {
    params: { _ts: Date.now() }
  })
}

/** 提交出席回复：超过截止时间时 http 层会抛出 Error('已超过回复截止时间') */
export function submitRsvp(payload: RsvpSubmitPayload): Promise<RsvpSubmitResponse> {
  return http.post<unknown, RsvpSubmitResponse>('/api/rsvp', payload)
}

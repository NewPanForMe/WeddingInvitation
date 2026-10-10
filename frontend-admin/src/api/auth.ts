import http from './http'
import type { ChangePasswordPayload, LoginPayload, LoginResult, SimpleResult } from '@/types'

/** POST /api/admin/login */
export async function login(payload: LoginPayload): Promise<LoginResult> {
  const { data } = await http.post<LoginResult>('/admin/login', payload)
  return data
}

/** POST /api/admin/logout */
export async function logout(): Promise<SimpleResult> {
  const { data } = await http.post<SimpleResult>('/admin/logout')
  return data
}

/** PUT /api/admin/password */
export async function changePassword(payload: ChangePasswordPayload): Promise<SimpleResult> {
  const { data } = await http.put<SimpleResult>('/admin/password', payload)
  return data
}

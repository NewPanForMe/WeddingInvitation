import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { EXPIRES_AT_KEY, TOKEN_KEY, USERNAME_KEY } from '@/api/http'
import * as authApi from '@/api/auth'
import type { LoginPayload } from '@/types'

/** 登录态：token / username / expiresAt，持久化到 localStorage（key: wedding_admin_token） */
export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem(TOKEN_KEY) ?? '')
  const username = ref<string>(localStorage.getItem(USERNAME_KEY) ?? '')
  const expiresAt = ref<string>(localStorage.getItem(EXPIRES_AT_KEY) ?? '')

  const isLoggedIn = computed(() => {
    if (!token.value || !expiresAt.value) return false
    const expiresTime = new Date(expiresAt.value).getTime()
    return Number.isFinite(expiresTime) && expiresTime > Date.now() + 30_000
  })

  function setAuth(payload: { token: string; username: string; expiresAt: string }) {
    token.value = payload.token
    username.value = payload.username
    expiresAt.value = payload.expiresAt
    localStorage.setItem(TOKEN_KEY, payload.token)
    localStorage.setItem(USERNAME_KEY, payload.username)
    localStorage.setItem(EXPIRES_AT_KEY, payload.expiresAt)
  }

  /** 清空登录态（401 / 退出登录） */
  function clear() {
    token.value = ''
    username.value = ''
    expiresAt.value = ''
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USERNAME_KEY)
    localStorage.removeItem(EXPIRES_AT_KEY)
  }

  async function login(payload: LoginPayload) {
    const result = await authApi.login(payload)
    setAuth({
      token: result.token,
      username: result.username ?? payload.username,
      expiresAt: result.expiresAt ?? ''
    })
    return result
  }

  async function logout() {
    try {
      await authApi.logout()
    } catch {
      // 退出接口失败不影响本地登出
    } finally {
      clear()
    }
  }

  return { token, username, expiresAt, isLoggedIn, setAuth, clear, login, logout }
})

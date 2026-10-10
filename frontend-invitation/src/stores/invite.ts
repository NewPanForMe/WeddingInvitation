import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getInvite, submitRsvp } from '@/api/invite'
import type { InviteResponse, LoadState, RsvpSubmitPayload } from '@/types'

/**
 * 请柬状态：token 来自路由参数，负责拉取请柬数据与提交 RSVP。
 */
export const useInviteStore = defineStore('invite', () => {
  /** 当前邀请 token */
  const token = ref('')

  /** 请柬数据 */
  const invite = ref<InviteResponse | null>(null)

  /** 加载状态 */
  const loadState = ref<LoadState>('idle')
  const errorMessage = ref('')

  /** RSVP 提交状态 */
  const submitting = ref(false)
  const submitError = ref('')
  const submitted = ref(false)
  const submittedStatus = ref('')
  const submittedAttend = ref<boolean | null>(null)
  const submittedGuestCount = ref<number | null>(null)
  const submittedRemark = ref('')

  /** 是否有请柬数据 */
  const hasInvite = computed(() => invite.value !== null)

  /** 宾客信息 */
  const guest = computed(() => invite.value?.guest ?? null)

  /** 婚礼配置 */
  const config = computed(() => invite.value?.config ?? null)

  /** 照片墙（按 sortOrder 升序） */
  const photos = computed(() =>
    [...(invite.value?.photos ?? [])].sort((a, b) => a.sortOrder - b.sortOrder)
  )

  /** 后端记录的 RSVP */
  const rsvp = computed(() => invite.value?.rsvp ?? null)

  /** 是否仍可回复 */
  const rsvpOpen = computed(() => invite.value?.rsvpOpen ?? false)

  /** 是否已回复过 */
  const alreadySubmitted = computed(
    () => submitted.value || (rsvp.value?.submitted ?? false)
  )

  /** 当前生效的回复内容（提交结果优先，其次后端回显） */
  const effectiveAttend = computed<boolean | null>(() =>
    submitted.value ? submittedAttend.value : (rsvp.value?.attend ?? null)
  )
  const effectiveGuestCount = computed<number | null>(() =>
    submitted.value ? submittedGuestCount.value : (rsvp.value?.guestCount ?? null)
  )
  const effectiveRemark = computed<string>(() =>
    submitted.value ? submittedRemark.value : (rsvp.value?.remark ?? '')
  )

  /** 设置 token（来自路由参数） */
  function setToken(value: string): void {
    token.value = value
  }

  /** 重置提交状态 */
  function resetSubmitState(): void {
    submitting.value = false
    submitError.value = ''
    submitted.value = false
    submittedStatus.value = ''
    submittedAttend.value = null
    submittedGuestCount.value = null
    submittedRemark.value = ''
  }

  /** 拉取请柬数据 */
  async function load(): Promise<void> {
    if (!token.value) {
      loadState.value = 'error'
      errorMessage.value = '缺少邀请凭证，请使用新人发给您的专属链接打开'
      return
    }
    loadState.value = 'loading'
    errorMessage.value = ''
    try {
      invite.value = await getInvite(token.value)
      loadState.value = 'success'
    } catch (error) {
      invite.value = null
      loadState.value = 'error'
      errorMessage.value = error instanceof Error ? error.message : '请柬加载失败，请稍后重试'
    }
  }

  /** 提交 RSVP，成功返回 true */
  async function submit(payload: Omit<RsvpSubmitPayload, 'token'>): Promise<boolean> {
    submitting.value = true
    submitError.value = ''
    try {
      const result = await submitRsvp({
        token: token.value,
        attend: payload.attend,
        guestCount: payload.guestCount,
        remark: payload.remark
      })
      submitted.value = true
      submittedStatus.value = result.status
      submittedAttend.value = payload.attend
      submittedGuestCount.value = payload.guestCount
      submittedRemark.value = payload.remark
      // 同步回本地数据，保证结果卡片与列表一致
      if (invite.value) {
        invite.value.rsvp = {
          submitted: true,
          attend: payload.attend,
          guestCount: payload.guestCount,
          remark: payload.remark,
          updatedAt: new Date().toISOString()
        }
      }
      return true
    } catch (error) {
      submitError.value = error instanceof Error ? error.message : '提交失败，请稍后重试'
      return false
    } finally {
      submitting.value = false
    }
  }

  return {
    // state
    token,
    invite,
    loadState,
    errorMessage,
    submitting,
    submitError,
    submitted,
    submittedStatus,
    // getters
    hasInvite,
    guest,
    config,
    photos,
    rsvp,
    rsvpOpen,
    alreadySubmitted,
    effectiveAttend,
    effectiveGuestCount,
    effectiveRemark,
    // actions
    setToken,
    resetSubmitState,
    load,
    submit
  }
})

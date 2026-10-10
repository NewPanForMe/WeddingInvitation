<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { showToast } from 'vant'
import { useInviteStore } from '@/stores/invite'

const store = useInviteStore()

const attend = ref<boolean>(true)
const guestCount = ref<number>(1)
const remark = ref<string>('')

/** 是否已回复（后端回显或本次提交成功） */
const hasReply = computed(() => store.alreadySubmitted)

/** 是否可编辑：截止时间已过则整表单禁用 */
const editable = computed(() => store.rsvpOpen)

/** 结果卡片文案 */
const resultTitle = computed(() => (store.effectiveAttend ? '已确认出席' : '已回复不出席'))
const resultDesc = computed(() => {
  if (store.effectiveAttend) {
    return `出席人数：${store.effectiveGuestCount ?? 1} 人`
  }
  return '感谢您的回复，期待下次相聚'
})

/** 用已有回复（或后端回显）初始化表单 */
function syncFormFromStore(): void {
  const attendValue = store.effectiveAttend
  if (attendValue !== null) {
    attend.value = attendValue
  }
  const count = store.effectiveGuestCount
  guestCount.value = Math.max(1, count ?? 1)
  remark.value = store.effectiveRemark ?? ''
}

watch(
  () => [store.effectiveAttend, store.effectiveGuestCount, store.effectiveRemark],
  syncFormFromStore,
  { immediate: true }
)

// 不出席时不校验、不展示人数
watch(attend, (value) => {
  if (!value) {
    guestCount.value = 1
  }
})

const canSubmit = computed(() => editable.value && !store.submitting)

async function onSubmit(): Promise<void> {
  if (!editable.value) {
    showToast('已超过回复截止时间')
    return
  }
  if (attend.value && guestCount.value < 1) {
    showToast('出席人数不能少于 1 人')
    return
  }
  const ok = await store.submit({
    attend: attend.value,
    guestCount: attend.value ? guestCount.value : 0,
    remark: remark.value.trim()
  })
  if (ok) {
    showToast('回复成功，感谢您的确认')
  } else {
    showToast(store.submitError || '提交失败，请稍后重试')
  }
}
</script>

<template>
  <section id="rsvp" class="rsvp card">
    <h2 class="section-title">出席回复</h2>

    <!-- 已截止提示 -->
    <van-notice-bar
      v-if="!editable"
      wrapable
      :scrollable="false"
      left-icon="info-o"
      text="回复已截止，如需变更请直接联系新人"
      class="rsvp__notice"
    />

    <!-- 已提交结果卡片 -->
    <div v-if="hasReply" class="rsvp__result" :class="{ 'is-declined': !store.effectiveAttend }">
      <div class="rsvp__result-head">
        <van-icon :name="store.effectiveAttend ? 'checked' : 'close'" />
        <span>{{ resultTitle }}</span>
      </div>
      <p class="rsvp__result-desc">{{ resultDesc }}</p>
      <p v-if="store.effectiveRemark" class="rsvp__result-remark">
        备注：{{ store.effectiveRemark }}
      </p>
      <p class="rsvp__result-tip">
        {{ editable ? '如需修改，可在下方重新提交' : '回复已截止，无法修改' }}
      </p>
    </div>

    <!-- 表单 -->
    <div class="rsvp__form" :class="{ 'is-disabled': !editable }">
      <div class="rsvp__row">
        <span class="rsvp__label">是否出席</span>
        <van-radio-group v-model="attend" direction="horizontal" :disabled="!editable">
          <van-radio :name="true">出席</van-radio>
          <van-radio :name="false">不出席</van-radio>
        </van-radio-group>
      </div>

      <div v-if="attend" class="rsvp__row">
        <span class="rsvp__label">出席人数</span>
        <div class="rsvp__stepper">
          <van-stepper
            v-model="guestCount"
            :min="1"
            integer
            :disabled="!editable"
          />
          <span class="rsvp__limit">不限人数</span>
        </div>
      </div>

      <van-field
        v-model="remark"
        rows="2"
        autosize
        type="textarea"
        maxlength="100"
        show-word-limit
        label="备注"
        placeholder="饮食禁忌、到达时间等（选填）"
        :disabled="!editable"
        class="rsvp__field"
      />

      <van-notice-bar
        v-if="store.submitError"
        wrapable
        :scrollable="false"
        left-icon="warning-o"
        :text="store.submitError"
        class="rsvp__notice"
      />

      <button type="button" class="btn-gold rsvp__submit" :disabled="!canSubmit" @click="onSubmit">
        {{ store.submitting ? '提交中…' : hasReply ? '更新回复' : '提交回复' }}
      </button>
    </div>
  </section>
</template>

<style scoped>
.rsvp__notice {
  margin-bottom: 12px;
  border-radius: var(--wedding-radius-sm);
  font-size: 13px;
}

.rsvp__result {
  margin-bottom: 16px;
  padding: 14px 16px;
  border-radius: var(--wedding-radius-sm);
  border: 1px solid rgba(201, 162, 39, 0.35);
  background: linear-gradient(180deg, #fdf6e6 0%, #f9f0dd 100%);
}

.rsvp__result.is-declined {
  border-color: var(--wedding-border);
  background: linear-gradient(180deg, #f7f2ec 0%, #f2ebe3 100%);
}

.rsvp__result-head {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 16px;
  font-weight: 600;
  color: var(--wedding-primary-dark);
}

.rsvp__result-desc {
  margin: 6px 0 0;
  font-size: 14px;
  color: var(--wedding-text);
}

.rsvp__result-remark {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--wedding-text-sub);
  word-break: break-all;
}

.rsvp__result-tip {
  margin: 8px 0 0;
  font-size: 12px;
  color: var(--wedding-text-muted);
}

.rsvp__form.is-disabled {
  opacity: 0.7;
}

.rsvp__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 12px 0;
  border-bottom: 1px solid var(--wedding-border);
}

.rsvp__label {
  flex: 0 0 auto;
  font-size: 14px;
  color: var(--wedding-text-sub);
}

.rsvp__stepper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.rsvp__limit {
  font-size: 12px;
  color: var(--wedding-text-muted);
}

.rsvp__field {
  margin-top: 10px;
  padding-left: 0;
  padding-right: 0;
  background-color: transparent;
}

.rsvp__field :deep(.van-field__label) {
  width: 62px;
  color: var(--wedding-text-sub);
}

.rsvp__submit {
  margin-top: 18px;
}
</style>

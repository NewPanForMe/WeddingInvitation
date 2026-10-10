<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

/** 支持 ?code=404&message=邀请链接无效，也支持无参数直接访问 */
const code = computed(() => {
  const raw = route.query.code
  const value = Array.isArray(raw) ? raw[0] : raw
  return value ? String(value) : ''
})

const queryMessage = computed(() => {
  const raw = route.query.message
  const value = Array.isArray(raw) ? raw[0] : raw
  return value ? String(value) : ''
})

const title = computed(() => {
  if (queryMessage.value) return queryMessage.value
  return code.value === '400' ? '请求有误' : '邀请链接无效'
})

const description = computed(() => {
  if (code.value === '400') {
    return '请检查链接是否完整，或联系新人重新发送。'
  }
  return '链接可能已失效、已过期，或输入有误。'
})

const tips = [
  '确认链接是否被聊天软件截断（可长按复制完整链接）',
  '请使用新人发送给您的专属链接打开',
  '仍有疑问？直接联系新人即可'
]

onMounted(() => {
  document.title = title.value
})
</script>

<template>
  <main class="page error-page">
    <div class="error-page__icon">
      <van-icon name="warning-o" size="34" />
    </div>

    <van-empty image="error" :description="title" />

    <p class="error-page__desc">{{ description }}</p>

    <ul class="error-page__tips">
      <li v-for="tip in tips" :key="tip">{{ tip }}</li>
    </ul>

    <router-link to="/" class="btn-ghost error-page__back">返回首页</router-link>
  </main>
</template>

<style scoped>
.error-page {
  padding: 60px var(--wedding-page-x) calc(40px + var(--wedding-safe-bottom));
  text-align: center;
}

.error-page__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 72px;
  height: 72px;
  margin: 0 auto 8px;
  border-radius: 50%;
  color: var(--wedding-primary);
  background: linear-gradient(180deg, #f7efe6 0%, #f0e2d3 100%);
  box-shadow: var(--wedding-shadow-soft);
}

.error-page__desc {
  margin: 0 0 18px;
  font-size: 14px;
  color: var(--wedding-text-sub);
}

.error-page__tips {
  margin: 0 auto 26px;
  padding: 16px 18px 16px 34px;
  max-width: 320px;
  border-radius: var(--wedding-radius);
  background-color: var(--wedding-surface);
  box-shadow: var(--wedding-shadow-soft);
  text-align: left;
  font-size: 13px;
  line-height: 1.9;
  color: var(--wedding-text-sub);
}

.error-page__tips li::marker {
  color: var(--wedding-gold);
}

.error-page__back {
  display: inline-flex;
  width: 180px;
  text-decoration: none;
}
</style>

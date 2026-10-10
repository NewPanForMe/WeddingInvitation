<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  groomName: string
  brideName: string
  weddingTime: string
  salutation?: string
}>()

const WEEKDAYS = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六']

/** 婚礼时间：接口返回的是带时区 ISO 8601 字符串，这里按打开请柬的设备本地时区渲染 */
const info = computed(() => {
  const date = new Date(props.weddingTime)
  if (Number.isNaN(date.getTime())) {
    return {
      dateText: props.weddingTime,
      timeText: '',
      weekday: '',
      rawText: ''
    }
  }
  const pad = (value: number): string => String(value).padStart(2, '0')
  return {
    dateText: `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`,
    timeText: `${pad(date.getHours())}:${pad(date.getMinutes())}`,
    weekday: WEEKDAYS[date.getDay()] ?? '',
    rawText: `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(
      date.getHours()
    )}:${pad(date.getMinutes())}`
  }
})

const isSameMonth = computed(() => {
  const date = new Date(props.weddingTime)
  if (Number.isNaN(date.getTime())) return true
  const now = new Date()
  return date.getFullYear() === now.getFullYear() && date.getMonth() === now.getMonth()
})
</script>

<template>
  <section class="wedding-info card">
    <h2 class="section-title">婚礼时间</h2>

    <p v-if="salutation" class="wedding-info__salutation">{{ salutation }}</p>

    <p class="wedding-info__couple">
      <span>{{ groomName }}</span>
      <span class="divider-dot">❤</span>
      <span>{{ brideName }}</span>
    </p>

    <div class="wedding-info__time">
      <div class="wedding-info__date">{{ info.dateText }}</div>
      <div class="wedding-info__meta">
        <span v-if="info.weekday">{{ info.weekday }}</span>
        <span v-if="info.timeText" class="wedding-info__clock">{{ info.timeText }}</span>
      </div>
      <div v-if="!isSameMonth" class="wedding-info__raw">{{ info.rawText }}</div>
    </div>

    <p class="wedding-info__note">吉日良辰，恭候您的到来</p>
  </section>
</template>

<style scoped>
.wedding-info {
  text-align: center;
}

.wedding-info__salutation {
  margin: 0 0 6px;
  font-size: 14px;
  letter-spacing: 1px;
  color: var(--wedding-text-sub);
}

.wedding-info__couple {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin: 0 0 16px;
  font-size: 24px;
  font-weight: 600;
  letter-spacing: 3px;
  color: var(--wedding-primary-dark);
}

.wedding-info__time {
  padding: 14px 10px;
  border-radius: var(--wedding-radius-sm);
  background: linear-gradient(180deg, #fdf8f3 0%, #f7efe6 100%);
  border: 1px solid var(--wedding-border);
}

.wedding-info__date {
  font-size: 20px;
  font-weight: 600;
  letter-spacing: 2px;
  color: var(--wedding-text);
}

.wedding-info__meta {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin-top: 4px;
  font-size: 14px;
  color: var(--wedding-text-sub);
}

.wedding-info__clock {
  font-weight: 600;
  color: var(--wedding-gold);
}

.wedding-info__raw {
  margin-top: 4px;
  font-size: 12px;
  color: var(--wedding-text-muted);
}

.wedding-info__note {
  margin: 14px 0 0;
  font-size: 13px;
  letter-spacing: 1px;
  color: var(--wedding-text-muted);
}
</style>

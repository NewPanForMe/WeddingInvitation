<script setup lang="ts">
import { computed } from 'vue'
import type { InvitePhoto } from '@/types'

const props = defineProps<{
  coverUrl: string
  groomName: string
  brideName: string
  weddingTime?: string
  photos?: InvitePhoto[]
}>()

/** 封面缺图时退化为渐变色块 */
const hasCover = computed(() => Boolean(props.coverUrl))
const firstPhoto = computed(() => props.photos?.[0]?.url ?? '')

/** 显示用日期：2026年10月1日 */
const dateText = computed(() => {
  if (!props.weddingTime) return ''
  const date = new Date(props.weddingTime)
  if (Number.isNaN(date.getTime())) return ''
  return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`
})

function scrollToRsvp(): void {
  const target = document.getElementById('rsvp')
  if (target) {
    target.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}
</script>

<template>
  <header class="cover" :class="{ 'cover--empty': !hasCover }">
    <van-image v-if="hasCover" :src="coverUrl" fit="cover" class="cover__image" alt="婚礼封面">
      <template #error>
        <van-image v-if="firstPhoto" :src="firstPhoto" fit="cover" class="cover__image" />
      </template>
    </van-image>

    <!-- 渐变遮罩 -->
    <div class="cover__mask" />

    <div class="cover__content">
      <p class="cover__eyebrow">We are getting married</p>
      <h1 class="cover__names">
        <span>{{ groomName }}</span>
        <span class="cover__amp">&amp;</span>
        <span>{{ brideName }}</span>
      </h1>
      <p v-if="dateText" class="cover__date">{{ dateText }}</p>
      <button type="button" class="cover__cta" @click="scrollToRsvp">诚邀您的到来</button>
    </div>
  </header>
</template>

<style scoped>
.cover {
  position: relative;
  width: 100%;
  height: clamp(320px, 58vh, 460px);
  min-height: 320px;
  max-height: 460px;
  overflow: hidden;
  background: linear-gradient(150deg, #c9a227 0%, #b4886b 45%, #8a6a52 100%);
}

.cover__image,
.cover__image :deep(img) {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cover__mask {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    180deg,
    rgba(74, 58, 48, 0.35) 0%,
    rgba(74, 58, 48, 0.12) 35%,
    rgba(74, 58, 48, 0.55) 78%,
    rgba(74, 58, 48, 0.72) 100%
  );
}

.cover__content {
  position: absolute;
  left: 0;
  right: 0;
  top: 52%;
  padding: 0 20px;
  transform: translateY(-50%);
  text-align: center;
  color: var(--wedding-text-invert);
}

.cover__eyebrow {
  margin: 0 0 8px;
  font-size: 10px;
  letter-spacing: 2.4px;
  text-transform: uppercase;
  opacity: 0.86;
}

.cover__names {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin: 0 0 10px;
  font-size: 28px;
  font-weight: 600;
  letter-spacing: 3px;
  text-shadow: 0 2px 12px rgba(74, 58, 48, 0.4);
}

.cover__amp {
  font-size: 20px;
  color: var(--wedding-gold-soft);
}

.cover__date {
  margin: 0 0 16px;
  font-size: 13px;
  letter-spacing: 1.6px;
  opacity: 0.92;
}

.cover__cta {
  padding: 8px 22px;
  border: 1px solid rgba(255, 253, 250, 0.7);
  border-radius: 20px;
  font-family: var(--wedding-font);
  font-size: 12px;
  letter-spacing: 1.6px;
  color: var(--wedding-text-invert);
  background-color: rgba(255, 253, 250, 0.12);
  backdrop-filter: blur(4px);
  cursor: pointer;
}

@media (min-width: 768px) {
  .cover {
    height: 68vh;
    min-height: 520px;
    max-height: 720px;
  }

  .cover__content {
    top: auto;
    bottom: calc(42px + var(--wedding-safe-bottom));
    padding: 0 24px;
    transform: none;
  }

  .cover__names {
    gap: 12px;
    margin-bottom: 12px;
    font-size: 30px;
    letter-spacing: 4px;
  }

  .cover__date {
    margin-bottom: 20px;
    font-size: 14px;
    letter-spacing: 2px;
  }
}
</style>

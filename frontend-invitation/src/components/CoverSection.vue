<script setup lang="ts">
import { computed } from 'vue'
import type { InvitePhoto } from '@/types'
import DecorativeMotes from './DecorativeMotes.vue'

const props = defineProps<{
  coverUrl: string
  groomName: string
  brideName: string
  weddingTime?: string
  photos?: InvitePhoto[]
  openingMessage?: string | null
}>()

const firstPhoto = computed(() => props.photos?.[0]?.url ?? '')
const coverImage = computed(() => props.coverUrl)
const hasCover = computed(() => Boolean(props.coverUrl))

const dateText = computed(() => {
  if (!props.weddingTime) return ''
  const date = new Date(props.weddingTime)
  if (Number.isNaN(date.getTime())) return ''
  return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`
})

function scrollToRsvp(): void {
  const target = document.getElementById('rsvp')
  if (!target) return
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  target.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'start' })
}
</script>

<template>
  <header class="cover" :class="{ 'cover--empty': !hasCover }">
    <DecorativeMotes :count="6" tone="light" />

    <div class="cover__frame">
      <div class="cover__visual">
        <div
          v-if="hasCover"
          class="cover__backdrop"
          :style="{ backgroundImage: `url(${JSON.stringify(coverImage)})` }"
          aria-hidden="true"
        />

        <van-image v-if="hasCover" :src="coverImage" fit="contain" class="cover__image" alt="婚礼封面">
          <template #error>
            <van-image v-if="firstPhoto && firstPhoto !== coverImage" :src="firstPhoto" fit="contain" class="cover__image" />
          </template>
        </van-image>

        <div class="cover__mask" />

        <div class="cover__content">
          <p class="cover__eyebrow">We are getting married</p>
          <h1 class="cover__names">
            <span>{{ groomName }}</span>
            <span class="cover__amp">&amp;</span>
            <span>{{ brideName }}</span>
          </h1>
          <p v-if="dateText" class="cover__date">{{ dateText }}</p>
          <p v-if="openingMessage" class="cover__message">{{ openingMessage }}</p>
          <button type="button" class="cover__cta" @click="scrollToRsvp">诚邀您的到来</button>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.cover {
  position: relative;
  display: grid;
  place-items: center;
  min-height: 500px;
  padding: var(--space-5) var(--wedding-page-x) var(--space-8);
  overflow: hidden;
  background:
    radial-gradient(circle at 16% 18%, rgba(234, 215, 181, 0.58), transparent 34%),
    radial-gradient(circle at 82% 76%, rgba(166, 107, 99, 0.34), transparent 40%),
    linear-gradient(155deg, #f3e4d5 0%, #d9b6aa 44%, #8d5a53 100%);
}

.cover::after {
  position: absolute;
  z-index: 1;
  right: -38px;
  bottom: -20px;
  width: 170px;
  height: 170px;
  border: 1px solid rgba(255, 248, 238, 0.26);
  border-radius: 50%;
  content: '';
  box-shadow: inset 0 0 0 18px rgba(255, 248, 238, 0.04);
  pointer-events: none;
}

.cover__frame {
  position: relative;
  z-index: 3;
  width: min(100%, 390px, calc((100svh - 48px) * 4 / 5));
  aspect-ratio: 4 / 5;
  padding: 8px;
  overflow: hidden;
  border: 1px solid rgba(234, 215, 181, 0.94);
  border-radius: 22px;
  background: var(--wedding-mat);
  box-shadow: var(--wedding-frame-shadow);
  animation: cover-enter 680ms cubic-bezier(0.2, 0.72, 0.22, 1) both;
}

.cover__frame::after {
  position: absolute;
  z-index: 5;
  inset: 0;
  content: '';
  pointer-events: none;
  background: linear-gradient(110deg, transparent 24%, rgba(255, 250, 244, 0.42) 46%, transparent 68%);
  transform: translateX(-120%);
  animation: frame-shimmer 900ms ease-out 520ms 1 both;
}

.cover__visual {
  position: relative;
  width: 100%;
  height: 100%;
  overflow: hidden;
  border-radius: 15px;
  background: linear-gradient(145deg, var(--wedding-cocoa-soft), var(--wedding-cocoa));
}

.cover__backdrop {
  position: absolute;
  inset: -10%;
  background-position: center;
  background-size: cover;
  filter: blur(22px) saturate(0.76) brightness(0.78);
  opacity: 0.86;
  transform: scale(1.12);
}

.cover__image,
.cover__image :deep(img) {
  position: relative;
  z-index: 1;
  display: block;
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.cover__mask {
  position: absolute;
  z-index: 2;
  inset: 0;
  background:
    linear-gradient(180deg, rgba(47, 33, 30, 0.1) 0%, rgba(47, 33, 30, 0.04) 38%, rgba(47, 33, 30, 0.72) 72%, rgba(47, 33, 30, 0.92) 100%),
    linear-gradient(90deg, rgba(47, 33, 30, 0.12), transparent 30%, transparent 70%, rgba(47, 33, 30, 0.12));
}

.cover__content {
  position: absolute;
  z-index: 3;
  right: 0;
  bottom: clamp(16px, 5%, 28px);
  left: 0;
  padding: 0 var(--space-5);
  max-height: calc(100% - 24px);
  overflow-y: auto;
  overscroll-behavior: contain;
  scrollbar-width: thin;
  text-align: center;
  color: var(--wedding-text-invert);
}

.cover__eyebrow {
  margin: 0 0 var(--space-2);
  font-size: 10px;
  letter-spacing: 0.24em;
  text-transform: uppercase;
  opacity: 0.9;
}

.cover__names {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
  margin: 0 0 var(--space-2);
  font-family: var(--wedding-font-display);
  font-size: clamp(24px, 7.4vw, 31px);
  font-weight: 600;
  letter-spacing: 0.12em;
  text-shadow: 0 3px 16px rgba(20, 12, 10, 0.52);
}

.cover__amp {
  color: var(--wedding-gold-soft);
  font-size: 0.72em;
}

.cover__date {
  margin: 0 0 var(--space-3);
  font-size: 13px;
  letter-spacing: 0.14em;
}

.cover__message {
  max-width: 20em;
  margin: calc(var(--space-1) * -1) auto var(--space-3);
  font-family: var(--wedding-font-display);
  font-size: 14px;
  line-height: 1.7;
  letter-spacing: 0.06em;
  text-shadow: 0 2px 12px rgba(20, 12, 10, 0.48);
}

.cover__cta {
  min-height: 40px;
  padding: var(--space-2) var(--space-5);
  border: 1px solid rgba(255, 250, 244, 0.72);
  border-radius: 999px;
  color: var(--wedding-text-invert);
  background: rgba(96, 51, 47, 0.46);
  backdrop-filter: blur(10px) saturate(110%);
  font-size: 12px;
  letter-spacing: 0.14em;
  cursor: pointer;
}

.cover--empty .cover__visual {
  background:
    radial-gradient(circle at 34% 28%, rgba(234, 215, 181, 0.34), transparent 26%),
    linear-gradient(155deg, #a66b63, #60332f 76%);
}

@keyframes cover-enter {
  from {
    opacity: 0;
    transform: translateY(18px) scale(0.985);
  }
  to {
    opacity: 1;
    transform: none;
  }
}

@keyframes frame-shimmer {
  to {
    transform: translateX(120%);
  }
}

@media (max-height: 620px), (orientation: landscape) and (max-height: 620px) {
  .cover {
    min-height: 100svh;
    padding-block: var(--space-3);
  }

  .cover__frame {
    width: min(82vw, 340px, calc((100svh - 24px) * 4 / 5));
  }

  .cover__content {
    bottom: var(--space-3);
  }

}

@media (prefers-reduced-motion: reduce) {
  .cover__frame {
    animation: cover-fade 180ms ease both;
  }

  .cover__frame::after {
    display: none;
    animation: none;
  }
}

@media (prefers-reduced-transparency: reduce) {
  .cover__backdrop {
    display: none;
  }

  .cover__cta {
    background: var(--wedding-primary-deep);
    backdrop-filter: none;
  }
}

@media (prefers-contrast: more) {
  .cover__mask {
    background: linear-gradient(180deg, rgba(20, 12, 10, 0.06) 0%, rgba(20, 12, 10, 0.94) 68%);
  }

  .cover__cta {
    border-width: 2px;
    background: var(--wedding-cocoa);
    backdrop-filter: none;
  }
}

@keyframes cover-fade {
  from { opacity: 0; }
  to { opacity: 1; }
}
</style>

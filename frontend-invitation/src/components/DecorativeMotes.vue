<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { normalizeMoteCount } from './decorative-motes'

const props = withDefaults(
  defineProps<{
    count?: number
    tone?: 'light' | 'dark'
  }>(),
  {
    count: 6,
    tone: 'light'
  }
)

const root = ref<HTMLElement | null>(null)
const isActive = ref(false)
const motes = computed(() => Array.from({ length: normalizeMoteCount(props.count) }, (_, index) => index))
let observer: IntersectionObserver | null = null

onMounted(() => {
  if (!root.value) return
  if (!('IntersectionObserver' in window)) {
    isActive.value = true
    return
  }

  observer = new IntersectionObserver(
    ([entry]) => {
      isActive.value = Boolean(entry?.isIntersecting)
    },
    { threshold: 0.01 }
  )
  observer.observe(root.value)
})

onBeforeUnmount(() => observer?.disconnect())
</script>

<template>
  <span
    ref="root"
    class="decorative-motes"
    :class="[`decorative-motes--${tone}`, { 'is-active': isActive }]"
    aria-hidden="true"
  >
    <i v-for="index in motes" :key="index" class="decorative-motes__item" />
  </span>
</template>

<style scoped>
.decorative-motes {
  position: absolute;
  z-index: 2;
  inset: 0;
  display: block;
  overflow: hidden;
  pointer-events: none;
}

.decorative-motes__item {
  position: absolute;
  left: var(--mote-left);
  bottom: -8%;
  width: var(--mote-size);
  height: var(--mote-size);
  border-radius: 50%;
  background: currentColor;
  box-shadow: 0 0 12px currentColor;
  opacity: 0;
  animation: mote-drift var(--mote-duration) linear var(--mote-delay) infinite;
  animation-play-state: paused;
  will-change: transform, opacity;
}

.decorative-motes.is-active .decorative-motes__item {
  animation-play-state: running;
}

.decorative-motes--light {
  color: rgba(239, 211, 160, 0.58);
}

.decorative-motes--dark {
  color: rgba(226, 181, 133, 0.42);
}

.decorative-motes__item:nth-child(1) {
  --mote-left: 12%;
  --mote-size: 3px;
  --mote-duration: 13s;
  --mote-delay: -4s;
}

.decorative-motes__item:nth-child(2) {
  --mote-left: 27%;
  --mote-size: 5px;
  --mote-duration: 16s;
  --mote-delay: -10s;
}

.decorative-motes__item:nth-child(3) {
  --mote-left: 44%;
  --mote-size: 2px;
  --mote-duration: 11s;
  --mote-delay: -2s;
}

.decorative-motes__item:nth-child(4) {
  --mote-left: 63%;
  --mote-size: 4px;
  --mote-duration: 15s;
  --mote-delay: -7s;
}

.decorative-motes__item:nth-child(5) {
  --mote-left: 78%;
  --mote-size: 3px;
  --mote-duration: 12s;
  --mote-delay: -9s;
}

.decorative-motes__item:nth-child(6) {
  --mote-left: 91%;
  --mote-size: 5px;
  --mote-duration: 14s;
  --mote-delay: -5s;
}

@keyframes mote-drift {
  0% {
    opacity: 0;
    transform: translate3d(0, 0, 0) scale(0.7);
  }
  18% {
    opacity: 0.55;
  }
  70% {
    opacity: 0.25;
  }
  100% {
    opacity: 0;
    transform: translate3d(18px, -112svh, 0) scale(1.15);
  }
}

@media (prefers-reduced-motion: reduce) {
  .decorative-motes__item {
    display: none;
    animation: none;
  }
}

@media (prefers-reduced-transparency: reduce), (prefers-contrast: more) {
  .decorative-motes {
    display: none;
  }
}
</style>

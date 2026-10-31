<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'

const props = defineProps<{
  src?: string | null
  enabled: boolean
}>()

const audio = ref<HTMLAudioElement | null>(null)
const isPlaying = ref(false)
const isAvailable = computed(() => props.enabled && Boolean(props.src?.trim()))

async function play(): Promise<void> {
  if (!audio.value || !isAvailable.value) return
  try {
    await audio.value.play()
  } catch {
    isPlaying.value = false
  }
}

function toggle(): void {
  if (!audio.value) return
  if (isPlaying.value) {
    audio.value.pause()
    return
  }
  void play()
}

function unlockAudio(): void {
  removeUnlockListeners()
  void play()
}

function removeUnlockListeners(): void {
  window.removeEventListener('pointerdown', unlockAudio, true)
  window.removeEventListener('keydown', unlockAudio, true)
}

onMounted(() => {
  window.addEventListener('pointerdown', unlockAudio, { capture: true, once: true })
  window.addEventListener('keydown', unlockAudio, { capture: true, once: true })
})

onBeforeUnmount(() => {
  removeUnlockListeners()
  audio.value?.pause()
})

watch(
  () => props.src,
  () => {
    isPlaying.value = false
  }
)
</script>

<template>
  <div v-if="isAvailable" class="background-music">
    <audio
      ref="audio"
      :src="src ?? undefined"
      loop
      preload="metadata"
      @play="isPlaying = true"
      @pause="isPlaying = false"
    />
    <button
      type="button"
      class="background-music__toggle"
      :class="{ 'is-playing': isPlaying }"
      :aria-label="isPlaying ? '暂停背景音乐' : '播放背景音乐'"
      :title="isPlaying ? '暂停背景音乐' : '播放背景音乐'"
      @click="toggle"
    >
      <van-icon name="music-o" />
    </button>
  </div>
</template>

<style scoped>
.background-music {
  position: fixed;
  z-index: 20;
  top: max(16px, env(safe-area-inset-top, 0px));
  right: max(16px, env(safe-area-inset-right, 0px));
}

.background-music__toggle {
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  padding: 0;
  border: 1px solid rgba(234, 215, 181, 0.84);
  border-radius: 50%;
  color: var(--wedding-primary-deep);
  background-color: rgba(255, 248, 238, 0.9);
  box-shadow: var(--wedding-shadow-soft);
  backdrop-filter: blur(8px);
  cursor: pointer;
}

.background-music__toggle :deep(.van-icon) {
  font-size: 19px;
}

.background-music__toggle.is-playing :deep(.van-icon) {
  animation: music-spin 5s linear infinite;
}

@keyframes music-spin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .background-music__toggle.is-playing :deep(.van-icon) {
    animation: none;
  }
}

@media (prefers-reduced-transparency: reduce) {
  .background-music__toggle {
    background: var(--wedding-surface);
    backdrop-filter: none;
  }
}

@media (prefers-contrast: more) {
  .background-music__toggle {
    border: 2px solid var(--wedding-primary-deep);
    background: var(--wedding-surface);
    backdrop-filter: none;
  }
}
</style>

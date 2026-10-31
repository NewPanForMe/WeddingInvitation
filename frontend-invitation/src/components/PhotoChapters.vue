<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { showImagePreview } from 'vant'
import type { InvitePhoto } from '@/types'
import DecorativeMotes from './DecorativeMotes.vue'
import { getAdjacentChapterIndex, getChapterKeyboardStep } from './photo-chapter-navigation'
import { getPhotoCopyMode } from './invitation-presentation'

const props = defineProps<{
  photos: InvitePhoto[]
}>()

const gallery = computed(() => props.photos)
const photoCopyModes = computed(() =>
  gallery.value.map((photo) => getPhotoCopyMode(photo.title, photo.description))
)
const storyRoot = ref<HTMLElement | null>(null)
const activeIndex = ref(0)
const visibleIndices = ref<Set<number>>(new Set([0]))
const failedIndices = ref<Set<number>>(new Set())
const intersectionRatios = new Map<number, number>()
let observer: IntersectionObserver | null = null

function chapterElements(): HTMLElement[] {
  return Array.from(storyRoot.value?.querySelectorAll<HTMLElement>('[data-photo-chapter]') ?? [])
}

function observeChapters(): void {
  observer?.disconnect()
  intersectionRatios.clear()
  const elements = chapterElements()
  if (elements.length === 0) return

  if (!('IntersectionObserver' in window)) {
    visibleIndices.value = new Set(elements.map((_, index) => index))
    return
  }

  observer = new IntersectionObserver(
    (entries) => {
      const nextVisible = new Set(visibleIndices.value)
      for (const entry of entries) {
        const index = Number((entry.target as HTMLElement).dataset.photoChapter)
        intersectionRatios.set(index, entry.isIntersecting ? entry.intersectionRatio : 0)
        if (entry.isIntersecting && entry.intersectionRatio >= 0.12) nextVisible.add(index)
      }

      visibleIndices.value = nextVisible
      const mostVisible = [...intersectionRatios.entries()].sort((a, b) => b[1] - a[1])[0]
      if (mostVisible && mostVisible[1] > 0) activeIndex.value = mostVisible[0]
    },
    { threshold: [0, 0.12, 0.35, 0.6, 0.8] }
  )

  elements.forEach((element) => observer?.observe(element))
}

function preview(index: number): void {
  if (gallery.value.length === 0) return
  showImagePreview({
    images: gallery.value.map((photo) => photo.url),
    startPosition: index,
    closeable: true,
    loop: false
  })
}

function markImageFailed(index: number): void {
  failedIndices.value = new Set(failedIndices.value).add(index)
}

function handleKeydown(event: KeyboardEvent): void {
  if (document.querySelector('.van-image-preview')) return
  const storyVisible = [...intersectionRatios.values()].some((ratio) => ratio >= 0.12)
  const target = event.target instanceof HTMLElement ? event.target : null
  const copy = target?.closest<HTMLElement>('.photo-chapter__copy') ?? null
  const step = getChapterKeyboardStep(
    event.key,
    {
      tagName: target?.tagName,
      isContentEditable: target?.isContentEditable,
      isScrollableContent: Boolean(copy && copy.scrollHeight > copy.clientHeight)
    },
    storyVisible
  )
  if (step === 0) return
  const targetIndex = getAdjacentChapterIndex(activeIndex.value, step, gallery.value.length)
  if (targetIndex === null) return

  event.preventDefault()
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  chapterElements()[targetIndex]?.scrollIntoView({
    behavior: reduceMotion ? 'auto' : 'smooth',
    block: 'start'
  })
}

watch(
  () => gallery.value.map((photo) => photo.id).join(','),
  async () => {
    activeIndex.value = 0
    visibleIndices.value = new Set([0])
    failedIndices.value = new Set()
    await nextTick()
    observeChapters()
  }
)

onMounted(() => {
  observeChapters()
  window.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  observer?.disconnect()
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <section v-if="gallery.length" ref="storyRoot" class="photo-story" aria-label="美好瞬间">
    <div class="photo-story__transition photo-story__transition--in" aria-hidden="true" />

    <article
      v-for="(photo, index) in gallery"
      :key="photo.id"
      :data-photo-chapter="index"
      class="photo-chapter"
      :class="{
        'is-visible': visibleIndices.has(index),
        'is-active': activeIndex === index,
        'has-image-error': failedIndices.has(index),
        [`photo-chapter--${photoCopyModes[index]}`]: true
      }"
      :aria-label="`美好瞬间，第 ${index + 1} 张，共 ${gallery.length} 张`"
    >
      <div class="photo-chapter__stage">
        <div
          class="photo-chapter__backdrop"
          :style="failedIndices.has(index) ? undefined : { backgroundImage: `url(${JSON.stringify(photo.url)})` }"
          aria-hidden="true"
        />
        <DecorativeMotes :count="6" tone="dark" />

        <p v-if="gallery.length > 1" class="photo-chapter__progress" aria-hidden="true">
          {{ String(index + 1).padStart(2, '0') }} / {{ String(gallery.length).padStart(2, '0') }}
        </p>

        <button
          type="button"
          class="photo-chapter__media"
          :aria-label="`全屏查看${photo.title ? `：${photo.title}` : `第 ${index + 1} 张婚礼照片`}`"
          @click="preview(index)"
        >
          <span class="photo-chapter__visual">
            <van-image
              :src="photo.url"
              fit="contain"
              :alt="photo.title || `第 ${index + 1} 张婚礼照片`"
              class="photo-chapter__image"
              @error="markImageFailed(index)"
            >
              <template #loading>
                <van-loading type="spinner" color="#fffaf4" size="24" />
              </template>
              <template #error>
                <span class="photo-chapter__error">
                  <span aria-hidden="true">◇</span>
                  <span>照片暂时无法加载</span>
                </span>
              </template>
            </van-image>
          </span>
        </button>

        <p v-if="index === 0 && gallery.length > 1" class="photo-chapter__hint" aria-hidden="true">
          <span>向上滑动</span>
          <span class="photo-chapter__hint-line" />
        </p>
      </div>

      <div
        v-if="photoCopyModes[index] !== 'none'"
        class="photo-chapter__copy"
        role="region"
        :aria-label="`第 ${index + 1} 张照片文字`"
        :tabindex="photoCopyModes[index] === 'description' ? 0 : undefined"
      >
        <div class="photo-chapter__copy-inner">
          <p v-if="index === 0" class="photo-chapter__eyebrow">美好瞬间</p>
          <h3 v-if="photo.title?.trim()" class="photo-chapter__title">{{ photo.title.trim() }}</h3>
          <p v-if="photo.description?.trim()" class="photo-chapter__description">
            {{ photo.description.trim() }}
          </p>
        </div>
      </div>
    </article>

    <div class="photo-story__transition photo-story__transition--out" aria-hidden="true" />
  </section>
</template>

<style scoped>
.photo-story {
  margin-top: var(--space-4);
  background: var(--wedding-bg);
}

.photo-story__transition {
  height: clamp(56px, 12vw, 88px);
}

.photo-story__transition--in {
  background: linear-gradient(180deg, var(--wedding-bg), #8f6259 48%, var(--wedding-cocoa));
}

.photo-story__transition--out {
  background: linear-gradient(180deg, var(--wedding-surface), #d6b7a5 52%, var(--wedding-bg));
}

.photo-chapter {
  position: relative;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-height: 100svh;
  overflow: hidden;
  color: var(--wedding-text);
  background: var(--wedding-surface);
}

.photo-chapter__stage {
  position: relative;
  display: grid;
  flex: 1 0 auto;
  place-items: center;
  min-height: clamp(430px, 72svh, 700px);
  padding: calc(var(--space-8) + env(safe-area-inset-top, 0px)) var(--wedding-page-x)
    var(--space-8);
  overflow: hidden;
  isolation: isolate;
  color: var(--wedding-text-invert);
  background: linear-gradient(155deg, var(--wedding-cocoa-soft), var(--wedding-cocoa) 68%);
}

.photo-chapter--none .photo-chapter__stage {
  min-height: 100vh;
  min-height: 100svh;
}

.photo-chapter__backdrop {
  position: absolute;
  z-index: -2;
  inset: -10%;
  background-position: center;
  background-size: cover;
  filter: blur(34px) saturate(0.68) brightness(0.53);
  opacity: 0.58;
  transform: scale(1.16);
  transition: opacity 700ms ease, transform 900ms cubic-bezier(0.2, 0.72, 0.22, 1);
}

.photo-chapter__stage::after {
  position: absolute;
  z-index: -1;
  inset: 0;
  content: '';
  pointer-events: none;
  background:
    radial-gradient(circle at 16% 16%, rgba(199, 163, 107, 0.15), transparent 30%),
    linear-gradient(180deg, rgba(47, 33, 30, 0.28), rgba(47, 33, 30, 0.72));
}

.photo-chapter__media {
  position: relative;
  z-index: 2;
  width: min(calc(100% - 16px), clamp(200px, calc((72svh - 150px) * 0.8), 360px));
  aspect-ratio: 4 / 5;
  padding: 7px;
  overflow: hidden;
  border: 1px solid rgba(234, 215, 181, 0.82);
  border-radius: 20px;
  color: inherit;
  background: var(--wedding-mat);
  box-shadow: var(--wedding-frame-shadow);
  cursor: zoom-in;
  opacity: 0;
  transform: translateY(18px) scale(0.985);
  transition: opacity 560ms ease, transform 680ms cubic-bezier(0.2, 0.72, 0.22, 1);
}

.photo-chapter__media::after {
  position: absolute;
  z-index: 4;
  inset: 0;
  content: '';
  pointer-events: none;
  background: linear-gradient(110deg, transparent 24%, rgba(255, 250, 244, 0.42) 46%, transparent 68%);
  transform: translateX(-120%);
}

.photo-chapter.is-visible .photo-chapter__media::after {
  animation: photo-frame-shimmer 900ms ease-out 440ms 1 both;
}

.photo-chapter__media:focus-visible {
  outline-color: var(--wedding-gold-soft);
}

.photo-chapter__visual {
  position: relative;
  display: block;
  width: 100%;
  height: 100%;
  overflow: hidden;
  border-radius: 13px;
  background: linear-gradient(145deg, #614740, var(--wedding-cocoa));
}

.photo-chapter__image,
.photo-chapter__image :deep(.van-image__img) {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.photo-chapter__image :deep(.van-image__loading),
.photo-chapter__image :deep(.van-image__error) {
  color: rgba(255, 250, 244, 0.88);
  background: transparent;
}

.photo-chapter__error {
  display: grid;
  justify-items: center;
  gap: var(--space-3);
  font-size: 13px;
  letter-spacing: 0.08em;
}

.photo-chapter__error > span:first-child {
  font-size: 32px;
  line-height: 1;
}

.photo-chapter__copy {
  position: relative;
  width: 100%;
  max-height: min(30svh, 236px);
  padding: var(--space-5) var(--wedding-page-x) calc(var(--space-6) + env(safe-area-inset-bottom, 0px));
  overflow-y: auto;
  overscroll-behavior: contain;
  scrollbar-gutter: stable;
  scrollbar-width: thin;
  -webkit-overflow-scrolling: touch;
  border-top: 1px solid var(--wedding-border);
  background: var(--wedding-surface);
  opacity: 0;
  transform: translateY(16px);
  transition: opacity 500ms ease 120ms, transform 620ms cubic-bezier(0.2, 0.72, 0.22, 1) 120ms;
}

.photo-chapter--title .photo-chapter__copy {
  padding-block: var(--space-4) calc(var(--space-5) + env(safe-area-inset-bottom, 0px));
}

.photo-chapter__copy-inner {
  width: min(100%, 376px);
  margin-inline: auto;
}

.photo-chapter__eyebrow {
  margin: 0 0 var(--space-1);
  color: var(--wedding-primary-dark);
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.24em;
}

.photo-chapter__title {
  margin: 0;
  color: var(--wedding-primary-deep);
  font-family: var(--wedding-font-display);
  font-size: clamp(21px, 6vw, 28px);
  font-weight: 600;
  line-height: 1.3;
  letter-spacing: 0.02em;
}

.photo-chapter__description {
  margin: var(--space-2) 0 0;
  color: var(--wedding-text-sub);
  font-size: clamp(14px, 3.6vw, 16px);
  line-height: 1.75;
  letter-spacing: 0.015em;
  white-space: pre-line;
}

.photo-chapter__progress {
  position: absolute;
  z-index: 4;
  top: calc(var(--space-4) + env(safe-area-inset-top, 0px));
  right: var(--wedding-page-x);
  margin: 0;
  color: rgba(255, 250, 244, 0.84);
  font-size: 11px;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0.16em;
}

.photo-chapter__hint {
  position: absolute;
  z-index: 4;
  right: var(--wedding-page-x);
  bottom: calc(var(--space-3) + env(safe-area-inset-bottom, 0px));
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin: 0;
  color: rgba(255, 250, 244, 0.76);
  font-size: 11px;
  letter-spacing: 0.15em;
}

.photo-chapter__hint-line {
  width: 32px;
  height: 1px;
  background: currentColor;
  transform-origin: right center;
}

.photo-chapter.is-visible .photo-chapter__backdrop {
  opacity: 0.72;
  transform: scale(1.1);
}

.photo-chapter.is-visible .photo-chapter__media,
.photo-chapter.is-visible .photo-chapter__copy {
  opacity: 1;
  transform: none;
}

@keyframes photo-frame-shimmer {
  to { transform: translateX(120%); }
}

@media (max-height: 620px) and (orientation: portrait) {
  .photo-chapter__stage {
    min-height: 72svh;
    padding-block: calc(var(--space-4) + env(safe-area-inset-top, 0px))
      var(--space-4);
  }

  .photo-chapter--none .photo-chapter__stage {
    min-height: 100svh;
  }

  .photo-chapter__media {
    width: min(calc(100% - 32px), calc((72svh - 100px) * 0.8));
  }

  .photo-chapter__copy {
    max-height: 28svh;
    padding-block: var(--space-3);
  }

  .photo-chapter__hint {
    display: none;
  }
}

@media (orientation: landscape) and (max-height: 620px) {
  .photo-chapter {
    display: grid;
    grid-template-columns: minmax(0, 0.92fr) minmax(0, 1.08fr);
    min-height: 100svh;
    background: var(--wedding-cocoa);
  }

  .photo-chapter__stage {
    grid-column: 1;
    min-height: 100svh;
    padding: calc(var(--space-4) + env(safe-area-inset-top, 0px)) var(--space-3)
      calc(var(--space-4) + env(safe-area-inset-bottom, 0px));
  }

  .photo-chapter__media {
    width: min(100%, calc((100svh - 96px) * 0.8));
    max-width: 190px;
  }

  .photo-chapter__copy {
    grid-column: 2;
    grid-row: 1;
    align-self: stretch;
    max-height: 100svh;
    padding: var(--space-5) var(--space-4);
  }

  .photo-chapter--none .photo-chapter__stage {
    grid-column: 1 / -1;
  }

  .photo-chapter__hint {
    display: none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .photo-chapter__backdrop,
  .photo-chapter__media,
  .photo-chapter__copy {
    transform: none;
    transition: opacity 180ms ease;
  }

  .photo-chapter__media::after {
    display: none;
    animation: none;
  }
}

@media (prefers-reduced-transparency: reduce) {
  .photo-chapter__backdrop {
    display: none;
  }

}

@media (prefers-contrast: more) {
  .photo-chapter__media {
    border: 2px solid var(--wedding-text-invert);
  }

  .photo-chapter__copy {
    border-top: 2px solid var(--wedding-text);
    background: var(--wedding-surface);
  }

  .photo-chapter__progress,
  .photo-chapter__hint {
    color: var(--wedding-text-invert);
  }
}
</style>

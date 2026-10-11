<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { showImagePreview } from 'vant'
import type { InvitePhoto } from '@/types'

const props = defineProps<{
  photos: InvitePhoto[]
  coverUrl?: string
}>()

// 无照片时用封面兜底，避免空区块
const gallery = computed<InvitePhoto[]>(() => {
  if (props.photos.length > 0) return props.photos
  if (props.coverUrl) {
    return [{ id: 0, url: props.coverUrl, title: '婚礼现场', sortOrder: 0 }]
  }
  return []
})

const previewImages = computed(() => gallery.value.map((item) => item.url))
const activeIndex = ref(0)

watch(
  () => gallery.value.length,
  () => {
    activeIndex.value = 0
  }
)

function preview(index: number): void {
  if (previewImages.value.length === 0) return
  showImagePreview({
    images: previewImages.value,
    startPosition: index,
    closeable: true,
    loop: false
  })
}
</script>

<template>
  <section v-if="gallery.length > 0" class="photo-wall card">
    <h2 class="section-title">美好瞬间</h2>

    <!-- 移动端：滑动轮播 -->
    <div class="photo-wall__swipe">
      <van-swipe
        :autoplay="0"
        :show-indicators="gallery.length > 1"
        indicator-color="#ffffff"
        lazy-render
        @change="(index: number) => (activeIndex = index)"
      >
        <van-swipe-item v-for="(photo, index) in gallery" :key="photo.id">
          <div class="photo-wall__slide" @click="preview(index)">
            <van-image :src="photo.url" fit="contain" radius="12" class="photo-wall__image">
              <template #loading>
                <van-loading type="spinner" color="#B4886B" size="20" />
              </template>
              <template #error>
                <div class="photo-wall__error">图片加载失败</div>
              </template>
            </van-image>
            <p v-if="photo.title" class="photo-wall__caption">{{ photo.title }}</p>
          </div>
        </van-swipe-item>
      </van-swipe>
    </div>

    <!-- 缩略图网格，点击全屏预览 -->
    <div class="photo-wall__grid">
      <button
        v-for="(photo, index) in gallery"
        :key="`thumb-${photo.id}`"
        type="button"
        class="photo-wall__thumb"
        :class="{ 'is-active': index === activeIndex }"
        @click="preview(index)"
      >
        <van-image :src="photo.url" fit="contain" radius="8" width="100%" height="100%" />
      </button>
    </div>

    <p class="photo-wall__hint">点击照片可全屏查看</p>
  </section>
</template>

<style scoped>
.photo-wall {
  padding-bottom: 16px;
}

.photo-wall__swipe {
  overflow: hidden;
  border-radius: 12px;
}

.photo-wall__slide {
  position: relative;
  padding: 0 2px;
}

.photo-wall__image {
  display: block;
  width: 100%;
  height: 320px;
  overflow: hidden;
  border-radius: 12px;
  background-color: var(--wedding-surface-soft);
}

.photo-wall__slide :deep(.van-image),
.photo-wall__slide :deep(img) {
  width: 100%;
  height: 320px;
  object-fit: contain;
}

.photo-wall__error {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 320px;
  color: var(--wedding-text-muted);
  font-size: 13px;
  background-color: var(--wedding-surface-soft);
}

.photo-wall__caption {
  position: absolute;
  left: 12px;
  bottom: 12px;
  max-width: 70%;
  margin: 0;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  color: var(--wedding-text-invert);
  background-color: rgba(74, 58, 48, 0.42);
  backdrop-filter: blur(4px);
}

.photo-wall__grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
  margin-top: 12px;
}

.photo-wall__thumb {
  padding: 0;
  width: 100%;
  aspect-ratio: 1 / 1;
  border: 2px solid transparent;
  border-radius: 10px;
  overflow: hidden;
  background-color: var(--wedding-surface-soft);
  transition: border-color 0.2s ease;
}

.photo-wall__thumb.is-active {
  border-color: var(--wedding-gold);
}

.photo-wall__hint {
  margin: 12px 0 0;
  text-align: center;
  font-size: 12px;
  color: var(--wedding-text-muted);
}
</style>

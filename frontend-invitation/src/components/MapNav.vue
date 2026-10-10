<script setup lang="ts">
import { computed } from 'vue'
import { showToast } from 'vant'
import type { InviteConfig } from '@/types'

const props = defineProps<{
  venue: string
  address: string
  weddingTime?: string
  config?: InviteConfig | null
}>()

const hasLocation = computed(() => Boolean(props.venue || props.address))

/** 高德搜索链接（按约定不使用经纬度，直接用详细地址搜索） */
const amapUrl = computed(
  () => `https://uri.amap.com/search?keyword=${encodeURIComponent(props.address)}&src=wedding`
)

async function copyAddress(): Promise<void> {
  const text = props.address
  if (!text) {
    showToast('暂无可复制的地址')
    return
  }
  try {
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(text)
    } else {
      // 非 https / 旧内核兜底
      const input = document.createElement('textarea')
      input.value = text
      input.setAttribute('readonly', '')
      input.style.position = 'fixed'
      input.style.opacity = '0'
      document.body.appendChild(input)
      input.select()
      document.execCommand('copy')
      document.body.removeChild(input)
    }
    showToast('地址已复制')
  } catch {
    showToast('复制失败，请长按地址手动复制')
  }
}

function openMap(): void {
  if (!props.address) {
    showToast('暂无地址信息')
    return
  }
  window.open(amapUrl.value, '_blank')
}
</script>

<template>
  <section v-if="hasLocation" class="map-nav card">
    <h2 class="section-title">婚礼地点</h2>

    <div class="map-nav__body">
      <p class="map-nav__venue">
        <van-icon name="location-o" class="map-nav__icon" />
        <span>{{ venue || '婚礼现场' }}</span>
      </p>
      <p class="map-nav__address">{{ address }}</p>
    </div>

    <div class="map-nav__actions">
      <button type="button" class="btn-gold" @click="openMap">
        <van-icon name="guide-o" />
        打开高德地图导航
      </button>
      <button type="button" class="btn-ghost" @click="copyAddress">
        <van-icon name="records-o" />
        复制地址
      </button>
    </div>
  </section>
</template>

<style scoped>
.map-nav__body {
  padding: 4px 0 14px;
}

.map-nav__venue {
  display: flex;
  align-items: flex-start;
  gap: 6px;
  margin: 0 0 6px;
  font-size: 16px;
  font-weight: 600;
  color: var(--wedding-text);
}

.map-nav__icon {
  margin-top: 3px;
  color: var(--wedding-primary);
}

.map-nav__address {
  margin: 0;
  padding-left: 22px;
  font-size: 14px;
  line-height: 1.7;
  color: var(--wedding-text-sub);
}

.map-nav__actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
</style>

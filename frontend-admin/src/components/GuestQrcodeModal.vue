<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  NAlert,
  NButton,
  NModal,
  NSkeleton,
  NSpace,
  NText,
  useMessage
} from 'naive-ui'
import QRCode from 'qrcode'
import { fetchGuestLink } from '@/api/guests'
import type { GuestDto } from '@/types'

const props = defineProps<{
  show: boolean
  guest: GuestDto | null
}>()

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void
}>()

const message = useMessage()
const loading = ref(false)
const linkUrl = ref('')
const qrcodeDataUrl = ref('')

const title = computed(() => `邀请二维码 - ${props.guest?.name ?? ''}`)
const fileName = computed(() => `邀请二维码-${props.guest?.name ?? 'guest'}.png`)

async function load() {
  if (!props.guest) return
  loading.value = true
  linkUrl.value = ''
  qrcodeDataUrl.value = ''
  try {
    const link = await fetchGuestLink(props.guest.id)
    linkUrl.value = link.url
    qrcodeDataUrl.value = await QRCode.toDataURL(link.url, {
      width: 512,
      margin: 2,
      errorCorrectionLevel: 'M',
      color: { dark: '#000000', light: '#ffffff' }
    })
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    loading.value = false
  }
}

watch(
  () => [props.show, props.guest?.id] as const,
  ([show]) => {
    if (show) load()
  },
  { immediate: true }
)

async function copyLink() {
  if (!linkUrl.value) return
  try {
    if (navigator.clipboard?.writeText) {
      await navigator.clipboard.writeText(linkUrl.value)
    } else {
      const input = document.createElement('input')
      input.value = linkUrl.value
      document.body.appendChild(input)
      input.select()
      document.execCommand('copy')
      document.body.removeChild(input)
    }
    message.success('链接已复制')
  } catch {
    message.error('复制失败，请手动选择链接复制')
  }
}

/** 另存为 PNG */
function downloadPng() {
  if (!qrcodeDataUrl.value) return
  const a = document.createElement('a')
  a.href = qrcodeDataUrl.value
  a.download = fileName.value
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  message.success('二维码已保存')
}

/** 打印二维码（新窗口，仅打印图片与链接） */
function printQrcode() {
  if (!qrcodeDataUrl.value) return
  const win = window.open('', '_blank', 'width=720,height=860')
  if (!win) {
    message.error('浏览器拦截了打印窗口，请允许弹窗后重试')
    return
  }
  const guestName = props.guest?.name ?? ''
  win.document.write(`<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8" />
  <title>邀请二维码 - ${guestName}</title>
  <style>
    body { font-family: "Microsoft YaHei", sans-serif; text-align: center; padding: 40px 20px; }
    img { width: 320px; height: 320px; }
    .name { font-size: 20px; font-weight: 600; margin-bottom: 16px; }
    .url { font-size: 12px; color: #666; word-break: break-all; margin-top: 12px; }
  </style>
</head>
<body>
  <div class="name">${guestName}</div>
  <img src="${qrcodeDataUrl.value}" alt="邀请二维码" />
  <div class="url">${linkUrl.value}</div>
  <script>window.onload = function () { window.print(); }<\/script>
</body>
</html>`)
  win.document.close()
}
</script>

<template>
  <n-modal
    :show="show"
    :title="title"
    preset="card"
    style="width: 460px; max-width: 94vw"
    @update:show="emit('update:show', $event)"
  >
    <n-skeleton v-if="loading" text :repeat="4" />
    <template v-else>
      <div v-if="qrcodeDataUrl" class="qrcode-box">
        <img :src="qrcodeDataUrl" alt="宾客邀请二维码" />
        <n-text class="link-text" depth="3">{{ linkUrl }}</n-text>
      </div>
      <n-alert v-else type="warning" :show-icon="false">
        未获取到邀请链接，请稍后重试。
      </n-alert>
    </template>

    <template #footer>
      <n-space justify="space-between" style="width: 100%">
        <n-button size="small" :loading="loading" @click="load">重新获取</n-button>
        <n-space>
          <n-button size="small" :disabled="!linkUrl" @click="copyLink">复制链接</n-button>
          <n-button size="small" type="primary" :disabled="!qrcodeDataUrl" @click="downloadPng">
            下载 PNG
          </n-button>
          <n-button size="small" secondary :disabled="!qrcodeDataUrl" @click="printQrcode">
            打印
          </n-button>
        </n-space>
      </n-space>
    </template>
  </n-modal>
</template>

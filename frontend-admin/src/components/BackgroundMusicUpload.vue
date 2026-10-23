<script setup lang="ts">
import { ref } from 'vue'
import { NButton, NIcon, NUpload, useMessage, type UploadCustomRequestOptions } from 'naive-ui'
import { MusicalNotesOutline } from '@vicons/ionicons5'
import { uploadBackgroundMusic } from '@/api/config'
import type { BackgroundMusicUploadResult } from '@/types'

const emit = defineEmits<{
  (e: 'uploaded', music: BackgroundMusicUploadResult): void
}>()

const message = useMessage()
const uploading = ref(false)
const maxSizeMb = 15

function isSupported(file: File): boolean {
  return /\.(mp3|m4a)$/i.test(file.name)
}

async function customRequest({ file, onFinish, onError }: UploadCustomRequestOptions) {
  const raw = file.file
  if (!raw) {
    onError()
    return
  }
  if (!isSupported(raw)) {
    message.error('仅支持 mp3 / m4a 格式的背景音乐')
    onError()
    return
  }
  if (raw.size > maxSizeMb * 1024 * 1024) {
    message.error(`背景音乐不能超过 ${maxSizeMb}MB`)
    onError()
    return
  }

  uploading.value = true
  try {
    const music = await uploadBackgroundMusic(raw)
    emit('uploaded', music)
    message.success('背景音乐已上传，请记得保存配置')
    onFinish()
  } catch {
    onError()
  } finally {
    uploading.value = false
  }
}
</script>

<template>
  <n-upload
    accept=".mp3,.m4a,audio/mpeg,audio/mp4"
    :max="1"
    :show-file-list="false"
    :disabled="uploading"
    :custom-request="customRequest"
  >
    <n-button secondary type="primary" :loading="uploading">
      <template #icon>
        <n-icon><MusicalNotesOutline /></n-icon>
      </template>
      上传背景音乐
    </n-button>
  </n-upload>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import {
  NButton,
  NIcon,
  NUpload,
  useMessage,
  type UploadCustomRequestOptions
} from 'naive-ui'
import { CloudUploadOutline } from '@vicons/ionicons5'
import { uploadPhoto } from '@/api/photos'
import type { PhotoDto } from '@/types'

const props = withDefaults(
  defineProps<{
    /** 接受的图片类型 */
    accept?: string
    /** 最大体积（MB） */
    maxSizeMb?: number
    /** 是否多选 */
    multiple?: boolean
    /** 按钮文案 */
    text?: string
    /** 是否使用拖拽区域样式 */
    dragger?: boolean
  }>(),
  {
    accept: '.jpg,.jpeg,.png,.webp',
    maxSizeMb: 10,
    multiple: false,
    text: '上传图片',
    dragger: false
  }
)

const emit = defineEmits<{
  (e: 'uploaded', photo: PhotoDto): void
}>()

const message = useMessage()
const uploading = ref(false)

const ALLOWED = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']

async function customRequest({ file, onFinish, onError }: UploadCustomRequestOptions) {
  const raw = file.file
  if (!raw) {
    onError()
    return
  }
  if (raw.type && !ALLOWED.includes(raw.type)) {
    message.error('仅支持 jpg / jpeg / png / webp 格式的图片')
    onError()
    return
  }
  if (raw.size > props.maxSizeMb * 1024 * 1024) {
    message.error(`图片体积不能超过 ${props.maxSizeMb}MB`)
    onError()
    return
  }

  uploading.value = true
  try {
    const photo = await uploadPhoto(raw)
    message.success('上传成功')
    emit('uploaded', photo)
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
    :accept="accept"
    :max="multiple ? 99 : 1"
    :multiple="multiple"
    :show-file-list="false"
    :disabled="uploading"
    :custom-request="customRequest"
  >
    <slot>
      <n-button :loading="uploading" secondary type="primary">
        <template #icon>
          <n-icon><CloudUploadOutline /></n-icon>
        </template>
        {{ text }}
      </n-button>
    </slot>
  </n-upload>
</template>

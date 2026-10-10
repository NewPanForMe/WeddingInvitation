<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  NAlert,
  NButton,
  NIcon,
  NModal,
  NSpace,
  NStatistic,
  NTag,
  NText,
  NUpload,
  NUploadDragger,
  useMessage,
  type UploadCustomRequestOptions
} from 'naive-ui'
import { CloudUploadOutline } from '@vicons/ionicons5'
import { importGuests } from '@/api/guests'
import type { GuestImportResult } from '@/types'

const props = defineProps<{ show: boolean }>()

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void
  (e: 'imported'): void
}>()

const message = useMessage()
const uploading = ref(false)
const result = ref<GuestImportResult | null>(null)

const hasResult = computed(() => !!result.value)

watch(
  () => props.show,
  (show) => {
    if (show) result.value = null
  }
)

/** n-upload 自定义请求：直接调用导入接口（multipart/form-data，字段名 file） */
async function customRequest({ file, onFinish, onError }: UploadCustomRequestOptions) {
  const raw = file.file
  if (!raw) {
    onError()
    return
  }
  uploading.value = true
  try {
    result.value = await importGuests(raw)
    message.success(`导入完成：成功 ${result.value.imported} 条，跳过 ${result.value.skipped} 条`)
    emit('imported')
    onFinish()
  } catch {
    onError()
  } finally {
    uploading.value = false
  }
}

function close() {
  emit('update:show', false)
}
</script>

<template>
  <n-modal
    :show="show"
    title="批量导入宾客（CSV）"
    preset="card"
    style="width: 620px; max-width: 94vw"
    :mask-closable="false"
    @update:show="emit('update:show', $event)"
  >
    <n-space vertical :size="14">
      <n-alert type="info" :show-icon="false">
        <div>1. 文件需为 UTF-8 编码的 CSV；</div>
        <div>2. 列顺序：姓名,称呼,电话,关系,桌号,可带人数（表头行以「姓名」开头会自动跳过）；</div>
        <div>3. 姓名为必填，其余列可为空。</div>
      </n-alert>

      <n-upload
        accept=".csv,text/csv"
        :max="1"
        :show-file-list="false"
        :custom-request="customRequest"
        :disabled="uploading"
      >
        <n-upload-dragger>
          <div style="padding: 16px 0">
            <n-icon size="40" :depth="3">
              <CloudUploadOutline />
            </n-icon>
            <div style="margin-top: 8px">点击或拖拽 CSV 文件到此处，选择后立即上传导入</div>
            <n-text depth="3" style="font-size: 12px">
              支持 UTF-8 CSV，建议不超过 5000 行
            </n-text>
          </div>
        </n-upload-dragger>
      </n-upload>

      <n-space v-if="uploading" align="center">
        <n-text depth="3">正在导入，请稍候…</n-text>
      </n-space>

      <template v-if="hasResult && !uploading">
        <n-space :size="20">
          <n-statistic label="总行数" :value="result!.total" />
          <n-statistic label="导入成功" :value="result!.imported" />
          <n-statistic label="跳过" :value="result!.skipped" />
        </n-space>

        <n-alert v-if="result!.errors?.length" type="warning" title="部分行未导入">
          <div
            v-for="(err, index) in result!.errors"
            :key="index"
            style="font-size: 12px; line-height: 1.8"
          >
            · {{ err }}
          </div>
        </n-alert>
        <n-tag v-else type="success" size="small">全部行处理成功</n-tag>
      </template>
    </n-space>

    <template #footer>
      <n-space justify="end">
        <n-button @click="close">关闭</n-button>
      </n-space>
    </template>
  </n-modal>
</template>

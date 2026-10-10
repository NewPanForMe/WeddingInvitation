<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import {
  NAlert,
  NButton,
  NCard,
  NDatePicker,
  NDivider,
  NForm,
  NFormItem,
  NGrid,
  NGi,
  NImage,
  NInput,
  NSpin,
  NSpace,
  NText,
  useMessage,
  type FormInst,
  type FormRules
} from 'naive-ui'
import PhotoUpload from '@/components/PhotoUpload.vue'
import { fetchConfig, updateConfig } from '@/api/config'
import { formatDateTime, isoToTimestamp, timestampToIso } from '@/utils/format'
import type { WeddingConfig } from '@/types'

const message = useMessage()
const formRef = ref<FormInst | null>(null)
const loading = ref(false)
const saving = ref(false)
const loaded = ref(false)

/** 表单内部用毫秒时间戳，提交时统一转 ISO */
const form = reactive<{
  groomName: string
  brideName: string
  weddingTime: number | null
  venue: string
  address: string
  coverUrl: string
  rsvpDeadline: number | null
}>({
  groomName: '',
  brideName: '',
  weddingTime: null,
  venue: '',
  address: '',
  coverUrl: '',
  rsvpDeadline: null
})

const rules: FormRules = {
  groomName: [{ required: true, message: '请输入新郎姓名', trigger: ['input', 'blur'] }],
  brideName: [{ required: true, message: '请输入新娘姓名', trigger: ['input', 'blur'] }],
  weddingTime: [
    { type: 'number', required: true, message: '请选择婚礼时间', trigger: ['change', 'blur'] }
  ],
  venue: [{ required: true, message: '请输入婚礼场所', trigger: ['input', 'blur'] }],
  address: [{ required: true, message: '请输入详细地址', trigger: ['input', 'blur'] }]
}

function applyConfig(cfg: WeddingConfig) {
  form.groomName = cfg.groomName ?? ''
  form.brideName = cfg.brideName ?? ''
  form.weddingTime = isoToTimestamp(cfg.weddingTime)
  form.venue = cfg.venue ?? ''
  form.address = cfg.address ?? ''
  form.coverUrl = cfg.coverUrl ?? ''
  form.rsvpDeadline = isoToTimestamp(cfg.rsvpDeadline)
}

async function loadData() {
  loading.value = true
  try {
    applyConfig(await fetchConfig())
    loaded.value = true
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }
  saving.value = true
  try {
    const saved = await updateConfig({
      groomName: form.groomName.trim(),
      brideName: form.brideName.trim(),
      weddingTime: timestampToIso(form.weddingTime),
      venue: form.venue.trim(),
      address: form.address.trim(),
      coverUrl: form.coverUrl.trim(),
      rsvpDeadline: timestampToIso(form.rsvpDeadline)
    })
    applyConfig(saved)
    message.success('婚礼配置已保存')
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    saving.value = false
  }
}

function handleCoverUploaded(photo: { url: string }) {
  form.coverUrl = photo.url
  message.success('封面已上传，请记得保存配置')
}

onMounted(loadData)
</script>

<template>
  <div class="page-container">
    <n-card :bordered="false" size="small" title="婚礼配置">
      <template #header-extra>
        <n-button size="small" quaternary :loading="loading" @click="loadData">重新加载</n-button>
      </template>

      <n-spin :show="loading">
        <n-alert v-if="loaded && form.weddingTime" type="info" :show-icon="false" style="margin-bottom: 16px">
          当前展示（北京时间）：婚礼 {{ formatDateTime(form.weddingTime) }} ·
          RSVP 截止 {{ formatDateTime(form.rsvpDeadline) }}
        </n-alert>

        <n-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-placement="top"
          require-mark-placement="right-hanging"
          style="max-width: 860px"
        >
          <n-grid :cols="24" :x-gap="20" item-responsive>
            <n-gi span="24 s:12">
              <n-form-item label="新郎姓名" path="groomName">
                <n-input v-model:value="form.groomName" placeholder="例如：李四" clearable />
              </n-form-item>
            </n-gi>
            <n-gi span="24 s:12">
              <n-form-item label="新娘姓名" path="brideName">
                <n-input v-model:value="form.brideName" placeholder="例如：王五" clearable />
              </n-form-item>
            </n-gi>
          </n-grid>

          <n-grid :cols="24" :x-gap="20" item-responsive>
            <n-gi span="24 s:12">
              <n-form-item label="婚礼时间" path="weddingTime">
                <n-date-picker
                  v-model:value="form.weddingTime"
                  type="datetime"
                  clearable
                  format="yyyy-MM-dd HH:mm"
                  placeholder="选择婚礼时间"
                  style="width: 100%"
                />
              </n-form-item>
            </n-gi>
            <n-gi span="24 s:12">
              <n-form-item label="RSVP 回复截止时间" path="rsvpDeadline">
                <n-date-picker
                  v-model:value="form.rsvpDeadline"
                  type="datetime"
                  clearable
                  format="yyyy-MM-dd HH:mm"
                  placeholder="选择回复截止时间"
                  style="width: 100%"
                />
              </n-form-item>
            </n-gi>
          </n-grid>
          <n-text depth="3" class="text-muted">
            时间选择器按本机时区录入，保存时自动转换为带时区的 ISO 字符串；展示统一为北京时间。
          </n-text>

          <n-divider />

          <n-form-item label="婚礼场所" path="venue">
            <n-input v-model:value="form.venue" placeholder="例如：XX酒店 3楼宴会厅" clearable />
          </n-form-item>

          <n-form-item label="详细地址" path="address">
            <n-input
              v-model:value="form.address"
              type="textarea"
              :autosize="{ minRows: 2, maxRows: 4 }"
              placeholder="例如：XX市XX区XX路88号"
            />
          </n-form-item>

          <n-form-item label="封面图">
            <n-space vertical :size="10" style="width: 100%">
              <n-input v-model:value="form.coverUrl" placeholder="例如：/uploads/cover.jpg" clearable />
              <n-space align="center" :size="12">
                <PhotoUpload text="上传封面图" @uploaded="handleCoverUploaded" />
                <n-text depth="3" class="text-muted">jpg / jpeg / png / webp，≤ 10MB</n-text>
              </n-space>
              <n-image
                v-if="form.coverUrl"
                :src="form.coverUrl"
                width="240"
                object-fit="cover"
                style="border-radius: 8px"
              />
            </n-space>
          </n-form-item>

          <n-space justify="end">
            <n-button @click="loadData">重置</n-button>
            <n-button type="primary" :loading="saving" @click="handleSave">保存配置</n-button>
          </n-space>
        </n-form>
      </n-spin>
    </n-card>
  </div>
</template>

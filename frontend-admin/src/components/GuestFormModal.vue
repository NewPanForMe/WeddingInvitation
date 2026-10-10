<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import {
  NButton,
  NForm,
  NFormItem,
  NInput,
  NInputNumber,
  NModal,
  NSpace,
  useMessage,
  type FormInst,
  type FormRules
} from 'naive-ui'
import { createGuest, updateGuest } from '@/api/guests'
import type { GuestDto, GuestUpsertPayload } from '@/types'

const props = defineProps<{
  show: boolean
  /** 传入表示编辑，null 表示新增 */
  guest: GuestDto | null
}>()

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void
  (e: 'saved', guest: GuestDto): void
}>()

const message = useMessage()
const formRef = ref<FormInst | null>(null)
const submitting = ref(false)

const model = reactive<{
  name: string
  salutation: string
  phone: string
  relation: string
  tableNo: string
  maxGuests: number | null
}>({
  name: '',
  salutation: '',
  phone: '',
  relation: '',
  tableNo: '',
  maxGuests: 1
})

const isEdit = computed(() => !!props.guest)
const title = computed(() => (isEdit.value ? `编辑宾客 - ${props.guest?.name ?? ''}` : '新增宾客'))

const rules: FormRules = {
  name: [{ required: true, message: '请输入姓名', trigger: ['input', 'blur'] }],
  maxGuests: [
    {
      type: 'number',
      min: 0,
      max: 99,
      message: '可带人数需为 0 ~ 99 之间的数字',
      trigger: ['input', 'blur']
    }
  ]
}

function resetForm() {
  const g = props.guest
  model.name = g?.name ?? ''
  model.salutation = g?.salutation ?? ''
  model.phone = g?.phone ?? ''
  model.relation = g?.relation ?? ''
  model.tableNo = g?.tableNo ?? ''
  model.maxGuests = g?.maxGuests ?? 1
  formRef.value?.restoreValidation()
}

watch(
  () => [props.show, props.guest] as const,
  ([show]) => {
    if (show) resetForm()
  },
  { immediate: true }
)

function close() {
  emit('update:show', false)
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  const payload: GuestUpsertPayload = {
    name: model.name.trim(),
    salutation: model.salutation.trim() || null,
    phone: model.phone.trim() || null,
    relation: model.relation.trim() || null,
    tableNo: model.tableNo.trim() || null,
    maxGuests: model.maxGuests ?? null
  }

  submitting.value = true
  try {
    const saved = props.guest
      ? await updateGuest(props.guest.id, payload)
      : await createGuest(payload)
    message.success(isEdit.value ? '宾客信息已更新' : '宾客已新增')
    emit('saved', saved)
    close()
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <n-modal
    :show="show"
    :title="title"
    preset="card"
    style="width: 560px; max-width: 94vw"
    :mask-closable="false"
    @update:show="emit('update:show', $event)"
  >
    <n-form
      ref="formRef"
      :model="model"
      :rules="rules"
      label-placement="left"
      label-width="88"
      require-mark-placement="right-hanging"
    >
      <n-form-item label="姓名" path="name">
        <n-input v-model:value="model.name" placeholder="例如：张三" clearable />
      </n-form-item>
      <n-form-item label="称呼" path="salutation">
        <n-input v-model:value="model.salutation" placeholder="例如：亲爱的张三" clearable />
      </n-form-item>
      <n-form-item label="电话" path="phone">
        <n-input v-model:value="model.phone" placeholder="例如：13800000000" clearable />
      </n-form-item>
      <n-form-item label="关系" path="relation">
        <n-input v-model:value="model.relation" placeholder="例如：同学 / 亲戚 / 同事" clearable />
      </n-form-item>
      <n-form-item label="桌号" path="tableNo">
        <n-input v-model:value="model.tableNo" placeholder="例如：A区 8号桌" clearable />
      </n-form-item>
      <n-form-item label="可带人数" path="maxGuests">
        <n-input-number
          v-model:value="model.maxGuests"
          :min="0"
          :max="99"
          placeholder="可携带的同行人数"
          style="width: 200px"
        />
      </n-form-item>
    </n-form>

    <template #footer>
      <n-space justify="end">
        <n-button @click="close">取消</n-button>
        <n-button type="primary" :loading="submitting" @click="handleSubmit">
          {{ isEdit ? '保存' : '创建' }}
        </n-button>
      </n-space>
    </template>
  </n-modal>
</template>

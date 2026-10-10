<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  NAlert,
  NButton,
  NCard,
  NForm,
  NFormItem,
  NInput,
  NSpace,
  useMessage,
  type FormInst,
  type FormRules
} from 'naive-ui'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const message = useMessage()
const auth = useAuthStore()

const formRef = ref<FormInst | null>(null)
const loading = ref(false)

const model = reactive({
  username: 'admin',
  password: 'admin123'
})

const rules: FormRules = {
  username: [{ required: true, message: '请输入用户名', trigger: ['input', 'blur'] }],
  password: [{ required: true, message: '请输入密码', trigger: ['input', 'blur'] }]
}

async function handleLogin() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }
  loading.value = true
  try {
    await auth.login({ username: model.username, password: model.password })
    message.success('登录成功')
    const redirect = (route.query.redirect as string | undefined) ?? '/dashboard'
    router.replace(redirect)
  } catch {
    // 错误提示由 http 响应拦截器统一处理
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <n-card class="login-card" title="婚礼请柬后台管理" :bordered="false" size="large">
      <template #header-extra>
        <span style="font-size: 20px">💍</span>
      </template>

      <n-form
        ref="formRef"
        :model="model"
        :rules="rules"
        label-placement="top"
        size="large"
        @keyup.enter="handleLogin"
      >
        <n-form-item label="用户名" path="username">
          <n-input v-model:value="model.username" placeholder="请输入用户名" clearable />
        </n-form-item>
        <n-form-item label="密码" path="password">
          <n-input
            v-model:value="model.password"
            type="password"
            show-password-on="click"
            placeholder="请输入密码"
          />
        </n-form-item>

        <n-space vertical :size="12">
          <n-button type="primary" block :loading="loading" @click="handleLogin">
            登 录
          </n-button>
          <n-alert type="info" :show-icon="false" size="small">
            默认管理员账号：<b>admin</b> / 密码：<b>admin123</b>
          </n-alert>
        </n-space>
      </n-form>
    </n-card>
  </div>
</template>

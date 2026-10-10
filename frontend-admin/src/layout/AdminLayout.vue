<script setup lang="ts">
import { computed, h, reactive, ref, type Component } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import {
  NAvatar,
  NButton,
  NForm,
  NFormItem,
  NIcon,
  NInput,
  NLayout,
  NLayoutContent,
  NLayoutHeader,
  NLayoutSider,
  NMenu,
  NModal,
  NPopconfirm,
  NSpace,
  NText,
  useMessage,
  type MenuOption
} from 'naive-ui'
import {
  BarChartOutline,
  CalendarOutline,
  ImagesOutline,
  KeyOutline,
  LogOutOutline,
  PeopleOutline,
  SettingsOutline
} from '@vicons/ionicons5'
import { changePassword } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'
import { formatDateTime } from '@/utils/format'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const auth = useAuthStore()

const collapsed = ref(false)

function renderIcon(icon: Component) {
  return () => h(NIcon, null, { default: () => h(icon) })
}

function renderLink(to: string, label: string) {
  return () => h(RouterLink, { to }, { default: () => label })
}

const menuOptions: MenuOption[] = [
  {
    label: renderLink('/dashboard', '数据概览'),
    key: 'dashboard',
    icon: renderIcon(BarChartOutline)
  },
  {
    label: renderLink('/guests', '宾客管理'),
    key: 'guests',
    icon: renderIcon(PeopleOutline)
  },
  {
    label: renderLink('/photos', '照片管理'),
    key: 'photos',
    icon: renderIcon(ImagesOutline)
  },
  {
    label: renderLink('/config', '婚礼配置'),
    key: 'config',
    icon: renderIcon(SettingsOutline)
  },
  {
    label: renderLink('/statistics', '统计报表'),
    key: 'statistics',
    icon: renderIcon(CalendarOutline)
  }
]

const activeKey = computed(() => (route.name as string) ?? 'dashboard')
const currentTitle = computed(() => (route.meta.title as string | undefined) ?? '后台管理')

async function handleLogout() {
  await auth.logout()
  message.success('已退出登录')
  router.replace('/login')
}

/* 修改密码 */
const pwdModal = reactive({
  show: false,
  saving: false,
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

async function handleChangePassword() {
  if (!pwdModal.oldPassword || !pwdModal.newPassword) {
    message.warning('请填写原密码与新密码')
    return
  }
  if (pwdModal.newPassword.length < 6) {
    message.warning('新密码至少 6 位')
    return
  }
  if (pwdModal.newPassword !== pwdModal.confirmPassword) {
    message.warning('两次输入的新密码不一致')
    return
  }
  pwdModal.saving = true
  try {
    const result = await changePassword({
      oldPassword: pwdModal.oldPassword,
      newPassword: pwdModal.newPassword
    })
    message.success(result.message || '密码修改成功')
    pwdModal.show = false
    pwdModal.oldPassword = ''
    pwdModal.newPassword = ''
    pwdModal.confirmPassword = ''
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    pwdModal.saving = false
  }
}
</script>

<template>
  <n-layout has-sider style="height: 100vh">
    <n-layout-sider
      bordered
      collapse-mode="width"
      :collapsed-width="64"
      :width="220"
      :collapsed="collapsed"
      show-trigger
      @collapse="collapsed = true"
      @expand="collapsed = false"
    >
      <div class="brand" :class="{ 'is-collapsed': collapsed }">
        <span v-if="!collapsed">💍 婚礼请柬后台</span>
        <span v-else>💍</span>
      </div>
      <n-menu
        :value="activeKey"
        :collapsed="collapsed"
        :collapsed-width="64"
        :collapsed-icon-size="22"
        :options="menuOptions"
      />
    </n-layout-sider>

    <n-layout>
      <n-layout-header bordered class="admin-header">
        <div class="header-left">
          <h2 class="page-title">{{ currentTitle }}</h2>
        </div>
        <div class="header-right">
          <n-space align="center" :size="12">
            <n-avatar round size="small">{{ auth.username.slice(0, 1).toUpperCase() || 'A' }}</n-avatar>
            <div class="admin-meta">
              <n-text strong>{{ auth.username || 'admin' }}</n-text>
              <div class="text-muted">
                有效期至 {{ formatDateTime(auth.expiresAt) }}
              </div>
            </div>
            <n-button size="small" quaternary @click="pwdModal.show = true">
              <template #icon>
                <n-icon><KeyOutline /></n-icon>
              </template>
              修改密码
            </n-button>
            <n-popconfirm @positive-click="handleLogout">
              <template #trigger>
                <n-button size="small" quaternary type="error">
                  <template #icon>
                    <n-icon><LogOutOutline /></n-icon>
                  </template>
                  退出登录
                </n-button>
              </template>
              确认退出当前管理员账号？
            </n-popconfirm>
          </n-space>
        </div>
      </n-layout-header>

      <n-layout-content content-style="min-height: calc(100vh - 64px);">
        <router-view />
      </n-layout-content>
    </n-layout>

    <n-modal
      v-model:show="pwdModal.show"
      title="修改管理员密码"
      preset="card"
      style="width: 460px; max-width: 94vw"
      :mask-closable="false"
    >
      <n-form label-placement="left" label-width="90">
        <n-form-item label="原密码">
          <n-input
            v-model:value="pwdModal.oldPassword"
            type="password"
            show-password-on="click"
            placeholder="请输入当前密码"
          />
        </n-form-item>
        <n-form-item label="新密码">
          <n-input
            v-model:value="pwdModal.newPassword"
            type="password"
            show-password-on="click"
            placeholder="至少 6 位"
          />
        </n-form-item>
        <n-form-item label="确认新密码">
          <n-input
            v-model:value="pwdModal.confirmPassword"
            type="password"
            show-password-on="click"
            placeholder="再次输入新密码"
          />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="pwdModal.show = false">取消</n-button>
          <n-button type="primary" :loading="pwdModal.saving" @click="handleChangePassword">
            确认修改
          </n-button>
        </n-space>
      </template>
    </n-modal>
  </n-layout>
</template>

<style scoped>
.brand {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  padding: 0 20px;
  font-weight: 600;
  font-size: 15px;
  color: #d03050;
  border-bottom: 1px solid #efeff5;
  white-space: nowrap;
  overflow: hidden;
}

.brand.is-collapsed {
  justify-content: center;
  padding: 0;
}

.admin-header {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  background: #fff;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-right {
  display: flex;
  align-items: center;
}

.admin-meta {
  line-height: 1.2;
}

.admin-meta .text-muted {
  font-size: 12px;
}
</style>

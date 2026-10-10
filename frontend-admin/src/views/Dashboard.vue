<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  NAlert,
  NButton,
  NCard,
  NGi,
  NGrid,
  NIcon,
  NProgress,
  NSkeleton,
  NSpace,
  NStatistic,
  NTag
} from 'naive-ui'
import {
  CheckmarkCircleOutline,
  CloseCircleOutline,
  PeopleOutline,
  PersonAddOutline,
  TimeOutline
} from '@vicons/ionicons5'
import { fetchStatistics } from '@/api/statistics'
import { fetchConfig } from '@/api/config'
import { formatDateTime } from '@/utils/format'
import type { StatisticsDto, WeddingConfig } from '@/types'

const loading = ref(true)
const stats = ref<StatisticsDto | null>(null)
const config = ref<WeddingConfig | null>(null)
const loadError = ref('')

const responsePercent = computed(() =>
  stats.value ? Math.round((stats.value.responseRate ?? 0) * 1000) / 10 : 0
)

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    const [statResult, configResult] = await Promise.allSettled([fetchStatistics(), fetchConfig()])
    stats.value = statResult.status === 'fulfilled' ? statResult.value : null
    config.value = configResult.status === 'fulfilled' ? configResult.value : null
    if (statResult.status === 'rejected' || configResult.status === 'rejected') {
      loadError.value = '数据加载失败，请刷新或重新登录后再试'
    }
  } finally {
    loading.value = false
  }
}

onMounted(load)

const cards = computed(() => [
  {
    key: 'total',
    label: '总宾客数',
    value: stats.value?.totalGuests ?? 0,
    icon: PeopleOutline,
    color: '#2080f0'
  },
  {
    key: 'responded',
    label: '已回复',
    value: stats.value?.responded ?? 0,
    icon: CheckmarkCircleOutline,
    color: '#18a058'
  },
  {
    key: 'attending',
    label: '确认出席',
    value: stats.value?.attending ?? 0,
    icon: CheckmarkCircleOutline,
    color: '#0e7a0d'
  },
  {
    key: 'declined',
    label: '不能出席',
    value: stats.value?.declined ?? 0,
    icon: CloseCircleOutline,
    color: '#d03050'
  },
  {
    key: 'pending',
    label: '待回复',
    value: stats.value?.pending ?? 0,
    icon: TimeOutline,
    color: '#f0a020'
  },
  {
    key: 'expected',
    label: '预计到场人数',
    value: stats.value?.expectedAttendees ?? 0,
    icon: PersonAddOutline,
    color: '#8a2be2'
  }
])

const shortcuts = [
  { label: '宾客管理', to: '/guests', type: 'primary' as const },
  { label: '导入名单 (CSV)', to: '/guests', type: 'info' as const },
  { label: '照片管理', to: '/photos', type: 'success' as const },
  { label: '婚礼配置', to: '/config', type: 'warning' as const },
  { label: '统计报表', to: '/statistics', type: 'default' as const }
]
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h2 class="page-title">数据概览</h2>
      <n-button size="small" :loading="loading" @click="load">刷新</n-button>
    </div>

    <n-alert v-if="loadError" type="error" :show-icon="false" style="margin-bottom: 16px">
      {{ loadError }}
    </n-alert>

    <n-alert v-if="config" type="success" :show-icon="false" style="margin-bottom: 16px">
      <n-space align="center" :size="16" wrap>
        <span>🎊 {{ config.groomName }} &amp; {{ config.brideName }}</span>
        <span>婚礼时间：{{ formatDateTime(config.weddingTime) }}</span>
        <span>地点：{{ config.venue }}</span>
        <span>RSVP 截止：{{ formatDateTime(config.rsvpDeadline) }}</span>
      </n-space>
    </n-alert>

    <n-grid :cols="24" :x-gap="16" :y-gap="16" responsive="screen" item-responsive>
      <n-gi v-for="card in cards" :key="card.key" span="24 s:12 m:8 l:4">
        <n-card size="small" :bordered="false" class="stat-card">
          <n-skeleton v-if="loading" text :repeat="2" />
          <template v-else>
            <n-space align="center" :size="10">
              <n-icon :size="26" :color="card.color">
                <component :is="card.icon" />
              </n-icon>
              <n-statistic :label="card.label" :value="card.value" />
            </n-space>
          </template>
        </n-card>
      </n-gi>
    </n-grid>

    <n-card title="回复进度" size="small" :bordered="false" style="margin-top: 16px">
      <n-skeleton v-if="loading" text :repeat="3" />
      <template v-else>
        <n-progress
          type="line"
          :percentage="responsePercent"
          :height="18"
          :border-radius="6"
          indicator-placement="inside"
          processing
        />
        <n-space justify="space-between" style="margin-top: 10px">
          <span class="text-muted">
            已回复 {{ stats?.responded ?? 0 }} / {{ stats?.totalGuests ?? 0 }} 位宾客
          </span>
          <n-space :size="8">
            <n-tag size="small" type="success">出席 {{ stats?.attending ?? 0 }}</n-tag>
            <n-tag size="small" type="error">不出席 {{ stats?.declined ?? 0 }}</n-tag>
            <n-tag size="small" type="warning">待回复 {{ stats?.pending ?? 0 }}</n-tag>
          </n-space>
        </n-space>
      </template>
    </n-card>

    <n-card title="快捷入口" size="small" :bordered="false" style="margin-top: 16px">
      <n-space :size="12" wrap>
        <router-link v-for="item in shortcuts" :key="item.label" :to="item.to">
          <n-button :type="item.type" secondary>{{ item.label }}</n-button>
        </router-link>
      </n-space>
    </n-card>
  </div>
</template>

<style scoped>
.stat-card {
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}
</style>

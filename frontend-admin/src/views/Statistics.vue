<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  NCard,
  NGi,
  NGrid,
  NProgress,
  NSkeleton,
  NSpace,
  NStatistic,
  NTable,
  NTag
} from 'naive-ui'
import { fetchStatistics } from '@/api/statistics'
import { formatPercent } from '@/utils/format'
import type { StatisticsDto } from '@/types'

const loading = ref(true)
const stats = ref<StatisticsDto | null>(null)

const responsePercent = computed(() =>
  stats.value ? Math.round((stats.value.responseRate ?? 0) * 1000) / 10 : 0
)

const attendPercent = computed(() => {
  const s = stats.value
  if (!s || !s.responded) return 0
  return Math.round((s.attending / s.responded) * 1000) / 10
})

const rows = computed(() => {
  const s = stats.value
  const total = s?.totalGuests || 0
  const item = (label: string, value: number, type: 'default' | 'success' | 'error' | 'warning') => ({
    label,
    value,
    percent: total ? Math.round((value / total) * 1000) / 10 : 0,
    type
  })
  return [
    item('确认出席', s?.attending ?? 0, 'success'),
    item('不能出席', s?.declined ?? 0, 'error'),
    item('待回复', s?.pending ?? 0, 'warning')
  ]
})

const cards = computed(() => [
  { label: '总宾客数', value: stats.value?.totalGuests ?? 0 },
  { label: '已回复', value: stats.value?.responded ?? 0 },
  { label: '确认出席', value: stats.value?.attending ?? 0 },
  { label: '不能出席', value: stats.value?.declined ?? 0 },
  { label: '待回复', value: stats.value?.pending ?? 0 },
  { label: '预计到场人数', value: stats.value?.expectedAttendees ?? 0 }
])

async function load() {
  loading.value = true
  try {
    stats.value = await fetchStatistics()
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h2 class="page-title">统计报表</h2>
      <n-space align="center" :size="12">
        <n-tag size="small" :bordered="false">回复率 {{ formatPercent(stats?.responseRate) }}</n-tag>
      </n-space>
    </div>

    <n-grid :cols="24" :x-gap="16" :y-gap="16" responsive="screen" item-responsive>
      <n-gi v-for="card in cards" :key="card.label" span="24 s:12 m:8 l:4">
        <n-card size="small" :bordered="false" class="stat-card">
          <n-skeleton v-if="loading" text :repeat="2" />
          <n-statistic v-else :label="card.label" :value="card.value" />
        </n-card>
      </n-gi>
    </n-grid>

    <n-card title="回复率" size="small" :bordered="false" style="margin-top: 16px">
      <n-skeleton v-if="loading" text :repeat="2" />
      <template v-else>
        <n-progress
          type="line"
          :percentage="responsePercent"
          :height="20"
          :border-radius="6"
          indicator-placement="inside"
        />
        <div class="text-muted" style="margin-top: 8px">
          已回复 {{ stats?.responded ?? 0 }} 人，共 {{ stats?.totalGuests ?? 0 }} 人
        </div>
      </template>
    </n-card>

    <n-card title="出席率（已回复宾客中）" size="small" :bordered="false" style="margin-top: 16px">
      <n-skeleton v-if="loading" text :repeat="2" />
      <template v-else>
        <n-progress
          type="line"
          color="#18a058"
          :percentage="attendPercent"
          :height="20"
          :border-radius="6"
          indicator-placement="inside"
        />
        <div class="text-muted" style="margin-top: 8px">
          确认出席 {{ stats?.attending ?? 0 }} 人 / 已回复 {{ stats?.responded ?? 0 }} 人 ·
          预计到场 {{ stats?.expectedAttendees ?? 0 }} 人
        </div>
      </template>
    </n-card>

    <n-card title="状态分布" size="small" :bordered="false" style="margin-top: 16px">
      <n-skeleton v-if="loading" text :repeat="3" />
      <n-table v-else size="small" :single-line="false">
        <thead>
          <tr>
            <th style="width: 140px">状态</th>
            <th style="width: 100px">人数</th>
            <th style="width: 110px">占比</th>
            <th>分布</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in rows" :key="row.label">
            <td>
              <n-tag size="small" :type="row.type" :bordered="false">{{ row.label }}</n-tag>
            </td>
            <td>{{ row.value }}</td>
            <td>{{ row.percent }}%</td>
            <td>
              <n-progress
                type="line"
                :percentage="row.percent"
                :height="12"
                :show-indicator="false"
                :border-radius="4"
              />
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td>合计</td>
            <td>{{ stats?.totalGuests ?? 0 }}</td>
            <td>100%</td>
            <td />
          </tr>
        </tfoot>
      </n-table>
    </n-card>
  </div>
</template>

<style scoped>
.stat-card {
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}
</style>

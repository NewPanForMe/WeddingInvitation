<script setup lang="ts">
import { computed, h, onMounted, reactive, ref } from 'vue'
import {
  NButton,
  NCard,
  NDataTable,
  NInput,
  NPopconfirm,
  NSelect,
  NSpace,
  NTag,
  NText,
  useMessage,
  type DataTableColumns,
  type PaginationProps,
  type TagProps
} from 'naive-ui'
import { deleteGuest, fetchGuests } from '@/api/guests'
import GuestFormModal from '@/components/GuestFormModal.vue'
import GuestQrcodeModal from '@/components/GuestQrcodeModal.vue'
import ImportDialog from '@/components/ImportDialog.vue'
import { formatDateTime } from '@/utils/format'
import type { GuestDto, GuestListQuery, GuestStatus } from '@/types'

const message = useMessage()

const loading = ref(false)
const rows = ref<GuestDto[]>([])
const query = reactive<{ keyword: string; status: GuestStatus | '' }>({
  keyword: '',
  status: ''
})

const pagination = reactive<PaginationProps>({
  page: 1,
  pageSize: 20,
  itemCount: 0,
  showSizePicker: true,
  pageSizes: [10, 20, 50, 100],
  prefix: ({ itemCount }) => `共 ${itemCount ?? 0} 条`
})

const formModal = reactive({ show: false, guest: null as GuestDto | null })
const qrcodeModal = reactive({ show: false, guest: null as GuestDto | null })
const importVisible = ref(false)

const STATUS_META: Record<GuestStatus, { label: string; type: TagProps['type'] }> = {
  pending: { label: '待回复', type: 'warning' },
  attending: { label: '确认出席', type: 'success' },
  declined: { label: '不能出席', type: 'error' }
}

const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '待回复', value: 'pending' },
  { label: '确认出席', value: 'attending' },
  { label: '不能出席', value: 'declined' }
]

const respondedCount = computed(() => rows.value.filter((r) => r.rsvpSubmitted).length)

function renderRsvp(row: GuestDto) {
  if (!row.rsvpSubmitted) {
    return h(NText, { depth: 3 }, { default: () => '未提交' })
  }
  const count = row.guestCount ?? 0
  if (row.attend === true) {
    return h(NTag, { size: 'small', type: 'success', bordered: false }, { default: () => `出席 ${count} 人` })
  }
  if (row.attend === false) {
    return h(NTag, { size: 'small', type: 'error', bordered: false }, { default: () => '不出席' })
  }
  return h(NText, { depth: 3 }, { default: () => '已提交' })
}

const columns = computed<DataTableColumns<GuestDto>>(() => [
  { title: 'ID', key: 'id', width: 70 },
  { title: '姓名', key: 'name', width: 110, ellipsis: { tooltip: true } },
  {
    title: '称呼',
    key: 'salutation',
    width: 140,
    ellipsis: { tooltip: true },
    render: (row) => row.salutation || '-'
  },
  { title: '电话', key: 'phone', width: 130, render: (row) => row.phone || '-' },
  { title: '关系', key: 'relation', width: 100, render: (row) => row.relation || '-' },
  { title: '桌号', key: 'tableNo', width: 120, render: (row) => row.tableNo || '-' },
  { title: '可带人数', key: 'maxGuests', width: 100, render: (row) => row.maxGuests ?? '-' },
  {
    title: '状态',
    key: 'status',
    width: 110,
    render: (row) => {
      const meta = STATUS_META[row.status] ?? STATUS_META.pending
      return h(NTag, { size: 'small', type: meta.type, bordered: false }, { default: () => meta.label })
    }
  },
  { title: 'RSVP 结果', key: 'rsvp', width: 120, render: renderRsvp },
  {
    title: '更新时间',
    key: 'rsvpUpdatedAt',
    width: 160,
    render: (row) => formatDateTime(row.rsvpUpdatedAt)
  },
  {
    title: '操作',
    key: 'actions',
    width: 210,
    fixed: 'right',
    render: (row) =>
      h(NSpace, { size: 4, wrap: false }, {
        default: () => [
          h(
            NButton,
            { size: 'tiny', quaternary: true, type: 'primary', onClick: () => openEdit(row) },
            { default: () => '编辑' }
          ),
          h(
            NButton,
            {
              size: 'tiny',
              quaternary: true,
              onClick: () => {
                qrcodeModal.guest = row
                qrcodeModal.show = true
              }
            },
            { default: () => '二维码/链接' }
          ),
          h(
            NPopconfirm,
            { onPositiveClick: () => handleDelete(row) },
            {
              trigger: () =>
                h(
                  NButton,
                  { size: 'tiny', quaternary: true, type: 'error' },
                  { default: () => '删除' }
                ),
              default: () => `确认删除宾客「${row.name}」？该操作不可恢复。`
            }
          )
        ]
      })
  }
])

async function loadData() {
  loading.value = true
  try {
    const params: GuestListQuery = {
      page: pagination.page ?? 1,
      pageSize: pagination.pageSize ?? 20
    }
    if (query.keyword.trim()) params.keyword = query.keyword.trim()
    if (query.status) params.status = query.status
    const result = await fetchGuests(params)
    rows.value = result.items ?? []
    pagination.itemCount = result.total ?? 0
    pagination.page = result.page ?? params.page
    pagination.pageSize = result.pageSize ?? params.pageSize
  } catch {
    rows.value = []
    pagination.itemCount = 0
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  pagination.page = 1
  loadData()
}

function handleReset() {
  query.keyword = ''
  query.status = ''
  pagination.page = 1
  loadData()
}

function openCreate() {
  formModal.guest = null
  formModal.show = true
}

function openEdit(row: GuestDto) {
  formModal.guest = row
  formModal.show = true
}

async function handleDelete(row: GuestDto) {
  try {
    await deleteGuest(row.id)
    message.success('已删除')
    if (rows.value.length === 1 && (pagination.page ?? 1) > 1) {
      pagination.page = (pagination.page ?? 1) - 1
    }
    loadData()
  } catch {
    // 错误提示由 http 拦截器处理
  }
}

function handleSaved() {
  loadData()
}

function handleImported() {
  pagination.page = 1
  loadData()
}

onMounted(loadData)
</script>

<template>
  <div class="page-container">
    <n-card :bordered="false" size="small">
      <div class="page-header">
        <h2 class="page-title">宾客管理</h2>
        <n-space :size="8">
          <n-button type="primary" @click="openCreate">新增宾客</n-button>
          <n-button secondary type="info" @click="importVisible = true">CSV 导入</n-button>
        </n-space>
      </div>

      <div class="toolbar" style="margin-bottom: 12px">
        <n-input
          v-model:value="query.keyword"
          placeholder="搜索姓名 / 电话 / 称呼"
          clearable
          style="width: 240px"
          @keyup.enter="handleSearch"
        />
        <n-select
          v-model:value="query.status"
          :options="statusOptions"
          style="width: 150px"
          placeholder="状态筛选"
        />
        <n-button type="primary" ghost @click="handleSearch">查询</n-button>
        <n-button quaternary @click="handleReset">重置</n-button>
        <n-text depth="3" class="text-muted">
          本页已回复 {{ respondedCount }} / {{ rows.length }} 条
        </n-text>
      </div>

      <n-data-table
        remote
        size="small"
        :columns="columns"
        :data="rows"
        :loading="loading"
        :pagination="pagination"
        :row-key="(row: GuestDto) => row.id"
        :scroll-x="1500"
        :single-line="false"
        @update:page="(p: number) => { pagination.page = p; loadData() }"
        @update:page-size="(s: number) => { pagination.pageSize = s; pagination.page = 1; loadData() }"
      />
    </n-card>

    <GuestFormModal
      v-model:show="formModal.show"
      :guest="formModal.guest"
      @saved="handleSaved"
    />
    <GuestQrcodeModal v-model:show="qrcodeModal.show" :guest="qrcodeModal.guest" />
    <ImportDialog v-model:show="importVisible" @imported="handleImported" />
  </div>
</template>

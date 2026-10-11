<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import {
  NButton,
  NCard,
  NEmpty,
  NForm,
  NFormItem,
  NGi,
  NGrid,
  NImage,
  NInput,
  NInputNumber,
  NModal,
  NPopconfirm,
  NSpace,
  NSpin,
  NSwitch,
  NTag,
  useMessage,
  type FormInst
} from 'naive-ui'
import PhotoUpload from '@/components/PhotoUpload.vue'
import { deletePhoto, fetchPhotos, sortPhotos, updatePhoto } from '@/api/photos'
import { formatDateTime } from '@/utils/format'
import type { PhotoDto } from '@/types'

const message = useMessage()
const loading = ref(false)
const photos = ref<PhotoDto[]>([])
const sortSaving = ref(false)

const editModal = reactive({
  show: false,
  saving: false,
  id: 0,
  title: '',
  sortOrder: 0,
  enabled: true
})

const total = computed(() => photos.value.length)
const enabledCount = computed(() => photos.value.filter((p) => p.enabled).length)
onMounted(()=>{
  loadData() 
})
async function loadData() {
  loading.value = true
  try {
    const list = await fetchPhotos()
    photos.value = [...(list ?? [])].sort((a, b) => a.sortOrder - b.sortOrder)
  } catch {
    photos.value = []
  } finally {
    loading.value = false
  }
}

function handleUploaded() {
  loadData()
}

function openEdit(photo: PhotoDto) {
  editModal.id = photo.id
  editModal.title = photo.title ?? ''
  editModal.sortOrder = photo.sortOrder
  editModal.enabled = photo.enabled
  editModal.show = true
}

async function saveEdit() {
  editModal.saving = true
  try {
    await updatePhoto(editModal.id, {
      title: editModal.title.trim() || null,
      sortOrder: editModal.sortOrder,
      enabled: editModal.enabled
    })
    message.success('照片信息已更新')
    editModal.show = false
    loadData()
  } catch {
    // 错误提示由 http 拦截器处理
  } finally {
    editModal.saving = false
  }
}

async function toggleEnabled(photo: PhotoDto, value: boolean) {
  try {
    await updatePhoto(photo.id, {
      title: photo.title,
      sortOrder: photo.sortOrder,
      enabled: value
    })
    photo.enabled = value
    message.success(value ? '已启用' : '已停用')
  } catch {
    loadData()
  }
}

async function handleDelete(photo: PhotoDto) {
  try {
    await deletePhoto(photo.id)
    message.success('已删除')
    loadData()
  } catch {
    // 错误提示由 http 拦截器处理
  }
}

/** 上移/下移：本地重排后调用 /photos/sort 提交新的顺序 */
async function move(index: number, delta: number) {
  const target = index + delta
  if (target < 0 || target >= photos.value.length) return
  const next = [...photos.value]
  const [item] = next.splice(index, 1)
  next.splice(target, 0, item)
  photos.value = next

  sortSaving.value = true
  try {
    await sortPhotos(next.map((p) => p.id))
    message.success('排序已保存')
    loadData()
  } catch {
    loadData()
  } finally {
    sortSaving.value = false
  }
}
</script>

<template>
  <div class="page-container">
    <n-card :bordered="false" size="small">
      <div class="page-header">
        <h2 class="page-title">照片管理</h2>
        <n-space align="center" :size="12">
          <n-tag size="small" :bordered="false">共 {{ total }} 张</n-tag>
          <n-tag size="small" type="success" :bordered="false">启用 {{ enabledCount }} 张</n-tag>
          <span v-if="sortSaving" class="text-muted">排序保存中…</span>
          <PhotoUpload text="上传照片" @uploaded="handleUploaded" />
          <n-button size="small" quaternary :loading="loading" @click="loadData">刷新</n-button>
        </n-space>
      </div>

      <n-spin :show="loading">
        <n-empty v-if="!photos.length && !loading" description="还没有照片，点击右上角「上传照片」开始" style="padding: 60px 0" />

        <n-grid v-else :cols="24" :x-gap="16" :y-gap="16" responsive="screen" item-responsive>
          <n-gi v-for="(photo, index) in photos" :key="photo.id" span="24 s:12 m:8 l:6">
            <n-card size="small" :bordered="false" class="photo-card">
              <template #cover>
                <n-image
                  :src="photo.url"
                  class="photo-thumb"
                  object-fit="contain"
                  :alt="photo.title ?? '婚礼照片'"
                  lazy
                />
              </template>

              <n-space vertical :size="8">
                <n-space justify="space-between" align="center">
                  <n-tag size="tiny" :bordered="false">排序 {{ photo.sortOrder }}</n-tag>
                  <n-switch
                    size="small"
                    :value="photo.enabled"
                    @update:value="(v: boolean) => toggleEnabled(photo, v)"
                  >
                    <template #checked>启用</template>
                    <template #unchecked>停用</template>
                  </n-switch>
                </n-space>

                <div>
                  <div style="font-weight: 600">{{ photo.title || '未命名照片' }}</div>
                  <div class="text-muted">上传于 {{ formatDateTime(photo.createdAt) }}</div>
                </div>

                <n-space :size="6" justify="space-between">
                  <n-space :size="6">
                    <n-button size="tiny" secondary :disabled="index === 0" @click="move(index, -1)">
                      上移
                    </n-button>
                    <n-button
                      size="tiny"
                      secondary
                      :disabled="index === photos.length - 1"
                      @click="move(index, 1)"
                    >
                      下移
                    </n-button>
                    <n-button size="tiny" quaternary type="primary" @click="openEdit(photo)">
                      编辑
                    </n-button>
                  </n-space>
                  <n-popconfirm @positive-click="handleDelete(photo)">
                    <template #trigger>
                      <n-button size="tiny" quaternary type="error">删除</n-button>
                    </template>
                    确认删除这张照片？该操作不可恢复。
                  </n-popconfirm>
                </n-space>
              </n-space>
            </n-card>
          </n-gi>
        </n-grid>
      </n-spin>
    </n-card>

    <n-modal
      v-model:show="editModal.show"
      title="编辑照片信息"
      preset="card"
      style="width: 460px; max-width: 94vw"
    >
      <n-form label-placement="left" label-width="80">
        <n-form-item label="标题">
          <n-input v-model:value="editModal.title" placeholder="例如：外景" clearable />
        </n-form-item>
        <n-form-item label="排序号">
          <n-input-number v-model:value="editModal.sortOrder" :min="0" style="width: 160px" />
        </n-form-item>
        <n-form-item label="是否启用">
          <n-switch v-model:value="editModal.enabled" />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="editModal.show = false">取消</n-button>
          <n-button type="primary" :loading="editModal.saving" @click="saveEdit">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<style scoped>
.photo-card {
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}

.photo-card :deep(.n-card-cover) {
  height: 160px;
  overflow: hidden;
  background: #f7f7f9;
}
</style>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useInviteStore } from '@/stores/invite'
import CoverSection from '@/components/CoverSection.vue'
import WeddingInfo from '@/components/WeddingInfo.vue'
import PhotoSwipe from '@/components/PhotoSwipe.vue'
import MapNav from '@/components/MapNav.vue'
import RsvpForm from '@/components/RsvpForm.vue'

const route = useRoute()
const store = useInviteStore()

const token = computed(() => String(route.params.token ?? ''))

/** 页面标题：{新人姓名} 的婚礼邀请 */
function applyTitle(): void {
  const config = store.config
  if (config?.groomName && config?.brideName) {
    document.title = `${config.groomName} & ${config.brideName} 的婚礼邀请`
  } else {
    document.title = '婚礼邀请函'
  }
}

// 初始化：设置 token 并拉取数据；token 变化时重新加载
watch(
  token,
  (value) => {
    store.setToken(value)
    store.resetSubmitState()
    void store.load().then(applyTitle)
  },
  { immediate: true }
)

watch(() => store.config, applyTitle, { deep: true })

const isLoading = computed(() => store.loadState === 'loading' || store.loadState === 'idle')
const isError = computed(() => store.loadState === 'error')
</script>

<template>
  <main class="page invite">
    <!-- 加载态：Vant Skeleton -->
    <div v-if="isLoading" class="invite__skeleton">
      <div class="invite__skeleton-cover" />
      <div class="card">
        <van-skeleton title :row="3" />
      </div>
      <div class="card">
        <van-skeleton title :row="2" />
      </div>
      <div class="card">
        <van-skeleton>
          <template #template>
            <van-skeleton-title />
            <van-skeleton-paragraph :row="4" />
          </template>
        </van-skeleton>
      </div>
    </div>

    <!-- 错误态 -->
    <div v-else-if="isError" class="invite__error">
      <van-empty image="error" :description="store.errorMessage || '请柬加载失败'" />
      <div class="invite__error-tip">
        <van-notice-bar
          wrapable
          :scrollable="false"
          left-icon="info-o"
          text="请确认链接完整，或联系新人重新发送专属链接。"
        />
        <router-link to="/" class="invite__error-link">返回首页</router-link>
      </div>
    </div>

    <!-- 正常内容 -->
    <template v-else-if="store.hasInvite && store.config && store.guest">
      <CoverSection
        :cover-url="store.config.coverUrl"
        :groom-name="store.config.groomName"
        :bride-name="store.config.brideName"
        :wedding-time="store.config.weddingTime"
        :photos="store.photos"
      />

      <WeddingInfo
        :groom-name="store.config.groomName"
        :bride-name="store.config.brideName"
        :wedding-time="store.config.weddingTime"
        :salutation="store.guest.salutation"
      />

      <MapNav :venue="store.config.venue" :address="store.config.address" />

      <PhotoSwipe :photos="store.photos" :cover-url="store.config.coverUrl" />

      <section class="table-card card card--soft">
        <h2 class="section-title">您的席位</h2>
        <p class="table-card__no">{{ store.guest.tableNo || '待安排' }}</p>
        <p class="table-card__desc">
          {{ store.guest.salutation || store.guest.name }}，我们已为您预留座位
        </p>
      </section>

      <RsvpForm />

      <footer class="invite__footer">
        <p class="invite__footer-main">期待与您共同见证这一刻</p>
        <p class="invite__footer-sub">
          {{ store.config.groomName }} &amp; {{ store.config.brideName }} 敬邀
        </p>
      </footer>
    </template>

    <!-- 兜底 -->
    <div v-else class="invite__error">
      <van-empty image="error" description="暂无可展示的请柬内容" />
    </div>
  </main>
</template>

<style scoped>
.invite {
  padding-bottom: calc(20px + var(--wedding-safe-bottom));
}

.invite__skeleton-cover {
  width: 100%;
  height: clamp(320px, 58vh, 460px);
  min-height: 320px;
  max-height: 460px;
  background: linear-gradient(150deg, #f0e5d9 0%, #e6d8c8 100%);
}

.invite__error {
  padding-top: 40px;
}

.invite__error-tip {
  padding: 0 var(--wedding-page-x);
}

.invite__error-link {
  display: block;
  margin-top: 18px;
  text-align: center;
  font-size: 14px;
  color: var(--wedding-primary-dark);
  text-decoration: none;
}

.table-card {
  text-align: center;
}

.table-card__no {
  margin: 0 0 6px;
  font-size: 26px;
  font-weight: 700;
  letter-spacing: 2px;
  color: var(--wedding-primary-dark);
}

.table-card__desc {
  margin: 0;
  font-size: 13px;
  color: var(--wedding-text-sub);
}

.invite__footer {
  margin: 24px var(--wedding-page-x) 0;
  padding-top: 18px;
  border-top: 1px solid var(--wedding-border);
  text-align: center;
}

.invite__footer-main {
  margin: 0 0 6px;
  font-size: 14px;
  letter-spacing: 2px;
  color: var(--wedding-primary-dark);
}

.invite__footer-sub {
  margin: 0;
  font-size: 12px;
  color: var(--wedding-text-muted);
}

@media (min-width: 768px) {
  .invite__skeleton-cover {
    height: 68vh;
    min-height: 520px;
    max-height: 720px;
  }
}
</style>

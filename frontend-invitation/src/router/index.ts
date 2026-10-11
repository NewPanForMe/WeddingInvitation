import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  // 请柬主页面：新链接使用 /Wedding/i?token=xxx，避免 IIS history rewrite 依赖。
  {
    path: '/',
    name: 'invite',
    component: () => import('@/views/Invite.vue'),
    meta: { title: '婚礼邀请函' }
  },
  // 兼容旧链接：/Wedding/i/{token}
  {
    path: '/:token',
    name: 'invite-legacy',
    component: () => import('@/views/Invite.vue'),
    meta: { title: '婚礼邀请函' }
  },
  // 兜底：地址无效 / token 失效等统一走错误页
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/Error.vue'),
    meta: { title: '邀请链接无效' }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

router.afterEach((to) => {
  const title = to.meta.title
  if (typeof title === 'string' && title) {
    document.title = title
  }
})

export default router

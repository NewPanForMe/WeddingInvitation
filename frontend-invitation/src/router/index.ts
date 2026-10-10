import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  // 请柬主页面：应用部署在 /Wedding/i，宾客收到的是 /Wedding/i/{token}
  {
    path: '/:token',
    name: 'invite',
    component: () => import('@/views/Invite.vue'),
    meta: { title: '婚礼邀请函' }
  },
  // 无 token 的提示页
  {
    path: '/',
    name: 'home',
    component: () => import('@/views/Home.vue'),
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

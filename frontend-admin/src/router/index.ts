import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { EXPIRES_AT_KEY, TOKEN_KEY, USERNAME_KEY } from '@/api/http'
import AdminLayout from '@/layout/AdminLayout.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/Login.vue'),
    meta: { title: '登录', public: true }
  },
  {
    path: '/',
    component: AdminLayout,
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '数据概览' }
      },
      {
        path: 'guests',
        name: 'guests',
        component: () => import('@/views/Guests.vue'),
        meta: { title: '宾客管理' }
      },
      {
        path: 'photos',
        name: 'photos',
        component: () => import('@/views/Photos.vue'),
        meta: { title: '照片管理' }
      },
      {
        path: 'config',
        name: 'config',
        component: () => import('@/views/Config.vue'),
        meta: { title: '婚礼配置' }
      },
      {
        path: 'statistics',
        name: 'statistics',
        component: () => import('@/views/Statistics.vue'),
        meta: { title: '统计报表' }
      }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior: () => ({ top: 0 })
})

function clearAuthStorage(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USERNAME_KEY)
  localStorage.removeItem(EXPIRES_AT_KEY)
}

function hasValidSession(): boolean {
  const token = localStorage.getItem(TOKEN_KEY)
  if (!token) return false

  const expiresAt = localStorage.getItem(EXPIRES_AT_KEY)
  if (!expiresAt) return false

  const expiresTime = new Date(expiresAt).getTime()
  if (!Number.isFinite(expiresTime) || expiresTime <= Date.now() + 30_000) {
    clearAuthStorage()
    return false
  }

  return true
}

/* 路由守卫：无 token → /login；已登录访问 /login → 首页 */
router.beforeEach((to) => {
  const validSession = hasValidSession()
  if (!to.meta.public && !validSession) {
    return { path: '/login', query: to.fullPath === '/' ? undefined : { redirect: to.fullPath } }
  }
  if (to.path === '/login' && validSession) {
    return { path: '/dashboard' }
  }
  return true
})

router.afterEach((to) => {
  const title = (to.meta.title as string | undefined) ?? ''
  document.title = title ? `${title} - 婚礼请柬后台管理` : '婚礼请柬后台管理'
})

export default router

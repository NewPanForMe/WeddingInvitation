import { fileURLToPath, URL } from 'node:url'

import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// 后端地址：开发环境代理目标。默认 http://localhost:5080，
// 需要指向别的后端时改这里（或设置环境变量 BACKEND_ORIGIN）。
const BACKEND_ORIGIN = process.env.BACKEND_ORIGIN || 'http://localhost:5080'

const proxyTarget = {
  target: BACKEND_ORIGIN,
  changeOrigin: true
}

export default defineConfig({
  base: '/Wedding/i/',
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    host: true,
    port: 5173,
    proxy: {
      // 接口
      '/api': proxyTarget,
      '/Wedding/api': {
        ...proxyTarget,
        rewrite: (path) => path.replace(/^\/Wedding/, '')
      },
      // 图片等静态资源（合同约定照片 url 为 /uploads/x.jpg 相对路径）
      '/uploads': proxyTarget,
      '/Wedding/uploads': {
        ...proxyTarget,
        rewrite: (path) => path.replace(/^\/Wedding/, '')
      }
    }
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    chunkSizeWarningLimit: 1200
  }
})

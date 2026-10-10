import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// 构建产物部署在正式站点 /Wedding/admin 子目录下：
// - base: '/Wedding/admin/' 让 index.html 中的静态资源带上 /Wedding/admin 前缀
// - 路由使用 createWebHistory('/Wedding/admin/')
export default defineConfig({
  base: '/Wedding/admin/',
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true
      },
      '/Wedding/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/Wedding/, '')
      },
      '/uploads': {
        target: 'http://localhost:5080',
        changeOrigin: true
      },
      '/Wedding/uploads': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/Wedding/, '')
      }
    }
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false,
    chunkSizeWarningLimit: 1600
  }
})

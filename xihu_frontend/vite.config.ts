
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
 define: {
    __VUE_PROD_DEVTOOLS__: false,
  },
  server: {
    proxy: {
      '/api': {
        //target: 'http://localhost:5000', // Ocelot 网关
        target: "https://8.133.201.233",
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '') // 适配 Ocelot 路由
      }
    }
  }
})


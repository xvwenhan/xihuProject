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
        target: 'https://localhost:5000', // Ocelot 网关
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '') // 适配 Ocelot 路由
      },
      '/wechat-api': {
        target: 'https://62472329.r3.cpolar.top',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/wechat-api/, '/api')
      }
    },
    headers: {
      'Content-Security-Policy': `
        default-src 'self';
        connect-src 'self' 
          http://localhost:* 
          https://api.dicebear.com 
          https://api.translate.zvo.cn 
          https://*.api.translate.zvo.cn 
          https://686a4cd9.r3.cpolar.top;
        script-src 'self' 'unsafe-inline' 'unsafe-eval';
        style-src 'self' 'unsafe-inline';
        img-src 'self' data: https:;
        frame-src 'self' https://open.weixin.qq.com;
      `.replace(/\s+/g, ' ').trim()
    }
  }
})


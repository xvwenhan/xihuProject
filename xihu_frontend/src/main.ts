
import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

import App from './App.vue'
import router from './router'

const app = createApp(App)

// 添加错误处理
app.config.errorHandler = (err, vm, info) => {
  console.error('Vue Error:', err)
  console.error('Error Info:', info)
}

window.onerror = function(message, source, lineno, colno, error) {
  console.error('Global Error:', { message, source, lineno, colno, error })
  return false
}

console.log('Starting app...')

app.use(createPinia())
app.use(router)
app.use(ElementPlus)

app.mount('#app')

import './assets/main.css'

import { createApp,nextTick } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import App from './App.vue'
import router from './router'
// import translate from 'i18n-jsautotranslate'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

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

// app.use(createPinia())
app.use(router)
app.use(ElementPlus)


// 基本配置
// translate.setUseVersion2()
// translate.selectLanguageTag.show = false
// translate.listener.start()



// 安全获取语言状态（不直接依赖store）
// const getCurrentLanguage = () => {
//   return localStorage.getItem('language') || 'zh'
// }
// 5. 注册全局指令（无依赖版本）
// app.directive('translate', {
//   mounted(el) {
//     const lang = getCurrentLanguage()
//     if (lang !== 'zh') {
//       nextTick(() => {
//         translate.execute(el) // 仅翻译当前元素
//       })
//     }
//   }
// })

// 挂载到全局属性（兼容Options API）
// app.config.globalProperties.$translate = translate

app.mount('#app')

console.log('App mounted')
import { defineStore } from 'pinia'

export const useLanguageStore = defineStore('language', {
  state: () => ({
    currentLang: localStorage.getItem('language') || 'zh'
  }),
  actions: {
    setLanguage(lang) {
      this.currentLang = lang
      localStorage.setItem('language', lang)
    }
  }
})

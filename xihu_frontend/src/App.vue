<script setup lang="ts">
import { RouterView } from 'vue-router'
// import { useLanguageStore } from '@/stores/language.ts'
import { onMounted } from 'vue'
// import translate from 'i18n-jsautotranslate'
// const langStore = useLanguageStore()

// onMounted(() => {
//   // 初始化时执行翻译
//   translate.changeLanguage(langStore.currentLang === 'zh'
//     ? 'chinese_simplified'
//     : 'english'
//   )
//   translate.execute()
// })
</script>

<template>
  <RouterView />
  <!-- <router-view v-translate /> -->

</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body {
  width: 100%;
  height: 100%;
  overflow: hidden;
}

#app {
  width: 100%;
  height: 100vh;
  margin: 0;
  padding: 0;
}
</style>

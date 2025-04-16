<template>
  <!-- 初始状态隐藏内容 -->
  <!-- <div v-if="!isTranslating"> -->
  <div class="home">
    <Header ref="header_ref" @toggleNotifications="toggleNotifications" @toggleSettings="toggleSettings"
      @toggleUser="toggleUser" />
    <!-- <button @click="changeLanguage('en')">English</button>
    <button @click="changeLanguage('zh')">中文</button> -->
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />


    </div>
  </div>

</template>

<script setup>
import 'element-plus/dist/index.css';
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue'
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import AvatarGenerator from '@/components/AvatarGenerator.vue'
import { ElContainer, ElAside, ElDialog, ElMessage, ElLoading, ElPagination } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import api from '../../api/index.js';
</script>

<style scoped>

</style>

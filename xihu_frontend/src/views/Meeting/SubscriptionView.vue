<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <div class="content-container">
             Sub

      </div>
    </div>
  </div>
</template>


<script setup>
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import { ElContainer, ElMessage } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import loading from '@/components/LoadComponent.vue'


import { ref, computed, onMounted } from "vue";
import api from '../../api/index.js';






</script>

<style scoped>
</style>

<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-container class="content-container">
          compass
      </el-container>
    </div>
  </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue';
import Header from '@/components/Header.vue';
import { ElContainer, ElMessage, ElTable } from 'element-plus';
import { ref } from 'vue';
import 'element-plus/es/components/menu/style/css';
import 'element-plus/es/components/menu-item/style/css';
import api from '../../api/index.js';
import overallSrc from '@/assets/pic/3dmap.png';
// import plainSrc from '@/assets/pic/map.png';


</script>

<style scoped>

</style>

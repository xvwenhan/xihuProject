<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
   <div class="layout-container">
     <!-- 左侧导航栏 -->
     <Navbar class="sidebar" />

     <!-- 主要内容与右侧区域 -->
     <el-container class="content-container">
         agenda
     </el-container>
   </div>
 </div>
</template>


<script setup>
  import Navbar from '@/components/Navbar.vue'
  import Header from '@/components/Header2.vue'
  import 'element-plus/es/components/menu/style/css'
  import 'element-plus/es/components/menu-item/style/css'
  import api from '../../api/index.js'

</script>

<style scoped>

</style>

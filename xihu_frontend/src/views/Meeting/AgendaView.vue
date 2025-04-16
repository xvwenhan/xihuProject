
<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
   <div class="layout-container">
     <!-- 左侧导航栏 -->
     <Navbar class="sidebar" />

     <!-- 主要内容与右侧区域 -->
     <el-container class="content-container">
             会议议程
     </el-container>
   </div>
 </div>
</template>


  <script setup>
  import Navbar from '@/components/Navbar.vue'
  import Header from '@/components/Header2.vue'
  import { ElContainer} from 'element-plus'
  import 'element-plus/es/components/menu/style/css'
  import 'element-plus/es/components/menu-item/style/css'
  </script>

<style scoped>
.home {
 display: flex;
 flex-direction: column;
}

.sidebar{
  flex: 1;
}

.layout-container {
  display: flex;
  flex-direction: row;
  height: 100vh; /* 让布局容器填满整个视口高度 */

}


.content-container {
  display: flex;
  flex: 6; /* 主要内容区域占满剩下的空间 */
  padding: 20px;
  height: 100%; /* 内容区域填满 */
  align-items: stretch; /* 保证子项填满容器 */
}

</style>

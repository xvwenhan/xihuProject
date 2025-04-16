
<script setup lang="ts">
</script>

<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
   <div class="layout-container">
     <!-- 左侧导航栏 -->
     <Navbar class="sidebar" />

     <!-- 主要内容与右侧区域 -->
     <el-container class="content-container">
       <el-main class="main-content">
         <!-- 主要内容区域 -->
         <div class="intro">
           <div class="intro-icon">Icon</div>
           <div class="intro-text">我是西湖论剑，你的个人会议助手</div>
         </div>
         <div class="rank-area">
           <div class="rank-box">
             热度排行
           </div>
           <div class="rank-box">
             个性推荐
           </div>
         </div>
       </el-main>

       <!-- 右侧区域 -->
       <el-aside class="right-sidebar">
          <div class="notification-title">{{ showNotifications ? '通知' : '历史' }}</div>
          <!-- 通知内容 -->
          <template v-if="showNotifications">
            <div class="notification-box notification">
              <div class="notification-time">10:30</div>
              <div class="notification-content">新的会议邀请</div>
            </div>
          </template>

          <!-- 历史内容 -->
          <template v-else>
            <div class="notification-box history">
              <div class="history-title">项目进度会议</div>
              <div class="history-time">2024-03-20</div>
            </div>
          </template>
        </el-aside>
     </el-container>
   </div>
 </div>
</template>

<script setup>
import { ref } from 'vue'
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import { ElContainer,ElAside} from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'

const showNotifications = ref(false)

const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
}
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

.main-content {
 flex: 3; /* 主内容区域比右侧通知区域更宽 */
 background-color: #ffffff;
 padding: 20px;
 border-radius: 8px;
}

.right-sidebar {
 flex: 1; /* 右侧通知区域的相对宽度 */
 background-color: #ffffff;
 padding: 20px;
 border-radius: 8px;
 box-shadow: -2px 0 5px rgba(0, 0, 0, 0.05);
}

/* 主要内容区域布局样式 */
.intro {
 display: flex;
 align-items: center;
 margin-bottom: 20px;
}

.intro-icon {
 width: 50px;
 height: 50px;
 background-color: #d9d9d9;
 border-radius: 50%;
 display: flex;
 justify-content: center;
 align-items: center;
 margin-right: 10px;
}

.intro-text {
 font-size: 18px;
 font-weight: bold;
 color: #333;
}

.rank-area {
 display: flex;
 justify-content: space-between;
 gap: 20px;
}

.rank-box {
 flex: 1;
 height: 150px;
 background-color: #e9e9e9;
 border-radius: 8px;
 display: flex;
 justify-content: center;
 align-items: center;
 font-size: 16px;
 font-weight: bold;
}

/* 通知区域样式 */
.notification-title {
 font-weight: bold;
 margin-bottom: 20px;
}

.notification-box {
 height: 60px;
 background-color: #e9e9e9;
 border-radius: 8px;
 margin-bottom: 10px;
}
</style>

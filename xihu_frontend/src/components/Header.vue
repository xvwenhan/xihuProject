<!-- components/Header.vue -->
<template>
  <el-header height="50px" class="common-header">
    <div class="header-content">
      <!-- 左侧 logo 区域 -->
      <div class="logo-container">
        <img :src="logoSrc" :alt="logoAlt" class="logo-image">
      </div>

      <!-- 右侧内容区域 -->
      <div class="right-content">
        <!-- 动态显示图标 -->
        <div class="icons-container" v-if="$route.path == '/home'">
          <img v-for="icon in headerIcons" :key="icon.name"
            :src="icon.isActive ? convertIconPath(icon.activePath) : convertIconPath(icon.path)" :alt="icon.name"
            class="header-icon" @click="toggleIcon(icon)">
        </div>
        <slot name="right"></slot>
      </div>
    </div>
  </el-header>
</template>

<script setup>
import { useRoute } from 'vue-router'
import { ref } from 'vue'
import logo from '@/assets/logo.webp'
import settingIcon from '@/assets/icons/setting.svg'
import messageIcon from '@/assets/icons/message.svg'
import userIcon from '@/assets/icons/user.svg'
import settingFillIcon from '@/assets/icons/setting.fill.svg'
import messageFillIcon from '@/assets/icons/message.fill.svg'
import userFillIcon from '@/assets/icons/user.fill.svg'

const $route = useRoute()
const logoSrc = logo;
const logoAlt = 'Logo'

// 将静态路径转换为动态路径
const convertIconPath = (path) => {
  return new URL(path, import.meta.url).href
}

// 定义所有要发送的事件
const emit = defineEmits(['toggleNotifications', 'toggleSettings', 'toggleUser'])
defineExpose({
  clearAllActiveState
})

// 图标配置
const headerIcons = ref([
  {
    name: 'setting',
    path: settingIcon,
    activePath: settingFillIcon,
    isActive: false,
    onClick: () => emit('toggleSettings')
  },
  {
    name: 'message',
    path: messageIcon,
    activePath: messageFillIcon,
    isActive: false,
    onClick: () => emit('toggleNotifications')
  },
  {
    name: 'user',
    path: userIcon,
    activePath: userFillIcon,
    isActive: false,
    onClick: () => emit('toggleUser')
  }
])

// 切换图标激活状态
const toggleIcon = (icon) => {
  if (icon.isActive) {
    icon.isActive = false
  } else {
    // 否则重置所有图标状态
    headerIcons.value.forEach(item => {
      item.isActive = false
    })
    // 设置当前点击图标为激活状态
    icon.isActive = true
  }
  // 执行点击回调
  icon.onClick()
}

function clearAllActiveState() {
  // 清除所有图标的激活状态
  headerIcons.value.forEach(item => {
    item.isActive = false
  })
}
</script>

<style scoped>
.common-header {
  background-color: #fff;
  width: 100vw;
  margin-top: 3px;
  /* box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); */
}

.header-content {
  height: 100%;
  display: flex;
  align-items: center;
  /* justify-content: space-between; */
  padding: 0 20px;
  position: relative;
}

.logo-container {
  height: 95%;
  display: flex;
  align-items: center;
}

.logo-image {
  height: 40px;
  width: auto;
}

.right-content {
  display: flex;
  align-items: center;
  gap: 20px;
  position: absolute;
  right: 20px;
}

.icons-container {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-icon {
  width: 33px;
  height: 33px;
  cursor: pointer;
}
</style>

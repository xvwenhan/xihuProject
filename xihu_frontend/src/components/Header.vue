@ -0,0 +1,108 @@
<!-- components/Header.vue -->
<template>
  <el-header height="60px" class="common-header">
    <div class="header-content">
      <!-- 左侧 logo 区域 -->
      <div class="logo-container">
        <img
          :src="logoSrc"
          :alt="logoAlt"
          class="logo-image"
        >
      </div>

        <!-- 右侧内容区域 -->
      <div class="right-content">
        <!-- 三个图标 -->
        <div class="icons-container">
          <img
            v-for="icon in headerIcons"
            :key="icon.name"
            :src="icon.path"
            :alt="icon.name"
            class="header-icon"
            @click="icon.onClick"
          >
        </div>
        <slot name="right"></slot>
      </div>
    </div>
  </el-header>
</template>

<script setup>
const logoSrc = '/src/assets/logo.svg'
const logoAlt = 'Logo'

// 图标配置
const headerIcons = [
  {
    name: 'setting',
    path: '/src/assets/icons/setting.svg',
    onClick: () => console.log('setting clicked')
  },
  {
    name: 'message',
    path: '/src/assets/icons/message.svg',
    onClick: () => emit('toggleNotifications')
  },
  {
    name: 'user',
    path: '/src/assets/icons/user.svg',
    onClick: () => console.log('user clicked')
  }
]

const emit = defineEmits(['toggleNotifications'])
</script>

<style scoped>
.common-header {
  background-color: #fff;
  /* box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); */
}

.header-content {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
}

.logo-container {
  height: 100%;
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
  margin-left: 1400px;
}

.icons-container {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-icon {
  width: 40px;
  height: 40px;
  cursor: pointer;
  opacity: 0.7;
  transition: opacity 0.3s;
}

.header-icon:hover {
  opacity: 1;
}
</style>

<template>
  <div class="sidebar">
    <ul class="nav-list">
      <li v-for="item in menuItems" :key="item.index">
        <router-link :to="item.route" class="nav-item" :class="{ 'active-nav-item': $route.path === item.route }">
          <img class="icon"
            :src="$route.path === item.route ? convertIconPath(item.activeIcon) : convertIconPath(item.icon)" />
          {{ item.title }}
        </router-link>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'
import homeIcon from '@/assets/icons/home.svg'
import homeFillIcon from '@/assets/icons/home.fill.svg'
import compassIcon from '@/assets/icons/compass.svg'
import compassFillIcon from '@/assets/icons/compass.fill.svg'
import rocketIcon from '@/assets/icons/rocket.svg'
import rocketFillIcon from '@/assets/icons/rocket.fill.svg'
import planIcon from '@/assets/icons/plan.svg'
import planFillIcon from '@/assets/icons/plan.fill.svg'
import myIcon from '@/assets/icons/my.svg'
import myFillIcon from '@/assets/icons/my.fill.svg'

const $route = useRoute()

// 将静态路径转换为动态路径
const convertIconPath = (path) => {
  return new URL(path, import.meta.url).href
}

const menuItems = [
  {
    index: '1',
    icon: homeIcon,
    activeIcon: homeFillIcon,
    title: '对话',
    route: '/home'
  },
  {
    index: '2',
    icon: compassIcon,
    activeIcon: compassFillIcon,
    title: '参会指南',
    route: '/compass'
  },
  {
    index: '3',
    icon: rocketIcon,
    activeIcon: rocketFillIcon,
    title: '会议直播',
    route: '/meeting'
  },
  {
    index: '4',
    icon: planIcon,
    activeIcon: planFillIcon,
    title: '会议议程',
    route: '/agenda'
  },
  {
    index: '5',
    icon: myIcon,
    activeIcon: myFillIcon,
    title: '我的订阅',
    route: '/subscription'
  }
]

// 可选：用于处理高亮时的图标切换（你可以自己定义高亮图标命名规则）
</script>

<style scoped>
.logo {
  color: #0E0E0E;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  margin: 20px 70px 40px 70px;
}

.sidebar {
  width: 170px;
  height: 100vh;
  border-right: 2px solid rgba(53, 92, 125, 0.3);
  background-color: #FFF;

  display: flex;
  flex-direction: column;
}

.nav-list {
  list-style-type: none;
  padding: 0;
  margin: 0;
}

.nav-item {
  color: #333;
  text-decoration: none;
  padding-top: 12px;
  padding-bottom: 12px;
  padding-left: 25px;

  display: flex;
  align-items: center;
  font-size: 16px;
}

.nav-item:hover {
  background-color: rgba(204, 204, 204, 0.5);
  border-radius: 30px;
}

.active-nav-item {
  background-color: rgba(204, 204, 204, 0.3);
  border-left: 3px solid #355c7d;
}

.icon {
  width: 27px;
  height: 27px;
  margin-right: 10px;
  color: #252525;
}
</style>
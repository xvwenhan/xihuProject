@ -0,0 +1,138 @@
<template>
  <el-container class="container">
    <el-aside class="aside">
      <el-row >
        <el-col :span="24">
          <el-menu
          :default-active="2"
          class="el-menu-vertical-demo"
          @select="handleSelect"
          >
            <el-menu-item v-for="item in menuItems" :key="item.index" :index="item.index">
              <img :src="item.icon" class="menu-icon">
              <span slot="title">{{ item.title }}</span>
            </el-menu-item>
          </el-menu>
        </el-col>
      </el-row>
    </el-aside>
  </el-container>

</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElAside,ElMenu,ElMenuItem,ElRow,ElCol,ElContainer} from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'

const router = useRouter()
const route = useRoute()

const activeIndex = ref('1')
const menuItems = [
  {
    index: '1',
    icon: 'src/assets/icons/home.svg',
    title: '对话',
    route: '/home'
  },
  {
    index: '2',
    icon:'src/assets/icons/compass.svg',
    title: '参会指南',
    route: '/compass'
  },
  {
    index: '3',
    icon: 'src/assets/icons/rocket.svg',
    title: '会议',
    route: '/meeting'
  },
  {
    index: '4',
    icon: 'src/assets/icons/plan.svg',
    title: '会议议程',
    route: '/agenda'
  },
  {
    index: '5',
    icon: 'src/assets/icons/my.svg',
    title: '我的订阅',
    route: '/subscription'
  }
]

// 处理菜单选择
const handleSelect = (index) => {
  const selectedItem = menuItems.find(item => item.index === index)
  if (selectedItem) {
    router.push(selectedItem.route)
  }
}

// 监听路由变化，更新激活菜单项
const updateActiveIndex = () => {
  const currentRoute = route.path
  const matchedItem = menuItems.find(item => item.route === currentRoute)
  if (matchedItem) {
    activeIndex.value = matchedItem.index
  }
}

// 初始化时设置激活菜单项
updateActiveIndex()
</script>

<style scoped>
.container {
  /* margin-left: -400px; */
  height: 100vh;
}

.aside {
  height: 100vh;
  background-color: #ffffff;
  border-right: none; /* 移除默认右边框 */
  display: flex; /* 使用 flex 布局来控制线的位置 */
  flex-direction: column; /* 内容按列排列 */
  padding: 20px 0; /* 给顶部和底部留出空白 */
  position: relative; /* 为伪元素创造定位上下文 */
}

.aside::after {
  content: ""; /* 创建伪元素用于显示竖线 */
  position: absolute;
  top: 5px; /* 给竖线顶部留空白 */
  bottom: 5px; /* 给竖线底部留空白 */
  right: 0; /* 竖线靠右对齐 */
  width: 1px; /* 竖线宽度 */
  background-color: #dcdcdc; /* 竖线颜色 */
}

.el-menu {
  height: 100%; /* 菜单占满侧边栏高度 */
}

.el-menu-item {
  font-size: 16px; /* 增大菜单项文字大小 */
  height: 60px; /* 增加菜单项高度 */
  line-height: 60px; /* 调整文字垂直居中 */
}

.el-menu-item i {
  font-size: 20px; /* 增大图标大小 */
}

/* 修改菜单宽度 */
.el-aside {
  width: 250px !important; /* 增加侧边栏宽度 */
}

.menu-icon {
  width: 20px;
  height: 20px;
  margin-right: 5px;
}
</style>

@ -1,52 +1,262 @@
<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
   <div class="layout-container">
     <!-- 左侧导航栏 -->
     <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-container class="content-container">
        <!-- 图片区域 -->
        <div class="image-area">
          <!-- 五个顶部按钮 -->
          <div class="top-buttons">
            <button
              class="top-button"
              v-for="(item, index) in buttons"
              :key="index"
              :class="{ active: activeIndex === index }"
              @click="handleButtonClick(index)"
            >
              {{ item.label }}
            </button>
          </div>

          <!-- 动态内容区域 -->
          <div class="dynamic-content">
            <!-- 第一级按钮的内容 -->
            <div v-if="activeIndex === 0" class="content-section">
              <!-- 二级按钮（移到 dynamic-content 外面） -->
              <div class="secondary-buttons">
                <button
                  class="secondary-button"
                  v-for="(item, index_first) in buttons_first"
                  :key="index_first"
                  :class="{ active: activeIndex_first === index_first }"
                  @click="handleButtonClick_first(index_first)"
                >
                  {{ item.label }}
                </button>
              </div>

              <!-- 图片内容 -->
              <div class="image-wrapper">
                <img v-if="activeIndex_first === 0" class="main-image" :src="overallSrc" alt="场馆图片" />
                <img v-if="activeIndex_first === 1" class="main-image" :src="plainSrc" alt="平面图片" />
              </div>
            </div>

            <!-- 其他内容 -->
            <div v-if="activeIndex === 1" class="content-section">
              <h2>大会交通</h2>
              <p>这是大会交通的相关信息。</p>
            </div>
            <div v-if="activeIndex === 2" class="content-section">
              <h2>酒店住宿</h2>
              <p>这是酒店住宿的相关信息。</p>
            </div>
            <div v-if="activeIndex === 3" class="content-section">
              <h2>大会签到</h2>
              <p>这是大会签到的相关信息。</p>
            </div>
            <div v-if="activeIndex === 4" class="content-section">
              <h2>联系我们</h2>
              <p>这是联系信息的相关内容。</p>
            </div>
          </div>

          <!-- 右下角按钮 -->
          <button @click="handleBottomButtonClick" class="bottom-button">补充场馆导航按钮</button>
        </div>
      </el-container>
    </div>
  </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue';
import Header from '@/components/Header2.vue';
import { ElContainer } from 'element-plus';
import { ref } from 'vue';
import 'element-plus/es/components/menu/style/css';
import 'element-plus/es/components/menu-item/style/css';

const overallSrc = 'src/assets/pic/map.png';
const plainSrc = 'src/assets/pic/map.png';

// 按钮数据和状态
const buttons = [
  { label: '大会场馆' },
  { label: '大会交通' },
  { label: '酒店住宿' },
  { label: '大会签到' },
  { label: '联系我们' },
];
const buttons_first = [
  { label: '杭州洲际酒店' },
  { label: '平面图' },
];

const activeIndex = ref(0) ;// 当前选中的按钮索引
const activeIndex_first = ref(0) ;
// 方法：更新当前选中的按钮索引
const handleButtonClick = (index) => {
  activeIndex.value = index;
};

const handleButtonClick_first = (index_first) => {
  activeIndex_first.value = index_first;
};

// 点击右下角按钮跳转到导航
const handleBottomButtonClick = () => {
  // 目标地点（目的地：天安门广场）
  const destination = {
    longitude: 120.214188, // 经度
    latitude: 30.242087,  // 纬度
    name: '杭州洲际酒店',
  };

  // 构造高德导航链接
  const navigationUrl = `https://uri.amap.com/navigation?to=${destination.longitude},${destination.latitude},${destination.name}&mode=car&policy=0&src=webapp&coordinate=gaode&callnative=0`;

  // 跳转到高德地图
  window.location.href = navigationUrl;

};
</script>

<style scoped>
.home {
 display: flex;
 flex-direction: column;
  display: flex;
  flex-direction: column;
}

.sidebar {
  flex: 1;
}


.layout-container {
  display: flex;
  flex-direction: row;
  height: 100vh; /* 让布局容器填满整个视口高度 */

  height: 100vh;
}



.content-container {
  display: flex;
  flex: 6; /* 主要内容区域占满剩下的空间 */
  flex: 6;
  padding: 20px;
  height: 100%; /* 内容区域填满 */
  align-items: stretch; /* 保证子项填满容器 */

}

.image-area {
  position: relative;
  width: 100%;
  height: 100%;
  overflow: hidden;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  background-color: #ffffff;
}

.top-buttons {
  position: absolute;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 15px;
  z-index: 10;
}

.top-button {
  background: #fff;
  color: #333;
  border: 1px solid #dddddd;
  border-radius: 5px;
  padding: 8px 15px;
  font-size: 14px;
  cursor: pointer;
}

.top-button.active,
.top-button:hover {
  background-color: #008cff;
  color: #fff;
}

.dynamic-content {
  padding-top: 60px; /* 为二级按钮留出空间 */
  height: calc(100% - 60px); /* 调整高度 */
  overflow-y: auto; /* 允许滚动 */
}

.secondary-buttons {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-bottom: 20px; /* 与图片的间距 */
}

.secondary-button {
  background: #fff;
  color: #333;
  border: 1px solid #dddddd;
  border-radius: 5px;
  padding: 8px 15px;
  font-size: 14px;
  cursor: pointer;
}

.secondary-button.active,
.secondary-button:hover {
  background-color: #008cff;
  color: #fff;
}

.image-wrapper {
  width: 100%;
  height: calc(100% - 40px); /* 减去按钮和间距的高度 */
  display: flex;
  justify-content: center;
  align-items: center;
}

.main-image {
  max-width: 60%;
  max-height: 60%;
  border-radius: 8px;
}

.content-section {
  padding: 20px;
}

.content-section h2 {
  font-size: 20px;
  margin-bottom: 10px;
}

.content-section p {
  font-size: 16px;
  color: #666;
}

.bottom-button {
  position: absolute;
  bottom: 20px;
  right: 20px;
  background: #008cff;
  color: #fff;
  border: none;
  border-radius: 5px;
  padding: 8px 15px;
  font-size: 14px;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}

.bottom-button:hover {
  background-color: #005ab5;
}
</style>

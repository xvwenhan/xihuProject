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
            <button class="button" v-for="(item, index) in buttons" :key="index"
              :class="{ active: activeIndex === index }" @click="handleButtonClick(index)">
              {{ item.label }}
            </button>
          </div>

          <!-- 动态内容区域 -->
          <div class="dynamic-content">
            <!-- 第一级按钮的内容 -->
            <div v-if="activeIndex === 0" class="content-section">
              <!-- 二级按钮（移到 dynamic-content 外面） -->
              <div class="secondary-buttons">
                <button class="button" v-for="(item, index_first) in buttons_first" :key="index_first"
                  :class="{ active: activeIndex_first === index_first }" @click="handleButtonClick_first(index_first)">
                  {{ item.label }}
                </button>
              </div>

              <!-- 图片内容 -->
              <div class="image-wrapper">
                <img :src="overallSrc" alt="场馆图片" style="width: 60%; height: 300px;" />
                <!-- <img v-if="activeIndex_first === 1" class="main-image" :src="plainSrc" alt="平面图片" /> -->
              </div>
            </div>

            <!-- 其他内容 -->
            <div v-if="activeIndex === 1" class="content-section-route">
              <h2>大会交通</h2>

              <div class="transport-info">
                <div class="route">
                  <h3>杭州东站 — 杭州国际博览中心</h3>
                  <ul>
                    <li><strong>地铁：</strong>搭乘地铁 6 号线（桂花西路方向）至博览中心站，DTI 出站后步行约 500 米。</li>
                    <li><strong>出租车：</strong>东广场路程 12 公里（西广场路程 9 公里），时长约 30 分钟，车费 30 元左右。</li>
                  </ul>
                </div>

                <div class="route">
                  <h3>杭州萧山国际机场 — 杭州国际博览中心</h3>
                  <ul>
                    <li><strong>地铁：</strong>搭乘地铁 7 号线（吴山广场方向）至奥体中心站，换乘 6 号线（梅格弄方向）至博览中心站，DTI 出站后步行约 500 米。</li>
                    <li><strong>出租车：</strong>路程 22 公里，时长约 30 分钟，车费 50 元左右。</li>
                  </ul>
                </div>
              </div>
            </div>
            <!-- <div v-if="activeIndex === 2" class="content-section"> -->
              <!-- <div v-if="activeIndex === 2" class="content-section">
                <h2>我的会议地址</h2>
                <el-table :data="meetings.map(loc => ({ location: loc }))" border>
                  <el-table-column prop="location" label="地址" width="300" />
                </el-table>
              </div> -->

            <!-- </div> -->
            <div v-if="activeIndex === 2" class="content-section-route">
              <h2>联系我们</h2>
              <div class="transport-info">
                <div class="route">
              <h3>© 杭州安恒信息技术股份有限公司</h3>
              <p>浙ICP备09102757号-28 浙公网安备 33010802009170号</p>
            </div>
            </div>
            </div>
          </div>

          <!-- 右下角按钮 -->
          <button @click="handleBottomButtonClick" style="position: absolute;bottom: 20px;right: 20px;" class="button">前往导航</button>


        </div>
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

const meetings = ref([]);

// 按钮数据和状态
const buttons = [
  { label: '大会场馆' },
  { label: '大会交通' },
  { label: '联系我们' },
];
const buttons_first = [
  { label: '杭州洲际酒店' },
  { label: '平面图' },
];

const activeIndex = ref(0);// 当前选中的按钮索引
const activeIndex_first = ref(0);
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
    name: '杭州国际博览中心',
  };

  // 构造高德导航链接

  const navigationUrl = `https://uri.amap.com/navigation?to=${destination.longitude},${destination.latitude},${destination.name}&mode=car&policy=0&src=webapp&coordinate=gaode&callnative=0`;

  // 跳转到高德地图
  // window.location.href = navigationUrl;
  window.open(navigationUrl, '_blank');

};

// 获取用户订阅的会议
// const getLocation = async () => {
//   try {
//     const response = await api.get("/user/private/subscribe");
//     meetings.value = [...new Set(response.data.data.map(item => item.location))];
//     console.log("地址",meetings.value)
//   } catch (error) {
//     console.error("获取订阅会议失败:", error);
//     ElMessage.error("获取订阅会议失败");
//   }
// };

const getLocation = async () => {
  try {
    const response = await api.get("/user/private/locations");
    meetings.value = response.data;
    console.log("地址", meetings.value)
  } catch (error) {
    console.error("获取地址失败:", error);

  }
};
getLocation();

</script>

<style scoped>
@import '@/assets/button.css';

.home {
  display: flex;
  flex-direction: column;
  background-color: #FFFFFF;
}

.sidebar {
  flex: 1;
}

.layout-container {
  display: flex;
  flex-direction: row;
  height: 100vh;
}

.content-container {
  display: flex;
  flex: 6;
  padding: 20px;
  height: 95%;
  align-items: stretch;
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
  padding-top: 60px;
  /* 为二级按钮留出空间 */
  height: calc(100% - 60px);
  /* 调整高度 */
  overflow-y: auto;
  /* 允许滚动 */
}

.secondary-buttons {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-bottom: 20px;
  /* 与图片的间距 */
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
  height: calc(100% - 40px);
  /* 减去按钮和间距的高度 */
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


.bottom-button:hover {
  background-color: #005ab5;
}


.content-section-route {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.transport-info {
  margin-top: 20px;
}

.route {
  margin-bottom: 30px;
  padding: 15px;
  background: #f7fbfc;
  border-radius: 8px;
  box-shadow: 5px 10px 5px -4px rgba(222, 222, 222, 0.356);
}

.route h3 {
  margin-top: 5px;
  color: #053e5f;
  border-bottom: 1px solid #eee;
  padding-bottom: 5px;
  font-weight: bold;
}

.route ul {
  padding-left: 20px;
}

.route li {
  margin-top:15px;
  margin-bottom: 8px;
  line-height: 1.5;
}

.route strong {
  color: #2c3e50;
}
.button.active{
  background-color: #769fcdda;
  color: #fff;
}
</style>

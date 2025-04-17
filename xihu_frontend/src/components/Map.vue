<template>
  <div id="map-container" class="map-wrapper">
    <!-- 搜索栏 -->
    <div class="search-bar-overlay">
      <div class="search-bar">
        <input
          type="text"
          v-model="searchKeywords"
          placeholder="请输入搜索关键词"
          class="search-input"
        />
        <button @click="handleSearch" class="search-button">搜索</button>
      </div>
    </div>

    <!-- 搜索结果列表 -->
    <div v-if="searchResults.length > 0" class="search-results-overlay">
      <div class="search-results">
        <p>搜索结果：</p>
        <ul>
          <li
            v-for="(result, index) in searchResults"
            :key="index"
            @click="handleResultClick(result)"
          >
            {{ result.name }} - {{ result.address }}
          </li>
        </ul>
      </div>
    </div>

    <!-- 地图渲染区域 -->
    <div id="map" class="map-container"></div>
  </div>
</template>

<script setup>
// Vue Composition API 引入
import { ref, onMounted, onBeforeUnmount } from "vue"
//import axios from "axios"

//const mapApiKey = "6565719b85f1b8fda5b8e836c6c7559d";

// 数据绑定
const mapInstance = ref(null); // 地图实例
const searchKeywords = ref(""); // 用户输入的搜索关键词
const mapContainerId = "map"; // 地图容器的 ID
const searchResults = ref([]); // 搜索结果列表
const clickedPosition = ref(null); // 点击位置的经纬度
const customLayer = ref(null); // 自定义图层引用

// 初始化地图
const initMap = () => {
  if (!window.AMap) {
    console.error("高德地图 JavaScript SDK 未正确加载，请检查网络或 API Key");
    return;
  }

  // 创建地图实例并设置基础属性
  mapInstance.value = new window.AMap.Map(mapContainerId, {
    zoom: 18, // 初始缩放级别
    maxZoom: 22,
    center: [120.213662, 30.242504],
    rotation: -38, // 初始旋转角度
  });

  // 添加自定义图片图层并实现旋转功能
  const addCustomImageLayer = () => {
    // 创建一个空白 canvas
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    // 设置 canvas 的宽高
    canvas.width = 102.4; // 根据您需要的宽度
    canvas.height = 76.8; // 根据您需要的高度

    // 加载图片
    const image = new Image();
    image.src = new URL("../assets/pic/map.png", import.meta.url).href; // 图片路径
    image.onload = () => {

      redrawCanvas(); // 初始绘制
    };

    // 重绘canvas，根据当前地图状态
  const redrawCanvas = () => {
    // 获取地图的缩放级别
    const zoom = mapInstance.value.getZoom();
    const scale = Math.pow(2, zoom - 18); // 基于初始地图级别（18级）

    // 动态调整 canvas 尺寸以适配缩放
    const scaledWidth = image.width * scale;
    const scaledHeight = image.height * scale;

    canvas.width = scaledWidth; // 调整 canvas 尺寸
    canvas.height = scaledHeight;

    // 清空画布，并绘制图片
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(
      image,
      0,
      0,
      scaledWidth,
      scaledHeight // 根据缩放比例绘制图片到画布
    );

    console.log(`Canvas Size: ${canvas.width}x${canvas.height}, Scale: ${scale}`);

  };

    // 创建自定义图层
    const layer = new window.AMap.CustomLayer(canvas, {
      zooms: [3, 22], // 图层显示范围的缩放级别
      bounds: new window.AMap.Bounds(
        [120.211388, 30.243091], // 左下角坐标
        [120.215894, 30.24185] // 右上角坐标
      ),
      zIndex: 100, // 图层优先级
    });

    // 将创建的图层保存到 customLayer 引用中
    customLayer.value = layer;

    // 如果需要实时更新的逻辑，可以写在 render 方法中
    customLayer.value.render = function () {
      console.log("Custom Layer render called!");
    };

    // 将自定义图层添加到地图中
    mapInstance.value.add(customLayer.value);
  };

  // 监听地图加载完成事件
  mapInstance.value.on("complete", () => {
    console.log("地图加载完成，可以正常使用");
  });

  // 调用添加图片图层函数
  addCustomImageLayer();

  // 初始化地图事件监听器
  initMapListeners(); // 确保传入 `map` 实例
};

// 监听地图事件
const initMapListeners = () => {
  if (mapInstance.value) {
    // 监听缩放级别变动
    mapInstance.value.on("zoomchange", () => {
      console.log("地图缩放级别变更为：", mapInstance.value.getZoom());
      if (customLayer.value) {
        customLayer.value.render(); // 调用 customLayer 的 render 方法
      }
    });

    // 监听地图中心点变动
    mapInstance.value.on("moveend", () => {
      console.log("地图中心点变化：", mapInstance.value.getCenter());
    });

    // 监听地图点击事件
    mapInstance.value.on("click", (event) => {
      const { lng, lat } = event.lnglat;
      console.log(`点击位置的经纬度：lng=${lng}, lat=${lat}`);

      // 如果需要保存点击的经纬度用于显示等操作
      clickedPosition.value = { lng, lat };
    });
  }
};

// 生命周期：组件挂载时
onMounted(() => {
  console.log("组件挂载中...");
  initMap(); // 初始化地图
});

// 生命周期：组件卸载时
onBeforeUnmount(() => {
  if (mapInstance.value) {
    mapInstance.value.destroy(); // 销毁地图实例
    mapInstance.value = null;
    console.log("地图实例已销毁");
  }
});

// 清理标点管理数组的功能（如果有标记需要清理）
const clearMarkers = () => {
  // 示例清理逻辑（根据需求修改）
  console.log("清理标记数组");
};
</script>

<style scoped>
.map-wrapper {
  position: relative;
  height: 100vh;
  width: 75vw;

}

/* 地图充满整个屏幕 */
.map-container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  transform-origin: center center;
}

/* 搜索栏悬浮在地图顶部 */
.search-bar-overlay {
  position: absolute;
  top: 20px;
  left: 20px;
  right: 20px;
  z-index: 1000;
}

/* 搜索结果悬浮在地图上 */
.search-results-overlay {
  position: absolute;
  top: 70px; /* 搜索栏下方 */
  left: 20px;
  right: 20px;
  max-height: 40%;
  overflow-y: auto;
  z-index: 1000;
}

.search-bar {
  display: flex;
  align-items: center;
  background-color: rgba(255, 255, 255, 0.9); /* 半透明白色背景 */
  border-radius: 8px;
  padding: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.search-input {
  flex-grow: 1;
  padding: 8px;
  font-size: 16px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.search-button {
  padding: 8px 12px;
  margin-left: 8px;
  background-color: #007bff;
  color: #fff;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.search-button:hover {
  background-color: #0056b3;
}

/* 搜索结果样式 */
.search-results {
  background-color: rgba(255, 255, 255, 0.9); /* 半透明白色背景 */
  border-radius: 8px;
  padding: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.search-results p {
  margin: 0 0 10px;
  font-weight: bold;
}

.search-results ul {
  list-style-type: none;
  padding: 0;
  margin: 0;
}

.search-results li {
  padding: 5px;
  border-bottom: 1px solid #ddd;
  cursor: pointer;
}

.search-results li:hover {
  background-color: #f0f0f0;
}
</style>

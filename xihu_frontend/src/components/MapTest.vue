<template>
    <div id="map" class="map-container"></div>
    <div class="controls">
      <button @click="fetchRoute">获取路线</button>
      <p>路线信息：{{ routeInfo }}</p>
    </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import axios from "axios";

// 数据：存储结果和地图相关状态
const mapInstance = ref(null); // 高德地图实例
const routeInfo = ref(""); // 路线规划信息

// 高德地图 API Key，请替换成你实际申请的 KEY
const API_KEY = "3272bab38d31ea9ac069abdbe018f86e"; // 替换为真实 Key

// 起点和终点经纬度（示例位置）
const origin = "116.379028,39.865042"; // 示例起点经纬度
const destination = "116.427281,39.903719"; // 示例终点经纬度

// 初始化地图
const initMap = () => {
  // 创建地图实例
  mapInstance.value = new AMap.Map("map", {
    zoom: 13, // 地图显示的缩放级别
    center: [116.397428, 39.90923], // 地图的中心点（示例位置）
  });

  // 添加工具条和比例尺
  AMap.plugin(["AMap.ToolBar", "AMap.Scale"], () => {
    mapInstance.value.addControl(new AMap.ToolBar());
    mapInstance.value.addControl(new AMap.Scale());
  });
};

// 路线规划请求和绘制
const fetchRoute = async () => {
  try {
    // 调用高德地图步行路线规划 API
    const response = await axios.get("https://restapi.amap.com/v3/direction/walking", {
      params: {
        key: API_KEY,
        origin,
        destination,
      },
    });

    // 检查 API 返回结果是否有效
    if (response.data.status === "1" && response.data.route) {
      const route = response.data.route.paths[0]; // 获取第一条路线信息
      routeInfo.value = `距离：${route.distance}米，时间：${route.duration}秒`;

      // 在地图上绘制路线
      const path = route.steps.flatMap(step => step.polyline.split(";").map(point => point.split(",").map(Number)));
      const polyline = new AMap.Polyline({
        path,
        strokeColor: "#0091ff",
        strokeWeight: 5,
      });
      mapInstance.value.add(polyline);
    } else {
      routeInfo.value = "未能获取有效的路线规划结果。";
    }
  } catch (error) {
    console.error("路线规划请求失败：", error);
    routeInfo.value = "请求失败，请检查网络或 API Key。";
  }
};

// 生命周期：组件挂载时初始化地图
onMounted(() => {
  initMap();
});
</script>

<style scoped>
/* 将地图容器设置为占满屏幕 */
#map {
  width: 100%;
  height: 100vh;
  position: relative;
}

/* 控制区域样式 */
.controls {
  position: absolute;
  padding: 10px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

button,
p {
  margin: 5px 0;
}
</style>

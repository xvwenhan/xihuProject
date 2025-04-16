<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-container class="content-container">




        <div class="main-content" v-if="!isLoading">
          <h2 class="section-title">订阅列表</h2>

          <!-- 会议时间轴 -->
          <div class="timeline">
            <div v-for="(meetings, date) in groupedMeetings" :key="date" class="date-group">
              <!-- 日期标题 -->
              <div class="date-title">
                <span class="date-text">{{ date }}</span>
                <div class="date-line"></div>
              </div>
              <!-- 会议卡片 -->
              <div class="meeting-list">
                <div v-for="meeting in meetings" :key="meeting.id" class="meeting-card">
                  <p class="meeting-name">{{ meeting.name.length > 20 ? meeting.name.substring(0, 20) + '...' :
                    meeting.name }}</p>
                  <!-- <p class="meeting-name">{{ meeting.name }}</p> -->
                  <p class="meeting-time">时间: {{ meeting.time }}</p>

                  <!-- <p class="meeting-type">类型: {{ meeting.type }}</p> -->
                  <p class="meeting-location">地点: {{ meeting.location }}</p>
                  <!-- <p class="meeting-subscribers">订阅人数: {{ meeting.subscribeNum }}</p> -->
                  <p class="meeting-mode">
                    <!-- 方式: {{ meeting.isOnlyOffline ? "线下" : "线上 & 线下" }} -->
                  </p>

                  <!-- 按钮容器 -->
                  <div class="button-group">
                    <!-- 订阅状态按钮（查看信息 / 观看回放） -->
                    <button class="meeting-btn" :class="{ 'subscribed': isMeetingExpired(meeting.date, meeting.time) }"
                      @click="openMeetingDialog(meeting)">
                      {{ isMeetingExpired(meeting.date, meeting.time) ? "观看回放" : "查看信息" }}
                    </button>

                    <!-- 取消订阅按钮 -->
                    <button class="meeting-btn cancel-btn" @click="cancelSubscription(meeting.id)">
                      取消订阅
                    </button>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>
        <!-- 会议信息弹窗 -->
        <el-dialog v-model="dialogVisible" title="会议信息" width="50%" v-if="!isLoading">
          <p><strong>会议名称：</strong>{{ selectedMeeting?.name }}</p>
          <p><strong>会议时间：</strong>{{ selectedMeeting?.date }} {{ selectedMeeting?.time }}</p>
          <p><strong>会议类型：</strong>{{ selectedMeeting?.type }}</p>
          <p><strong>会议地点：</strong>{{ selectedMeeting?.location }}</p>
          <p><strong>会议方式：</strong>{{ selectedMeeting?.isOnlyOffline ? "线下" : "线上 & 线下" }}</p>
          <p><strong>订阅人数：</strong>{{ selectedMeeting?.subscribeNum }}</p>

          <!-- 显示会议 URL -->
          <p><strong>会议链接：</strong>
            <a v-if="selectedMeeting?.url" :href="selectedMeeting?.url" target="_blank" class="meeting-url">
              {{ selectedMeeting?.url }}
            </a>
            <span v-else>暂无会议链接</span>
          </p>

          <template #footer>
            <el-button type="primary" @click="dialogVisible = false">关闭</el-button>
          </template>
        </el-dialog>

        <div class="main-content" v-else>
          <loading />
        </div>

      </el-container>
    </div>
  </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import { ElContainer, ElMessage } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import loading from '@/components/LoadComponent.vue'


import { ref, computed, onMounted } from "vue";
import api from '../../api/index.js';


const meetings = ref([]); // 存储会议列表
const selectedMeeting = ref(null); // 选中的会议
const dialogVisible = ref(false); // 控制弹窗显示
const isLoading = ref(true); // 控制加载状态
// 按日期分组并排序
const groupedMeetings = computed(() => {
  const groups = {};
  meetings.value.forEach((meeting) => {
    if (!groups[meeting.date]) {
      groups[meeting.date] = [];
    }
    groups[meeting.date].push(meeting);
  });

  // 按时间排序
  for (const date in groups) {
    groups[date].sort((a, b) => a.time.localeCompare(b.time));
  }
  return groups;
});

// 获取用户订阅的会议
const fetchSubscribedMeetings = async () => {
  try {
    const response = await api.get("/user/private/subscribe");
    meetings.value = response.data.data;
    isLoading.value = false;
  } catch (error) {
    console.error("获取订阅会议失败:", error);
    ElMessage.info("订阅列表为空");
    isLoading.value = false;
  }
};

// 判断会议是否已过期
const isMeetingExpired = (date, time) => {
  const meetingDateTime = new Date(`${date} ${time}`);
  return meetingDateTime < new Date();
};

// 点击 "查看信息 / 观看回放"打开会议详情弹窗
const openMeetingDialog = (meeting) => {
  selectedMeeting.value = meeting;
  dialogVisible.value = true;
};

// 取消订阅
const cancelSubscription = async (meetingId) => {
  try {
    await api.post("/user/private/subscribe", { conferenceId: meetingId });
    ElMessage.success("取消订阅成功");
    fetchSubscribedMeetings(); // 重新获取数据
  } catch (error) {
    console.error("取消订阅失败:", error);
    ElMessage.error("取消订阅失败");
  }
};

// 组件加载时获取数据
onMounted(() => {
  isLoading.value = true;
  fetchSubscribedMeetings();
});

</script>
<style scoped>
/* 页面整体布局 */

.home {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background-color: #FFFFFF;
}

.layout-container {
  display: flex;
  flex: 1;
  height: 100%;
}

.sidebar {
  flex: 1;
}


.content-container {
  display: flex;
  flex: 6;
  padding: 20px;
  height: 100%;
  width: 100%;
  align-items: stretch;
}

.main-content {
  padding: 40px;
  margin: 0px 20px;
  border-radius: 30px;
  border: 1px solid #E5E5E5;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  color: #000;
  height: calc(100vh - 50px - 20px - 20px);
  width: 100%;
  overflow-x: auto;

  display: flex;
  flex-direction: column;
}


/* 标题样式 */
.section-title {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 20px;
}

.search-input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 14px;
  padding: 6px;
}

.search-icon {
  cursor: pointer;
  font-size: 16px;
}

/* 时间轴 */
.timeline {
  background: white;
  padding-top: 1%;
  padding-right: 3%;
  padding-left: 3%;
  border-radius: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow-y: auto;
  /* 当内容超过容器高度时显示垂直滚动条 */
  overflow-x: hidden;
  /* 防止水平滚动条出现 */
  padding-bottom: 30px;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.timeline::-webkit-scrollbar {
  display: none;
}

.date-group {
  margin-bottom: 24px;
}

/* 日期标题 */
.date-title {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.date-text {
  font-size: 16px;
  font-weight: bold;
  color: #033958;
  margin-right: 10px;
}

.date-line {
  flex: 1;
  border-bottom: 2px dashed #ccc;
}

/* 会议列表 */
.meeting-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  perspective: 1000px;
  /* 设置视角距离，值越小效果越强烈 */
}

/* 会议卡片 */
.meeting-card {
  background: #f7fbfc;
  padding: 12px;
  border-radius: 8px;
  width: 200px;
  height: 200px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
  /* 添加过渡效果，使变换更加平滑 */
  transform-style: preserve-3d;
  /* 保持子元素的3D转换 */
  transform-origin: 50% 0;
  /* 设置旋转轴为卡片的顶部横边 */

  position: relative;
  /* 必须设置为relative */
}


/* 鼠标悬停时应用翻转效果 */
.meeting-card:hover {
  transform: rotateY(3deg) scale(1.01);
  /* 翻转并稍微放大 */
  box-shadow: 3px 6px 9px rgba(0, 0, 0, 0.2);
  /* 增强阴影效果以增强立体感 */
}

.meeting-time {
  font-size: 14px;
  font-weight: bold;
  color: #555;
}

.meeting-name {
  font-size: 15px;
  font-weight: bold;
  color: #033958;
  margin: 6px 0;
}



.meeting-location {
  font-size: 12px;
  color: #555;
}

/* 按钮容器 */
.button-group {
  position: absolute;
  /* 绝对定位 */
  bottom: 15%;
  /* 距离父级容器底部20% */
  left: 14%;
  /* 将 .button-group 的左边沿移到父容器中心 */

  /* 向左移动 .button-group 自身宽度的一半，实现水平居中 */
  display: flex;
  padding-top: 3%;
  justify-content: space-between;
  gap: 10px;

  /* 按钮之间的间距 */
}

/* 按钮基础样式 */
.meeting-btn {
  flex: 1;
  /* 让按钮平均分配空间 */
  background-color: #007bff;
  padding: 6px 10px;
  border: none;
  border-radius: 16px;
  font-size: 12px;
  cursor: pointer;
  transition: background-color 0.2s ease-in-out;
}

/* 订阅状态按钮（查看信息 / 观看回放） */
.meeting-btn.subscribed {
  background-color: #007bff;
  color: white;
}

.meeting-btn:not(.subscribed) {
  border-color: #769fcd;
  border-width: 1px;
  background-color: #f7fbfc;
  color: rgb(0, 0, 0);
}

/* 取消订阅按钮 */
.cancel-btn {
  background-color: #dc3545;
  color: white;
}
</style>

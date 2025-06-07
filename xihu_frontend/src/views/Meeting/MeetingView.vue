<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <div class="content-container">
        <div class="main-row" :gutter="20">
          <!-- 左侧：视频直播与嘉宾 -->
          <div class="main-col" style="border-radius: 20px;">
            <div class="card-container" style="height: 100%; border-radius: 20px;overflow-y: auto;">
              <div class="video-choose">
                <h2 class="section-title">会议直播</h2>
                <!-- 这里选择会议直播 -->
                <el-select v-model="selectedId" placeholder="请选择直播" style="width: 300px;" :default-first-option="true"
                  class="custom-select rounded-select">
                  <el-option v-for="meeting in liveMeetings" :key="meeting.conferenceId"
                    :label="meeting.conferenceName + (meeting.liveStatus === 1 ? '(正在直播)' : meeting.liveStatus === 0 ? '(未开始)' : '(已结束)')"
                    :value="meeting.conferenceId" @click="handleMeetingSelect(meeting)" />
                </el-select>
              </div>
              <div class="video-box">
                <iframe v-if="isLive" style="width: 100%; height: 400px;border-radius: 20px;"
                  :src="`https://www.bilibili.com/blackboard/live/live-activity-player.html?cid=${currentCid}&quality=0`"
                  frameborder="no" framespacing="0" scrolling="no" allow="autoplay; encrypted-media"
                  allowfullscreen="true"></iframe>
              </div>
              <div style="display: flex;
              flex-direction: row;
              justify-content: space-between;
              align-items: center;
              margin-top: 20px;">
                <div v-for="(msg, index) in messages" :key="index"  v-if="isLive"
                style="font-size: 14px;color: #666;"><strong style="color: #000;">实时字幕：</strong>{{ msg.data }}</div>
                <button class="button"  @click="download">资料下载</button>
              </div>
              <div class="guest-section">
                <h2 class="section-title">会议嘉宾</h2>
                <div class="guest-row" style="border-radius: 20px;">
                  <div v-for="person in persons" :key="person.name" class="guest-col">
                    <div class="guest-card" shadow="hover" @click="handlePersonClick(person.url)">
                      <img :src="person.avatar" class="guest-avatar" />
                      <div class="guest-name">{{ person.name }}</div>
                      <div class="guest-position">{{ person.position }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <!-- 右侧：聊天/转录/总结 -->
          <div class="right-col">
            <div class="card-container"
              style="height: 100%; display: flex; flex-direction: column; border-radius: 20px;">
              <el-tabs v-model="activeTab" type="border-card" style="flex: 1; " class="custom-tabs">
                <el-tab-pane label="历史转录" name="transcript">
                  <loading v-if="isFistLoading" />
                  <div v-else v-for="msg in summary"  v-show="!isNoStart"
                    :key="msg.id"
                    style="display: flex; flex-direction: column; width: 100%;">
                    <div style="background-color: rgba(90, 141, 191, 0.1);
                    padding: 10px;
                    border-radius: 10px;
                    margin-bottom: 10px;">
                      <div style="font-size: 12px;color: #666;">{{ msg.endTime }}</div>
                      <div style="font-size: 14px;">{{ msg.originalText }}</div>
                    </div>
                  </div>
                </el-tab-pane>
                <el-tab-pane label="总结" name="summary">
                  <loading v-if="isFistLoading" />
                  <div v-else v-for="msg in summary" :key="msg.id"
                    style="display: flex; flex-direction: column; width: 100%;">
                    <div style="background-color: rgba(90, 141, 191, 0.1);
                    padding: 10px;
                    border-radius: 10px;
                    margin-bottom: 10px;">
                      <div style="font-size: 12px;color: #666;">{{ msg.endTime }}</div>
                      <div style="font-size: 14px;">{{ msg.summary }}</div>
                    </div>
                  </div>
                </el-tab-pane>
              </el-tabs>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import { ref, onMounted } from 'vue'
import { ElMessage, ElTabs, ElTabPane, ElDialog, ElSelect, ElOption } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import api from '../../api/index.js'
import loading from '@/components/LoadComponent.vue'
import fy from '@/assets/pic/fy.png'
import lb from '@/assets/pic/lb.jpg'
import wjx from '@/assets/pic/wjx.png'
import cgy from '@/assets/pic/cgy.jpg'

const activeTab = ref('transcript')
let selectedId = ref('1')
const persons = [
  {
    name: '范渊',
    avatar: fy,
    url: 'https://www.gcsis.cn/mediaVideo/947.html',
    position: '安恒信息董事长',
  },
  {
    name: '刘博',
    avatar: lb,
    url: 'https://www.gcsis.cn/mediaVideo/667.html',
    position: '安恒信息CTO',
  },
  {
    name: '邬江兴',
    avatar: wjx,
    url: 'https://www.gcsis.cn/mediaVideo/943.html',
    position: '中国工程院院士',
  },
  {
    name: '崔光耀',
    avatar: cgy,
    url: 'https://www.gcsis.cn/mediaVideo/914.html',
    position: '中国工程院院士',
  }
]
let currentCid = ref('8178490')
let meetings = ref([]);
let liveMeetings = ref([]);
const summary = ref([]);
const messages = ref([]);
let eventSource = null;
const messageType = ref('');
const isListening = ref(false);
const isFistLoading = ref(true);
const isLive = ref(false);
const isNoStart=ref(false)

onMounted(() => {
  console.log('onMounted');
  getSummary();
  getMeetings();
  getSSE();
})

const getMeetings = async () => {
  console.log('getMeetings');
  try {
    const { data } = await api.get('/video/public/list');
    meetings.value = data.data;
    console.log('meetings.value is', meetings.value);
    liveMeetings.value = meetings.value.filter(m => m.isOnlyOffline == "否");
    console.log('liveMeetings.value is', liveMeetings.value);
    chooseMeeting();
  } catch (error) {
    let errorMessage = ref('获取直播会议失败，请稍后重试');
    console.error(errorMessage.value);
  }
}

const chooseMeeting = () => {
  const firstLive = liveMeetings.value.find(m => m.liveStatus === 1);
  if (firstLive) {
    selectedId.value = firstLive.conferenceId;
    handleMeetingSelect(firstLive);
  }

}
const handlePersonClick = (url) => {
  window.open(url, '_blank')
}
const download = () => {
  window.open('https://www.gcsis.cn/results/', '_blank')
}
const handleMeetingSelect = (meeting) => {
  isNoStart.value = false;
  currentCid.value = meeting.channelId;
  if (meeting.liveStatus === 1) {
    isLive.value = true;
    ElMessage.success('当前会议正在直播');
    // getSSE();
  } else if (meeting.liveStatus === 0) {
    isNoStart.value = true;
    isLive.value = false;
    ElMessage.info('当前会议未开始');
  } else {
    isLive.value = false;
    ElMessage.info('当前会议已结束');
  }
  getSummary();
}

function parseSSE(event) {
  if (event.type === 'mid_text') {
    const newText = event.data;
    if (messages.value.length > 0) {
      // 替换最后一句
      messages.value[messages.value.length - 1].data = newText;
    } else {
      // 第一次直接插入
      messages.value.push({
        type: 'mid_text',
        data: newText
      });
    }
  } else if (event.type === 'error') {
    messages.value.push({ type: 'error', data: event.data || '连接错误' });
  } else {
    messages.value.push({ type: event.type || 'message', data: event.data });
  }
}

const getSSE = () => {
  if (!selectedId.value) {
    ElMessage.error('会议 ID 不能为空')
    return
  }
  if (eventSource) {
    eventSource.close();
  }
  // const url = `https://8.133.201.233/api/video/public/stream/${selectedId.value}`
  const url = `https://localhost:5000/api/video/public/stream/${selectedId.value}`
  console.log('🔌 Connecting to SSE:', url)


  eventSource = new EventSource(url)
  isListening.value = true;
  messageType.value = 'info';
  messages.value = [];
  eventSource.onopen = () => {
    console.log('✅ SSE 连接成功')
    messageType.value = 'success';
  }

  eventSource.addEventListener('mid_text', parseSSE);
  eventSource.addEventListener('message', parseSSE);

  eventSource.onerror = (err) => {
    console.error('❌ SSE 连接错误', err)
    eventSource?.close()
    eventSource = null
  }
}
//获取summary
const getSummary = async () => {
  try {
    const response = await api.get(`/video/public/summary/${selectedId.value}`);
    console.log("完整响应:", response);
    isFistLoading.value = false;
    summary.value.splice(0, summary.value.length);
    console.log("summary.value is", summary.value);
    summary.value = response.data
    console.log("summary.value is", summary.value)
  } catch (error) {
    console.log("获取失败", error.response?.data?.message || error.message)
  }
}

// 每30秒调用一次getSummary
setInterval(() => {
  if (selectedId.value) {
    getSummary();
  }
}, 300000);
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
  /* 让布局容器填满整个视口高度 */
}

.section-title {
  font-size: 20px;
  font-weight: bold;
}

.content-container {
  display: flex;
  flex: 6;
  /* 主要内容区域占满剩下的空间 */
  padding: 20px;
  height: 100%;
  /* 内容区域填满 */
  align-items: stretch;
  /* 保证子项填满容器 */
}

.video-box {
  width: 100%;
  height: 400px;
  background-color: rgba(0, 0, 0, 0.1);
  border-radius: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.video-placeholder {
  display: flex;
  width: 100%;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.download-button {
  margin-top: 10px;
  float: right;
}

.guest-section {
  margin-top: 30px;
  max-height: 400px;
  overflow-y: auto;
  padding-right: 10px;
}

.guest-section::-webkit-scrollbar {
  width: 6px;
}

.guest-section::-webkit-scrollbar-thumb {
  background-color: #888;
  border-radius: 3px;
}

.guest-section::-webkit-scrollbar-track {
  background-color: #f1f1f1;
}

.guest-card {
  text-align: center;
  border-radius: 20px;
  border: 1px solid #ebeef5;
  padding: 10px;
  transition: border-color 0.3s ease;
  cursor: pointer;
}

.guest-card:hover {
  border: 1px solid #cccccc;
}

.guest-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  object-fit: cover;
}

.guest-name {
  margin-top: 10px;
  font-size: 14px;
}

.guest-position {
  font-size: 12px;
  color: #666;
}

.speech-block {
  margin: 10px 0;
}

.speaker-info {
  font-size: 14px;
  color: #666;
}

.speech-placeholder {
  height: 80px;
  background-color: #dcdcdc;
  border-radius: 10px;
  margin-top: 5px;
}

.language-button {
  margin-top: 20px;
  float: right;
}

::v-deep(.el-tabs__nav) {
  display: flex;
  justify-content: space-between;
  width: 100%;
}

::v-deep(.el-tabs__item) {
  flex: 1;
  text-align: center;
  color: #000000;
}

::v-deep(.el-tabs--border-card>.el-tabs__header .el-tabs__item.is-active) {
  color: #000000;
}

::v-deep(.el-tabs--border-card>.el-tabs__header .el-tabs__item:hover) {
  color: #000000;
}

.main-row {
  width: 100%;
  height: 100%;
  display: flex;
  margin: 0 -10px;
}

.main-col {
  flex: 0 0 66.66667%;
  max-width: 66.66667%;
  padding: 0 10px;
}


.right-col {
  flex: 0 0 33.33333%;
  max-width: 33.33333%;
  padding: 0 10px;
}

.card-container {
  background: #fff;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  padding: 20px;
  overflow-y: auto;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.card-container::-webkit-scrollbar {
  display: none;
}

.guest-row {
  display: flex;
  flex-wrap: wrap;
  margin: 0 -10px;
}

.guest-col {
  flex: 0 0 25%;
  max-width: 25%;
  padding: 0 10px;
  margin-bottom: 20px;
}

.video-choose {
  margin-bottom: 20px;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
}

.dialog-content {
  padding: 20px 0;
  display: flex;
  flex-direction: column;
  justify-content: center;
}
</style>

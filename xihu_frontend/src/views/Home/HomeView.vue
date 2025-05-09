<script setup lang="ts">
</script>

<template>
  <div class="home">
    <Header ref="header_ref" @toggleNotifications="toggleNotifications" @toggleSettings="toggleSettings"
      @toggleUser="toggleUser" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-main class="main-content">
        <!-- 主要内容区域 -->
        <div class="intro">
          <img :src="currentAvatar" class="intro-icon" />
          <div class="intro-text">我是西湖论剑，你的个人会议助手</div>
        </div>
        <div class="main-content-except-intro">
          <!-- 对话上方提示区域 -->
          <div class="rank-tips-wrapper"
            :style="(collapseRankArea && messages.length > 0) ? 'height: 20px; flex: 0;' : 'height: auto;'">
            <div
              style="width: 100%; display: flex; flex-direction: column; justify-content: center; align-items: center; height: 16px;">
              <button class="collapse-rank-area-btn" @click="collapseRankAreaFunc">
                <el-icon color="grey" size="15" v-if="!collapseRankArea">
                  <CaretTop />
                </el-icon>
                <el-icon color="grey" size="15" v-if="collapseRankArea">
                  <CaretBottom />
                </el-icon>
              </button>
            </div>
            <div class="rank-area" :class="{ 'is-collapsed': collapseRankArea }"
              :style="{ flex: (showHotRank || showRec) ? '1' : '0', borderColor: (showHotRank || showRec) ? 'transparent' : '' }">
              <!-- 热度排行容器 -->
              <div class="hot-rank-container">
                <!-- 热度排行按钮 -->
                <div class="rank-box" :class="{ 'is-collapsed': showHotRank }" @click="toggleHotRank">
                  热度排行
                </div>

                <!-- 动画包裹的会议信息 -->
                <transition name="slide-fade">
                  <div v-show="showHotRank" class="hot-rank-content">
                    <el-popover v-for="(meeting, index) in displayedMeetings" :key="meeting.id" trigger="click"
                      placement="bottom" offset="-12" width="200" persistent="false"
                      :ref="el => setPopoverMeetingRef(el, index)" hide-after="0">
                      <template #reference>
                        <div class="meeting-item">
                          <!-- <p>{{ meeting.name }} - 订阅人数：{{ meeting.subscribeNum }}</p> -->
                          <div class="meeting-item-inner-1">{{ meeting.name.length > 25 ? meeting.name.substring(0,
                            25)
                            + '...' :
                            meeting.name }}</div>
                          <div class="meeting-item-inner-2">订阅人数：{{ meeting.subscribeNum }}</div>
                        </div>
                      </template>
                      <div>
                        <div v-for="question in METTING_ASK" @click="meetingAsk(meeting.name, question, index, false)"
                          class="meeting-popover-item">
                          {{
                            question }}</div>
                      </div>
                    </el-popover>
                  </div>
                </transition>
              </div>

              <!-- 个性推荐 -->
              <div class="hot-rank-container">
                <!-- 个性推荐按钮 -->
                <div class="rank-box" :class="{ 'is-collapsed': showRec }" @click="toggleRec">
                  个性推荐
                </div>

                <!-- 动画包裹的会议信息 -->
                <transition name="slide-fade">
                  <div v-show="showRec" class="hot-rank-content">
                    <el-popover v-for="(meeting, index) in displayedRec" :key="meeting.id" trigger="click"
                      placement="bottom" offset="-12" width="200" persistent="false"
                      :ref="el => setPopoverRecRef(el, index)" hide-after="0">
                      <template #reference>
                        <div class="meeting-item">
                          <!-- <p>{{ meeting.name }} - 订阅人数：{{ meeting.subscribeNum }}</p> -->
                          <div class="meeting-item-inner-1">{{ meeting.name.length > 25 ? meeting.name.substring(0,
                            25) + '...' :
                            meeting.name }}</div>
                          <div class="meeting-item-inner-2">订阅人数：{{ meeting.subscribeNum }}</div>
                        </div>
                      </template>
                      <div>
                        <div v-for="question in METTING_ASK" @click="meetingAsk(meeting.name, question, index, true)"
                          class="meeting-popover-item">
                          {{ question }}</div>
                      </div>
                    </el-popover>
                  </div>
                </transition>
              </div>
            </div>

            <!-- 提示区域 -->
            <Transition name="ask-tips-trans">
              <div v-if="showAskTips" class="ask-tips-region">
                <div style="display: flex; flex-wrap: wrap; justify-content: center;">
                  <span class="gradient-text" style="margin-bottom: 5px; font-weight: 700;">你好！我可以帮你做这些事情：</span>
                  <ul
                    style="padding-left: 20px; list-style: none; display: flex; flex-wrap: wrap; justify-content: center;">
                    <li v-for="(tip, index) in tips" :key="index" :class="['floating-bubble', `bubble-${index}`]"
                      @click="handleTipClick(index)">
                      <div :class="['tip-title', `b-${index}`]">{{ tip.title }}</div>
                      <div class="tip-content">{{ tip.content }}</div>
                    </li>
                  </ul>
                </div>
              </div>
            </Transition>
          </div>

          <!-- 对话框 -->
          <div class="chat-box" :style="(messages.length > 0 ? 'flex: 8;' : '')">
            <!-- <div class="chat-box" :style="(messages.length > 0 ? (collapseRankArea ? 'flex: 8;' : 'flex: 1;') : '')"> -->
            <!-- 消息列表 -->
            <div ref=" msg_list_main" class="message-list main">
              <div v-for="(message, index) in messages" :key="index" class="message-item">
                <div :class="message.isUser ? 'user-message' : 'bot-message'">
                  {{ message.text }}
                </div>
              </div>
              <!-- <div v-if="messages.length === 0" class="empty-message">
                暂无消息
              </div> -->
              <!-- 猜你想问 -->
              <div class="tooltip-wrapper-new" v-if="tooltipVisible">
                <div v-for="(tooltip, index) in questionToolTips" :key="index" class="tooltip-wrapper"
                  @click="handleTooltipClick(tooltip)">
                  <div class="custom-tooltip">
                    {{ tooltip }}
                  </div>
                </div>
              </div>
            </div>

            <!-- 输入框和发送按钮 -->
            <div class="input-container">
              <el-input v-model="chatInput" placeholder="请输入您的问题" class="chat-input" @keyup.enter="sendMessage">
                <!-- 将发送按钮作为图标插入到输入框内部 -->
                <template #append>
                  <el-button :class="{ notEmpty: chatInput.trim() }" @click="sendMessage" class="send-button"
                    type="primary">
                    <spam>发送</spam>
                    <spam class="send-button-icon" />
                  </el-button>
                </template>
              </el-input>
            </div>
          </div>
        </div>

      </el-main>

    </div>
  </div>
</template>

<script setup>
import 'element-plus/dist/index.css';
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue'
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import AvatarGenerator from '@/components/AvatarGenerator.vue'
import { ElContainer, ElAside, ElDialog, ElMessage, ElLoading, ElPagination } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import api from '../../api/index.js';
import { ipcRenderer } from 'electron'
import { Converter } from 'showdown';
// 初始化 Showdown 转换器
const converter = new Converter();
import { DArrowRight, DArrowLeft, CaretTop, CaretBottom } from '@element-plus/icons-vue';
import { useRouter } from 'vue-router';


let currentAvatar = ref('')

const toggleHotRank = () => {
  showHotRank.value = !showHotRank.value; // 切换显示状态
};
const toggleRec = () => {
  showRec.value = !showRec.value; // 切换显示状态
};

const router = useRouter()
const logout = () => {
  localStorage.removeItem('token')
  router.push('/login')
  ElMessage.success('退出登录成功')
}
function read_saved_notifications(receivedNotifications) {
  return new Promise((resolve, reject) => {
    ipcRenderer.invoke('get-notifications')
      .then((result) => {
        // console.log('获取通知成功:', result);
        // console.log('缓存:', receivedNotifications);
        receivedNotifications.clear(); // 清空缓存
        // 将接收到的通知添加到缓存中
        result.forEach(notification => {
          receivedNotifications.add(JSON.stringify(notification));
        });
        resolve();
      })
      .catch((error) => {
        console.error('获取通知失败:', error);
        reject(error);
      });
  });
}

const chatInput = ref('')
const notifications =
  [{conference_name: '大会1',
  start_time: '2025-04-14 10:00:00'}];


const messages = ref([])
const page = ref(1); // 当前页数
const pageSize = ref(10); // 每页显示的消息数量
const totalCount = ref(0); // 总消息数
const history = ref([])
const header_ref = ref()
const rankMeetings = ref([]) // 热度排行
const displayedRec = ref([])//个性推荐
const showHotRank = ref(false) // 热度排行是否显示
const showRec = ref(false) // 个性推荐是否显示
const showRightSidebar = ref(false) // 右侧通知栏是否显示
const collapseRankArea = ref(false) // 是否收起热度排行和个性推荐区域
const showAskTips = ref(true) // 是否显示提问提示词
// 计算属性，限制只显示前5条会议信息
const displayedMeetings = computed(() => rankMeetings.value.slice(0, 5));

const tooltipVisible = ref(false)
const questionToolTips = ref([])

const msg_list_main = ref()
const popover_meeting = ref([])
const popover_rec = ref([])


// 获取热度排行
const fetchRanking = async () => {
  console.log("热度排行显示？", showHotRank.value)
  try {
    const response = await api.get("/user/private/sort", {
      params: {
        SortByTime: false,
        IsAsc: false,
      }
    });
    rankMeetings.value = response.data.data;
    console.log("热度排行", rankMeetings.value)
  } catch (error) {
    console.error("获取订阅会议失败:", error);
    ElMessage.error("获取订阅会议失败");
  }
};

// 获取个性推荐
const fetchRec = async () => {
  console.log("个性推荐显示？", showRec.value)
  try {
    const response = await api.post("/user/private/recommend");
    displayedRec.value = response.data.data;
    console.log("个性推荐", displayedRec.value)
  } catch (error) {
    console.error("获取个性推荐失败:", error);
    ElMessage.error("获取个性推荐失败");
  }
};


function collapseRankAreaFunc() {
  collapseRankArea.value = !collapseRankArea.value
  if (collapseRankArea.value) {
    showHotRank.value = false
    showRec.value = false
  }
}

watch([showHotRank, showRec, messages], () => {
  refreshVisibleOfAskTips();
})

function refreshVisibleOfAskTips() {
  if (showAskTips.value || messages.value.length > 0) {
    showAskTips.value = (!(showHotRank.value || showRec.value) && messages.value.length === 0);
  } else {
    setTimeout(() => {
      showAskTips.value = (!(showHotRank.value || showRec.value) && messages.value.length === 0);
    }, 500); // 延迟0.5s渲染
  }
}

function setPopoverMeetingRef(el, index) {
  popover_meeting.value[index] = el;
}

function setPopoverRecRef(el, index) {
  popover_rec.value[index] = el;
}
</script>

<style scoped>
@import '@/assets/button.css';

/* 强制隐藏可能重复生成的容器 */
:deep([translated="true"])+[translated="false"],
:deep([data-translated])~[data-translated] {
  display: none !important;
}

/* 机器人消息样式 */
.bot-message {
  background-color: #f7fbfc;
  padding: 10px;
  border-radius: 10px;
  max-width: 70%;
  margin-left: 0;
  margin-right: auto;
}

.button-custom {
  background-color: #fff;
  color: #333;
  width: 100px;
  border: none;
  padding: 10px 20px;
  border-radius: 25px;
  cursor: pointer;
  font-size: 12px;
}

.button-custom:hover {
  border-color: rgb(90, 141, 191);
  border-width: 3px;
  /* 鼠标悬停时的背景色 */
  /* 鼠标悬停时的字体颜色 */
}

.chat-box {
  width: 100%;
  padding-left: 5px;
  padding-right: 5px;
  overflow: auto;
  position: relative;
  display: flex;
  flex-direction: column;
  margin-top: 10px;
  margin-bottom: 10px;
  padding-bottom: 5px;
}

/* 输入框和按钮的样式 */
.chat-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  /* overflow: hidden; */
}

.chat-input {
  height: var(--chat-input-height);
  --el-input-inner-height: 100%;
  --el-input-border-radius: 10px;

  --el-input-hover-border-color: var(--el-input-border-color);
  --el-input-focus-border-color: #769fcd;
}

.chat-input.el-input-group {
  border-radius: 10px;
}

.chat-input.el-input-group:hover {
  box-shadow: 0 0 5px #649cdb;
}

.chat_input .el-input-group__append {
  border-left: 0;
  box-shadow: 0 1px 0 0 var(--el-input-border-color) inset, 0 -1px 0 0 var(--el-input-border-color) inset, -1px 0 0 0 var(--el-input-border-color) inset;
}

.chat_input .el-input-group__append {
  padding: 0;
}

.chat-input .el-button {
  height: 100%;
}

.chat-input .el-button.notEmpty {
  border-radius: 0px;
  border-top-right-radius: 10px;
  border-bottom-right-radius: 10px;
  padding: 0 20px;
  color: #6a8eb8;
}

.chat-input .el-button.notEmpty spam {
  font-weight: bold;
}

.content-container {
  display: flex;
  flex: 6;
  /* 主要内容区域占满剩下的空间 */
  padding: 0px;
  padding-right: 0;
  height: 100%;
  width: 100%;
  /* 内容区域填满 */
  align-items: stretch;
  /* 保证子项填满容器 */
}

/* 弹出框 */
.custom-tooltip {
  margin-top: 3px;
  background-color: #d6e6f2;
  padding: 10px;
  border-radius: 10px;
  display: inline-block;
  max-width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1);
}

.conference-notification-box {
  overflow-y: auto;
  scrollbar-width: none;
  /* 隐藏滚动条（Firefox） */
  -ms-overflow-style: none;
  /* 隐藏滚动条（IE/Edge） */
}

.conference-notification-box>.conference-notification-item:last-child {
  margin-bottom: 0;
}

.conference-notification-box>.conference-notification-item:first-child {
  margin-top: 0;
}

.conference-notification-item {
  display: flex;
  flex-direction: column;
  margin-top: 15px;
  margin-bottom: 15px;
  border-radius: 10px;
  border: 1px solid #d9d9d9;
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1);
  padding: 10px;
}

.conference-notification-item-inner {
  margin: 5px;
  cursor: default;
}

/* 分页控件 */
.el-pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
}

/* 暂无消息样式 */
.empty-message {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  flex: 1;
  /* 填充整个消息列表区域 */
  color: #999;
  font-size: 16px;
  text-align: center;
}

.home {
  display: flex;
  flex-direction: column;
  --el-header-height: 60px;
  --notification-title-height: 40px;
  --chat-input-height: 40px;
  width: 100vw;
  background-color: #FFFFFF;
}

.hot-rank-container {
  display: flex;
  justify-content: center;
  flex-direction: column;
  width: 100%;
  max-height: 100%;
}

.hot-rank-content {
  background-color: #f7fbfc;
  border-radius: 8px;
  margin-top: 10px;
  padding: 10px;
  box-shadow: 0 9px 9px rgba(0, 0, 0, 0.1);
  overflow: auto;
}

/* 引入部分 */
.intro {
  margin-bottom: 20px;
  display: flex;
  flex-direction: row;
  align-items: center;
  /* 与下方内容保持一定间距 */
}

/* 输入框容器样式 */
.input-container {
  margin-top: 20px;
  /* 固定间距 */
  width: 100%;
  display: flex;
  justify-content: center;
}

/* 主要内容区域布局样式 */
.intro-icon {
  width: 50px;
  height: 50px;
  background-color: #d9d9d9;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-right: 10px;
  border: 1px solid #769fcd;
  box-shadow: 0 0px 5px #b9d7ea;
}

.intro-text {
  font-size: 18px;
  font-weight: bold;
  color: #333;
}

.layout-container {
  display: flex;
  flex-direction: row;
  /* 让布局容器填满整个视口高度 */
  height: calc(100vh - var(--el-header-height));
  border-top: 4px solid #769fcd;
}

.main-content {
  --el-main-padding: 0;
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 70%;
  padding: 30px;
  /* padding: 20px; */
  /* background-color: #ffffff; */
  background-color: transparent;
  border-radius: 8px;
  overflow: hidden;
  background: radial-gradient(circle, rgb(240, 247, 255) 0%, rgba(255, 255, 255, 1) 50%);
}

/* 消息列表 */
.message-list {
  height: 100%;
  overflow-y: auto;
  /* 允许垂直滚动 */
  scrollbar-width: none;
  /* 隐藏滚动条（Firefox） */
  -ms-overflow-style: none;
  /* 隐藏滚动条（IE/Edge） */
}

/* 主页消息列表 */
/* .message-list.main {
  height: calc(100% - var(--chat-input-height) - 20px);
} */

/* 针对 Webkit 浏览器隐藏滚动条 */
.message-list::-webkit-scrollbar {
  display: none;
  /* 完全隐藏滚动条 */
}

/* 单条消息样式 */
.message-item {
  margin: 10px 0;
  word-wrap: break-word;
}

.meeting-item {
  margin-bottom: 10px;
  padding: 5px;
  border-bottom: 1px dashed #ddd;
  cursor: pointer;
}

.meeting-item:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.meeting-item:hover div {
  font-weight: 600;
  color: #5d9ae1;
  /* text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.1); */
}

.meeting-popover-item {
  margin-top: 4px;
  margin-bottom: 4px;
  padding: 10px;
  border-bottom: 1px dashed #ddd;
  cursor: pointer;
}

.meeting-popover-item:hover {
  font-weight: 600;
}

.meeting-popover-item:first-child {
  margin-top: 0;
}

.meeting-popover-item:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.meeting-item-inner-1 {
  margin: 0;
  font-size: 16px;
  color: #333;
}

.meeting-item-inner-2 {
  margin: 0;
  font-size: 14px;
  color: #545454bb;
}

.notification-title {
  height: var(--notification-title-height);
  margin-bottom: 20px;
  position: sticky;
  top: 0;
}

.notification-title-inner {
  font-weight: bold;
  font-size: 20px;
  color: #333;
}

.notification-box {
  background-color: #e9e9e9;
  border-radius: 8px;
  margin-bottom: 10px;
  color: #333;
}

.right-sidebar-collapse-btn-region {
  position: relative;
  margin-left: 30px;
  margin-right: 0px;
  height: 100%;
  top: 0;
  display: flex;
  justify-content: center;
  align-items: center;
  border-right: 2px solid #d6e6f2;
}

.right-sidebar-collapse-btn-region-inner button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 35px;
  height: 35px;
  background-color: transparent;
  color: #5c5c5c;
}

.right-sidebar-collapse-btn-region-inner div {
  display: flex;
  align-items: center;
  justify-content: center;
  writing-mode: vertical-lr;
  margin-top: 5px;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: bold;
  color: #5c5c5c;
  letter-spacing: 4px;
}

.right-sidebar-collapse-btn-region-inner {
  width: 35px;
  padding: 0;
  border-radius: 0;
  border-top-left-radius: 10px;
  border-bottom-left-radius: 10px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background-color: #d6e6f2;
  cursor: pointer;
}

.right-sidebar {
  flex: 1;
  /* 右侧通知区域的相对宽度 */
  background-color: #ffffff;
  padding: 20px;
  min-width: 320px;
  border-radius: 8px;
  /* box-shadow: -2px 0 5px rgba(0, 0, 0, 0.05); */
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.rank-tips-wrapper {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: center;
  flex: 1;
  width: 100%;
}

.rank-area {
  height: 100%;
  padding: 5px;
  display: flex;
  flex: 0;
  justify-content: space-between;
  align-items: flex-start;
  width: 100%;
  gap: 2vw;
  border: 2px solid #b9d7ea75;
  border-radius: 15px;
  overflow: unset;
  transition: all 0.5s ease-in-out;
}

.rank-area.is-collapsed {
  height: 0;
  padding: 0;
  border-width: 1px;
  border-color: #b9d7ea;
  overflow: hidden;
  width: 40%;
}

.rank-box {
  height: 5vw;
  background-color: #769fcdA0;
  background-image: linear-gradient(120deg, #d9afd9 0%, #97d9e1 100%);
  border-radius: 8px;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  /* 添加鼠标悬停时的手型 */
  transition: all 0.3s ease;
  /* 平滑过渡 */
  width: 100%;
  /* color: #333 */
  color: #fff;
  text-shadow: 0px 0px 10px #649cdb;
  /* 默认宽度占满父容器 */
}

.rank-box:hover {
  background-color: #769fcd;
  /* 鼠标悬停时的背景色 */
  color: #fff;
  /* 鼠标悬停时的字体颜色 */
}

.rank-box.is-collapsed {
  background-color: #769fcd;
  /* 鼠标悬停时的背景色 */
  color: #fff;
  height: 40px;
  min-height: 40px;
  /* 缩小高度 */
  width: auto;
  /* 自动调整宽度 */
  padding: 0 10px;
  /* 添加内边距 */
  font-size: 14px;
  /* 缩小字体 */
}

.send-button-icon {
  margin-left: 7px;
  margin-bottom: 3px;
  width: 20px;
  height: 20px;
  background: url('@/assets/icons/send.svg') no-repeat center center;
  background-size: contain;
  border: none;
}

.send-button {
  width: 80px;
  height: 30px;
  border: none;
  padding-left: 10px;
  padding-right: 10px;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
}

.send-button:hover {
  background-color: #769fcd;
  color: #fff;
  /* 鼠标悬停时的字体颜色 */
}

.sidebar {
  flex: 1;
  min-width: 150px;
}

.gradient-text {
  font-size: 34px;
  padding-bottom: 20px;
  font-weight: bold;
  background:
    linear-gradient(90deg, rgb(125, 177, 236) 0%, rgb(216, 182, 249) 100%) 0%/200% 100%,
    linear-gradient(45deg, rgba(255, 0, 0, 0.427) 0%, rgba(0, 255, 0, 0.368) 100%) 0%/150% 200%;
  /* linear-gradient(90deg, rgb(138, 184, 238) 0%, rgb(229, 203, 255) 100%) 0%/200% 100%,
    linear-gradient(45deg, rgba(255, 0, 0, 0.3) 0%, rgba(0, 255, 0, 0.3) 100%) 0%/150% 200%; */
  animation:
    gradient1 8s linear infinite,
    gradient2 6s linear infinite;

  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;

  /* text-shadow: 0 0 1px #649cdb; */
  filter: drop-shadow(0 0 1px #cee5ff);
}

@keyframes gradient1 {

  0%,
  100% {
    background-position: 0% 50%, 0% 50%;
  }

  25% {
    background-position: 50% 0%, 30% 70%;
  }

  50% {
    background-position: 100% 50%, 60% 30%;
  }

  75% {
    background-position: 50% 100%, 90% 80%;
  }
}

@keyframes gradient2 {

  0%,
  100% {
    background-position: 0% 50%, 0% 50%;
  }

  33% {
    background-position: 50% 30%, 20% 80%;
  }

  66% {
    background-position: 100% 70%, 80% 20%;
  }
}

/* Vue 过渡动画 */
.slide-fade-enter-to,
.slide-fade-leave-from {
  max-height: 500px;
  margin-top: 10px;
  overflow: auto;
  padding: 10px;
}

.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.5s ease-in-out;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  opacity: 0;
  max-height: 0;
  margin-top: 0;
  padding-top: 0;
  padding-bottom: 0;
}

.tooltip-wrapper {
  cursor: pointer;
  width: 300px;
  height: 50px;
  margin-top: 2px;
  margin-bottom: 2px;
}

/* 用户消息样式 */
.user-message {
  background-color: #b9d7ea;
  padding: 10px;
  border-radius: 10px;
  max-width: 70%;
  margin-left: auto;
  margin-right: 0;
  text-align: right;
}

.ask-tips-trans-enter-active {
  transition: all 0.5s ease-in-out;
}

.ask-tips-trans-enter-from {
  opacity: 0;
  max-height: 0;
  padding: 0;
}

.collapse-rank-area-btn {
  height: 100%;
  width: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: transparent;
  border: 0;
  cursor: pointer;
}

.main-content-except-intro {
  height: calc(100% - 70px);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  width: 100%;
}

.ask-tips-region {
  max-height: 100%;
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  overflow: hidden;
}

/* 浮动小气泡基础样式 */
.floating-bubble {
  border-radius: 20px;
  padding: 8px 15px;
  margin: 5px;
  font-size: 14px;
  color: #333;
  cursor: pointer;
  transition: transform 0.3s ease-in-out, box-shadow 0.3s ease-in-out;
  border: 2px solid transparent;
  /* 默认透明边框 */
}

.floating-bubble:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
}

/* 每个气泡的独立样式 */
.bubble-0 {
  background-color: #ffffff;
  border-color: rgba(80, 43, 212, 0.4);
}

.bubble-0:hover {
  border-color: rgba(80, 43, 212, 0.8);
}

.bubble-1 {
  background-color: #ffffff;
  border-color: rgba(246, 81, 100, 0.4);
}

.bubble-1:hover {
  border-color: rgba(246, 81, 100, 0.8);
}

.bubble-2 {
  background-color: #ffffff;
  border-color: rgba(214, 0, 171, 0.4);
}

.bubble-2:hover {
  border-color: rgba(214, 0, 171, 0.8);
}

.bubble-3 {
  background-color: #ffffff;
  border-color: rgba(11, 109, 255, 0.4);
}

.bubble-3:hover {
  border-color: rgba(11, 109, 255, 0.8);
}

.bubble-4 {
  background-color: #ffffff;
  border-color: rgba(80, 43, 212, 0.4);
}

.bubble-4:hover {
  border-color: rgba(80, 43, 212, 0.8);
}

/* 淡入淡出动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.b-0 {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(128.87deg, #8a73e6 14.05%, #ff79c6 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.floating-bubble:hover>.b-0 {
  background: linear-gradient(128.87deg, #512bd4 14.05%, #d600aa 89.3%);
  ;
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.b-1 {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(128.87deg, #e68b99 14.05%, #8a73e6 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.floating-bubble:hover>.b-1 {
  background: linear-gradient(128.87deg, #cb4150 14.05%, #512bd4 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.b-2 {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(128.87deg, #ff79c6 14.05%, #e68b99 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.floating-bubble:hover>.b-2 {
  background: linear-gradient(128.87deg, #d600aa 14.05%, #cb4150 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.b-3 {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(128.87deg, #6ea8ff 14.05%, #8a73e6 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.floating-bubble:hover>.b-3 {
  background: linear-gradient(128.87deg, #0b6cff 14.05%, #512bd4 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.b-4 {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(128.87deg, #6ea8ff 14.05%, #8a73e6 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}

.floating-bubble:hover>.b-4 {
  background: linear-gradient(128.87deg, #cb4150 14.05%, #512bd4 89.3%);
  -webkit-background-clip: text;
  /* 裁剪背景到文字 */
  background-clip: text;
  -webkit-text-fill-color: transparent;
  /* 设置文字颜色为透明 */
  text-fill-color: transparent;

}
</style>

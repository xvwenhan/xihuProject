<template>
  <!-- 初始状态隐藏内容 -->
  <!-- <div v-if="!isTranslating"> -->
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" @toggleSettings="toggleSettings" @toggleUser="toggleUser" />
    <!-- <button @click="changeLanguage('en')">English</button>
    <button @click="changeLanguage('zh')">中文</button> -->
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-container class="content-container">
        <el-main class="main-content">
          <!-- 主要内容区域 -->
          <div class="intro">
            <div class="intro-icon">Icon</div>
            <div class="intro-text">我是西湖论剑，你的个人会议助手</div>
          </div>
          <div class="rank-area" v-if="messages.length === 0" style="flex: 1;">
            <div class="rank-box">
              热度排行
            </div>
            <div class="rank-box">
              个性推荐
            </div>
          </div>

          <!-- 对话框 -->
          <div class="chat-box" :style="messages.length > 0 ? 'flex: 1;' : ''">
            <!-- 消息列表 -->
            <div ref="msg_list_main" class="message-list main">
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
                  <el-button @click="sendMessage" class="send-button" type="primary">
                    <spam>发送</spam>
                    <spam class="send-button-icon" />
                  </el-button>
                </template>
              </el-input>
            </div>
          </div>



          <!-- 猜你想问 旧版 -->
          <!-- <div style="position: absolute; bottom: 10px; left: 10px; display: flex; flex-direction: column;">
            <div v-if="tooltipVisible" v-for="(tooltip, index) in questionToolTips" :key="index" class="tooltip-wrapper"
              @click="handleTooltipClick(tooltip)">
              <div class="custom-tooltip">
                {{ tooltip }}
              </div>
            </div>
            <div
              style="height: 65px; width: 65px; background-color: #aa96da; border-radius: 50%; display: flex; justify-content: center; align-items: center; flex-direction: column; cursor: pointer;"
              @click="getGuessAsk">
              <div style=" font-weight: 600; color: #ffffff;">猜你</div>
              <div style="font-weight: 600; color: #ffffff;">想问</div>
            </div>
          </div> -->

        </el-main>

        <!-- 右侧区域 -->
        <el-aside class="right-sidebar">
          <div class="notification-title">
            <spam class="notification-title-inner">{{ showNotifications }}</spam>
            <el-divider style="margin-top: 6px;" />
          </div>
          <!-- 会话历史 -->
          <template v-if="showNotifications === STATES.HISTORY">
            <div class="chat-container">
              <div class="message-list">
                <div v-if="history.length > 0" class="message-list">
                  <div v-for="(message, index) in history" :key="index" class="message-item">
                    <div :class="message.isUser ? 'user-message' : 'bot-message'">
                      {{ message.text }}
                    </div>
                  </div>
                </div>
                <div v-else class="empty-history-message">
                  暂无历史记录
                </div>



              </div>


              <!-- 分页控件 -->
              <el-pagination :current-page="page" :page-size="pageSize" :total="totalCount"
                layout="prev, pager, next, jumper" @current-change="handlePageChange" />
            </div>
          </template>
          <!-- 智能体设置 -->
          <template v-else-if="showNotifications === STATES.SETTINGS">
            <AvatarGenerator />
          </template>
          <!-- 议程通知 -->
          <template v-else-if="showNotifications === STATES.NOTIFICATIONS">
            <div v-if="notifications.length > 0" class="conference-notification-box">
              <div v-for="(notification, index) in notifications" :key="index" class="conference-notification-item">
                <div style="font-weight: bold;" class="conference-notification-item-inner">{{
                  notification.conference_name }}</div>
                <div class="conference-notification-item-inner">{{ notification.start_time }}</div>
              </div>
            </div>
            <div v-else>
              <p>暂无通知</p>
            </div>
          </template>
          <!-- 个人中心 -->
          <template v-else-if="showNotifications === STATES.USER">
            <div style="display: flex; justify-content: center; align-items: center; flex-direction: column;">
              <el-input style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['name']" placeholder="姓名"
                class="chat-input"></el-input>
              <el-input style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['phone']" placeholder="电话"
                class="chat-input"></el-input>
              <el-input style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['company']" placeholder="公司"
                class="chat-input"></el-input>
              <el-input style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['department']" placeholder="部门"
                class="chat-input"></el-input>
              <el-input style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['position']" placeholder="职位"
                class="chat-input"></el-input>
              <el-input readonly style="margin-top: 5px; margin-bottom: 5px;" v-model="userInfo['email']"
                placeholder="邮箱" class="chat-input"></el-input>
              <el-button
                style="margin-top: 10px; margin-bottom: 5px; margin-left: 10px; margin-right: 10px; width: 50%;"
                @click="setUserInfo" class="send-button" type="primary">保存</el-button>
            </div>
          </template>
        </el-aside>
      </el-container>
    </div>
  </div>
  <!-- </div>
<div v-else>Loading translations...</div> -->
  <!-- <button style="position: absolute; top: 10px; left: 10px;" @click="test">test</button> -->
</template>

<script setup>
import 'element-plus/dist/index.css';
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import Navbar from '@/components/Navbar.vue'
import Header from '@/components/Header.vue'
import AvatarGenerator from '@/components/AvatarGenerator.vue'
import { ElContainer, ElAside, ElDialog, ElMessage, ElLoading, ElPagination } from 'element-plus'
import 'element-plus/es/components/menu/style/css'
import 'element-plus/es/components/menu-item/style/css'
import api from '../../api/index.js';
import { ipcRenderer } from 'electron'

//import { useTranslation } from '@/composables/useTranslation'
// const { changeLanguage } = useTranslation()

function test() {
  let data1 = {
    conference_name: '会议名称',
    start_time: '2023-10-01 10:00:00',
  }
  ipcRenderer.send('save-notifications', data1);
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
const notifications = ref([]);
// 是否正在尝试重新连接
let isReconnecting = false;
const receivedNotifications = new Set(); // 已接收通知的缓存（存储 JSON 字符串）
// SSE 重连的最大尝试次数（可以设置为 Infinity 表示无限重试）
const MAX_RECONNECT_ATTEMPTS = Infinity;

// SSE 重连的延迟时间（毫秒）
const RECONNECT_DELAY = 5000; // 5 秒


const messages = ref([])
const page = ref(1); // 当前页数
const pageSize = ref(10); // 每页显示的消息数量
const totalCount = ref(0); // 总消息数
const history = ref([])
const rankMeetings = ref([]) // 热度排行
const tooltipVisible = ref(false)
const questionToolTips = ref([])

const msg_list_main = ref()

const userInfo = ref({
  name: '',
  phone: '',
  company: '',
  department: '',
  position: '',
  email: '',
})

////////////////////////////////////窗口切换////////////////////////////////////

const STATES = {
  HISTORY: '会话历史',
  SETTINGS: '智能体设置',
  NOTIFICATIONS: '议程通知',
  USER: '个人中心'
}
const showNotifications = ref(STATES.HISTORY)

// 切换函数
const toggleState = (newState) => {
  if (showNotifications.value === newState) {
    showNotifications.value = STATES.HISTORY
    getChatHistory();
  } else {
    showNotifications.value = newState
  }
}

//sse连接
// 格式化时间
const formatTime = (time) => {
  const date = new Date(time);
  return date.toLocaleString(); // 将时间格式化为本地时间字符串
};
// 启动 SSE 连接
const startSSEWithFetch = async () => {
  const token = localStorage.getItem('token'); // 获取 JWT
  if (!token) {
    console.error('未找到有效的 JWT');
    return;
  }

  let reconnectAttempts = 0; // 当前重连尝试次数

  const connect = async () => {
    try {
      const response = await fetch('https://localhost:5000/api/user/private/sse', {
        method: 'GET',
        headers: {
          'Accept': 'text/event-stream',
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        console.error('SSE 请求失败:', response.statusText);
        throw new Error(`HTTP 错误: ${response.status}`);
      }

      console.log('SSE 连接成功');

      // 从持久化保存数据中获取已接收的通知
      await read_saved_notifications(receivedNotifications);
      // console.log('已接收通知:', receivedNotifications);

      // 更新通知列表
      notifications.value = Array.from(receivedNotifications).map(notification => JSON.parse(notification));
      // console.log('通知列表:', notifications.value);

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = ''; // 缓存未完成的消息

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        // 解码数据并拼接到缓冲区
        buffer += decoder.decode(value, { stream: true });

        // 按行分割数据
        const lines = buffer.split('\n');
        buffer = lines.pop(); // 保留最后一行（可能未完成）

        lines.forEach(line => {
          if (line.startsWith('data:')) {
            const dataStr = line.slice(5).trim();

            // 忽略空数据或心跳消息
            if (!dataStr || dataStr === ': heartbeat') {
              console.log('收到心跳消息，不触发通知。');
              return;
            }

            try {
              // 解析消息数据
              const data = JSON.parse(dataStr);

              // 将通知对象序列化为字符串，用于去重比较
              const notificationKey = JSON.stringify(data);

              // 检查是否已经接收过该通知
              if (receivedNotifications.has(notificationKey)) {
                console.log('重复的通知，已忽略:', data);
                return;
              }

              // 添加到已接收通知缓存
              receivedNotifications.add(notificationKey);

              // 更新通知列表
              notifications.value.push(data);
              console.log('新增通知:', data);
              ipcRenderer.send('save-notifications', data);
            } catch (error) {
              console.error('无法解析消息数据:', error);
            }
          }
        });
      }
    } catch (error) {
      console.error('SSE 错误:', error);

      // 如果未超出最大重连次数，则尝试重新连接
      if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
        reconnectAttempts++;
        isReconnecting = true;

        console.log(`尝试重新连接 (${reconnectAttempts}/${MAX_RECONNECT_ATTEMPTS})...`);
        setTimeout(connect, RECONNECT_DELAY); // 延迟一段时间后重连
      } else {
        console.error('已达到最大重连次数，停止重连。');
      }
    }
  };

  // 初始连接
  connect();
};

// 在组件挂载时启动 SSE
onMounted(() => {
  startSSEWithFetch();
  getGuessAsk();
  fetchRanking();
});

// 在组件卸载时关闭 SSE 连接
onUnmounted(() => {
  console.log('SSE 连接已关闭。');
});

// 图标点击处理
const toggleSettings = () => {
  toggleState(STATES.SETTINGS)
}

const toggleNotifications = () => {
  toggleState(STATES.NOTIFICATIONS)
}

const toggleUser = () => {
  toggleState(STATES.USER)
  if (STATES.USER === showNotifications.value) {
    getUserInfo()
  }
}

//获取猜你想问
const getGuessAsk = async () => {
  console.log("猜你想问")
  const loading = ElLoading.service({
    lock: true,
    text: '获取猜你想问...',
    background: 'rgba(255, 255, 255, 0.7)',
  });
  try {
    const data = await api.get('/chat/public/guessAsk');
    console.log(data)
    if (data.status === 200) {
      questionToolTips.value = data.data.data;
    }
    console.log(questionToolTips.value)
  } catch (error) {
    console.error(error);
    let errorMessage = ref('获取猜你想问失败，请稍后重试');
    ElMessage.error(errorMessage.value);
  } finally {
    loading.close();
  }
}

//获取个人信息
const getUserInfo = async () => {
  const loading = ElLoading.service({
    lock: true,
    text: '数据同步中...',
    background: 'rgba(255, 255, 255, 0.7)',
  });
  try {
    const data = await api.get('/user/private/getUserInfo');
    console.log(data)
    if (data.status === 200) {
      userInfo.value = data.data.data;
    }
    console.log(userInfo.value)
  } catch (error) {
    console.error(error);
    let errorMessage = ref('获取个人信息失败，请稍后重试');
    ElMessage.error(errorMessage.value);
  } finally {
    loading.close();
  }
}

const setUserInfo = async () => {
  try {
    const data = await api.post('/user/private/setUserInfo', userInfo.value);
    if (data.code === 200) {
      ElMessage.success('保存成功');
    } else {
      ElMessage.error('保存失败，请稍后重试');
    }
  } catch (error) {
    ElMessage.error('保存失败，请稍后重试');
  }
}

// 发送消息的函数
const sendMessage = async () => {
  if (!chatInput.value.trim()) {
    ElMessage.warning('请输入问题');
    return;
  }
  tooltipVisible.value = false; // 隐藏猜你想问提示框

  // 用户消息
  const userMessage = {
    text: chatInput.value,
    isUser: true
  };
  messages.value.push(userMessage);
  let chatinput_temp = chatInput.value
  chatInput.value = '';

  // // 测试消息
  // messages.value.push({
  //   text: '测试消息',
  //   isUser: false
  // });
  // // 猜你想问
  // getGuessAsk();
  // tooltipVisible.value = true; // 显示猜你想问提示框
  // nextTick(() => {
  //   msg_list_main.value.scrollTop = msg_list_main.value.scrollHeight; // 滚动到底部
  // })
  // return;

  try {
    const { data } = await api.post('/chat/private/execute', {
      input: chatinput_temp
    });

    if (data.success) {
      console.log("返回", data)
      // 获取返回的消息
      //const sessionMessages = data.data.answer;
      const botMessage = data.data.answer;
      console.log("botMessage", botMessage)

      // 获取机器人回复的消息
      //const botMessage = sessionMessages.find(message => message.role === 'assistant');

      if (botMessage) {

        const botResponse = {
          text: botMessage,
          isUser: false
        };
        messages.value.push(botResponse);
      }
      //messages.value.push(botResponse);

      // 猜你想问
      getGuessAsk();
      tooltipVisible.value = true; // 显示猜你想问提示框
      console.log("tooltipVisible", tooltipVisible.value)
      nextTick(() => {
        msg_list_main.value.scrollTop = msg_list_main.value.scrollHeight; // 滚动到底部
      })
    } else {
      ElMessage.error(data.message);
    }
  } catch (error) {
    ElMessage.error('发送失败');
    console.error(error);
    chatInput.value = chatinput_temp;
  }
};

// 获取聊天历史
const getChatHistory = async () => {
  try {
    const response = await api.get('/chat/private/getChatLogs', {
      params: {
        page: page.value,
        pageSize: pageSize.value
      }
    });

    if (response.data.success) {
      console.log("历史", response.data.data.data)
      const sessionMessages = response.data.data.data;
      sessionMessages.forEach(message => {
        // 格式化每条消息
        history.value.push({
          text: message.question,
          isUser: true // 判断消息是用户还是机器人
        });
        history.value.push({
          text: message.answer,
          isUser: false // 判断消息是用户还是机器人
        });
      });

      totalCount.value = response.data.data.totalCount;
      page.value = response.data.data.page;

    } else {
      ElMessage.error(response.data.message || '加载失败');
    }
  } catch (error) {
    ElMessage.error('请求失败');
    console.error(error);
  }
};
getChatHistory();

// 页码变更
const handlePageChange = (newPage) => {
  page.value = newPage;
  getChatHistory(); // 请求新页面的数据
};

// 处理猜你想问点击
const handleTooltipClick = (tooltip) => {
  chatInput.value = tooltip;
  sendMessage();
};

// 获取热度排行
const fetchRanking = async () => {
  try {
    const response = await api.get("/user/private/sort", {
      params: {
        SortByTime: false,
        IsAsc: false,
      }
    });
    rankMeetings.value = response.data.data;
  } catch (error) {
    console.error("获取订阅会议失败:", error);
    ElMessage.error("获取订阅会议失败");
  }
};

// onMounted(async () => {
//   await nextTick()
//   translate.execute()
//   isTranslating.value = false
// })

// const changeLanguage = (lang) => {
//   if (window.translate) {
//     window.translate.changeLanguage(lang);
//   }
// };
</script>

<style scoped>
/* 强制隐藏可能重复生成的容器 */
:deep([translated="true"])+[translated="false"],
:deep([data-translated])~[data-translated] {
  display: none !important;
}

.home {
  display: flex;
  flex-direction: column;
  --el-header-height: 60px;
  --notification-title-height: 40px;
  --chat-input-height: 40px;
  width: 100vw;
}

.sidebar {
  flex: 1;
}

.layout-container {
  display: flex;
  flex-direction: row;

  /* 让布局容器填满整个视口高度 */
  height: calc(100vh - var(--el-header-height));
}


.content-container {
  display: flex;
  flex: 6;
  /* 主要内容区域占满剩下的空间 */
  padding: 20px;
  height: 100%;
  width: 100%;
  /* 内容区域填满 */
  align-items: stretch;
  /* 保证子项填满容器 */
}

.right-sidebar {
  flex: 1;
  /* 右侧通知区域的相对宽度 */
  background-color: #ffffff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: -2px 0 5px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.main-content {
  display: flex;
  flex-direction: column;
  height: 100%;
  max-width: 70%;
  /* 确保 main-content 填满父容器高度 */
  padding: 20px;
  background-color: #ffffff;
  border-radius: 8px;
  overflow: hidden;
  /* 防止内容溢出 */
}


/* 引入部分 */
.intro {
  margin-bottom: 20px;
  /* 与下方内容保持一定间距 */
}

.chat-box {
  position: relative;
  display: flex;
  flex-direction: column;
  margin-top: 10px;
  max-height: calc(100% - 100px);
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
.message-list.main {
  height: calc(100% - var(--chat-input-height) - 20px);
}

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
}

.intro-text {
  font-size: 18px;
  font-weight: bold;
  color: #333;
}

.rank-area {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 5vw;
}

.rank-box {
  flex: 1;
  height: 5vw;
  background-color: #cdc8c8;
  border-radius: 8px;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 16px;
  font-weight: bold;
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

/* 用户消息样式 */
.user-message {
  background-color: #dbd8d8;
  padding: 10px;
  border-radius: 10px;
  max-width: 70%;
  margin-left: auto;
  margin-right: 0;
  text-align: right;
}

/* 机器人消息样式 */
.bot-message {
  background-color: #e9d7f5;
  padding: 10px;
  border-radius: 10px;
  max-width: 70%;
  margin-left: 0;
  margin-right: auto;
}



/* 分页控件 */
.el-pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
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

/* 弹出框 */
.custom-tooltip {
  margin-top: 3px;
  background-color: #cbf1f5;
  padding: 10px;
  border-radius: 10px;
  display: inline-block;
  max-width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1);
}

.tooltip-wrapper {
  cursor: pointer;
  width: 300px;
  height: 50px;
  margin-top: 2px;
  margin-bottom: 2px;
}

.conference-notification-box {
  overflow-y: auto;
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
</style>

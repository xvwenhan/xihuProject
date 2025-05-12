<template>
  <!-- <button style="position: absolute; top:10px; left: 10px;" @click="switchToHome">@TEST 跳转到home</button> -->

  <div class="page-wrapper">
    <div class="slider-container">
      <!-- 登录表单部分 -->
      <div class="form-section login-form" :class="{ 'inactive': isSwapped }">
        <h2 class="form-title">登录</h2>
        <el-form :model="form" ref="form" class="auth-form">
          <el-form-item>
            <el-input v-model="email" placeholder="Email"></el-input>
          </el-form-item>
          <el-form-item>
            <el-input v-model="password"
            placeholder="Password"
            type="password"
            :disabled="false"
            @keyup.enter="onSignIn"></el-input>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" class="action-btn" @click="onSignIn">登录</el-button>
          </el-form-item>
          <el-form-item>
            <el-button type="success" class="action-btn wechat-btn" @click="onClickFollow">
              <i class="el-icon-wechat"></i>
              微信登录
            </el-button>
          </el-form-item>
        </el-form>
      </div>

      <!-- 图片和切换按钮部分 -->
      <div class="image-section" :class="{ 'swapped': isSwapped }">
        <img :src="loginIcon" alt="Decorative" class="decorative-image" />
        <button class="toggle-btn" @click="toggleSwap">
          {{ isSwapped ? '登&nbsp;&nbsp;录' : '注&nbsp;&nbsp;册' }}
        </button>
      </div>

      <!-- 注册表单部分 -->
      <div class="form-section signup-form" :class="{ 'inactive': !isSwapped }">
        <h2 class="form-title">注册</h2>
        <el-form :model="form" class="auth-form">
          <el-form-item>
            <el-input v-model="email" placeholder="Email"></el-input>
          </el-form-item>
          <el-form-item>
            <el-input v-model="password" placeholder="Password" type="password"></el-input>
          </el-form-item>
          <el-form-item>
              <el-input v-model="code" placeholder="Code" @keyup.enter="onSignUp"></el-input>
          </el-form-item>
          <el-form-item>
              <el-button @click="sendVerificationCode">发送验证码</el-button>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" class="action-btn" @click="onSignUp">注册</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>

    <!-- 添加公众号二维码对话框 -->
    <el-dialog v-model="qrDialogVisible" title="关注公众号" width="350px" :close-on-click-modal="false" center>
      <div class="qrcode-container">
        <img src="@/assets/qrcode.jpg" alt="公众号二维码" class="qr-img" />
        <p class="qrcode-tip">获取最新资讯和功能提醒</p>
        <el-button @click="onWechatLogin" type="primary">已关注公众号，微信扫码登录</el-button>
      </div>
    </el-dialog>

    <!-- 添加二维码对话框 -->
    <el-dialog v-model="wechatDialogVisible" title="微信扫码登录" width="350px" :close-on-click-modal="false" center>
      <div class="qrcode-container">
        <canvas ref="qrcodeCanvas"></canvas>
        <p class="qrcode-tip">请使用微信扫描二维码登录</p>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { reactive, ref, nextTick, onUnmounted,watch } from "vue";
import { ElMessage, ElInput, ElButton, ElDialog } from "element-plus";
import api_nojwt from '../../api/index_nojwt.js';
import api from '../../api/index.js';
import { useRouter } from 'vue-router'
import QRCode from 'qrcode';
import loginIcon from '@/assets/pic/login.png';

const router = useRouter()

const email = ref("");
const password = ref("");
const code = ref("");
// 表单数据
const form = reactive({
  email: "",
  password: "",
  code: "",
});

// 是否切换至注册
const isSwapped = ref(false);

// 切换状态
const toggleSwap = () => {
  isSwapped.value = !isSwapped.value;
};

// 点击登录
const onSignIn = async () => {
  try {
    const { data } = await api_nojwt.post('/user/public/login', {
      account: email.value,
      password: password.value,
    });

    if (data.success) {
      ElMessage.success('登录成功');
      localStorage.setItem('token', data.data.token);
      isSwapped.value = false; // 切换到登录页面
      router.push('/home'); // 替换为你想跳转的路径
    } else {
      ElMessage.error(data.message);
    }
  } catch (error) {
    ElMessage.error('登录失败');
    console.error(error);
  }
};

// 发送验证码
const sendVerificationCode = async () => {
  try {
    const { data } = await api.post('/user/public/send-verification-code', {
      email: email.value
    });

    if (data.success) {
      ElMessage.success('验证码已发送到您的邮箱');
    } else {
      ElMessage.error(data.message);
    }
  } catch (error) {
    ElMessage.error('发送验证码失败');
    console.error(error);
  }
};


const onSignUp = async () => {
  try {


    // 验证成功：继续注册逻辑
    const { data } = await api.post('/user/public/register', {
      email: email.value,
      password: password.value,
      code: code.value
    });

    // 检查注册结果
    if (data.success) {
      ElMessage.success('注册成功');
    } else {
      ElMessage.error(data.message);
    }
  } catch (error) {
    ElMessage.error('注册失败');
    console.error(error);
  }
};


// 添加二维码相关的响应式变量
// 控制公众号二维码对话框的显示与隐藏
const qrDialogVisible = ref(false);
// 控制微信扫码二维码对话框的显示与隐藏
const wechatDialogVisible = ref(false);
const qrcodeCanvas = ref(null);
const currentState = ref('');
const stateCheckInterval = ref(null);
const qrExpireTimeout = ref(null); // 新增定时器引用

// 出现公众号
const onClickFollow = async () => {
  qrDialogVisible.value = true; // 显示公众号二维码对话框
};

// 修改微信登录方法
const onWechatLogin = async () => {
  try {

    qrDialogVisible.value = false;
    console.log('开始获取微信登录URL');
    const response = await fetch('https://8.133.201.233/api/WeChatAuth/login-url', {
      method: 'GET',
      headers: {
        'Accept': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    console.log('获取到的微信登录URL:', data);

    // 显示对话框
    wechatDialogVisible.value = true;

    // 在下一个 tick 后生成二维码
    await nextTick();
    if (qrcodeCanvas.value) {
      await QRCode.toCanvas(qrcodeCanvas.value, data.url, {
        width: 300,
        margin: 2,
        color: {
          dark: '#000000',
          light: '#ffffff'
        }
      });

      // 从URL中提取state参数
      const urlObj = new URL(data.url);
      currentState.value = urlObj.searchParams.get('state');
      console.log('获取到的state参数:', currentState.value);

      // 开始轮询检查登录状态
      if (currentState.value) {
        startPolling();
      }
    }
  } catch (error) {
    console.error('获取微信登录二维码失败:', error);
    ElMessage.error('获取微信登录二维码失败');
  }
};

// 开始轮询检查登录状态
const startPolling = () => {
  // 清除可能存在的之前的轮询
  if (stateCheckInterval.value) {
    clearInterval(stateCheckInterval.value);
  }

  // 每3秒检查一次登录状态
  stateCheckInterval.value = setInterval(() => {
    checkLoginState();
  }, 3000);

  // 60秒后停止轮询
  qrExpireTimeout.value = setTimeout(() => {
      clearInterval(stateCheckInterval.value);
      ElMessage.error('二维码已过期，请重新扫描');
      wechatDialogVisible.value = false;
  }, 60000);
};

// 检查登录状态
const checkLoginState = async () => {
  try {
    console.log('开始检查登录状态，state:', currentState.value);
    const response = await fetch(`https://8.133.201.233/api/WeChatAuth/check-state?state=${currentState.value}`);
    const data = await response.json();
    console.log('检查登录状态数据:', data);

    if (data.isLoggedIn && data.data) {
      // 登录成功，清除轮询
      clearInterval(stateCheckInterval.value);
      if (qrExpireTimeout.value) {
        clearTimeout(qrExpireTimeout.value);
        qrExpireTimeout.value = null;
      }
      // 保存token和用户信息
      localStorage.setItem('token', data.data.token);
      localStorage.setItem('userInfo', JSON.stringify(data.data.user));

      // 关闭二维码对话框
      wechatDialogVisible.value = false;

      // 显示成功消息
      ElMessage.success('微信登录成功');

      // 跳转到首页
      router.push('/home');
    } else {
      console.log('登录未完成:', data);
    }
  } catch (error) {
    console.error('检查登录状态失败:', error);
  }
};

// @TEST 测试用！跳转至首页
const switchToHome = () => {
  router.push('/home');
};

watch(wechatDialogVisible, (newVal) => {
  if (!newVal) {
    // 用户关闭了二维码对话框，清除轮询和过期提示
    if (stateCheckInterval.value) {
      clearInterval(stateCheckInterval.value);
      stateCheckInterval.value = null;
    }
    if (qrExpireTimeout.value) {
      clearTimeout(qrExpireTimeout.value);
      qrExpireTimeout.value = null;
    }
  }
});

// 在组件卸载时清除轮询
onUnmounted(() => {
  if (stateCheckInterval.value) {
    clearInterval(stateCheckInterval.value);
  }
  if (qrExpireTimeout.value) {
    clearTimeout(qrExpireTimeout.value);
  }
});

</script>

<style scoped>
/* 页面整体样式 */
.page-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  width: 100vw;
  background-color: #f9f9f9;
}

.slider-container {
  position: relative;
  width: 800px;
  height: 400px;
  background-color: #ffffff;
  display: flex;
  overflow: hidden;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(212, 187, 239, 0.2);
}

/* 表单基础样式 */
.form-section {
  position: absolute;
  width: 50%;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  transition: transform 0.5s ease-in-out;
  z-index: 100;
  opacity: 1;
  /* 默认启用交互 */
  pointer-events: auto;
}

/* 注册表单 → 默认左侧 */
.signup-form {
  left: 0;
  transform: translateX(0%);
  opacity: 1;
  /* 默认启用交互 */
  pointer-events: auto;
}

.signup-form.inactive {
  transform: translateX(-100%);

}

/* 登录表单 → 默认右侧 */
.login-form {
  right: 0;
  transform: translateX(0%);
  opacity: 1;
  /* 默认启用交互 */
  pointer-events: auto;

}

.login-form.inactive {
  transform: translateX(100%);

}

/* 图片部分 */
.image-section {
  position: relative;
  width: 50%;
  height: 100%;
  background-color: #f9f9f9;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  transition: transform 0.5s ease-in-out;
  z-index: 0;
}

.image-section.swapped {
  transform: translateX(100%);
}

/* 装饰图片样式 */
.decorative-image {
  width: 70%;
  margin-bottom: 20px;
}

/* 切换按钮样式 */
.toggle-btn {
  position: absolute;
  bottom: 30px;
  border: 2px solid #0d7377;
  padding: 5px 25px;
  color: #0d7377;
  background: transparent;
  border-radius: 8px;
  font-weight: bold;
  font-size: 15px;
  cursor: pointer;
  transition: background-color 0.3s, color 0.3s;
}

.toggle-btn:hover {
  background-color: #0d7377;
  color: #ffffff;
}

/* 表单标题 */
.form-title {
  font-size: 24px;
  margin-bottom: 20px;
  color: #333;
}

/* 表单内容样式 */
.auth-form {
  width: 80%;
}

/* 提交按钮样式 */
.action-btn {
  width: 100%;
  background-color: #333;
  color: #ffffff;
  border-radius: 8px;
  padding: 10px 15px;
}

.action-btn:hover {
  background-color: #555;
}

.wechat-btn {
  width: 100%;
  background-color: #07C160 !important;
  border-color: #07C160 !important;
}

.wechat-btn:hover {
  background-color: #06ae56 !important;
  border-color: #06ae56 !important;
}

.qrcode-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px;
}

.qrcode-tip {
  margin-top: 15px;
  color: #666;
  font-size: 14px;
}

:deep(.el-dialog) {
  border-radius: 8px;
}

:deep(.el-dialog__header) {
  text-align: center;
  margin-right: 0;
  padding: 20px;
}

:deep(.el-dialog__headerbtn) {
  font-size: 18px;
}

:deep(.el-dialog__body) {
  padding: 0;
}

.qrcode-container {
  text-align: center;
}

.qr-img {
  width: 180px;
  height: 180px;
  object-fit: contain;
  margin-bottom: 10px;
}

.qrcode-tip {
  font-size: 14px;
  color: #666;
  margin-top: 8px;
  margin-bottom: 15px;
}
</style>

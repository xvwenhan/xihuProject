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
  </div>
</template>

<script setup>
import { reactive, ref, nextTick, onUnmounted,watch } from "vue";
import { ElMessage, ElInput, ElButton, ElDialog } from "element-plus";
import api_nojwt from '../../api/index_nojwt.js';
import api from '../../api/index.js';
import { useRouter } from 'vue-router'
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

const stateCheckInterval = ref(null);
const qrExpireTimeout = ref(null); // 新增定时器引用


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
</style>

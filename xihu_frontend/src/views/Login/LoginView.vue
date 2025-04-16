<template>
  <!-- <button style="position: absolute; top:10px; left: 10px;" @click="switchToHome">@TEST 跳转到home</button> -->

  <div class="page-wrapper">
   登录
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



</script>

<style scoped>
</style>

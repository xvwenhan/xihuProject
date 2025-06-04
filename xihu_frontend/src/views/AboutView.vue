<template>
    <div>
        <Navbar />
        <h1>登录页面</h1>
    </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue';
import { loginApi } from '@/api/auth';

const testApi = async () => {
    try {
        const response = await loginApi({ username: 'test', password: '123456' });
        console.log('后端返回数据：', response);
    } catch (error) {
        console.error('调用失败：', error);
    }
};
</script>

<style scoped>
button {
    color: white;
    background: green;
    padding: 10px 20px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
}
</style>

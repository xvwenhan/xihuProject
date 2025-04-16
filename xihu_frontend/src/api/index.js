import axios from 'axios';

const API_BASE_URL = 'https://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 5000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// 请求拦截器：自动携带 JWT 令牌
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token'); // 获取 JWT 令牌
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 响应拦截器：处理 401 认证错误
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('token'); // 清除过期 token
      window.location.href = '/login'; // 跳转到登录页面
    }
    return Promise.reject(error);
  }
);

export default api;

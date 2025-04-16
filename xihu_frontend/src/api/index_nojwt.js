import axios from 'axios';

const API_BASE_URL = 'https://localhost:5000/api'; 

const api_nojwt = axios.create({
  baseURL: API_BASE_URL,
  timeout: 50000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default api_nojwt;

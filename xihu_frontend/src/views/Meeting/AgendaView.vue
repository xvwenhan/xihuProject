<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
   <div class="layout-container">
     <!-- 左侧导航栏 -->
     <Navbar class="sidebar" />
       
     <!-- 主要内容与右侧区域 -->
     <el-container class="content-container">
          <div class="sort">
            <div class="sort-item" @click="sortByTime">按时间排序</div>
            <div>|</div>
            <div class="sort-item" @click="sortByHot">按热度排序</div>
          </div>
          <div class="agendalist">
            <div class="agenda-item" v-for="meeting in meetings" :key="i">
              <div class="info-container">         
                <div class="time">{{ meeting.time }}</div>
                <div style="display: flex;flex-direction: row;">
                  <div style="font-size: 20px;">{{ meeting.name }}</div>
                  <div style="display: flex;flex-direction: row;padding:5px;">30/
                    <div style="color: #999;text-decoration: underline;">{{ meeting.offlineNum }}</div>
                  </div>
                </div>
                <div style="font-size: 13px;color: #999;">类型:{{ meeting.type }}</div>
                <div style="font-size: 13px;color: #999;">地点:{{ meeting.location }}</div>
              </div>
              <div class="button-container">
                <button class="button" @click="subscribeMeeting(meeting.id)">订阅</button>
                <button class="button">跳转</button>
              </div>
            </div>
          </div>
     </el-container>
   </div>
 </div>
</template>

  
<script setup>
  import Navbar from '@/components/Navbar.vue'
  import Header from '@/components/Header2.vue'
  import { ElContainer,ElMessage} from 'element-plus'
  import 'element-plus/es/components/menu/style/css'
  import 'element-plus/es/components/menu-item/style/css'
  import api from '../../api/index.js'
  import { onMounted,reactive,ref } from 'vue'

  let meetings=ref([])
  let isAsc=ref(true)
  onMounted(() => {
    //开始按时间升序
    getSortedMeetings(true);
  })

  const getSortedMeetings = async (SortByTime) => {
    try {
      const {data} = await api.get('/user/private/sort', {
        params: {
          SortByTime: SortByTime,
          IsAsc: isAsc.value
        }
      });
      isAsc.value=!isAsc.value;
      meetings.value=data.data;
      console.log(meetings.value);
    } catch (error) {
      let errorMessage = ref('An unknown error occurred');
      ElMessage.error(errorMessage.value);
    }
  }

  const sortByTime = async() => {
    getSortedMeetings(true);
  }

  const sortByHot = async() => {
    getSortedMeetings(false);
  }

  const subscribeMeeting = async(meetingId) => {
    console.log(meetingId);
    try {
      const {data} = await api.post('/user/private/subscribe', {
        conferenceId: meetingId
      });
      ElMessage.success(data.message);
    } catch (error) {
      let errorMessage = ref('订阅失败，请稍后重试');
      ElMessage.error(errorMessage.value);
    }
  }
</script>
  
<style scoped>
.home {
  display: flex;
  flex-direction: column;
  min-height: 100vh; 
  background-color: #FFFFFF;
}
  
.layout-container {
  display: flex;
  flex: 1; /* 占据除header外的所有空间 */
  position: relative; 
}
  
.sidebar {
  position: fixed; /* 固定在左侧 */
  left: 0;
  top: 60px; 
  bottom: 0;
}
  
.content-container {
  flex: 1;
  margin: 20px 30px 20px 280px;
  padding: 40px;
  border-radius: 30px;
  border: 1px solid #E5E5E5;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  color: #000;
  /* 视口高度-header -margin上 -margin下 */
  height: calc(100vh - 64px - 20px - 20px);
  
  min-width: 800px; 
   /* 视口宽度-左边距-右边距 */
  max-width: calc(100vw - 280px - 30px);
  overflow-x: auto; 
  
  display: flex;
  flex-direction: column;
}

.sort {
  display: flex;
  flex-direction: row;
  margin-bottom: 20px;
}

.sort-item {
  cursor: pointer;
  margin-left: 10px;
  margin-right: 10px;
}

.agendalist{
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  color: #000;
  border-radius: 20px;
  /* 滚动条设置 */
  scrollbar-width: none; 
  -ms-overflow-style: none; 
}

.agendalist::-webkit-scrollbar {
  display: none; 
}

.agenda-item{
  border-bottom: 2px solid #E5E5E5;
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  padding: 15px;
  min-width: 700px; 
}

.info-container {
  flex: 1;
  min-width: 0; 
  margin-right: 20px;
}

.button-container{
  display: flex;
  flex-direction: column;
  gap: 10px;
  width:100px;
  margin-right: 50px;
  padding:15px 0px;
}
.button{
  width: 100%;
  height: 30px;
  border-radius: 20px;
  border: 2px solid #E5E5E5;
  background-color: #FFFFFF;
  cursor: pointer;
  transition: border-color 0.3s;
  font-size: 15px;
}

.button:hover {
  border-color: #BDBDBD;
}
</style>
  
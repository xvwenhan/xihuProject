<template>
  <div class="home">
    <Header @toggleNotifications="toggleNotifications" />
    <div class="layout-container">
      <!-- 左侧导航栏 -->
      <Navbar class="sidebar" />

      <!-- 主要内容与右侧区域 -->
      <el-container class="content-container">
        <div class="content-container-inner" v-if="!isLoading">
          <div class="sort">
            <div :class="!isSortByTime?'sort-item':'sort-item-active'" @click="sortByTime">按时间排序</div>
            <div>|</div>
            <div :class="isSortByTime?'sort-item':'sort-item-active'" @click="sortByHot">按热度排序</div>
            <img src="@/assets/icons/up.svg" alt="up" class="sort-icon" v-if="isAsc">
            <img src="@/assets/icons/down.svg" alt="down" class="sort-icon" v-else>
          </div>
          <div class="agendalist">
            <div class="agenda-item" v-for="meeting in meetings" :key="i">
              <div class="info-container">
                <div class="time">{{ meeting.time }}</div>
                <div style="display: flex;flex-direction: row;">
                  <div style="font-size: 20px;font-weight: bold;">{{ meeting.name }}</div>
                  <div style="display: flex;flex-direction: row;padding:5px;">
                    <div style="color: #999;font-weight: bold;">{{ meeting.offlineNum }}</div>/30
                  </div>
                </div>
                <div style="font-size: 13px;color: #999;">类型:{{ meeting.type }}</div>
                <div style="font-size: 13px;color: #999;">地点:{{ meeting.location }}</div>
              </div>
              <div class="button-container">
                <button :class="meeting.isSubscribed?'active-button':'button'" @click="toggleSubscription(meeting)">
                  {{meeting.isSubscribed?"已订阅":"订阅"}}</button>
                <button :class="meeting.url?'button':'unactive-button'" :disabled="!meeting.url"
                  :title="!meeting.url ? '会议仅线下，无跳转链接' : ''" @click="redirectToUrl(meeting.url)">跳转</button>
              </div>
            </div>
          </div>
        </div>
        <div class="content-container-inner" v-else>
          <loading />
        </div>
      </el-container>
    </div>
  </div>
</template>


<script setup>
  import Navbar from '@/components/Navbar.vue'
  import Header from '@/components/Header.vue'
  import { ElContainer,ElMessage} from 'element-plus'
  import 'element-plus/es/components/menu/style/css'
  import 'element-plus/es/components/menu-item/style/css'
  import api from '../../api/index.js'
  import { onMounted,reactive,ref } from 'vue'


  let meetings=ref([])
  let isAsc=ref(true)
  let isLoading=ref(true)
  let isSortByTime=ref(true)
  onMounted(() => {
    //开始按时间升序
    isLoading.value=true;
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
      isLoading.value=false;
    } catch (error) {
      let errorMessage = ref('获取会议失败，请稍后重试');
      ElMessage.error(errorMessage.value);
    }
  }

  const sortByTime = async() => {
    isSortByTime.value=true;
    isLoading.value=true;
    getSortedMeetings(true);
  }

  const sortByHot = async() => {
    isSortByTime.value=false;
    isLoading.value=true;
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

  const toggleSubscription = async(meeting) => {
    meeting.isSubscribed = !meeting.isSubscribed;
    subscribeMeeting(meeting.id);
  }

  const redirectToUrl = (url) => {
    if(url){
      window.open(url, '_blank');
    }else{
      ElMessage.error('url缺失');
    }
  }
</script>

<style scoped>
@import '@/assets/button.css';

.home {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #FFFFFF;
}

.layout-container {
  display: flex;
  /* flex: 1; */
  position: relative;
}

.sidebar {
  /* position: fixed; */
  flex:1;
}


.content-container {
  display: flex;
  flex: 6;
  padding: 20px;
  height: 100%;
  width: 100%;
  align-items: stretch;
}

.content-container-inner{
  padding: 40px;
  margin: 0px 20px;
  border-radius: 30px;
  border: 1px solid #E5E5E5;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  color: #053d5d;
  height: calc(100vh - 50px - 20px - 20px);
  width: 100%;
  overflow-x: auto;

  display: flex;
  flex-direction: column;
}

.sort {
  display: flex;
  flex-direction: row;
  margin-bottom: 20px;
  align-items: center;
}

.sort-item,.sort-item-active {
  cursor: pointer;
  margin-left: 10px;
  margin-right: 10px;
}

.sort-item-active{
  color: #053d5d;
  font-weight: bold;
}

.agendalist{
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  color: #053d5dc9;
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

.sort-icon{
  width: 20px;
  height: 20px;
}
</style>

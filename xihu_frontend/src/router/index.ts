import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/Home/HomeView.vue'
import LoginView from '../views/Login/LoginView.vue'
import MeetingView from '../views/Meeting/MeetingView.vue'
import AgendaView from '../views/Meeting/AgendaView.vue'
import SubscriptionView from '../views/Meeting/SubscriptionView.vue'
import CompassView from '../views/Compass/CompassView.vue'
import translate from 'i18n-jsautotranslate'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
     {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/home',
      name: 'home',
      component: HomeView

    },
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/meeting',
      name: 'meeting',
      component: MeetingView
    },
    {
      path: '/agenda',
      name: 'agenda',
      component: AgendaView
    },
    {
      path: '/subscription',
      name: 'subscription',
      component: SubscriptionView
    },
    {
      path: '/compass',
      name: 'compass',
      component: CompassView
    },
  ],
})

router.afterEach(() => {
  // 等待DOM更新后执行翻译
  setTimeout(() => translate.execute(), 100)
})
export default router

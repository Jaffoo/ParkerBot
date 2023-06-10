import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Index.vue'
import Config from '../views/Config.vue'
import Mirai from '../views/Mirai.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/mirai',
      name: 'mirai',
      component: Mirai
    },
    {
      path: '/config',
      name: 'config',
      component: Config
    }
  ]
})

export default router

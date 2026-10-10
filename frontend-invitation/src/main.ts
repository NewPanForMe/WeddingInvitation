import { createApp } from 'vue'
import { createPinia } from 'pinia'
import {
  Button,
  Empty,
  Field,
  Icon,
  Image as VanImage,
  ImagePreview,
  Loading,
  NoticeBar,
  Radio,
  RadioGroup,
  Skeleton,
  SkeletonParagraph,
  SkeletonTitle,
  Step,
  Stepper,
  Steps,
  Swipe,
  SwipeItem,
  Tag,
  Toast
} from 'vant'

import 'vant/lib/index.css'
import '@/styles/theme.css'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// 按需注册本项目用到的 Vant 组件（避免全量引入）
app.use(Button)
app.use(Empty)
app.use(Field)
app.use(Icon)
app.use(VanImage)
app.use(ImagePreview)
app.use(Loading)
app.use(NoticeBar)
app.use(Radio)
app.use(RadioGroup)
app.use(Skeleton)
app.use(SkeletonTitle)
app.use(SkeletonParagraph)
app.use(Step)
app.use(Stepper)
app.use(Steps)
app.use(Swipe)
app.use(SwipeItem)
app.use(Tag)
app.use(Toast)

app.mount('#app')

<!--
 ///////////////////////使用方法
    <AvatarGenerator
        width="90%"
        backgroundColor="rgba(233, 233, 233, 1)"
        initialAvatar="https://api.dicebear.com/9.x/notionists/svg?seed=西湖论剑"
        :initialIndex="4"
    />
    组件可以接受上述传参：
    width: 组件宽度，默认100%
    backgroundColor: 组件背景颜色，默认rgba(233, 233, 233, 0.3)
    initialAvatar: 组件初始头像url，默认https://api.dicebear.com/9.x/notionists/svg?seed=西湖论剑
    initialIndex: 组件初始头像类型索引，默认0

    上面四个可以传可以不传，反正有默认值啦
-->
<template>
  <div class="avatar-settings" :style="containerStyle">
    <p>头像生成</p>
    <div class="avatar-container">
      <div class="img-container">
        <img :src="currentAvatar" style="width: 200px; height: 200px;border-radius: 50%;" />
        <img :src="flush_img" @click="nextType" title="换个主题~" style="width: 30px; height: 30px;cursor: pointer;"
          class="flushed-icon" />
      </div>
      <div class="avatar-input">
        <div style="width: 46%;display: flex;flex-direction: column;">
          <input type="text" placeholder=" 随机种子" v-model="seed" @change="changeUrl" />
          <div style="font-size: 12px;color: rgba(51, 51, 51, 0.7);">示例值：随便填</div>
        </div>
        <div style="width: 46%;display: flex;flex-direction: column;">
          <div class="color-input">
            <span class="hash">#</span>
            <input type="text" maxlength="6" placeholder="RRGGBB" v-model="background" @change="changeUrl" />
          </div>
          <div style="font-size: 12px;color: rgba(51, 51, 51, 0.7);">&nbsp;&nbsp;示例值：000000</div>
        </div>

      </div>
      <div style="font-size: 13px;color: rgba(51, 51, 51, 0.7);text-decoration: underline;">输入后回车查看结果</div>
      <button class="button" style="margin-top: 10px;" @click="emitSave">保存</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import flush_img from '@/assets/icons/flushed.svg'

const props = defineProps({
  width: {
    type: String,
    default: '100%'
  },
  backgroundColor: {
    type: String,
    default: 'rgba(233, 233, 233, 0.3)'
  },
  initialAvatar: {
    type: String,
    default: 'https://api.dicebear.com/9.x/notionists/svg?seed=西湖论剑'
  },
  initialIndex: {
    type: Number,
    default: 0
  },
})

const emit = defineEmits(['saveAvatar'])

const containerStyle = computed(() => ({
  width: props.width,
  backgroundColor: props.backgroundColor
}))

const currentAvatar = ref(props.initialAvatar)
const seed = ref('')
const background = ref('')
const randomIndex = ref(props.initialIndex)

const types = [
  "notionists-neutral", "adventurer-neutral", "notionists", "lorelei-neutral", "personas",
  "avataaars", "dylan", "croodles-neutral", "avataaars-neutral", "lorelei",
  "miniavs", "thumbs", "open-peeps"
]

const isValidHexColor = (color) => {
  return /^[0-9A-Fa-f]{6}$/.test(color)
}

const changeUrl = () => {
  if (isValidHexColor(background.value)) {
    currentAvatar.value = `https://api.dicebear.com/9.x/${types[randomIndex.value]}/svg?seed=${seed.value}&backgroundColor=${background.value}`
  } else {
    currentAvatar.value = `https://api.dicebear.com/9.x/${types[randomIndex.value]}/svg?seed=${seed.value}&backgroundColor=FFFFFF`
  }
}

const nextType = () => {
  randomIndex.value = (randomIndex.value + 1) % types.length
  changeUrl()
}

const emitSave = () => {
  emit('saveAvatar', currentAvatar.value)
}
</script>

<style scoped>
@import '@/assets/button.css';

.avatar-settings {
  border-radius: 8px;
  margin-bottom: 10px;
  color: #333;
  border-radius: 30px;
  padding: 20px;
}

.img-container {
  position: relative;
  width: 200px;
  height: 200px;
}

.avatar-container {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.flushed-icon {
  position: absolute;
  bottom: 0px;
  right: 0px;
}

.avatar-input {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  margin: 25px 0 10px 0;
}

.avatar-input input {
  width: 100%;
  height: 30px;
  border-radius: 10px;
  border: 1px solid rgba(51, 51, 51, 0.5);
  transition: all 0.3s ease;
}

.avatar-input input:focus {
  border: 1px solid rgba(51, 51, 51, 0.7);
  outline: none;
}

.avatar-input input:hover {
  border: 1px solid rgba(51, 51, 51, 0.7);
}

.color-input {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.color-input input {
  width: 100%;
}
</style>

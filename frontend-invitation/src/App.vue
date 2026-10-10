<script setup lang="ts">
// 根组件：页面内容全部由路由渲染
</script>

<template>
  <router-view v-slot="{ Component }">
    <component :is="Component" />
  </router-view>
</template>

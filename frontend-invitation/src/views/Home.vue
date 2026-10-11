<script setup lang="ts">
// 无 token 的提示页：宾客直接打开站点根路径时看到
</script>

<template>
  <main class="page home">
    <div class="home__cover">
      <h1 class="home__title">婚礼邀请函</h1>
      <p class="home__subtitle">Wedding Invitation</p>
    </div>

    <div class="card home__card">
      <h2 class="section-title">请使用专属链接打开</h2>
      <p class="home__text">
        本页面是为每位宾客单独生成的电子请柬，需要通过新人发送的专属链接访问。
      </p>
      <p class="home__text home__text--muted">
        链接格式为：<code>/Wedding/i?token=您的专属码</code>
      </p>
      <p class="home__text">
        如果您是通过微信收到的邀请，请直接点击消息中的链接；若链接打不开，请联系新人重新发送。
      </p>
    </div>

    <p class="home__footer">期待与您相见</p>
  </main>
</template>

<style scoped>
.home {
  padding-bottom: calc(40px + var(--wedding-safe-bottom));
}

.home__cover {
  padding: 64px var(--wedding-page-x) 44px;
  text-align: center;
  color: var(--wedding-text-invert);
  background: linear-gradient(150deg, #c9a227 0%, #b4886b 55%, #8a6a52 100%);
  border-bottom-left-radius: 28px;
  border-bottom-right-radius: 28px;
}

.home__title {
  margin: 0 0 8px;
  font-size: 28px;
  font-weight: 600;
  letter-spacing: 6px;
}

.home__subtitle {
  margin: 0;
  font-size: 11px;
  letter-spacing: 3px;
  text-transform: uppercase;
  opacity: 0.85;
}

.home__card {
  margin-top: -26px;
}

.home__text {
  margin: 0 0 12px;
  font-size: 14px;
  line-height: 1.9;
  color: var(--wedding-text-sub);
}

.home__text--muted {
  font-size: 13px;
  color: var(--wedding-text-muted);
}

.home__text code {
  padding: 2px 6px;
  border-radius: 6px;
  background-color: var(--wedding-surface-soft);
  color: var(--wedding-primary-dark);
  font-size: 13px;
}

.home__footer {
  margin: 28px 0 0;
  text-align: center;
  font-size: 13px;
  letter-spacing: 3px;
  color: var(--wedding-text-muted);
}
</style>

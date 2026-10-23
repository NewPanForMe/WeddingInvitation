# Plan: 浪漫化婚礼邀请页

**Created**: 2026-10-23 10:06:00
**Scope**: `frontend-invitation`、`frontend-admin`、请柬配置 DTO/服务/数据库兼容初始化、媒体上传接口
**Risk Level**: medium
**Rollback Strategy**: 所有新增字段均可为空；关闭音乐或清空文案后，邀请页退化为现有封面、时间、地点、照片与 RSVP 流程。若需完全回退，可恢复本次涉及文件，数据库新增列保持不影响旧版程序读取。

## Summary

将邀请页从信息展示页提升为一段可阅读、可感受的婚礼叙事：封面先呈现一句简短誓言，随后依次展开新人信息、相遇故事、婚礼地点、照片和回执。背景音乐为可选配置，首次访客交互后才开始播放，页面右上角始终保留播放/静音控制。

视觉延续现有封面照片与暖色主题，但将文字层级调整为更克制的中式浪漫风格：衬线标题、留白、细分隔线、低幅度入场动画；不使用侵占内容的装饰背景或强制声音。

## Steps

### Step 1: 扩展请柬配置与兼容迁移
- **Operation**: edit
- **Target**: `server/src/WeddingInvitation.Domain/Entities/InvitationConfig.cs`, `server/src/WeddingInvitation.Application/DTOs/*.cs`, `server/src/WeddingInvitation.Application/Services/{Mapping,ConfigService,InviteService}.cs`, `server/src/WeddingInvitation.Infrastructure/Persistence/DbInitializer.cs`
- **Description**: 新增可为空的 `openingMessage`、`storyTitle`、`storyContent`、`closingMessage`、`backgroundMusicUrl` 与 `musicEnabled` 配置；在启动初始化中通过幂等 SQL 添加对应列，并贯通后台配置接口及公开邀请接口。对文案执行长度限制、去首尾空白和空值归一化；音乐 URL 仅在开关开启时对公开接口返回。
- **Expected Result**: 既有数据库与旧配置无须人工迁移；未填写新内容的邀请页保持当前表现。

### Step 2: 提供受控的背景音乐上传
- **Operation**: create/edit
- **Target**: `server/src/WeddingInvitation.Application/Services`, `server/src/WeddingInvitation.Api/Controllers/Admin`, `server/src/WeddingInvitation.Application/Options`, `frontend-admin/src/api`, `frontend-admin/src/components`
- **Description**: 增加独立的音频上传路径，不复用照片记录或图片校验。只接受适合移动端 H5 的 MP3/M4A，设置明确的单文件上限，采用现有按日期存储及静态文件访问规则；返回公开 URL。后台使用专用上传控件，可试听、替换和清空音乐。
- **Expected Result**: 管理员不需要手工放置媒体文件；照片管理的数据和规则不受影响。

### Step 3: 增加后台“邀请氛围”配置区
- **Operation**: edit
- **Target**: `frontend-admin/src/views/Config.vue`, `frontend-admin/src/types/index.ts`, `frontend-admin/src/api/config.ts`
- **Description**: 在婚礼配置中加入“封面寄语”“我们的故事标题”“我们的故事”“落款寄语”和“背景音乐”字段。故事输入使用多行文本并给出合理字符提示；音乐开关只有在成功上传或已配置 URL 时可启用。保存、重置和预览状态与现有配置表单一致。
- **Expected Result**: 所有浪漫文案和音乐均由管理员维护，空内容不会在公开邀请页留下空白模块。

### Step 4: 重构公开邀请页的叙事与视觉节奏
- **Operation**: create/edit
- **Target**: `frontend-invitation/src/views/Invite.vue`, `frontend-invitation/src/components/{CoverSection,WeddingInfo,PhotoSwipe}.vue`, `frontend-invitation/src/components/LoveStory.vue`, `frontend-invitation/src/styles/theme.css`
- **Description**: 在封面新人姓名下展示可选寄语；新增“我们的故事”章节，使用保留换行的正文、日期/地点与照片墙形成阅读节奏；底部采用自定义落款。优化封面裁切与文本对比度，调整卡片为更轻的纸张感和细线分隔，并加入短促、可降级的入场动画。
- **Expected Result**: 375px 至桌面宽度均能完整阅读；照片仍是主视觉，故事与文字起到情绪引导而不喧宾夺主。

### Step 5: 增加可访问的音乐控制
- **Operation**: create/edit
- **Target**: `frontend-invitation/src/components/BackgroundMusic.vue`, `frontend-invitation/src/views/Invite.vue`, `frontend-invitation/src/main.ts`
- **Description**: 使用原生 `<audio loop preload="metadata">` 与固定的圆形音乐按钮。初始为静音/未播放；在封面 CTA、首次触摸或音乐按钮点击后尝试播放，若浏览器拦截则保持可点击播放状态。保存本次浏览的开关偏好，提供 `aria-label`、明确播放状态和 `prefers-reduced-motion` 降级。
- **Expected Result**: 不会因浏览器自动播放策略出现无响应或噪声；访客可随时控制音乐，音乐加载失败不影响 RSVP 和其他页面功能。

### Step 6: 回归测试与多端视觉验证
- **Operation**: create/edit/run
- **Target**: 服务端轻量回归测试、`frontend-admin`、`frontend-invitation`
- **Description**: 覆盖新增配置的空值、长度限制、公开 DTO 映射、音乐开关与上传扩展名；运行服务器 Release 构建及两个前端的类型检查/生产构建。以移动端和桌面端截图核对封面、故事、音乐控制、长文案、无音乐和低动效状态。
- **Expected Result**: 既有邀请链接、照片墙、地图导航和 RSVP 流程回归通过，新体验在无音乐或空故事配置时也稳定可用。

## Acceptance Criteria

- 管理员可独立配置封面寄语、故事、落款和背景音乐，无需改代码或手工部署媒体。
- 邀请页有清晰的叙事节奏，新增内容为空时不展示空区域。
- 背景音乐不强制自动播放，首次交互后可启动，且播放/暂停状态始终可见、可操作。
- 页面在手机与桌面均无文本截断、重叠或横向滚动；减少动效偏好得到尊重。
- 后端、管理端、邀请端构建与相关回归测试均通过。

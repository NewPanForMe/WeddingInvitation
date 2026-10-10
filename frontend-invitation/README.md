# 婚礼电子请柬 H5（frontend-invitation）

移动端优先的婚礼电子请柬前端。新人通过微信把专属链接 `https://www.tunn.work/Wedding/i/{token}` 发给宾客，
宾客打开后即可看到个性化请柬：封面、新人姓名、婚礼时间、地点（场所名 + 详细地址）、照片墙、
自己的桌号，并可提交出席回复（RSVP）。

无背景音乐、无微信 JS-SDK 依赖，纯 H5 页面。

---

## 1. 技术栈

| 类别 | 选型 |
| --- | --- |
| 框架 | Vue 3（`<script setup>` + Composition API） |
| 构建 | Vite 5 |
| 语言 | TypeScript（strict，`vue-tsc --noEmit` 类型检查） |
| UI 组件 | Vant 4（按需注册组件） |
| 状态管理 | Pinia |
| 路由 | Vue Router 4（HTML5 History 模式） |
| 请求 | Axios |

---

## 2. 环境要求

- Node.js ≥ 18（开发使用 Node v24 验证通过）
- npm（本机 registry 已指向 `https://registry.npmmirror.com`）

---

## 3. 安装与开发

```bash
cd frontend-invitation
npm install
npm run dev
```

开发服务器默认 `http://localhost:5173`。

### 本地预览一条请柬

浏览器打开：`http://localhost:5173/Wedding/i/abc`（`abc` 为后端签发的 token）。

### 后端地址怎么改

开发环境通过 Vite 代理访问后端，代理配置在 `vite.config.ts`：

```ts
const BACKEND_ORIGIN = process.env.BACKEND_ORIGIN || 'http://localhost:5080'

server: {
  proxy: {
    '/api': { target: BACKEND_ORIGIN, changeOrigin: true },
    '/uploads': { target: BACKEND_ORIGIN, changeOrigin: true }
  }
}
```

两种改法：

1. 直接修改 `vite.config.ts` 里的默认值；
2. 不改代码，启动时用环境变量覆盖（推荐）：

```powershell
# PowerShell
$env:BACKEND_ORIGIN="http://192.168.1.10:5080"; npm run dev
```

```bash
# bash / macOS
BACKEND_ORIGIN=http://192.168.1.10:5080 npm run dev
```

> 前端 axios 会从 Vite `base` 推导站点根路径，正式环境请求走同源相对路径（`/Wedding/api/...`、`/Wedding/uploads/...`）。
> 因此开发靠代理、生产靠同源部署，代码本身不需要改动。

---

## 4. 构建

```bash
npm run build      # vue-tsc --noEmit && vite build，类型不通过会直接失败
npm run preview    # 本地预览 dist 产物
npm run type-check # 只做类型检查
```

产物输出到 `dist/`。

---

## 4.1 环境约束与故障排查（重要）

### 本机 DSH 沙箱内无法执行 `vite` 相关命令（`npm run dev` / `npm run build` / `npm run preview`）

现象：

```
failed to load config from ...\vite.config.ts
Error: spawn EPERM
    at ensureServiceIsRunning (node_modules/esbuild/lib/main.js:1975:29)
```

原因：Vite（含 `@vitejs/plugin-vue`）必须调用 esbuild 完成 TS/JSX 转译与配置加载，
而 esbuild 以「子进程 + 管道 stdio」方式启动服务进程；本机 DSH 沙箱禁止 Node 向子进程
传递任何管道 FD（实测 `stdio: ['pipe','ignore','ignore']`、`['ignore','pipe','ignore']`、
`['ignore','ignore','pipe']`、`fork` + IPC 均抛 `EPERM`，仅 `inherit` / `ignore` 可用）。
这与本项目代码无关，属于沙箱边界。

解决办法：**在普通终端（非沙箱）执行构建**：

```bash
cd D:\System\Desktop\Harness\WeddingInvitation\frontend-invitation
npm run build
```

沙箱内仍可正常使用的高价值校验：

```bash
npx vue-tsc --noEmit   # 纯 TypeScript，不走 esbuild，可用于类型检查
```

### npm install 报 `EPERM ... npm-cache`

npm 默认缓存目录在 `C:\Users\<用户>\AppData\Local\npm-cache`，位于工作区之外，
在沙箱内会被拒绝写入。工程根目录的 `.npmrc` 已把缓存改到工程内：

```ini
cache=.npm-cache
registry=https://registry.npmmirror.com
fund=false
audit=false
```

`.npm-cache` 已加入 `.gitignore`。若要放到工作区其他位置，把 `cache` 改成对应路径即可
（例如与工程同级的 `cache=../.npm-cache`）。

### npm 12 拦截依赖的 install script

`npm install` 可能提示 `esbuild` / `vue-demi` 的 install script 被拦截，
导致 esbuild 缺失平台二进制。放行一次即可：

```bash
npm install-scripts approve esbuild vue-demi
```

---

## 6. 部署

把 `dist/` 里的**全部文件**放到站点 `/Wedding/i` 目录（`base` 为 `/Wedding/i/`）。

因为路由使用 History 模式（`/Wedding/i/{token}`），
服务器必须配置 **history 回退**：找不到的静态文件一律回退到 `index.html`。

### Nginx

```nginx
server {
    listen 80;
    server_name www.tunn.work;

    root /var/www/wedding/dist;
    index index.html;

    # 静态资源长缓存
    location /assets/ {
        expires 30d;
        add_header Cache-Control "public, max-age=2592000, immutable";
    }

    # 接口转发给后端（方案 A：前后端同机，由 Nginx 反代）
    location /api/ {
        proxy_pass http://127.0.0.1:5080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # 照片等上传资源同样由后端提供
    location /uploads/ {
        proxy_pass http://127.0.0.1:5080;
        proxy_set_header Host $host;
    }

    # history 回退：请柬链接 /Wedding/i/xxx 必须能直接打开
    location /Wedding/i/ {
        try_files $uri $uri/ /Wedding/i/index.html;
    }
}
```

若后端与前端**不同域**，则去掉 `/api/`、`/uploads/` 的 proxy_pass，
改为在后端配置 CORS，并让前端以完整域名访问（需要改 `src/api/http.ts` 的 `baseURL`）。

### IIS（web.config）

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="History Fallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/Wedding/(admin|api|uploads)(/|$)" negate="true" />
          </conditions>
          <action type="Rewrite" url="/Wedding/i/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
    </staticContent>
  </system.webServer>
</configuration>
```

IIS 需要安装 **URL Rewrite** 模块；`/api`、`/uploads` 由反向代理（ARR）或同站点应用处理。

### 分享封面

`index.html` 中 `og:image` 指向 `/share-cover.jpg`，
部署时把一张分享封面图（建议 300×300 以上、正方形）放到 `dist/share-cover.jpg`（即站点根目录）。
该文件不在仓库中，需要自行提供；缺图只会影响分享缩略图，不影响页面功能。
同理 `index.html` 引用的 `/favicon.ico` 也是可选的，未提供时浏览器只是显示默认图标。

---

## 7. 目录结构

```
frontend-invitation/
├── index.html                    # 入口 HTML（viewport / theme-color / SEO / 微信分享 meta）
├── package.json
├── tsconfig.json
├── tsconfig.node.json
├── vite.config.ts                # base '/Wedding/i/'，/api、/Wedding/api 与 /uploads、/Wedding/uploads 代理到 http://localhost:5080
├── .npmrc                        # 镜像源 + 工程内 npm 缓存目录
├── .gitignore
└── src/
    ├── main.ts                   # 应用入口：Pinia、Router、Vant 组件注册、主题样式
    ├── App.vue                   # 根组件（router-view）
    ├── env.d.ts
    ├── api/
    │   ├── http.ts               # axios 实例，baseURL ''，统一把 {code,message} 转成 Error(message)
    │   └── invite.ts             # getInvite(token) / submitRsvp(payload)
    ├── types/
    │   └── index.ts              # 与后端契约一一对应的 TS 类型
    ├── router/
    │   └── index.ts              # '/i/:token' → Invite.vue，'/' → Home.vue，兜底 → Error.vue
    ├── stores/
    │   └── invite.ts             # Pinia：token、load()、提交状态与结果
    ├── styles/
    │   └── theme.css             # 主题变量（暖色调）+ 通用卡片/按钮样式
    ├── views/
    │   ├── Invite.vue            # 请柬主体（封面→新人/称呼→婚礼信息→地点→照片墙→桌号→RSVP→底部）
    │   ├── Home.vue              # 无 token 提示页
    │   └── Error.vue             # 链接失效/错误页
    └── components/
        ├── CoverSection.vue      # 封面大图 + 渐变遮罩 + 新人姓名
        ├── WeddingInfo.vue       # 新人、称呼、婚礼时间（按访客本地时区渲染）
        ├── PhotoSwipe.vue        # 照片墙（van-swipe 轮播 + 缩略图，点击 Vant ImagePreview 全屏）
        ├── MapNav.vue            # 场所名 + 详细地址，高德搜索链接 + 复制地址
        └── RsvpForm.vue          # 出席回复表单与结果卡片
```

---

## 8. 后端接口依赖

前端只依赖下面两个接口，均以同源相对路径请求；错误结构统一为 `{ code, message }`。

### 8.1 `GET /api/invite/{token}`

- 200：

```json
{
  "guest": { "name": "张三", "salutation": "亲爱的张三", "tableNo": "A区 8号桌", "maxGuests": 2 },
  "config": {
    "groomName": "李四",
    "brideName": "王五",
    "weddingTime": "2026-10-01T04:00:00+00:00",
    "venue": "XX酒店 3楼宴会厅",
    "address": "XX市XX区XX路88号",
    "coverUrl": "/uploads/cover.jpg",
    "rsvpDeadline": "2026-09-20T15:59:59+00:00"
  },
  "photos": [{ "id": 1, "url": "/uploads/1.jpg", "title": "外景", "sortOrder": 1 }],
  "rsvp": { "submitted": false, "attend": null, "guestCount": null, "remark": null, "updatedAt": null },
  "rsvpOpen": true
}
```

- 404（token 无效）：`{ "code": 404, "message": "邀请链接无效" }` → 前端展示错误页

### 8.2 `POST /api/rsvp`

- 请求：`{ "token": "abc", "attend": true, "guestCount": 2, "remark": "无" }`
- 200：`{ "success": true, "message": "回复成功", "status": "attending" }`
- 400（已过截止时间）：`{ "code": 400, "message": "已超过回复截止时间" }` → 表单禁用并提示

### 8.3 约定与注意点

- 照片与封面 `url` 为相对路径（`/uploads/x.jpg`），由浏览器同源加载；
  开发环境见第 3 节的代理配置，生产环境见第 6 节的 Nginx `location /uploads/` / IIS 反向代理配置。
- `weddingTime` / `rsvpDeadline` 为带时区的 ISO 8601 字符串，
  前端按**访客设备本地时区**渲染；`rsvpOpen` 为 `false` 时表单整体禁用。
- 后端应允许宾客重复提交 RSVP（页面支持“更新回复”）；
  是否允许修改由后端通过 `rsvpOpen` 与 400 响应控制。
- 前端不做 token 校验，完全以后端返回为准；任何非 2xx 都会转成 `Error(message)` 展示。

---

## 9. 已实现的设计要点

- 移动端优先：页面容器 `max-width: 430px` 居中，375~430 宽为最佳视口。
- 暖色调：主色 `#B4886B`、金色 `#C9A227`、米白背景 `#FDF8F3`，卡片圆角 16px + 柔和阴影。
- 封面：大图 + 多层渐变遮罩，标题为「{新人姓名} & {新人姓名} 的婚礼邀请」（写入 `document.title`）。
- 底部适配 `env(safe-area-inset-bottom)`。
- 加载态用 Vant Skeleton，错误态用 Vant Empty / NoticeBar。
- 照片墙：Vant `van-swipe` 轮播 + 缩略图网格，点击调用 Vant `showImagePreview` 全屏预览。
- RSVP：`van-radio-group`（出席/不出席）+ `van-stepper`（1 ~ `guest.maxGuests`）+ `van-field` 备注；
  提交后展示结果卡片并回显 `attend` / `guestCount` / `remark`，允许修改；`rsvpOpen=false` 时禁用并提示。
- 地图导航：不使用经纬度，按钮打开
  `https://uri.amap.com/search?keyword=<encodeURIComponent(address)>&src=wedding`，另提供“复制地址”。

# 婚礼请柬后台管理（frontend-admin）

婚礼电子请柬的后台管理前端，负责宾客名单管理、邀请二维码生成、照片墙维护、婚礼信息配置与 RSVP 统计。

- 技术栈：Vue 3 + Vite 5 + TypeScript + Naive UI + Pinia + Vue Router 4 + Axios + qrcode + @vicons/ionicons5（图标）
- 部署形态：构建产物 `dist/` 部署到站点的 `/admin` 子目录，前端自带 `/admin` 前缀处理
- 后端默认地址：`http://localhost:5080`（开发环境通过 Vite 代理访问）

---

## 1. 环境要求

| 项目 | 版本 |
| --- | --- |
| Node.js | ≥ 18（本机验证版本 v24.19.0） |
| 包管理器 | npm（本机 registry 已指向 https://registry.npmmirror.com） |

> 工程根目录已提供 `.npmrc`，把 npm 缓存指向工作区内的 `../.npm-cache` 并锁定 npmmirror 源：
>
> ```
> cache=../.npm-cache
> registry=https://registry.npmmirror.com
> ```
>
> 在 DSH 沙箱等受限环境里，npm 默认缓存目录（`%LOCALAPPDATA%\npm-cache`）在可写范围之外，不加这个配置会出现 `EPERM ... npm-cache` 安装失败。直接在工程目录执行下面的命令即可：

## 2. 安装依赖

```bash
cd WeddingInvitation/frontend-admin
npm install --no-audit --no-fund
```

## 3. 本地开发

```bash
npm run dev
```

- 开发地址：<http://localhost:5173/Wedding/admin/>
- 代理规则（见 `vite.config.ts`）：

| 前缀 | 目标 |
| --- | --- |
| `/api`、`/Wedding/api` | `http://localhost:5080` |
| `/uploads`、`/Wedding/upload` | `http://localhost:5080` |

即开发时请在 `http://localhost:5080` 启动后端服务；若后端地址不同，直接修改 `vite.config.ts` 中的 `server.proxy` 目标即可（同时 `base` 保持 `/Wedding/admin/`）。

## 4. 构建与本地预览

```bash
npm run build      # 产物输出到 dist/（等价于 vite build）
npm run preview    # 本地预览构建产物（同样带 /admin 前缀）
npm run type-check # vue-tsc 类型检查（不依赖 esbuild，可在沙箱内直接跑）
```

> ⚠️ **沙箱/受限环境提示**：DSH 沙箱禁止 Node 以管道 stdio 启动子进程（`child_process.spawn` 带管道会返回 `EPERM`），
> 而 Vite 5 的 `vite build` 需要启动 esbuild 服务进程（配置打包、TS 转译、压缩都走它），因此在沙箱内 `npm run build` 必然失败，
> 与项目代码无关。**请务必在普通终端（非沙箱）执行 `npm run build`**；在沙箱内可以用 `npx vue-tsc --noEmit` 做零错误的类型校验。

构建产物结构：

```
dist/
  index.html
  assets/index-*.css
  assets/index-*.js
```

## 5. 部署到 /Wedding/admin 子目录

前端已按「正式站点子目录 /Wedding/admin」这一形态配置：

- `vite.config.ts` 中 `base: '/Wedding/admin/'`，构建后的 `index.html` 会自动引用 `/Wedding/admin/assets/...`；
- 路由使用 `createWebHistory(import.meta.env.BASE_URL)`，所以**必须**在服务器为 `/Wedding/admin/*` 配置 history 回退（SPA fallback），否则刷新子页面会 404；
- `/Wedding/upload/*` 由后端提供，前端按返回的路径直接引用，请确保站点下的 `/Wedding/upload` 已被后端/反向代理接管。

### IIS

1. 把 `dist/` 下的全部文件复制到站点物理目录下的 `admin` 文件夹，例如 `C:\inetpub\wwwroot\admin\`；
2. 确认「URL 重写（URL Rewrite）」模块已安装，在 `admin` 目录放置 `web.config`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Admin SPA fallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/api/" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/uploads/" negate="true" />
          </conditions>
          <action type="Rewrite" url="/admin/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".webp" />
      <mimeMap fileExtension=".webp" mimeType="image/webp" />
    </staticContent>
  </system.webServer>
</configuration>
```

> 注意：`/api` 与 `/uploads` 需要转发到后端（可用 IIS 的 ARR 反向代理，或让后端直接挂在同一个站点下）。

### Nginx

```nginx
server {
    listen 80;
    server_name www.tunn.work;

    root /var/www/wedding;

    # 后台管理前端（SPA）
    location /Wedding/admin/ {
        alias /var/www/wedding/admin/;
        try_files $uri $uri/ /admin/index.html;
    }

    # 静态上传资源由后端提供
    location /Wedding/upload/ {
        proxy_pass http://127.0.0.1:5080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # 后端接口
    location /api/ {
        proxy_pass http://127.0.0.1:5080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        client_max_body_size 20m;   # 照片上传 ≤ 10MB，留出余量
    }
}
```

## 6. 默认管理员账号

| 用户名 | 密码 |
| --- | --- |
| `admin` | `admin123` |

登录成功后 token 保存于 `localStorage`，键名 `wedding_admin_token`（另存 `wedding_admin_username`、`wedding_admin_expires_at`）。
登录页右上角「修改密码」可调用 `POST /api/admin/password` 更换密码。
所有后台接口（除 `POST /api/admin/login`）都会自动携带请求头 `Authorization: Bearer <token>`；收到 401 时前端会清空登录态并跳转登录页。

## 7. 页面与目录结构

```
src/
  api/
    http.ts          Axios 实例：请求拦截注入 token；响应拦截 401 跳登录 + 统一错误提示
    auth.ts          login / logout / changePassword
    guests.ts        宾客增删改查 / CSV 导入 / 邀请链接
    photos.ts        照片列表 / 上传 / 更新 / 删除 / 排序
    config.ts        婚礼配置读取与保存
    statistics.ts    统计数据
  components/
    GuestFormModal.vue     宾客新增/编辑弹窗
    GuestQrcodeModal.vue   邀请二维码弹窗（qrcode 生成，支持下载 PNG / 打印 / 复制链接）
    ImportDialog.vue       CSV 批量导入弹窗（展示导入结果与错误行）
    PhotoUpload.vue        通用图片上传（自定义 request 走 multipart/form-data）
  layout/AdminLayout.vue   侧边栏 + 顶部管理员信息 + 退出登录 + 修改密码
  router/index.ts          路由与登录守卫（createWebHistory(import.meta.env.BASE_URL)）
  stores/auth.ts           登录态（token / username / expiresAt）
  types/index.ts           接口契约类型
  utils/format.ts          北京时间格式化（Intl.DateTimeFormat + Asia/Shanghai）
  views/                   Login / Dashboard / Guests / Photos / Config / Statistics
  App.vue  main.ts  styles/index.css
```

页面入口（均位于 `/admin/` 前缀下）：

| 路由 | 说明 |
| --- | --- |
| `/login` | 登录 |
| `/dashboard` | 数据概览：统计卡片 + 回复进度条 + 快捷入口 |
| `/guests` | 宾客管理：分页表格、搜索、状态筛选、增删改、CSV 导入、邀请二维码 |
| `/photos` | 照片管理：网格卡片、上传、启用开关、编辑、删除、上移/下移排序 |
| `/config` | 婚礼配置：新郎/新娘、婚礼时间、场所、详细地址、封面图、RSVP 截止时间 |
| `/statistics` | 统计报表：数字卡片、回复率、出席率、状态分布 |

## 8. 接口清单（前端已对接，契约已冻结）

| # | 方法 | 路径 | 说明 |
| --- | --- | --- | --- |
| 1 | POST | `/api/admin/login` | 登录，返回 `{ token, expiresAt, username }` |
| 2 | POST | `/api/admin/logout` | 退出登录 |
| 3 | POST | `/api/admin/password` | 修改密码 `{ oldPassword, newPassword }` |
| 4 | GET | `/api/admin/guests` | 宾客分页列表，支持 `keyword`、`status`、`page`、`pageSize` |
| 5 | POST | `/api/admin/guests` | 新增宾客 |
| 6 | POST | `/api/admin/guests/{id}` | 编辑宾客 |
| 7 | DELETE | `/api/admin/guests/{id}` | 删除宾客 |
| 8 | POST | `/api/admin/guests/import` | CSV 导入（multipart，字段名 `file`，UTF-8，列序：姓名,称呼,电话,关系,桌号,可带人数） |
| 9 | GET | `/api/admin/guests/{id}/link` | 获取邀请链接 `{ token, url }`，前端渲染二维码 |
| 10 | GET | `/api/admin/photos` | 照片列表 |
| 11 | POST | `/api/admin/photos/upload` | 上传照片（multipart，字段名 `file`，jpg/jpeg/png/webp，≤10MB） |
| 12 | POST | `/api/admin/photos/{id}` | 更新标题 / 排序号 / 启用状态 |
| 13 | DELETE | `/api/admin/photos/{id}` | 删除照片 |
| 14 | POST | `/api/admin/photos/sort` | 按 `{ ids: [...] }` 顺序重排 sortOrder |
| 15 | GET | `/api/admin/config` | 读取婚礼配置 |
| 16 | POST | `/api/admin/config` | 保存婚礼配置（仅详细地址，无经纬度字段） |
| 17 | GET | `/api/admin/statistics` | 统计数据（总宾客/已回复/出席/不出席/待回复/预计到场/回复率） |

错误结构：非 2xx 时响应体为 `{ "code": 401, "message": "用户名或密码错误" }`，前端统一以 `message` 弹出提示。

宾客状态取值：`pending`（待回复）、`attending`（确认出席）、`declined`（不能出席）。

## 9. 时间与地址约定

- 后端返回的时间为带时区的 ISO 字符串（如 `2026-10-01T04:00:00+00:00`），前端统一用 `Intl.DateTimeFormat('zh-CN', { timeZone: 'Asia/Shanghai' })` 格式化为北京时间展示；
- 配置页的时间选择器按本机时区录入，保存时通过 `new Date(ts).toISOString()` 转为带时区的 ISO 字符串提交；
- 婚礼地址只保留「详细地址」一个文本字段，**不采集、不展示任何经纬度信息**。

## 10. 备注

- **构建必须在普通终端（非沙箱）执行**：沙箱环境禁止 Node 以管道 stdio 启动子进程（`EPERM`），Vite 5 依赖 esbuild 服务进程，`vite build` 无法在沙箱内完成；沙箱内推荐用 `npx vue-tsc --noEmit` 验证类型正确性（本工程已通过，0 错误）。
- 二维码使用 `qrcode` 库本地生成（`QRCode.toDataURL`），无需后端出图；生成后可下载 PNG 或直接打印。
- 若 `npm run build` 报 esbuild 二进制缺失（受限环境下 `esbuild` 的 postinstall 被 install-scripts 策略拦截时可能出现），执行 `npm rebuild esbuild` 后重试即可。

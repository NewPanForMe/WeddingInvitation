# 婚礼电子请柬系统（WeddingInvitation）

一套结婚用电子请柬系统：新人把**宾客专属链接**通过微信发给被邀请人，宾客打开 `/i/{token}` 即可看到个性化请柬（称呼、婚礼时间、地点与详细地址、照片墙、桌号），并提交**出席回复（RSVP）**；新人在后台维护宾客、照片、请柬配置与统计。

- 后端：**ASP.NET Core Web API 10.0** + EF Core 10 + Npgsql + JWT + OpenAPI + Scalar
- 数据库：**PostgreSQL 15+**（库名 `WeddingInvitation`，连接串由**取串接口 + AES-256-GCM 解密**获得）
- 请柬 H5：**Vue 3 + Vite 5 + TypeScript + Vant 4**
- 后台管理：**Vue 3 + Vite 5 + TypeScript + Naive UI**
- 部署：**IIS**（单站点同域，也适用于 Nginx）

> 地址信息**只使用详细地址，不使用经纬度**：地图导航通过高德搜索链接 `https://uri.amap.com/search?keyword={详细地址}&src=wedding` 打开，由宾客在高德内继续导航（无经纬度时无法精确导航到门口，只能按地址搜索）。

---

## 一、目录结构

```text
WeddingInvitation/
├── README.md                       # 本文件
├── global.json                     # 固定 .NET SDK 10.0.401
├── nuget.config                    # 离线模式：仅使用本机 NuGet 全局包缓存
├── .gitignore
├── db/
│   └── 001_init.sql                # 建表 + 初始化数据（幂等，可重复执行）
├── tools/                          # 运维小工具（Node.js）
│   ├── lib/connection.js           # 取串接口 + AES-256-GCM 解密（公共模块）
│   ├── decrypt-conn.js             # 取串并解密，打印连接串
│   ├── apply-sql.js                # 执行 db/*.sql
│   ├── reset-demo.js               # 重置演示宾客状态
│   ├── smoke-api.js                # 接口端到端冒烟测试（41 项）
│   ├── package.json
│   └── node_modules/               # npm install pg
├── server/                         # 后端解决方案
│   ├── WeddingInvitation.sln
│   ├── Directory.Build.props
│   ├── src/
│   │   ├── WeddingInvitation.Domain/          # 实体 + 枚举
│   │   ├── WeddingInvitation.Application/     # DTO / 服务 / 选项模型 / 口令哈希
│   │   ├── WeddingInvitation.Infrastructure/  # EF Core 上下文、JWT、取串解密
│   │   └── WeddingInvitation.Api/             # 控制器 / 中间件 / 日志 / 配置
│   └── tools/
│       └── WeddingInvitation.Tools.ConnProbe/ # 连接串取串/解密验证控制台
├── frontend-invitation/            # 请柬 H5（Vue3 + Vant4）
└── frontend-admin/                 # 后台管理（Vue3 + Naive UI）
```

---

## 二、环境要求

| 组件 | 版本 | 说明 |
|---|---|---|
| .NET SDK | 10.0.401 | `global.json` 已固定；运行时 10.0.x |
| Node.js | 18+（本机 24） | 前端构建与 tools 脚本 |
| PostgreSQL | 15+ | 远端库 `47.102.153.9:54321/WeddingInvitation` |
| IIS | 10 + ASP.NET Core Hosting Bundle 10.0 + URL Rewrite | 部署用 |

**离线依赖说明（重要）**：本机（Windows + DSH 沙箱）**无法访问 NuGet 源**（TLS 证书存储不可用，`NU1301`），因此：

- 所有 NuGet 包版本都按本机全局包缓存 `%USERPROFILE%\.nuget\packages` 中**已存在的版本**锁定（EF Core 10.0.5 / Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 / JwtBearer 10.0.11 / IdentityModel 8.19.2 / Microsoft.AspNetCore.OpenApi 10.0.11 / Scalar.AspNetCore 2.17.2）；
- `nuget.config` 已 `<clear />` 掉远程源，避免 restore 去连外网而失败。**在能联网的机器上**请把其中的 nuget.org 源取消注释，或删除该文件使用全局 NuGet 配置；
- 前端依赖走 `registry.npmmirror.com`（各前端工程 `.npmrc` 已配置），并把 npm 缓存指向工作区内目录（沙箱禁止写系统缓存目录）。

---

## 三、快速开始

### 1. 初始化数据库

```powershell
cd WeddingInvitation\tools
npm install --no-audit --no-fund
$env:DELIVERY_KEY='fvHCNAFpGh4zBrLQkHkers4MkK8ByF2prA/Hc965dsg='   # 32 字节密钥的 Base64
node apply-sql.js
```

`001_init.sql` 是**幂等**的，重复执行安全。它会创建 5 张表并写入：请柬配置（示例时间取当前时间 +20/30 天，保证 RSVP 窗口打开）、默认管理员 `admin / admin123`、演示宾客 `demo-token-0001`。

### 2. 启动后端 API

```powershell
cd WeddingInvitation\server
dotnet restore WeddingInvitation.sln
dotnet build WeddingInvitation.sln --no-restore -m:1 -nodeReuse:false -p:UseSharedCompilation=false
cd src\WeddingInvitation.Api
dotnet run          # 或 dotnet bin\Debug\net10.0\WeddingInvitation.Api.dll
```

- 默认监听 `http://localhost:5080`（由 `Hosting:Urls` 控制）
- Scalar API 文档：<http://localhost:5080/scalar/v1>
- OpenAPI JSON：<http://localhost:5080/openapi/v1.json>
- 健康检查：<http://localhost:5080/health>
- 日志：`src\WeddingInvitation.Api\logs\api-yyyyMMdd.log`

> `dotnet run` 会加载 `Properties/launchSettings.json`，环境为 `Development`：
> 使用 `appsettings.Development.json` 中的**本地连接串兜底**（`PostgreSql:UseRemote=false`），因为本沙箱内 .NET 走 HTTPS 取串会因 TLS 证书存储不可用而失败。
> 生产环境（`appsettings.json`）默认 `UseRemote=true`，启动时实时取串并解密，失败才回退本地串。

### 3. 启动前端

```powershell
# 请柬 H5（默认 http://localhost:5173）
cd WeddingInvitation\frontend-invitation
npm install --no-audit --no-fund
npm run dev

# 后台管理（默认 http://localhost:5174/admin/，vite 会自动选端口）
cd ..\frontend-admin
npm install --no-audit --no-fund
npm run dev
```

两个工程都已配置 dev 代理：`/api`、`/uploads` → `http://localhost:5080`（可用环境变量 `BACKEND_ORIGIN` 覆盖）。

本地体验：

- 请柬：<http://localhost:5173/i/demo-token-0001>（token 也可用后台"专属链接/二维码"里任取一个）
- 后台：请柬工程占 5173，后台工程启动时会自动选下一个端口（如 <http://localhost:5174/admin/>，以终端输出为准），账号 `admin / admin123`

---

## 四、连接串取串与解密（按接口返回的描述实现）

取串接口：

```text
GET https://www.tunn.work/ToolsConsole/ConnectionString/GetByConnName?connName=WeddingInvitation
```

返回：

```json
{
  "code": 0,
  "message": "操作成功",
  "data": {
    "connName": "WeddingInvitation",
    "dbType": "PostgreSQL",
    "connectionStringEncrypted": "Base64(nonce[12]+密文+tag[16])",
    "algorithm": "AES-256-GCM",
    "keySource": "Security:DeliveryKey",
    "payloadLayout": "Base64(nonce[12] + ciphertext + tag[16])",
    "decryptInstructions": "算法：AES-256-GCM（密钥 32 字节）。密钥：服务端 Security:DeliveryKey 配置值（32字节密钥的Base64…）"
  }
}
```

按描述实现：**AES-256-GCM，密钥 32 字节（`Security:DeliveryKey` 的 Base64），载荷 `Base64(nonce[12] + ciphertext + tag[16])`**。

- C# 实现：`server/src/WeddingInvitation.Infrastructure/ConnectionStrings/PostgreSqlConnectionStringResolver.cs`
  - 优先远程取串解密（`PostgreSql:UseRemote=true`），失败且有本地串时回退；
  - 本地串要求 **两层 Base64**（与历史项目约定一致），并自动纠正 `SSL Mode=Disabled` → `SSL Mode=Disable`。
- Node 实现：`tools/lib/connection.js`（`decrypt-conn.js` / `apply-sql.js` / `reset-demo.js` 共用）。
- 命令行验证（生产配置 + 真实接口）：

```powershell
# 1) 离线验证 C# 解密逻辑（喂真实密文，无需网络）
cd server\tools\WeddingInvitation.Tools.ConnProbe\bin\Debug\net10.0
dotnet ConnProbe.dll --payload "<connectionStringEncrypted>" --key "fvHCNAFpGh4zBrLQkHkers4MkK8ByF2prA/Hc965dsg="

# 2) 端到端验证取串（需本机可正常走 HTTPS）
dotnet ConnProbe.dll
```

本次实测（本机）：Node 侧真实取串解密成功，结果为
`Host=47.102.153.9;Port=54321;Database=WeddingInvitation;Username=postgres;Password=hyper;SSL Mode=Disable`；
C# 的 `ConnProbe --payload … --key …` 对**同一份密文**解密得到**完全相同**的连接串（算法实现一致）。
.NET 直连取串接口在本沙箱内因 `The SSL connection could not be established（安全包中没有可用的凭证）` 失败，属沙箱 TLS 限制，不影响有正常证书存储的服务器。

---

## 五、配置说明（`appsettings.json`）

| 节点 | 键 | 说明 |
|---|---|---|
| `Hosting` | `Urls` | 监听地址，多个用 `;` 分隔 |
| `PostgreSql` | `UseRemote` | 是否远程取串解密 |
| | `ConnectionStringEndpoint` / `ConnectionName` | 取串接口地址与连接名 |
| | `ConnectionString` | 本地兜底串（**两层 Base64**），留空表示不使用 |
| `Security` | `DeliveryKey` | 取串解密主密钥（32 字节 Base64） |
| | `Jwt:Key` / `Issuer` / `Audience` / `ExpiresMinutes` | 后台 JWT 配置，`Key` 至少 32 字节，默认有效期 120 分钟 |
| `Invite` | `BaseUrl` / `PathTemplate` | 生成宾客专属链接与二维码内容的基地址，默认 `https://www.tunn.work/Wedding/i` + `/{token}` |
| `Upload` | `RootPath` / `RequestPath` / `MaxSizeMB` / `AllowedExtensions` | 照片本地存储目录、访问前缀、大小与扩展名限制 |
| `AdminSeed` | `Enabled` / `Username` / `Password` | 管理员表为空时自动补种默认管理员 |
| `Cors` | `AllowedOrigins` | 留空＝允许任意来源；生产建议填站点域名 |
| `OpenApi` | `Enabled` | 是否开启 OpenAPI JSON 与 Scalar API 文档（Development 环境始终开启） |

**上线前必改**：`Security:Jwt:Key`、`Security:DeliveryKey`（按部署方下发）、`AdminSeed` 默认口令（登录后立即改）、`Invite:BaseUrl`、`Cors:AllowedOrigins`。`Upload:RootPath` 默认是项目根目录下的 `upload`，上传文件会按日期保存到 `upload/yyyy-MM-dd/`。

---

## 六、接口清单

统一约定：JSON camelCase；错误响应体恒为 `{ "code": <HTTP 状态码>, "message": "<提示>" }`；后台接口需 `Authorization: Bearer <token>`。

### 公开接口

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | `/api/invite/{token}` | 请柬数据：`guest` / `config` / `photos` / `rsvp` / `rsvpOpen`；token 无效返回 404 |
| POST | `/api/rsvp` | 出席回复 `{ token, attend, guestCount, remark }`；超过 `rsvp_deadline` 返回 400；出席时 `guestCount` 最小为 1，不限制上限 |

### 后台接口

| 方法 | 路径 | 说明 |
|---|---|---|
| POST | `/api/admin/login` | 登录，返回 `{ token, expiresAt, username }` |
| POST | `/api/admin/logout` | 退出（无状态，前端清 token） |
| PUT | `/api/admin/password` | 修改口令 `{ oldPassword, newPassword }` |
| GET | `/api/admin/guests` | 分页列表：`keyword` / `status` / `page` / `pageSize` |
| POST | `/api/admin/guests` | 新增宾客（自动生成 32 位 token） |
| PUT | `/api/admin/guests/{id}` | 编辑宾客 |
| DELETE | `/api/admin/guests/{id}` | 删除宾客（级联删除其 RSVP） |
| POST | `/api/admin/guests/import` | CSV 批量导入（multipart 字段 `file`） |
| GET | `/api/admin/guests/{id}/link` | 专属请柬链接（前端用 qrcode 生成二维码） |
| GET | `/api/admin/photos` | 照片列表 |
| POST | `/api/admin/photos/upload` | 上传照片（multipart 字段 `file`） |
| PUT | `/api/admin/photos/{id}` | 修改标题 / 排序 / 启用状态 |
| DELETE | `/api/admin/photos/{id}` | 删除照片（同时删除本地文件） |
| PUT | `/api/admin/photos/sort` | 按 `{ ids: [...] }` 顺序重排 |
| GET / PUT | `/api/admin/config` | 读取 / 更新请柬配置（**PUT 为整体替换语义**，前端始终提交完整表单） |
| GET | `/api/admin/statistics` | 统计：总数 / 已回复 / 出席 / 不出席 / 待回复 / 预计到场人数 / 回复率 |

CSV 导入列顺序：`姓名,称呼,电话,关系,桌号,可带人数`（UTF-8，首行表头可省略），返回 `{ total, imported, skipped, errors[] }`。

---

## 七、数据库

5 张表（`db/001_init.sql`，与 EF 模型一一对应，`snake_case` 命名）：

| 表 | 关键字段 |
|---|---|
| `admin_users` | `username`（唯一）、`password_hash`（PBKDF2-HMAC-SHA256，格式 `PBKDF2$迭代$salt$hash`） |
| `guests` | `name`、`salutation`、`phone`、`token`（唯一，32 位随机）、`relation`、`table_no`、`max_guests`、`status`（`pending`/`attending`/`declined`） |
| `photos` | `url`（`/uploads/yyyy-MM-dd/xxx.jpg`）、`title`、`sort_order`、`enabled` |
| `invitation_config` | `groom_name`、`bride_name`、`wedding_time`、`venue`、`address`、`cover_url`、`rsvp_deadline`（**无经纬度字段**） |
| `rsvp` | `guest_id`（唯一，级联删除）、`attend`、`guest_count`、`remark` |

Schema 以 `db/001_init.sql` 为唯一事实来源（未生成 EF Migration，避免双份真相；如需迁移可 `dotnet ef migrations add`，design 包已引用）。

---

## 八、验证记录（本次实测）

| 项目 | 结果 |
|---|---|
| 后端编译 | `dotnet build server/WeddingInvitation.sln` → **0 警告 0 错误**（5 个项目） |
| 数据库 | `001_init.sql` 在真实 PostgreSQL 执行成功，5 张表创建；**重复执行幂等** |
| 取串解密 | Node 与 C# 两条实现对同一份密文解密结果一致 |
| 接口冒烟 | `node tools/smoke-api.js` → **通过 41 / 失败 0**（登录鉴权、配置、RSVP 未截止/已截止/超员/无效 token、请柬查询、宾客 CRUD + 专属链接、CSV 导入、照片上传/静态访问/排序/删除、统计、口令修改与还原、级联删除） |
| 请柬 H5 构建 | `npm run build`（vue-tsc + vite）→ 通过，`dist` 10 个文件约 440 KB |
| 后台构建 | `npm run build` → 通过，`dist` 22 个文件约 1.13 MB |

> 沙箱注意：本机 DSH 沙箱禁止 Node 以**管道 stdio** 启动子进程（`spawn EPERM`），而 Vite 依赖 esbuild 服务进程，因此 `npm run build` 需在**放宽沙箱**或**普通终端**下执行；沙箱内可先跑 `npx vue-tsc --noEmit` 做类型校验。

---

## 九、部署（IIS 单站点同域）

```text
站点：https://www.tunn.work/Wedding
  ├── /Wedding/i/        → 请柬 H5（frontend-invitation/dist，base=/Wedding/i/）
  ├── /Wedding/admin/    → 后台管理（frontend-admin/dist，base=/Wedding/admin/）
  ├── /Wedding/uploads/  → 照片访问前缀（Upload:RootPath 默认项目根目录 upload，按日期分目录）
  └── /Wedding/api/      → ASP.NET Core Web API
```

1. 后端发布：`dotnet publish server/src/WeddingInvitation.Api -c Release -o D:\publish\api`，IIS 新建应用指向该目录，应用程序池选**无托管代码**，绑定 HTTPS。
2. 前端：`npm run build` 后把 `frontend-invitation/dist` 放到 `/Wedding/i`，`frontend-admin/dist` 放到 `/Wedding/admin` 子目录。
3. URL Rewrite：H5 与 `/Wedding/admin` 各自做 history 回退到对应 `index.html`，并排除 `/Wedding/api`、`/Wedding/uploads`（两个前端工程的 README 有可直接复制的 `web.config` / Nginx 配置示例）。
4. 权限：`Upload:RootPath` 需给应用程序池账号**写权限**，且**禁止执行权限**。
5. 微信分享：必须 HTTPS；无需公众号与 JS-SDK，页面 `title`/`description`/`og:image`（`/share-cover.jpg`）即可；微信自动抓取卡片。

Nginx 形态同理：`/Wedding/i/` 指向 H5 的 `try_files $uri $uri/ /Wedding/i/index.html`，`/Wedding/admin/` 指向后台，`/Wedding/api`、`/Wedding/uploads` 反代到后端。

---

## 十、安全要点

- `token` 为 32 位随机十六进制（`RandomNumberGenerator`），生成时校验唯一性；请柬接口**不返回手机号**等隐私字段。
- 后台 JWT 有效期默认 120 分钟，`ClockSkew` 30 秒；密码 PBKDF2-HMAC-SHA256（210,000 次迭代）存储。
- 上传仅允许 `.jpg/.jpeg/.png/.webp`，默认上限 10MB，文件名重命名为 GUID；删除照片时校验路径在根目录内。
- 专属链接被转发会给他人看到该宾客的称呼与桌号，介意时可另做通用分享页（本期未实现）。
- 生产环境务必修改默认口令与 JWT/取串密钥，并按最小权限配置数据库账号与上传目录。

---

## 十一、与原始方案的差异说明

| 方案原文 | 本次实现 | 原因 |
|---|---|---|
| 日志使用 Serilog | 内置 `ILogger` + 自研轻量文件 Provider + Console/Debug | 离线依赖缓存中缺少 `Serilog.AspNetCore` 及其传递依赖，无法还原；功能等价（按天写 `logs/api-*.log`） |
| `Infrastructure/Repositories` 仓储层 | 服务直接依赖 `IApplicationDbContext`（接口在 Application 层） | 5 张表 CRUD 直接经 EF Core，减少一层无价值转发；分层依赖方向仍为 Api → Application ← Infrastructure |
| `GET /api/admin/guests/{id}/qrcode` 服务端出图 | 改为 `GET /api/admin/guests/{id}/link` + 前端 `qrcode` 库出图 | 服务端二维码库（QRCoder 等）不在离线缓存中；前端出图可下载/打印，体验更好 |
| `POST /api/admin/guests/import`（未定格式） | 明确为 **CSV（UTF-8，multipart `file`）** | 与"不需要导出 Excel"一致，避免引入 Excel 解析依赖 |
| 宾客导入/导出 Excel | 未实现（方案本身也不需要导出） | 保持一致 |
| `Migrations/` 目录 | 未生成迁移，schema 由 `db/001_init.sql` 管理 | 单一事实来源，避免双份 schema 漂移 |
| 照片上传限流/压缩 | 仅校验扩展名与大小 | 方案未要求 |

---

## 十二、常用运维命令

```powershell
# 取串并查看解密后的连接串
cd tools; $env:DELIVERY_KEY='<key>'; node decrypt-conn.js

# 重新初始化/升级 schema（幂等）
node apply-sql.js

# 接口冒烟（需 API 已启动）
node smoke-api.js

# 冒烟后恢复演示数据
node reset-demo.js

# 连接串取串/解密验证（C#）
cd ..\server\tools\WeddingInvitation.Tools.ConnProbe\bin\Debug\net10.0
dotnet ConnProbe.dll --payload "<密文>" --key "<密钥>"
```

-- =============================================================================
-- 婚礼电子请柬 · 数据库初始化脚本 (PostgreSQL 15+)
-- 数据库：WeddingInvitation
-- 说明：EF Core 模型（WeddingInvitation.Infrastructure/Persistence/AppDbContext.cs）
--       与本脚本表/列名一一对应（snake_case），以本脚本为 schema 唯一事实来源。
-- 幂等：全部使用 IF NOT EXISTS / WHERE NOT EXISTS，可重复执行。
-- =============================================================================

-- 1. 管理员表（单管理员方案：只保留一条记录，后台仅提供修改密码）
create table if not exists admin_users (
    id            bigserial primary key,
    username      text        not null unique,
    password_hash text        not null,
    created_at    timestamptz not null default now(),
    updated_at    timestamptz not null default now()
);

-- 2. 宾客表
create table if not exists guests (
    id         bigserial   primary key,
    name       text        not null,
    salutation text,
    phone      text,
    token      text        not null unique,
    relation   text,
    table_no   text,
    max_guests integer     not null default 1,
    status     text        not null default 'pending', -- pending / attending / declined
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create index if not exists idx_guests_token  on guests (token);
create index if not exists idx_guests_status on guests (status);
create index if not exists idx_guests_name   on guests (name);

-- 3. 照片表（服务器本地目录存储，url 形如 /uploads/xxx.jpg）
create table if not exists photos (
    id         bigserial   primary key,
    url        text        not null,
    title      text,
    sort_order integer     not null default 0,
    enabled    boolean     not null default true,
    created_at timestamptz not null default now()
);

create index if not exists idx_photos_sort on photos (sort_order, id);

-- 4. 请柬配置表（单行；地址只存详细地址，不存经纬度）
create table if not exists invitation_config (
    id            bigserial   primary key,
    groom_name    text,
    bride_name    text,
    wedding_time  timestamptz,
    venue         text,
    address       text,
    cover_url     text,
    rsvp_deadline timestamptz,
    updated_at    timestamptz not null default now()
);

-- 5. 出席回复表（一名宾客一条，可修改）
create table if not exists rsvp (
    id          bigserial   primary key,
    guest_id    bigint      not null unique references guests (id) on delete cascade,
    attend      boolean,
    guest_count integer     not null default 1,
    remark      text,
    created_at  timestamptz not null default now(),
    updated_at  timestamptz not null default now()
);

create index if not exists idx_rsvp_guest on rsvp (guest_id);

-- =============================================================================
-- 初始化数据
-- =============================================================================

-- 6. 请柬配置（单行）
--    示例时间使用相对当前时间，保证初始化后 RSVP 窗口是打开的；正式使用请在后台改为真实日期。
insert into invitation_config (
    groom_name, bride_name, wedding_time, venue, address, cover_url, rsvp_deadline
)
select
    '李四', '王五',
    now() + interval '30 days',
    'XX酒店 3楼宴会厅',
    'XX市XX区XX路88号',
    '/uploads/cover.jpg',
    now() + interval '20 days'
where not exists (select 1 from invitation_config);

-- 7. 默认管理员 admin / admin123
--    password_hash 格式：PBKDF2$迭代次数$salt(Base64)$hash(Base64)，算法 PBKDF2-HMAC-SHA256 / 32 字节
--    ⚠ 正式上线后请立即在后台修改密码
insert into admin_users (username, password_hash)
select 'admin', 'PBKDF2$210000$Stux9L3mb9jttOMG6Oun3Q==$uqyQl+ZuLt1vs2W+ADQanjHaslob4G3pjYhPia9knXo='
where not exists (select 1 from admin_users);

-- 8. 示例宾客（可选，测试用）
insert into guests (name, salutation, phone, token, relation, table_no, max_guests, status)
select '张三', '亲爱的张三', '13800000000', 'demo-token-0001', '同学', 'A区 8号桌', 2, 'pending'
where not exists (select 1 from guests where token = 'demo-token-0001');

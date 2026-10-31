# 邀请页照片章节 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 用可编辑照片描述和纵向全屏照片章节替换邀请页现有轮播。

**Architecture:** 后端为照片增加可空描述，并通过独立展示策略规范描述及兼容历史文件名标题；管理端扩展现有编辑弹窗；邀请端用原生文档滚动、IntersectionObserver 和 CSS 合成动画渲染全部章节，不引入新依赖。

**Tech Stack:** .NET 10、EF Core、PostgreSQL、Vue 3、TypeScript、Vant 4、Naive UI

**Spec:** `docs/superpowers/specs/2026-10-31-photo-chapters-design.md`

## Global Constraints

- 描述为可空纯文本，trim 后最多 500 字。
- 历史标题兼容必须保守且非破坏性，不批量修改原数据。
- 不引入新的前端运行时依赖。
- 所有动效必须提供减少动态效果等价表现。
- 不执行 Git commit 或 push。

## Review Focus

- 历史人工标题不能被文件名兼容规则误隐藏。
- 说明文字、空标题和空描述的组合不能产生空浮层。
- 单图、零图和图片加载失败不能破坏后续席位及 RSVP 流程。
- 键盘事件不能劫持表单输入或全屏预览交互。
- 描述换行和超长输入必须在 API、后台与公开页保持一致。

---

### Task 1: 照片文字领域与 API 契约

**Files:**
- Modify: `server/tests/WeddingInvitation.Domain.Tests/Program.cs`
- Modify: `server/tests/WeddingInvitation.Domain.Tests/WeddingInvitation.Domain.Tests.csproj`
- Create: `server/src/WeddingInvitation.Application/Common/PhotoStoryPolicy.cs`
- Modify: `server/src/WeddingInvitation.Domain/Entities/Photo.cs`
- Modify: `server/src/WeddingInvitation.Application/DTOs/InviteDtos.cs`
- Modify: `server/src/WeddingInvitation.Application/DTOs/ConfigDtos.cs`
- Modify: `server/src/WeddingInvitation.Application/Services/Mapping.cs`
- Modify: `server/src/WeddingInvitation.Application/Services/PhotoService.cs`
- Modify: `server/src/WeddingInvitation.Application/Services/InviteService.cs`

**Interfaces:**
- Produces: `PhotoStoryPolicy.NormalizeDescription(string?)` and `PhotoStoryPolicy.ToPublicTitle(string?)`.
- Produces: nullable `Photo.Description` and `PhotoDto.Description`.

- [ ] Write failing policy tests for trim/500-character limit, blank descriptions, obvious generated filenames and preserved human titles.
- [ ] Run `dotnet run --project server/tests/WeddingInvitation.Domain.Tests/WeddingInvitation.Domain.Tests.csproj -c Release`; expect failure because `PhotoStoryPolicy` is absent.
- [ ] Implement policy, description contract, public/admin mapping split, null upload title and update normalization.
- [ ] Run the same test command; expect `Photo story rules passed.` and exit 0.

### Task 2: PostgreSQL 持久化与后台编辑

**Files:**
- Modify: `server/src/WeddingInvitation.Infrastructure/Persistence/AppDbContext.cs`
- Modify: `server/src/WeddingInvitation.Infrastructure/Persistence/DbInitializer.cs`
- Modify: `db/001_init.sql`
- Modify: `frontend-admin/src/types/index.ts`
- Modify: `frontend-admin/src/views/Photos.vue`

**Interfaces:**
- Consumes: nullable `PhotoDto.Description` and `PhotoUpdateRequest.Description` from Task 1.
- Produces: `photos.description` nullable text column and admin description editing payload.

- [ ] Add idempotent `photos.description` schema support.
- [ ] Extend the existing edit modal and all update payloads so description is preserved, trimmed and limited to 500 characters.
- [ ] Run admin type-check and build; expect both exit 0.

### Task 3: 邀请页照片章节

**Files:**
- Create: `frontend-invitation/src/components/PhotoChapters.vue`
- Delete: `frontend-invitation/src/components/PhotoSwipe.vue`
- Modify: `frontend-invitation/src/views/Invite.vue`
- Modify: `frontend-invitation/src/types/index.ts`
- Modify: `frontend-invitation/src/main.ts`

**Interfaces:**
- Consumes: public `InvitePhoto.title: string | null` and `description: string | null` from Task 1.
- Produces: full-viewport photo chapters with active progress, image preview, keyboard navigation and reduced-motion styles.

- [ ] Implement native scroll chapters and IntersectionObserver active-state tracking without a new dependency.
- [ ] Add full-screen preview, failed-image fallback, zero/single/multi-photo behavior and input-safe keyboard navigation.
- [ ] Remove Vant Swipe registrations and replace `PhotoSwipe` at the existing page position.
- [ ] Run invitation type-check and build; expect both exit 0.

### Task 4: 全量验证与视觉检查

**Files:**
- Verify all files above and `GLOSSARY.md`.

- [ ] Run domain rules, Release solution build, both frontend type-checks/builds and `git diff --check`.
- [ ] Start a local invitation preview with controlled fixture data and visually check mobile and desktop layouts, progress, empty text, failed image and reduced-motion behavior.
- [ ] Review the complete diff against the approved spec; fix Critical/Important findings with a failing regression test first where behavior is testable.

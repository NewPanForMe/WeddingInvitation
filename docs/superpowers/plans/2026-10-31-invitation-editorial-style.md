# Invitation Editorial Style Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在不改变邀请数据、业务行为和移动端骨架的前提下，将公开邀请页升级为参考婚礼纪页面的编辑式视觉风格。

**Architecture:** 保留现有 Vue 组件与 API 契约，在 `Invite.vue` 中形成“故事篇章”和“婚宴信息”两个视觉区域；新增可复用章节标题、邀请引导与一次性进入视口组件。照片仍由 `PhotoChapters.vue` 管理导航和预览，仅将每章改为深色照片舞台与奶油色文字区。

**Tech Stack:** Vue 3、TypeScript、Vant 4、CSS、Node test runner

**Spec:** `docs/superpowers/specs/2026-10-31-invitation-editorial-long-page-design.md`

## Global Constraints

- 只做样式参考，不复制参考页素材、商业功能、数据模型或交互流程。
- 不修改后台、接口、数据库和照片顺序。
- 邀请页最大宽度保持约 `430px`，桌面端居中展示。
- 不新增照片拼贴、自动分组、倒计时、自动滚动、社交浮层、礼物红包或持续花瓣动画。
- 保留地图、音乐、照片预览、键盘导航、RSVP 校验和状态行为。
- `prefers-reduced-motion`、`prefers-reduced-transparency` 和 `prefers-contrast` 必须继续可用。
- 本轮不执行 Git commit 或 push。

## Review Focus

- `openingMessage`、故事和照片均为空时不应留下空故事篇章；由 Task 1 单元测试和 Task 4 浏览器场景覆盖。
- 超长宾客称呼、邀请寄语和故事正文必须自然增高、不截断、不产生横向溢出；由 Task 4 浏览器场景覆盖。
- 矮屏与横屏中的 4:5 照片、奶油色文字区和长描述滚动必须同时可用；由 Task 4 浏览器场景覆盖。
- 减少动态、减少透明和高对比模式不应暴露不可读文字或循环装饰；由 Task 4 媒体查询检查和浏览器场景覆盖。
- 回复截止、不出席、提交失败和地图操作必须保持原行为；由 Task 2 差异审查和 Task 4 浏览器回归覆盖。

---

### Task 1: 视觉呈现规则与复用组件

**Files:**
- Create: `frontend-invitation/src/components/invitation-presentation.ts`
- Create: `frontend-invitation/src/composables/useRevealOnce.ts`
- Create: `frontend-invitation/src/components/EditorialSectionHeader.vue`
- Create: `frontend-invitation/src/components/InvitationStoryIntro.vue`
- Create: `frontend-invitation/tests/invitation-presentation.test.ts`

**Interfaces:**
- Produces: `shouldShowStorySection(openingMessage: string | null | undefined, storyContent: string | null | undefined, photoCount: number): boolean`
- Produces: `getPhotoCopyMode(title: string | null | undefined, description: string | null | undefined): 'none' | 'title' | 'description'`
- Produces: `useRevealOnce(target: Ref<HTMLElement | null>, threshold?: number): Ref<boolean>`
- Produces: `EditorialSectionHeader` props `{ index: string; eyebrow: string; title: string }`
- Produces: `InvitationStoryIntro` props `{ salutation?: string; message?: string | null }`

- [ ] **Step 1: Write failing presentation-rule tests**

Add table-driven tests proving:

- whitespace-only `openingMessage` and `storyContent` with `photoCount = 0` returns `false`;
- any nonblank opening message, story content, or positive photo count returns `true`;
- blank title/description returns `none`, title-only returns `title`, and any nonblank description returns `description`.

- [ ] **Step 2: Run the focused tests and verify RED**

Run from `frontend-invitation`: `node --test --experimental-strip-types tests/invitation-presentation.test.ts`

Expected: FAIL because `invitation-presentation.ts` does not exist.

- [ ] **Step 3: Implement the minimal pure presentation rules**

Use trimmed text for presence checks and never mutate photo data.

- [ ] **Step 4: Run the focused tests and verify GREEN**

Run from `frontend-invitation`: `node --test --experimental-strip-types tests/invitation-presentation.test.ts`

Expected: all new cases PASS.

- [ ] **Step 5: Add reveal and editorial components**

`useRevealOnce` must retain `true` after first intersection, disconnect on unmount, and fall back to visible when `IntersectionObserver` is unavailable. Components use semantic headings, hide decorative elements from assistive technology, and expose no new interaction.

- [ ] **Step 6: Verify component types**

Run from `frontend-invitation`: `npm run type-check`

Expected: exit 0.

### Task 2: 重组邀请页视觉分区并保持功能边界

**Files:**
- Modify: `frontend-invitation/src/views/Invite.vue`
- Modify: `frontend-invitation/src/components/CoverSection.vue`
- Modify: `frontend-invitation/src/components/WeddingInfo.vue`
- Modify: `frontend-invitation/src/components/LoveStory.vue`
- Modify: `frontend-invitation/src/components/MapNav.vue`
- Modify: `frontend-invitation/src/components/RsvpForm.vue`

**Interfaces:**
- Consumes: Task 1 `shouldShowStorySection`, `EditorialSectionHeader`, `InvitationStoryIntro`, and `useRevealOnce`.
- Produces: `.invite-story`, `.wedding-details`, and `.response-card` visual regions without changing child business events.

- [ ] **Step 1: Move invitation copy without duplicating it**

Remove `openingMessage` from `CoverSection`; render it once through `InvitationStoryIntro`. Move `guest.salutation` out of `WeddingInfo` and render it once in the same intro.

- [ ] **Step 2: Build the two top-level regions**

In `Invite.vue`, preserve the approved order:

1. Cover
2. `01 / OUR STORY / 我们的故事`
3. Invitation intro, optional love story, photos
4. `02 / WEDDING DAY / 婚宴信息`
5. Wedding time, location, response card containing seat then RSVP
6. Footer

Hide the story region only through `shouldShowStorySection`.

Use semantic heading levels: the two top-level editorial titles are `h2`; love story, wedding time, location and photo titles are `h3`. Preserve the cover names as the page `h1`.

- [ ] **Step 3: Convert narrative cards to editorial sections**

Remove independent card shells from `WeddingInfo` and `MapNav`; retain thin separators and content spacing. Keep one `.response-card` around seat plus `RsvpForm`, while leaving RSVP store logic and map handlers untouched.

- [ ] **Step 4: Add one-time content reveals**

Apply `useRevealOnce` to invitation intro, love story, and top-level headings. Do not delay or block interactive controls.

- [ ] **Step 5: Verify data and behavior contracts**

Run from `frontend-invitation`: `npm test`

Expected: all presentation and existing navigation tests PASS.

Run from `frontend-invitation`: `npm run type-check`

Expected: exit 0.

### Task 3: 将照片章节改为深色舞台与奶油色文字区

**Files:**
- Modify: `frontend-invitation/src/components/PhotoChapters.vue`
- Test: `frontend-invitation/tests/invitation-presentation.test.ts`
- Test: `frontend-invitation/tests/photo-chapter-navigation.test.ts`

**Interfaces:**
- Consumes: Task 1 `getPhotoCopyMode`.
- Preserves: `preview(index)`, `getAdjacentChapterIndex`, `getChapterKeyboardStep`, global photo indices, failed-image handling, and long-copy focus behavior.

- [ ] **Step 1: Establish the focused behavior baseline**

Run from `frontend-invitation`: `node --test --experimental-strip-types tests/invitation-presentation.test.ts tests/photo-chapter-navigation.test.ts`

Expected: presentation modes and existing photo navigation tests PASS before markup changes.

- [ ] **Step 2: Implement the two-region photo chapter**

Restructure each article into:

- `.photo-chapter__stage`: backdrop, motes, progress, 4:5 button/image, hint;
- `.photo-chapter__copy`: separate cream title/description region rendered only when mode is not `none`.

Use Task 1 `getPhotoCopyMode` so whitespace-only title/description values do not create an empty copy region. Use `h3` for photo titles. Portrait uses upper/lower layout; short landscape retains an adaptive left/right layout so the 4:5 frame and copy remain usable within one viewport.

- [ ] **Step 3: Preserve navigation and preview behavior**

Keep `data-photo-chapter` on the article, keep global indices unchanged, keep copy overflow detection based on `scrollHeight > clientHeight`, and ensure failed images do not disable preview attempts.

- [ ] **Step 4: Run focused and full tests**

Run from `frontend-invitation`: `node --test --experimental-strip-types tests/invitation-presentation.test.ts tests/photo-chapter-navigation.test.ts`

Expected: focused presentation and navigation tests PASS.

Run from `frontend-invitation`: `npm test`

Expected: all tests PASS, including existing five navigation cases.

- [ ] **Step 5: Verify types and production CSS compilation**

Run from `frontend-invitation`: `npm run build`

Expected: `vue-tsc --noEmit` and Vite build both exit 0.

### Task 4: 统一编辑式样式、文档与多视口验收

**Files:**
- Modify: `frontend-invitation/src/styles/theme.css`
- Modify: `frontend-invitation/src/views/Invite.vue`
- Modify: `frontend-invitation/src/components/EditorialSectionHeader.vue`
- Modify: `frontend-invitation/src/components/InvitationStoryIntro.vue`
- Modify: `frontend-invitation/src/components/LoveStory.vue`
- Modify: `frontend-invitation/src/components/WeddingInfo.vue`
- Modify: `frontend-invitation/src/components/MapNav.vue`
- Modify: `frontend-invitation/src/components/RsvpForm.vue`
- Modify: `frontend-invitation/README.md`
- Modify: `docs/superpowers/specs/2026-10-31-invitation-editorial-long-page-design.md`

**Interfaces:**
- Consumes: Tasks 1–3 markup and classes.
- Produces: final responsive visual system and updated documentation.

- [ ] **Step 1: Apply the editorial visual language**

Use current theme tokens for serif headings, generous vertical whitespace, thin rose/champagne rules, static petal outlines, and restrained surfaces. Do not add image assets, dependencies, fixed overlays, or looping petals.

- [ ] **Step 2: Complete accessibility media queries**

Ensure reveal transforms and delays are removed for reduced motion; transparent surfaces become solid for reduced transparency; headings, rules, photo copy, controls and state messages remain readable in high contrast.

- [ ] **Step 3: Update documentation**

Document the two visual regions, moved invitation copy, two-tone photo chapter, fixed visual labels, unchanged API contract, and explicit non-goals. Remove obsolete wording about overlapping photo copy.

- [ ] **Step 4: Run automated verification**

Run from `frontend-invitation`: `npm test`

Expected: all tests PASS.

Run from `frontend-invitation`: `npm run build`

Expected: exit 0.

Run from repository root: `git diff --check`

Expected: exit 0 with no whitespace errors.

- [ ] **Step 5: Run browser verification with representative mock data**

Check `390×844`, `390×568`, `844×390`, and `1440×900` with:

- long salutation, opening message, story and photo description;
- zero photos, one photo, multiple photos, and one broken image;
- absent title/description and title-only photos;
- RSVP open, closed, attending and declined states.

Verify no horizontal overflow, no clipped invitation/story text, correct 4:5 geometry, independent long-description scrolling, working map/preview controls, no repeated reveal, and no browser errors introduced by this change.

- [ ] **Step 6: Independent final review**

Review the complete working-tree diff against the spec, with explicit attention to behavior changes hidden inside visual refactors. Fix all Critical or Important findings, re-run their reproductions, then run the full test and build commands again.

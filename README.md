# duckov_mod

《逃离鸭科夫（Duckov）》Mod 开发仓库。

本仓库按“参考示例 / 实际开发 / 必要时查阅”的思路组织：

- **sample/**：官方 Mod 示例与文档（主要参考来源）
- **mods/**：你正在开发的各个 Mod（工作目录）
- **game-src/**：解包的游戏文件（仅在需要确认类型/资源/签名时做参考）

## 入口链接

- 开发工作目录：[`mods/`](mods/)（说明见 [`mods/README.md`](mods/README.md)）
- 官方示例与文档：[`sample/`](sample/)（说明见 [`sample/README.md`](sample/README.md)）
  - 官方示例主文档（中文）：[`sample/duckov_modding-main/README.md`](sample/duckov_modding-main/README.md)
  - 值得注意的 API（中文）：[`sample/duckov_modding-main/Documents/NotableAPIs_CN.md`](sample/duckov_modding-main/Documents/NotableAPIs_CN.md)
- 游戏解包参考：[`game-src/`](game-src/)（说明见 [`game-src/README.md`](game-src/README.md)）

## 快速开始（建议流程）

1. 先阅读官方示例文档：了解 Mod 的加载规则（`info.ini` 的 `name`、`YourMod.dll`、`YourMod.ModBehaviour` 等）。
2. 在 `mods/` 下创建/维护你的 Mod 项目：编写代码、构建 DLL、准备 `info.ini` 与 `preview.png`。
3. 本地测试：将整理好的 Mod 文件夹放进游戏的 Mods 目录后，在游戏主界面的 Mods 菜单中加载。
   - Windows：通常在 `Duckov_Data/Mods/`
   - macOS：通常在 `Duckov.app/Contents/Mods/`
4. 只有当你需要确认游戏内部的类型/字段/资源时，再去 `game-src/` 做对照。

## 约定

- `mods/` 是唯一“会经常改动”的目录；`sample/` 与 `game-src/` 更偏向参考。
- 若计划把仓库同步到远端/公开，请自行评估 `game-src/` 是否适合纳入版本控制（体积与合规风险）。

## MC 前置共享分类

如果某个物品属于 MC 系列，并且希望在仓库或工作台里显示到 MC 前置提供的共享分类中，优先遵循现有前置的托管模式，不要在各个 Mod 里重复写轮询或高频兜底逻辑。

要点：

- 先在 [MOD_ITEM_ID_LIST.md](/Volumes/Kingston-1TB/github/duckov_mod/MOD_ITEM_ID_LIST.md) 登记该物品的自定义 ID。
- 再把该物品的 TypeID 同步加入 [mods/MCPrerequisite/MCCategoryService.cs](/Volumes/Kingston-1TB/github/duckov_mod/mods/MCPrerequisite/MCCategoryService.cs) 里的 `ManagedItemTypeIds`。
- 物品所属 Mod 侧保持和现有 MC 系列 Mod 一样的简化约定：如果 `MCPrerequisite` 未加载，则移除共享分类 tag；如果已加载，则交给前置统一补挂 `ModWorkbench_Mystic` tag 与刷新动态元数据。
- 不要在单个 Mod 里额外维护一套独立的共享分类注册、轮询补挂或仓库刷新逻辑，避免和前置的统一托管逻辑分叉。

适用场景：

- 需要进入 MC 分类的共享物品，例如 `900001`、`900002`、`900011`、`900012` 这一类 MC 系列物品。
- 只是普通独立物品、不需要进入 MC 共享分类的 Mod，不需要接入这套前置托管逻辑。

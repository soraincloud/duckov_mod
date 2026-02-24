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

# 末影珍珠（EnderPearl）

版本号：v1.0.0  
更新日期：2026-02-27

## Mod 简介

功能介绍：
- 新增可投掷道具「末影珍珠」
- 投掷物落地（首次碰撞）时，将角色传送到落点
- 落地触发传送粒子与音效表现

用途：
- 快速位移
- 跨越地形
- 战斗走位

获取方式：
- NPC 橘子处购买（装备商人 `Merchant_Equipment`），售价 `$1`

开发者：soraincloud  
策划：吱吱歪

声明：本 Mod 为开源项目，使用 AI 辅助开发。

功能：
- 新增物品「末影珍珠」：可使用（消耗 1 个）并投掷出一个投掷物
- 投掷物落地（首次碰撞）时，将角色传送到落点
- 加入 NPC 商店「橘子」（装备商人 `Merchant_Equipment`）售价 `$1`

## 构建

需要设置 Duckov 安装路径（包含 `Duckov.app` 的目录），例如：

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/EnderPearl/EnderPearl.csproj -c Release
```

构建完成后会自动把 `EnderPearl.dll` 复制到本目录（与 `info.ini` 同级）。

## 替换物品贴图（Icon）

在本 MOD 目录放一个文件：

- `icon.png`

启动游戏后会优先加载它作为 `Item.Icon`（背包/商店/地面拾取图标都会用到）。如果没找到 `icon.png`，会回退到运行时生成的默认图标。

## 添加 3D 建模（可选）

游戏里“手持/地面掉落”显示的是 `ItemAgent` prefab：

- 手持：key = `Handheld`
- 掉落拾取物：key = `Pickup`

本 MOD 支持从 AssetBundle 注入 3D 资源（没有 AssetBundle 也不影响功能）。

1) 把 AssetBundle 文件放在本 MOD 目录的子目录下（推荐按类型分类）：

- 模型：`assets/bundles/models/`

文件名支持其一：

- `enderpearl_assets`
- `enderpearl_assets.bundle`
- `enderpearl_assets.unity3d`

2) AssetBundle 内提供以下 prefab（名字必须一致）：

-（推荐/最简单）纯模型 prefab（不需要任何游戏脚本组件）：
	- `EnderPearl_HandheldModel`（作为手持时挂到 agent 下面的模型）
	- `EnderPearl_PickupModel`（作为掉落时挂到 agent 下面的模型）

-（高级）完整 agent prefab（需要在 Unity 工程里能添加 `ItemAgent`/`DuckovItemAgent` 组件）：
	- `EnderPearl_HandheldAgent`
	- `EnderPearl_PickupAgent`

你可以只做其中一个：只提供 Handheld 或只提供 Pickup 都可以。

## 音效（SFX）

默认使用 `assets/sfx/*.wav`，通过 **FMOD CoreSystem 直接播放**（不依赖 Unity Audio / 不需要 bank），已验证在 Duckov 中可响：

- `assets/sfx/throw.wav`
- `assets/sfx/transmit1.wav`
- `assets/sfx/transmit2.wav`

## 渲染模式开关（默认 Unlit）

### 本次结论（记录）

末影珍珠的自带模型在 Unity 编辑器里预览正常，但在游戏运行时走 **URP/Lit** 渲染路径会稳定出现“纯蓝/发蓝”的异常（多轮验证 UV/Normals/Tangents/贴图内容后依旧如此）。

把材质强制切到 **Unlit** 时，游戏内显示稳定正常，因此本 MOD 选择：

- 默认：对末影珍珠相关的 Renderer 强制使用 Unlit（尽量保留贴图）。
- 目的：优先保证“最终可用/可交付”，Lit 的根因留待以后慢慢追。

### 如何切回 Lit（用于后续研究）

在 MOD 目录创建一个空文件：

- `force_lit.txt`

存在时：不再强制 Unlit（尽量回到游戏原本的 Lit 渲染路径）。

兼容说明：

- `force_unlit.txt` 仍会被识别，但由于默认已是 Unlit，一般不再需要。

### 影响

- Unlit 不参与场景光照/阴影与部分后处理效果，外观会更“平”，但能避免当前 Lit 路径的蓝色异常。

## TypeID

当前固定为：`900001`（如果与你的其他 MOD 冲突，改 [ModBehaviour.cs](ModBehaviour.cs) 里的常量即可）。

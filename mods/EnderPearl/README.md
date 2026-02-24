# 末影珍珠（EnderPearl）

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

1) 把 AssetBundle 文件放在本目录，文件名支持其一：

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

## TypeID

当前固定为：`900001`（如果与你的其他 MOD 冲突，改 [ModBehaviour.cs](ModBehaviour.cs) 里的常量即可）。

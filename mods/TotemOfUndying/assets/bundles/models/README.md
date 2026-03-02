# Totem 模型 Bundle 说明（当前状态）

本目录用于存放 TotemOfUndying 的模型 bundle 文件（如果你后续要扩展 3D 资源加载）。

## 先说明当前行为（避免误解）

- **目前 TotemOfUndying 运行时代码不会读取本目录的 bundle。**
- 当前只会读取图标（`assets/item-icons/TotemOfUndying.png` 或 `icon.png`）。
- 所以把 bundle 放到这里不会自动生效。

## 这个目录什么时候有用

当你后续给 Totem 增加类似 EnderPearl 的资源加载逻辑后，本目录可直接作为模型 bundle 放置位置。

推荐文件名示例：

- `totem_assets`
- `totem_assets.bundle`

## 如果你现在就要提前打包资源

你可以照常在 Unity 打包 AssetBundle，然后把产物放进本目录，等代码接入后即可复用：

- `mods/TotemOfUndying/assets/bundles/models/`

## Unity 侧建议（简版）

1. 使用与游戏兼容的 Unity 版本。
2. 导入 FBX/材质/贴图，检查缩放与材质。
3. 给主 Prefab 设 AssetBundle 名称。
4. 执行 `BuildPipeline.BuildAssetBundles(...)` 导出。
5. 将 bundle 文件复制到本目录。

## 常见问题

- 导出后模型粉色：通常是 Shader 不兼容。
- 明明放了 bundle 但游戏没变化：当前 Totem 代码尚未接入模型 bundle 加载（预期行为）。

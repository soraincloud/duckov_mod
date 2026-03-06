# Totem 模型 Bundle 说明

本目录用于存放 TotemOfUndying 的模型 bundle 文件（如果你后续要扩展 3D 资源加载）。

## 当前行为

- 运行时会尝试读取本目录（以及 `assets/bundles/`、mod 根目录）的模型 bundle。
- 目标预制体名为：`TotemOfUndying_PickupModel`。
- 若 bundle 或预制体未命中，会自动回退到运行时默认模型（可见且会旋转）。

## 推荐文件名

- `totem_assets`
- `totem_assets.bundle`
- `totemofundying_assets`
- `totemofundying_assets.bundle`

## 放置路径

把打包产物放到：

- `mods/TotemOfUndying/assets/bundles/models/`

## Unity 侧建议（简版）

1. 使用与游戏兼容的 Unity 版本。
2. 导入 FBX/材质/贴图，检查缩放与材质。
3. 给主 Prefab 设 AssetBundle 名称。
4. 执行 `BuildPipeline.BuildAssetBundles(...)` 导出。
5. 将 bundle 文件复制到本目录。

## 常见问题

- 导出后模型粉色：通常是 Shader 不兼容。
- 明明放了 bundle 但游戏没变化：检查预制体名是否为 `TotemOfUndying_PickupModel`，以及 bundle 文件名是否命中候选名。

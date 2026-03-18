# MCPrerequisite

作为 MC 系列模组的公共前置，负责统一注册共享分类，并托管一组通用的 MC 材料物品。

当前已接入：

- `TotemOfUndying`
- `EnderPearl`
- `SplashHealingPotion`

## 职责

- 在工作台 `CraftView` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC材料` 分类过滤按钮
- 统一提供共享分类贴图资源（`grass.png`、`ironIngot.png`）
- 直接注册并托管共享材料物品：`玻璃`、`铁锭`、`金锭`
- 为当前 MC 系列动态物品统一补齐共享分类标签与动态元数据标签
- 在切场景和仓库加载后自动重新应用过滤器

## 使用约定

当前已接入的 MC 系列 Mod 不再各自维护工作台/仓库分类 UI、分类图标或共享分类标签逻辑，统一由本前置处理。

其中由本前置直接托管的共享材料物品使用独立的 `800001+` ID 段，并统一归入仓库 `MC材料` 分类。
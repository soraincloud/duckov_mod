# MCPrerequisite

作为 MC 系列模组的公共前置，负责统一注册工作台与仓库中的 `MC` 分类按钮。

当前已接入：

- `TotemOfUndying`
- `EnderPearl`
- `SplashHealingPotion`

## 职责

- 在工作台 `CraftView` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC` 分类过滤按钮
- 统一提供 `MC` 分类贴图资源（`grass.png`）
- 为当前 MC 系列动态物品统一补齐共享分类标签与动态元数据标签
- 在切场景和仓库加载后自动重新应用过滤器

## 使用约定

当前已接入的 MC 系列 Mod 不再各自维护工作台/仓库分类 UI、分类图标或共享分类标签逻辑，统一由本前置处理。
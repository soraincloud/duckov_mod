# MC前置

作为 MC 系列模组的公共前置，负责统一注册工作台与仓库中的 `MC` 分类按钮。

当前已接入：

- `TotemOfUndying`
- `EnderPearl`
- `SplashHealingPotion`

## 职责

- 在工作台 `CraftView` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC` 分类过滤按钮
- 在切场景和仓库加载后自动重新应用过滤器

## 使用约定

其他 MC 系列 Mod 只需要给自己的物品打上标签 `ModWorkbench_Mystic`，不再各自实现分类 UI 逻辑。
# MCPrerequisite

作为 MC 系列模组的公共前置，负责统一注册共享分类，并直接托管 `玻璃`、`铁粒`、`铁锭`、`铁块`、`金粒`、`金锭`、`金块` 这 7 个通用的 MC 共享材料物品。

当前已接入：

- `TotemOfUndying`
- `EnderPearl`
- `GoldenApple`
- `SplashHealingPotion`

## 职责

- 在工作台 `CraftView` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC` 分类过滤按钮
- 在仓库 `InventoryFilterProvider` 中注入 `MC材料` 分类过滤按钮
- 统一提供共享分类贴图资源（`grass.png`、`ironIngot.png`）
- 直接注册并托管 7 个共享材料物品：`玻璃`、`铁粒`、`铁锭`、`铁块`、`金粒`、`金锭`、`金块`
- 为当前 MC 系列动态物品统一补齐共享分类标签与动态元数据标签
- 在切场景和仓库加载后自动重新应用过滤器

## 使用约定

当前已接入的 MC 系列 Mod 不再各自维护工作台/仓库分类 UI、分类图标或共享分类标签逻辑，统一由本前置处理。

其中由本前置直接托管的共享材料物品使用独立的 `800001+` ID 段，并统一归入仓库 `MC材料` 分类；像 `GoldenApple`、`SplashHealingPotion` 这类依赖共享材料的工作台配方，也应优先复用这些共享物品 ID。

## 附加说明

- 地图容器随机刷新中追加 `玻璃`、`铁粒`、`铁锭`、`铁块`、`金粒`、`金锭`、`金块` 的实现方式与概率说明见 `LOOTBOX_MATERIAL_SPAWN.md`
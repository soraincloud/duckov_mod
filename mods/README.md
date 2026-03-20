# mods/

本目录是本仓库的“工作目录”：用于放置你正在开发的各个 Mod。

当前目录结构已经包含“合并版主 Mod + 前置 + 其他独立 Mod”的组织方式，不再要求每个功能点都单独对应一个顶层目录。

## 约定与建议

- 一个子文件夹对应一个 Mod（例如 `mods/MyMod/`）。
- 如果多个功能模块最终需要以单一 `info.ini` 名称和单一 dll 对外发布，应该合并在同一个顶层 Mod 目录下，而不是依赖游戏自动加载多个入口。
- 建议把“源码工程”和“发布结构”区分开管理：
  - 源码：`csproj`、脚本、资源、构建配置等
  - 发布结构：按游戏 Mod 识别规则整理出的 `dll + info.ini + preview.png`

## 当前目录说明

- `MCItems/`：当前 MC 系列合并版主 Mod，实际对外发布目录
- `MCPrerequisite/`：MC 系列前置与共享物品/分类服务
- `CaptainDestroyAccount/`：其他独立 Mod

已被合并替代的旧独立目录不再保留在这里，而是移动到仓库根目录的 `archive/` 下。

## 游戏如何识别 Mod（要点）

游戏会扫描 Mods 文件夹（以及创意工坊订阅内容）。当某个文件夹同时包含：

- `info.ini`
- `preview.png`
- 与 `info.ini` 中 `name` 对应的 `*.dll`

则可在游戏 Mods 菜单中加载。

同时，游戏会以 `name=MyMod` 为例，尝试加载 `MyMod.dll` 中的 `MyMod.ModBehaviour`，并要求该类继承 `Duckov.Modding.ModBehaviour`（详见官方示例文档）。

这也意味着：如果你要把多个原独立模块合并成一个 Mod，应该提供单一入口，例如 `MCItems.ModBehaviour`，再由该入口在运行时初始化内部子模块。

## 本地测试放置位置（提示）

- Windows：通常在 `Duckov_Data/Mods/`
- macOS：官方示例提到位于 `Duckov.app/Contents/Mods/`

具体以你的游戏安装目录为准。

## 教程

- [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md)：编译并部署 Mod 到游戏目录（本地测试）
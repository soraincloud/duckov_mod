# mods/

本目录是本仓库的“工作目录”：用于放置你正在开发的各个 Mod。

## 约定与建议

- 一个子文件夹对应一个 Mod（例如 `mods/MyMod/`）。
- 建议把“源码工程”和“发布结构”区分开管理：
  - 源码：`csproj`、脚本、资源、构建配置等
  - 发布结构：按游戏 Mod 识别规则整理出的 `dll + info.ini + preview.png`

## 游戏如何识别 Mod（要点）

游戏会扫描 Mods 文件夹（以及创意工坊订阅内容）。当某个文件夹同时包含：

- `info.ini`
- `preview.png`
- 与 `info.ini` 中 `name` 对应的 `*.dll`

则可在游戏 Mods 菜单中加载。

同时，游戏会以 `name=MyMod` 为例，尝试加载 `MyMod.dll` 中的 `MyMod.ModBehaviour`，并要求该类继承 `Duckov.Modding.ModBehaviour`（详见官方示例文档）。

## 本地测试放置位置（提示）

- Windows：通常在 `Duckov_Data/Mods/`
- macOS：官方示例提到位于 `Duckov.app/Contents/Mods/`

具体以你的游戏安装目录为准。

## 教程

- [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md)：编译并部署 Mod 到游戏目录（本地测试）
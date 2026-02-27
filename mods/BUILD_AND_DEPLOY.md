# 编译并部署 Mod 到游戏目录（本地测试）

本文以本仓库的示例 Mod `EnderPearl` 为例，说明如何在本地编译 `dll` 并放到游戏的 Mods 目录中。

## 你需要准备什么

- 已安装 .NET SDK（能在终端运行 `dotnet`）
  - 验证：`dotnet --version`
- 已安装游戏《Escape from Duckov》，并能找到游戏安装目录

> 说明：该 Mod 工程构建产物是 `netstandard2.1` 的 `*.dll`，部署到游戏 Mods 目录后由游戏加载。

## 游戏 Mods 目录在哪

一般情况下：

- macOS：`.../Escape from Duckov/Duckov.app/Contents/Mods/`
- Windows：`.../Escape from Duckov/Duckov_Data/Mods/`

你的 SteamLibrary 路径可能不同，但游戏目录结构通常符合上面两种之一。

## 一键构建 + 部署（推荐）

本仓库的 `mods/EnderPearl/` 已提供脚本 `deploy.sh`，会：

- `dotnet build` 编译 `EnderPearl.csproj`
- 同步运行时所需文件到游戏 Mods 目录
- **保留目标目录里的 `publishedFileId`（用于 Workshop 更新同一条目）**

### 1）设置游戏目录环境变量

`DUCKOV_PATH` 指向“包含 `Duckov.app`（mac）或 `Duckov.exe`（win）”的那个目录（也就是 `Escape from Duckov/` 目录）。

macOS 示例：

```bash
export DUCKOV_PATH="/Volumes/Kingston-1TB/SteamLibrary/steamapps/common/Escape from Duckov"
```

Windows（Git Bash / MSYS2）示例：

```bash
export DUCKOV_PATH="/c/Program Files (x86)/Steam/steamapps/common/Escape from Duckov"
```

### 2）运行部署脚本

在仓库根目录执行：

```bash
bash mods/EnderPearl/deploy.sh
```

脚本最后会打印类似：

- `Deployed to: .../Duckov.app/Contents/Mods/EnderPearl`（mac）
- 或 `Deployed to: .../Duckov_Data/Mods/EnderPearl`（win）

### 3）确认部署结果

目标目录里应至少有：

- `EnderPearl.dll`
- `info.ini`
- `preview.png`（若该 Mod 提供）

然后启动游戏，在 Mods 菜单中启用/加载该 Mod。

## 手动构建（可选）

如果你只想先确认能编过，不部署：

```bash
dotnet build mods/EnderPearl/EnderPearl.csproj -c Release -v minimal
```

成功后，构建产物通常在：

- `mods/EnderPearl/bin/Release/netstandard2.1/EnderPearl.dll`

要让游戏识别，你仍需要把 `EnderPearl.dll` 复制到对应的 Mods 目录，并配套 `info.ini`。

## 常见问题

### 1）Mods 菜单里看不到 Mod

优先检查目标文件夹是否同时包含：

- `info.ini`
- `*.dll`，且 dll 名称与 `info.ini` 里的 `name` 对得上（例如 `name = EnderPearl` → `EnderPearl.dll`）

### 2）每次上传 Workshop 都变成“新条目”

通常是 `info.ini` 里的 `publishedFileId` 变成了 `0`，上传器会当作新 Mod 创建。

- 本仓库的 `mods/EnderPearl/deploy.sh` 会优先保留**游戏目录**下原本的非 0 `publishedFileId`，避免部署覆盖导致变回 0。
- 如果你要绑定到某个既有 Workshop 条目，确保你上传时使用的目录里 `info.ini` 的 `publishedFileId` 是正确的非 0 值。

# 模型资源（FBX）导入与 AssetBundle 导出指南

本目录用于存放“最终可发布”的模型 Bundle 文件（建议一个功能一个 bundle）。

## 目标

把 `fbx` 模型导入 Unity，并导出可在 Duckov Mod 中加载的 `AssetBundle`。

---

## 1. 准备 Unity 工程

建议使用**与游戏一致的大版本 Unity**（否则可能出现 Bundle 不兼容）。

在 Unity 工程中创建目录（注意是 Unity 的 `Assets` 目录）：

- `Assets/Bundles/Models/`

把你的 `*.fbx`、贴图、材质放到这个目录。

---

## 2. 导入 FBX（Inspector 关键设置）

选中 `fbx` 文件，在 Inspector 检查：

1. **Model**
   - `Scale Factor` 按项目单位调整（常见 `1` 或 `0.01`）
   - `Read/Write Enabled` 仅在运行时确实需要时开启
2. **Rig**（如果是角色动画）
   - `Animation Type` 选 `Humanoid` 或 `Generic`
3. **Materials**
   - 推荐使用项目内统一 Shader（URP 项目常需手动修正材质）
4. 点击 `Apply`

---

## 3. 给资源打 AssetBundle 标记

在 Project 视图选中要打包的**主资源**（Prefab 或 FBX）：

- Inspector 底部 `Asset Labels` / `AssetBundle` 区域
- `AssetBundle` 名称示例：`totem_models`
- `Variant` 可留空

建议把“一个功能所需模型/材质/贴图”统一归到同一个 bundle 名。

---

## 4. 导出 Bundle（Editor 脚本）

在 Unity 工程新建脚本：`Assets/Editor/BuildBundles.cs`

```csharp
using System.IO;
using UnityEditor;

public static class BuildBundles
{
    [MenuItem("Tools/Build AssetBundles")]
    public static void BuildAll()
    {
        var outputPath = "AssetBundleOutput";
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        UnityEngine.Debug.Log($"AssetBundles built to: {outputPath}");
    }
}
```

回到 Unity 后执行：

- `Tools -> Build AssetBundles`

导出后会在工程根目录得到：

- `AssetBundleOutput/totem_models`（你的 bundle 文件）
- `AssetBundleOutput/AssetBundleOutput`（manifest 索引文件）
- 以及 `*.manifest`

---

## 5. 放入本仓库目录

把生成的 bundle 文件复制到本目录：

- `mods/TotemOfUndying/assets/bundles/models/`

例如：

- `mods/TotemOfUndying/assets/bundles/models/totem_models`

如果你的加载代码使用固定文件名，请保持和代码一致。

---

## 6. 快速自检（是否“可用”）

1. Bundle 名称与加载时名称一致
2. 目标平台一致（Windows 构建给 Windows，macOS 构建给 macOS）
3. 材质 Shader 在目标运行环境可用（URP/HDRP/内置管线要匹配）
4. 依赖资源（贴图、材质、动画）都被同包或被正确依赖

---

## 常见问题

- **导出后模型是粉色**：Shader 不兼容，改用目标项目可用 Shader 并重新打包。
- **加载不到资源**：通常是 `bundle 文件名`、`资源路径` 或 `平台` 不一致。
- **动画丢失**：检查 FBX Rig/Animation 导入设置，并确保动画资源被打进 bundle。

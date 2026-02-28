# TotemOfUndying 资源目录与 Bundle 打包说明

你后续要放的 **模型 bundle / sfx / 图片** 目录已准备好：

- `assets/source/models/`：模型源文件（fbx、prefab、材质等）
- `assets/source/sfx/`：音效源文件（wav、ogg）
- `assets/source/images/`：图片源文件（png、tga）
- `assets/bundles/modelbundle/`：模型 AssetBundle 输出
- `assets/bundles/sfx/`：音效 AssetBundle 输出
- `assets/bundles/images/`：图片 AssetBundle 输出

---

## 一、建议命名（避免踩坑）

AssetBundle 名称建议全小写、无空格：

- `totem_modelbundle`
- `totem_sfx`
- `totem_images`

对应关系：

- 模型资源 -> `totem_modelbundle`
- 音效资源 -> `totem_sfx`
- 图片资源 -> `totem_images`

---

## 二、Unity 内手动打包（最快上手）

> 前提：请使用与 Duckov 兼容的 Unity 版本（尽量与游戏版本一致）。

1. 新建或打开一个 Unity 工程（专门用于打包资源）。
2. 把你的模型/音效/图片导入到 Unity `Assets/` 下。
3. 选中资源，在 Inspector 底部 `AssetBundle` 下拉里分别设置：
   - 模型资源：`totem_modelbundle`
   - 音效资源：`totem_sfx`
   - 图片资源：`totem_images`
4. 菜单 `Assets -> Build AssetBundles`（或用下面脚本菜单）。
5. 把生成结果复制到本模组目录：
   - `mods/TotemOfUndying/assets/bundles/modelbundle/`
   - `mods/TotemOfUndying/assets/bundles/sfx/`
   - `mods/TotemOfUndying/assets/bundles/images/`

---

## 三、推荐：固定输出目录的打包脚本

在 Unity 工程中新建文件：

- `Assets/Editor/BuildTotemBundles.cs`

内容如下：

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildTotemBundles
{
    [MenuItem("Tools/Totem/Build AssetBundles")]
    public static void Build()
    {
        var outputDir = "/Volumes/Kingston-1TB/github/duckov_mod/mods/TotemOfUndying/assets/bundles";

        Directory.CreateDirectory(Path.Combine(outputDir, "modelbundle"));
        Directory.CreateDirectory(Path.Combine(outputDir, "sfx"));
        Directory.CreateDirectory(Path.Combine(outputDir, "images"));

        BuildPipeline.BuildAssetBundles(
            outputDir,
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget
        );

        Debug.Log($"[Totem] AssetBundles built to: {outputDir}");
    }
}
```

> 说明：这个脚本会把 bundle 输出到 `assets/bundles/` 根目录。你可以再按类别移动到 `modelbundle/`、`sfx/`、`images/` 子目录。

---

## 四、建议的最终落盘结构

示例（你后续替换成自己的文件名即可）：

```text
mods/TotemOfUndying/
  assets/
    bundles/
      modelbundle/
        totem_modelbundle
      sfx/
        totem_sfx
      images/
        totem_images
```

---

## 五、常见问题

1. **加载不到资源**
   - 先确认 bundle 名称与代码里加载名一致（大小写要一致）。
2. **不同平台不能通用**
   - AssetBundle 与构建目标平台相关，macOS/Windows 通常要分别打。
3. **材质丢失/粉色材质**
   - 确认 shader 在目标运行环境可用，必要时改用更通用 shader。

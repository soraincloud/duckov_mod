using UnityEngine;

namespace MCPrerequisite;

/// <summary>
/// 挂在自定义材料拾取物上，持续把所有 SpriteRenderer 缩放锁定到指定倍率。
/// </summary>
internal sealed class MCPickupScaleEnforcer : MonoBehaviour
{
    public float Multiplier { get; set; } = 1f;

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    public void Apply()
    {
        // 某些动画或代理初始化会重置子节点缩放，所以在多个生命周期里重复施加同一结果。
        var fixedScale = Vector3.one * Multiplier;
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            spriteRenderer.transform.localScale = fixedScale;
        }
    }
}
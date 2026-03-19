using UnityEngine;

namespace MCPrerequisite;

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
using UnityEngine;

namespace GoldenApple;

internal sealed class GoldenAppleEnchantedIconHost : MonoBehaviour
{
    private void Update()
    {
        GoldenAppleEnchantedIcon.Tick();
    }
}
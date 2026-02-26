using UnityEngine;

namespace EnderPearl;

public class EnderPearlProjectile : MonoBehaviour
{
    private CharacterMainControl? _owner;
    private Rigidbody? _rb;
    private Collider? _col;
    private bool _teleported;
    private float _maxLifeSeconds;

    public static GameObject Create(Vector3 startPos, CharacterMainControl owner, float maxLifeSeconds)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnderPearl_Projectile";
        go.transform.position = startPos;
        // Keep root scale at 1 so attached model scaling is predictable.
        go.transform.localScale = Vector3.one;

        // Ensure collider size stays reasonable.
        var sphereCol = go.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            sphereCol.radius = 0.11f;
        }

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.2f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var proj = go.AddComponent<EnderPearlProjectile>();
        proj._owner = owner;
        proj._rb = rb;
        proj._col = go.GetComponent<Collider>();
        proj._maxLifeSeconds = Mathf.Max(0.5f, maxLifeSeconds);

        // Prefer bundle model for flight; fallback to colored sphere if not available.
        var attached = ModAssets.TryAttachModelToProjectile(go);
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (attached)
            {
                renderer.enabled = false;
            }
            else
            {
                // 简单上色（避免纯白球太突兀）
                renderer.material.color = new Color(0.55f, 0.2f, 0.9f, 1f);
            }
        }

        proj.StartCoroutine(proj.IgnoreOwnerCollisionForSeconds(0.35f));
        proj.StartCoroutine(proj.DestroyAfterSeconds());

        return go;
    }

    private System.Collections.IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(_maxLifeSeconds);
        if (this != null && gameObject != null) Destroy(gameObject);
    }

    private System.Collections.IEnumerator IgnoreOwnerCollisionForSeconds(float seconds)
    {
        if (_owner == null || _col == null)
        {
            yield break;
        }

        var ownerCol = _owner.GetComponent<Collider>();
        if (ownerCol == null)
        {
            yield break;
        }

        Physics.IgnoreCollision(ownerCol, _col, true);
        yield return new WaitForSeconds(seconds);
        if (ownerCol != null && _col != null)
        {
            Physics.IgnoreCollision(ownerCol, _col, false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_teleported)
        {
            return;
        }
        _teleported = true;

        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 point;
        if (collision.contactCount > 0)
        {
            point = collision.GetContact(0).point;
        }
        else
        {
            point = transform.position;
        }

        // 尽量把落点贴到地面上（不依赖游戏层配置，使用默认层掩码）
        if (Physics.Raycast(point + Vector3.up * 1.0f, Vector3.down, out var hit, 3.0f, Physics.DefaultRaycastLayers))
        {
            point = hit.point;
        }

        ModSfx.PlayTransmit(point);

        _owner.SetPosition(point + Vector3.up * 0.1f);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GameCamera?.ForceSyncPos();
        }

        Destroy(gameObject);
    }
}

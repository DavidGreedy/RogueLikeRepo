using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeaponBehaviour : WeaponBehaviour
{
    [SerializeField]
    private RangedWeaponProperties m_weaponProperties;

    [SerializeField]
    private int m_currentAmmo;

    [SerializeField]
    private LayerMask m_raycastLayer;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(m_weaponProperties.projectileOrigin), transform.forward);
    }

    public override void UsePrimary()
    {
        if (m_currentAmmo > 0)
        {
            ProjectileBehaviour p = PoolManager.Instance.Next<ProjectileBehaviour>();
            if (p == null)
            {
                Debug.Log("Projectile is null");
            }
            p.Launch(transform.position + transform.TransformDirection(m_weaponProperties.projectileOrigin), transform.forward * m_weaponProperties.muzzleVelocity);
            m_currentAmmo--;
            base.UsePrimary();
        }
    }

    public RaycastHit ForwardRaycastHit(float dist)
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(transform.position + transform.TransformDirection(m_weaponProperties.projectileOrigin), transform.forward);
        Physics.Raycast(ray, out hit, dist, m_raycastLayer);
        if (hit.transform != null)
        {
            Debug.DrawLine(ray.origin, hit.point);
        }
        return hit;
    }
}
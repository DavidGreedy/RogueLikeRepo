using System;
using UnityEngine;

public class SolidMonoBehaviour : MonoBehaviour
{
    public virtual event Action<RaycastHit> OnHit;

    public virtual void Hit(RaycastHit hit, ProjectileBehaviour projectile)
    {
        if (OnHit != null)
        {
            OnHit.Invoke(hit);
        }
    }
}
using System;
using UnityEngine;

public class SolidMonoBehaviour : MonoBehaviour
{
    public event Action<RaycastHit> OnHit;

    public void Hit(RaycastHit hit)
    {
        if (OnHit != null)
        {
            OnHit.Invoke(hit);
        }
    }
}
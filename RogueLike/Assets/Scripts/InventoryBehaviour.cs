using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBehaviour : MonoBehaviour
{
    [SerializeField]
    private List<WeaponBehaviour> m_weaponList;
    private int m_activeIndex;

    public event Action<WeaponBehaviour, WeaponBehaviour> OnWeaponSwap;

    public WeaponBehaviour ActiveWeapon
    {
        get { return m_weaponList[m_activeIndex]; }
    }

    private void Start()
    {
        OnWeaponSwap += LogWeaponSwap;
    }

    private void Cycle(float delta)
    {
        if (delta == 0) { return; }
        m_activeIndex += delta > 0 ? 1 : -1;

        if (m_activeIndex == m_weaponList.Count)
        {
            m_activeIndex = 0;
        }
        else if (m_activeIndex == 0)
        {
            m_activeIndex = m_weaponList.Count - 1;
        }
    }

    private void SwapActiveWeapon(WeaponBehaviour newWeapon)
    {
        WeaponBehaviour weaponToDrop = ActiveWeapon;
        m_weaponList[m_activeIndex] = newWeapon;
        if (OnWeaponSwap != null)
        {
            OnWeaponSwap.Invoke(weaponToDrop, newWeapon);
        }
    }

    private void LogWeaponSwap(WeaponBehaviour oldWeapon, WeaponBehaviour newWeapon)
    {
        Debug.Log(string.Format("{0} was swapped with {1}", oldWeapon.name, newWeapon.name));
    }
}
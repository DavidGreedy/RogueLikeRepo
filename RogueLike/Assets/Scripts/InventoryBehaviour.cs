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

    public void Cycle(float delta)
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

    public void SwapActiveWeapon(WeaponBehaviour newWeapon)
    {
        WeaponBehaviour oldWeapon = ActiveWeapon;
        m_weaponList[m_activeIndex] = newWeapon;

        newWeapon.transform.parent = oldWeapon.transform.parent;
        newWeapon.transform.position = oldWeapon.transform.position;
        newWeapon.transform.rotation = oldWeapon.transform.rotation;
        newWeapon.gameObject.SetActive(true);

        oldWeapon.gameObject.SetActive(false);

        if (OnWeaponSwap != null)
        {
            OnWeaponSwap.Invoke(oldWeapon, newWeapon);
        }
    }

    private void LogWeaponSwap(WeaponBehaviour oldWeapon, WeaponBehaviour newWeapon)
    {
        Debug.Log(string.Format("{0} was swapped with {1}", oldWeapon ? oldWeapon.name : "NOTHING", newWeapon.name));
    }
}
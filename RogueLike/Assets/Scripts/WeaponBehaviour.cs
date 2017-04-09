using System;
using System.Collections;
using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    [SerializeField]
    protected Animator m_animator;

    [SerializeField]
    protected bool m_isActive;

    [SerializeField]
    protected int m_damage;

    [SerializeField]
    protected float primaryCoolDown, secondaryCooldown;

    public virtual void UsePrimary()
    {
        if (m_isActive) { return; }
        if (m_animator)
        {
            m_animator.SetBool("PrimaryAttack", true);
        }
        StartCoroutine(StartCooldownTimer(primaryCoolDown, SetWeaponActive, SetWeaponInActive));
        //Debug.Log(string.Format("{0} used PRIMARY", name));
    }

    public virtual void UseSecondary()
    {
        if (m_isActive) { return; }
        if (m_animator)
        {
            m_animator.SetBool("SecondaryAttack", true);
        }
        StartCoroutine(StartCooldownTimer(secondaryCooldown, SetWeaponActive, SetWeaponInActive));
        //Debug.Log(string.Format("{0} used SECONDARY", name));
    }

    protected virtual void SetWeaponActive()
    {
        m_isActive = true;
    }

    protected virtual void SetWeaponInActive()
    {
        if (m_animator)
        {
            m_animator.SetBool("PrimaryAttack", false);
            m_animator.SetBool("SecondaryAttack", false);
        }
        m_isActive = false;
    }

    public IEnumerator StartCooldownTimer(float cooldownTime, Action beginCallback, Action endCallback)
    {
        beginCallback.Invoke();
        yield return new WaitForSeconds(cooldownTime);
        endCallback.Invoke();
    }
}
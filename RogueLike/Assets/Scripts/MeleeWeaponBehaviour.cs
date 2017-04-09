using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour
{
    private List<CharacterBehaviour> m_targetsHitWhilstActive;

    [SerializeField]
    private MeleeWeaponTrail m_trail;

    public void Start()
    {
        m_targetsHitWhilstActive = new List<CharacterBehaviour>();
        m_trail.Emit = false;
    }

    public override void UsePrimary()
    {
        base.UsePrimary();
    }

    public override void UseSecondary()
    {
        base.UseSecondary();
    }

    protected override void SetWeaponActive()
    {
        m_trail.Emit = true;
        base.SetWeaponActive();
    }

    protected override void SetWeaponInActive()
    {
        m_trail.Emit = false;
        m_targetsHitWhilstActive.Clear();
        base.SetWeaponInActive();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision with " + other.gameObject.name);

        if (!m_isActive) return;

        CharacterBehaviour otherCharacter = other.gameObject.GetComponent<CharacterBehaviour>();

        if (otherCharacter && !m_targetsHitWhilstActive.Contains(otherCharacter))
        {
            m_targetsHitWhilstActive.Add(otherCharacter);
            otherCharacter.Damage(m_damage);
        }
    }
}
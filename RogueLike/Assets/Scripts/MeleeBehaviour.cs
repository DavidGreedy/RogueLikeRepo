using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBehaviour : MonoBehaviour
{
    private List<CharacterBehaviour> m_targetsHitWhilstActive;

    [SerializeField]
    private int m_damage;

    [SerializeField]
    private float m_damageTime;

    private bool m_isEnabled;

    private void Start()
    {
        m_targetsHitWhilstActive = new List<CharacterBehaviour>();
    }

    public void EnableDamage()
    {
        m_isEnabled = true;
        StartCoroutine(DamageCoroutine());
    }

    IEnumerator DamageCoroutine()
    {
        yield return new WaitForSeconds(m_damageTime);
        DisableDamage();
    }

    public void DisableDamage()
    {
        m_isEnabled = false;
        m_targetsHitWhilstActive.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_isEnabled)
        {
            CharacterBehaviour otherCharacter = other.gameObject.GetComponent<CharacterBehaviour>();

            if (otherCharacter && !m_targetsHitWhilstActive.Contains(otherCharacter))
            {
                m_targetsHitWhilstActive.Add(otherCharacter);
                otherCharacter.Damage(m_damage);
            }
        }
    }
}
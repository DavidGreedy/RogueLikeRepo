using System;
using UnityEngine;

public class HealthBehaviour : MonoBehaviour
{
    [SerializeField]
    private int m_maxHealth, m_currentHealth;


    public event Action<float> OnValueChange;
    public event Action OnDeath;
    public event Action OnHeal;
    public event Action OnTakeDamage;

    public float PercentHealth
    {
        get { return (m_currentHealth / (float)m_maxHealth) * 100f; }
    }

    private void Start()
    {
        if (OnValueChange != null)
        {
            OnValueChange.Invoke(m_currentHealth);
        }
    }

    public void Modify(int amount)
    {
        m_currentHealth += amount;

        Mathf.Clamp(m_currentHealth, 0, m_maxHealth);

        if (amount < 0)
        {
            if (OnTakeDamage != null)
            {
                OnTakeDamage.Invoke();
            }
        }
        else if (amount > 0)
        {
            if (OnHeal != null)
            {
                OnHeal.Invoke();
            }
        }
        else Debug.Log("Modify called with 0");

        if (OnValueChange != null)
        {
            OnValueChange.Invoke(PercentHealth);
        }

        if (m_currentHealth <= 0 && OnDeath != null)
        {
            OnDeath.Invoke();
        }
    }
}
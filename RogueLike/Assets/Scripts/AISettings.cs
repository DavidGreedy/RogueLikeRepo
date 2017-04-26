using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AISettings", menuName = "ScriptableObject/AISettings", order = 0)]
public class AISettings : ScriptableObject
{
    [SerializeField]
    private float m_attackRange = 1.5f;

    [SerializeField]
    private float m_enterCombatRange = 3f;

    [SerializeField]
    private float m_exitCombatRange = 5f;

    [SerializeField]
    private float m_seekRange = 8f;

    [SerializeField]
    private float m_waitRange = 20f;

    public float AttackRange
    {
        get { return m_attackRange; }
    }

    public float EnterCombatRange
    {
        get { return m_enterCombatRange; }
    }

    public float ExitCombatRange
    {
        get { return m_exitCombatRange; }
    }

    public float SeekRange
    {
        get { return m_seekRange; }
    }

    public float WaitRange
    {
        get { return m_waitRange; }
    }
}
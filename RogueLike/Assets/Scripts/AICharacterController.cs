﻿using UnityEngine;
using UnityEngine.AI;

public class AICharacterController : MonoBehaviour
{
    [SerializeField]
    private CharacterBehaviour m_targetCharacter;

    private CharacterBehaviour m_character;

    [SerializeField]
    private AISettings m_behaviourSettings;

    private float m_remainingDistance;

    [SerializeField]
    private NavMeshPath m_path;

    public enum CombatState
    {
        COMBAT, SEEKING, WAITING
    }

    [SerializeField]
    private CombatState m_combatState;

    private void Start()
    {
        m_character = GetComponent<CharacterBehaviour>();
        m_combatState = CombatState.WAITING;
        m_path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, m_targetCharacter.transform.position, NavMesh.AllAreas, m_path);
    }

    private void Update()
    {
        if (m_targetCharacter.IsAlive)
        {
            NavMesh.CalculatePath(transform.position, m_targetCharacter.transform.position, NavMesh.AllAreas, m_path);
            m_remainingDistance = 0;

            for (int i = 0; i < m_path.corners.Length - 1; i++)
            {
                Debug.DrawLine(m_path.corners[i], m_path.corners[i + 1], Color.red);
                Debug.DrawLine(m_path.corners[i], m_path.corners[i] + Vector3.up, Color.red);

                m_remainingDistance += Vector3.Distance(m_path.corners[i], m_path.corners[i + 1]);
            }

            if (m_path.corners != null)
            {
                Vector3 targetVector = m_path.corners[1] - transform.position;

                targetVector.y = 0;
                if (targetVector.magnitude < 0.1f)
                {
                    targetVector = m_path.corners[2] - transform.position;
                    targetVector.y = 0;
                }

                //m_remainingDistance = targetVector.normalized;
                //float targetDistance = targetVector.magnitude;

                switch (m_combatState)
                {
                    case CombatState.SEEKING:
                    {
                        m_character.MoveVector = targetVector.normalized;

                        if (m_remainingDistance > m_behaviourSettings.WaitRange)
                        {
                            m_combatState = CombatState.WAITING;
                        }
                        if (m_remainingDistance < m_behaviourSettings.EnterCombatRange)
                        {
                            m_combatState = CombatState.COMBAT;
                        }
                    }
                    break;
                    case CombatState.COMBAT:
                    {
                        m_character.LookVector = targetVector.normalized;
                        m_character.MoveVector = targetVector;
                        if (m_remainingDistance > m_behaviourSettings.ExitCombatRange)
                        {
                            m_combatState = CombatState.SEEKING;
                        }
                        if (m_remainingDistance < m_behaviourSettings.AttackRange)
                        {
                            m_character.Attack();
                        }
                    }
                    break;
                    case CombatState.WAITING:
                    {
                        m_character.MoveVector = Vector3.zero;
                        if (m_remainingDistance < m_behaviourSettings.SeekRange)
                        {
                            m_combatState = CombatState.SEEKING;
                        }
                    }
                    break;
                }
            }
        }
    }
}
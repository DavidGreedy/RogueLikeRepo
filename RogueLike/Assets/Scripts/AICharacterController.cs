using UnityEngine;
using UnityEngine.AI;

public class AICharacterController : MonoBehaviour
{
    public Transform target;
    public CharacterBehaviour m_character;

    public NavMeshAgent navAgent;

    private void Start()
    {
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;

    }

    private void Update()
    {
        navAgent.SetDestination(target.position);

        Vector3 targetVector = navAgent.nextPosition - transform.position;
        targetVector.y = 0;

        m_character.MoveVector = targetVector;
        //m_character.LookVector = targetVector.normalized;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterController : MonoBehaviour
{
    public Transform target;

    public NavMeshAgent navAgent;
    private void Update()
    {
        navAgent.SetDestination(target.position);
    }
}
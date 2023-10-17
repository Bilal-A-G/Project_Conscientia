using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RunnerController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;

    [HideInInspector] public float runnerSpeed;

    private void Update()
    {
        agent.SetDestination(target.position);
        agent.speed = runnerSpeed;
    }
}

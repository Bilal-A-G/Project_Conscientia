using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    enum MonsterState {CHASE, FLEE};

    [SerializeField] private Transform target;
    [SerializeField] private RunnerController runnerController;
    [SerializeField] private Transform spider;
    [SerializeField] private Transform player;
    [Range(0, 1)] [SerializeField] private float playerVisionCone;
    [SerializeField] private MonsterState state;

    [SerializeField] private MonsterIKController controller;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private float fleeDistance;

    [SerializeField] private float stopChaseDistance;

    [SerializeField] private bool isRunning;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float runnerWalkSpeed;
    [SerializeField] private float walkStepThreshold;
    [SerializeField] private float walkStepSpeed;

    [SerializeField] private float runSpeed;
    [SerializeField] private float runnerRunSpeed;
    [SerializeField] private float runStepThreshold;
    [SerializeField] private float runStepSpeed;

    private void Update()
    {
        Vector3 toPlayer = player.position - spider.position;
        Vector3 toRunner = runnerController.transform.position - spider.position;

        float currentMoveSpeed = isRunning ? runSpeed : walkSpeed;
        controller.stepSpeed = isRunning ? runStepSpeed : walkStepSpeed;
        controller.stepTreshold = isRunning ? runStepThreshold : walkStepThreshold;
        runnerController.runnerSpeed = isRunning ? runnerRunSpeed : runnerWalkSpeed;

        if (state == MonsterState.CHASE)
        {
            Spawn();
            target.transform.position = player.position;
        }
        else if(state == MonsterState.FLEE)
        {
            target.transform.position = spider.transform.position + 
                player.transform.right * fleeDistance;

            RaycastHit hit;
            if (Physics.Raycast(spider.position, toPlayer, out hit))
            {
                if (hit.transform.gameObject != player.gameObject)
                    Despawn();
            }

            if (Vector3.Dot(player.forward, -toPlayer.normalized) < playerVisionCone)
            {
                Despawn();
            }
        }

        if(toRunner.magnitude > stopChaseDistance)
            spider.transform.position += toRunner.normalized * currentMoveSpeed * Time.deltaTime;
    }

    private void Spawn()
    {
        controller.enabled = true;
        runnerController.enabled = true;
        meshRenderer.enabled = true;
    }

    private void Despawn()
    {
        controller.enabled = false;
        runnerController.enabled = false;
        meshRenderer.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform runnerTarget;
    [SerializeField] private Transform spiderEmpty;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float stopRotatingTreshold;

    [SerializeField] private EnemyLegController legController;
    [SerializeField] private float tiltSpeed;

    void Update()
    {
        spiderEmpty.position = Vector3.Lerp(spiderEmpty.position, target.position, Time.deltaTime * movementSpeed);

        if ((runnerTarget.position - spiderEmpty.position).magnitude <= stopRotatingTreshold)
            return;

        spiderEmpty.forward = Vector3.Lerp(spiderEmpty.forward, target.forward, Time.deltaTime * rotationSpeed);

        spiderEmpty.rotation = Quaternion.Lerp(spiderEmpty.rotation,
            Quaternion.Euler(spiderEmpty.forward * legController.leftRightTilt +
            spiderEmpty.right * legController.frontBackTilt), Time.deltaTime * tiltSpeed);
    }
}

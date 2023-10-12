using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform spider;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stopChaseThreshold;

    void Update()
    {
        Vector3 toTarget = target.position - spider.position;

        if (toTarget.magnitude <= stopChaseThreshold)
            return;

        spider.position += toTarget.normalized * Time.deltaTime * movementSpeed;
    }
}

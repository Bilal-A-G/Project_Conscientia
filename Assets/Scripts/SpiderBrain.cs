using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform runnerTarget;
    [SerializeField] private Transform spider;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float stopRotatingTreshold;

    [SerializeField] private float wallClimbDistance;
    [SerializeField] private float heightOffGround;

    private Transform currentWall;
    private Vector3 newUpVec;

    void Update()
    {
        spider.position = Vector3.Lerp(spider.position, target.position + spider.up * heightOffGround, Time.deltaTime * movementSpeed);

        if ((runnerTarget.position - spider.position).magnitude <= stopRotatingTreshold)
            return;

        RaycastHit wall;
        if(Physics.Raycast(spider.position, spider.forward, out wall))
        {
            if((spider.position - wall.point).magnitude <= wallClimbDistance && 
                wall.transform.gameObject.name == "Wall" &&
                (currentWall == null || wall.transform.gameObject != currentWall.gameObject))
            {
                currentWall = wall.transform;
                newUpVec = (spider.position - wall.point).normalized;
            }
        }

        if (currentWall == null)
            return;

        spider.up = Vector3.Lerp(spider.up, newUpVec, Time.deltaTime * rotationSpeed);

    }
}

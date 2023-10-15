using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIKController : MonoBehaviour
{
    [Header("Inverse Kinematics")]
    [SerializeField] private Leg[] legs;
    [SerializeField] private Transform spider;
    [SerializeField] private float stepTreshold;
    [SerializeField] private float stepHeight;
    [SerializeField] private float stepSpeed;
    [SerializeField] private float minDistanceFromBody;
    [SerializeField] private float velocityPrediction;
    [SerializeField] private float planeMarchStepAmount;
    [SerializeField] private float legReachedTargetThreshold;

    [Header("Movement")]
    [SerializeField] private bool stopMoving;
    [SerializeField] private Transform runner;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stopChaseDistance;
    [SerializeField] private float rotationSpeed;

    private Leg currentMovingLeg = null;
    private int currentMovingLegIndex;
    private bool isMoving;
    private float lerp;

    private Dictionary<int, float> legsToMoveIndicesAndDistances;
    private Vector3 newUp;
    private Vector3 spiderPosLastFrame;

    private void Start()
    {
        legsToMoveIndicesAndDistances = new Dictionary<int, float>();

        newUp = spider.up;
        spiderPosLastFrame = spider.position;
    }

    void Update()
    {
        MoveAndRotate();

        for (int i = 0; i < legs.Length; i++)
        {
            RaycastHit hit;
            Leg currentLeg = legs[i];

            if (isMoving)
                break;

            //if (toIKTarget.magnitude < minDistanceFromBody)
            //{
            //    Vector3 nextStepPosition = currentLeg.Iktarget.position + 
            //        Vector3.ProjectOnPlane(toIKTarget.normalized * planeMarchStepAmount, currentLeg.currentPlane.normal);

            //    RaycastHit secondWallHit;
            //    if(Physics.Raycast(nextStepPosition + currentLeg.currentPlane.normal * 0.5f, -currentLeg.currentPlane.normal, out secondWallHit))
            //    {
            //        if(secondWallHit.transform.gameObject != currentLeg.currentPlane.transform.gameObject)
            //        {
            //            Debug.Log(currentLeg.Iktarget.name + 
            //                "Is supposed to shimmy off" + currentLeg.currentPlane.transform.name + " onto " 
            //                + secondWallHit.transform.name);

            //            currentLeg.Iktarget.position = secondWallHit.point;
            //            currentLeg.nextPosition = secondWallHit.point;
            //            currentLeg.previousPosition = secondWallHit.point;
            //            currentLeg.currentPlane = secondWallHit;
            //        }
            //        else
            //        {
            //            Debug.Log("Shimmied leg: " + currentLeg.Iktarget.name);
            //            currentLeg.Iktarget.position = nextStepPosition;
            //            currentLeg.nextPosition = nextStepPosition;
            //            currentLeg.previousPosition = nextStepPosition;
            //        }
            //    }
            //}

            if (Physics.Raycast(currentLeg.raycastPosition.position, -spider.up, out hit, 10.0f))
            {
                Vector3 currentPosition = currentLeg.Iktarget.position;

                RaycastHit wallHit;
                if(Physics.Raycast(spider.position, (hit.point - spider.position).normalized, out wallHit, 10.0f))
                {
                    if(wallHit.transform.gameObject != hit.transform.gameObject)
                        hit = wallHit;
                }

                Vector3 nextMovePosition = hit.point;

                if ((nextMovePosition - currentLeg.Iktarget.position).magnitude > stepTreshold)
                {
                    while ((nextMovePosition - spider.position).magnitude < minDistanceFromBody)
                    {
                        nextMovePosition += Vector3.up * 0.5f;
                    }

                    Vector3 toTarget = nextMovePosition - currentPosition;

                    currentLeg.nextPosition = nextMovePosition + (spider.position - spiderPosLastFrame).normalized * velocityPrediction;
                    currentLeg.previousPosition = currentPosition;
                    currentLeg.bezierTopPosition = currentPosition + (toTarget.normalized *
                        toTarget.magnitude / 2 + spider.up * stepHeight);
                    currentLeg.currentPlane = hit;

                    if (!legsToMoveIndicesAndDistances.ContainsKey(i))
                        legsToMoveIndicesAndDistances.Add(i, toTarget.magnitude + currentLeg.movePriority);
                }
            }

            legs[i] = currentLeg;
        }

        if (!isMoving)
        {
            float greatestDistance = 0;

            foreach (KeyValuePair<int, float> indexDistance in legsToMoveIndicesAndDistances)
            {
                if (indexDistance.Value > greatestDistance)
                {
                    greatestDistance = indexDistance.Value;
                    currentMovingLegIndex = indexDistance.Key;
                }
            }

            if (greatestDistance == 0)
                return;

            legsToMoveIndicesAndDistances.Remove(currentMovingLegIndex);
            currentMovingLeg = legs[currentMovingLegIndex];
            lerp = 0;
            isMoving = true;
        }

        if (currentMovingLeg == null)
            return;

        if (currentMovingLeg.nextPosition == Vector3.zero ||
            currentMovingLeg.bezierTopPosition == Vector3.zero ||
            currentMovingLeg.previousPosition == Vector3.zero)
            return;

        if ((currentMovingLeg.Iktarget.position - currentMovingLeg.nextPosition).magnitude < 
            legReachedTargetThreshold)
        {
            isMoving = false;
            return;
        }

        lerp += Time.deltaTime * stepSpeed;
        currentMovingLeg.Iktarget.position = Vector3.Lerp(
            Vector3.Lerp(currentMovingLeg.previousPosition, currentMovingLeg.bezierTopPosition, lerp),
            Vector3.Lerp(currentMovingLeg.bezierTopPosition, currentMovingLeg.nextPosition, lerp),
            lerp);

        legs[currentMovingLegIndex] = currentMovingLeg;

        spiderPosLastFrame = spider.position;
    }

    private void MoveAndRotate()
    {
        Vector3 toRunner = runner.position - spider.position;

        Vector3 backToFront =
            ((legs[0].Iktarget.position + legs[2].Iktarget.position) / 2
            -
            (legs[3].Iktarget.position + legs[1].Iktarget.position) / 2).normalized;

        Vector3 rightToLeft =
            ((legs[0].Iktarget.position + legs[3].Iktarget.position) / 2
            -
            (legs[2].Iktarget.position + legs[1].Iktarget.position) / 2).normalized;

        newUp = Vector3.Lerp(newUp, Vector3.Cross(rightToLeft, backToFront).normalized, Time.deltaTime);
        spider.up = Vector3.Lerp(spider.up, newUp, Time.deltaTime * rotationSpeed);

        if (toRunner.magnitude > stopChaseDistance)
        {
            if (stopMoving)
                return;
            spider.position += movementSpeed * Time.deltaTime * toRunner.normalized;
        }
    }
}

[System.Serializable]
public class Leg
{
    public Transform Iktarget;
    public Transform raycastPosition;
    public float movePriority;

    [HideInInspector] public Vector3 nextPosition;
    [HideInInspector] public Vector3 bezierTopPosition;
    [HideInInspector] public Vector3 previousPosition;
    [HideInInspector] public RaycastHit currentPlane;
}

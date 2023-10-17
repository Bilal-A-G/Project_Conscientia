using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIKController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Leg[] legs;
    [SerializeField] private Transform spider;

    [HideInInspector] public float stepTreshold;
    [HideInInspector] public float stepSpeed;

    [SerializeField] private float stepHeight;
    [SerializeField] private float minDistanceFromBody;
    [SerializeField] private float velocityPrediction;
    [SerializeField] private float planeMarchStepAmount;
    [SerializeField] private float legReachedTargetThreshold;
    [SerializeField] private float raycastRange;
    [Range(0, 1)] [SerializeField] private float climbingAggressiveness;

    [Header("Rotation")]
    [SerializeField] private Transform runner;
    [SerializeField] private float stopRotationDistance;
    [SerializeField] private float rotationSpeed;

    private Leg currentMovingLeg = null;
    private int currentMovingLegIndex;
    private bool isMoving;
    private float lerp;

    private Dictionary<int, float> legsToMoveIndicesAndDistances;
    private Vector3 spiderPosLastFrame;

    private void Start()
    {
        legsToMoveIndicesAndDistances = new Dictionary<int, float>();
        spiderPosLastFrame = spider.position;
    }

    void Update()
    {
        Rotate();

        for (int i = 0; i < legs.Length; i++)
        {
            RaycastHit hit;
            Leg currentLeg = legs[i];

            Vector3 toIkTarget = currentLeg.Iktarget.position - spider.position;

            if (toIkTarget.magnitude < minDistanceFromBody)
            {
                Vector3 nextStepPosition = currentLeg.Iktarget.position + 
                    Vector3.ProjectOnPlane(toIkTarget.normalized * planeMarchStepAmount, currentLeg.currentPlane.normal);

                RaycastHit secondWallHit;
                if(Physics.Raycast(nextStepPosition + currentLeg.currentPlane.normal * 0.5f, -currentLeg.currentPlane.normal, out secondWallHit))
                {
                    if (!isMoving)
                    {
                        if (secondWallHit.transform.gameObject != currentLeg.currentPlane.transform.gameObject)
                        {
                            currentLeg.Iktarget.position = secondWallHit.point;
                            currentLeg.nextPosition = secondWallHit.point;
                            currentLeg.previousPosition = secondWallHit.point;
                            currentLeg.currentPlane = secondWallHit;
                        }
                        else
                        {
                            currentLeg.Iktarget.position = nextStepPosition;
                            currentLeg.nextPosition = nextStepPosition;
                            currentLeg.previousPosition = nextStepPosition;
                        }
                    }
                }
            }

            if (currentLeg.currentPlane.normal == null || currentLeg.currentPlane.normal == Vector3.zero)
            {
                if(!isMoving)
                    currentLeg.currentPlane.normal = spider.up;
            }

            if (Physics.Raycast(currentLeg.raycastPosition.position, -spider.up, out hit, raycastRange))
            {
                Vector3 currentPosition = currentLeg.Iktarget.position;
                legs[i].raycastHit = hit.point;

                RaycastHit wallHit;
                if(Physics.Raycast(spider.position, (hit.point - spider.position).normalized, out wallHit, raycastRange))
                {
                    if (wallHit.transform.gameObject != hit.transform.gameObject)
                    {
                        if (Physics.Raycast(spider.position, (currentLeg.raycastPosition.position - spider.position).normalized, out wallHit, raycastRange))
                        {
                           if(wallHit.transform.gameObject != hit.transform.gameObject)
                                hit = wallHit;
                        }
                    }
                }

                Vector3 nextMovePosition = hit.point;

                if ((nextMovePosition - currentLeg.Iktarget.position).magnitude > stepTreshold)
                {
                    Vector3 toTarget = nextMovePosition - currentPosition;

                    if (!isMoving)
                    {
                        currentLeg.nextPosition = nextMovePosition + (spider.position - spiderPosLastFrame).normalized * velocityPrediction;
                        currentLeg.previousPosition = currentPosition;
                        currentLeg.bezierTopPosition = currentPosition + (toTarget.normalized *
                            toTarget.magnitude / 2 + spider.up * stepHeight);
                        currentLeg.currentPlane = hit;
                    }

                    if (!legsToMoveIndicesAndDistances.ContainsKey(i))
                        legsToMoveIndicesAndDistances.Add(i, toTarget.magnitude + currentLeg.movePriority);
                    else
                        legsToMoveIndicesAndDistances[i] = toTarget.magnitude + currentLeg.movePriority;
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

    private void Rotate()
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

        Vector3 legsUp = Vector3.Cross(rightToLeft, backToFront).normalized;
        Vector3 newUp = Vector3.Lerp(legsUp, runner.up, climbingAggressiveness);

        if (toRunner.magnitude < stopRotationDistance)
            return;

        spider.up = Vector3.Lerp(spider.up, newUp, Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legs[i].nextPosition, 0.3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(legs[i].raycastHit, 0.3f);
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
    [HideInInspector] public Vector3 raycastHit;
}

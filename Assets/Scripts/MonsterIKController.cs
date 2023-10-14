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

    [Header("Movement")]
    [SerializeField] private Transform runner;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stopChaseDistance;
    [SerializeField] private float yawSpeed;
    [SerializeField] private float pitchSpeed;
    [SerializeField] private float velocityPrediction;

    private Leg currentMovingLeg = null;
    private int currentMovingLegIndex;
    private bool isMoving;
    private float lerp;

    private Dictionary<int, float> legsToMoveIndicesAndDistances;
    private Vector3 newForward;
    private Vector3 newUp;
    private Vector3 spiderPosLastFrame;

    private void Start()
    {
        legsToMoveIndicesAndDistances = new Dictionary<int, float>();

        newUp = spider.up;
        newForward = spider.forward;
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

            if (Physics.Raycast(currentLeg.raycastPosition.position, -spider.up, out hit))
            {
                Vector3 currentPosition = currentLeg.Iktarget.position;
                Vector3 toTarget = hit.point - currentPosition;

                if ((hit.point - currentLeg.Iktarget.position).magnitude > stepTreshold)
                {
                    currentLeg.nextPosition = hit.point + (spider.position - spiderPosLastFrame).normalized * velocityPrediction;
                    currentLeg.previousPosition = currentPosition;
                    currentLeg.bezierTopPosition = currentPosition + (toTarget.normalized *
                        toTarget.magnitude / 2 + spider.up * stepHeight);

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

        lerp += Time.deltaTime * stepSpeed;
        currentMovingLeg.Iktarget.position = Vector3.Lerp(
            Vector3.Lerp(currentMovingLeg.previousPosition, currentMovingLeg.bezierTopPosition, lerp),
            Vector3.Lerp(currentMovingLeg.bezierTopPosition, currentMovingLeg.nextPosition, lerp),
            lerp);

        if (currentMovingLeg.Iktarget.position == currentMovingLeg.nextPosition)
        {
            isMoving = false;
            return;
        }

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

        newForward = Vector3.Lerp(newForward, toRunner.normalized, Time.deltaTime);

        if (toRunner.magnitude > stopChaseDistance)
        {
            float rawAngleXZ = Mathf.Atan2(newForward.z, newForward.x) -
                Mathf.Atan2(spider.forward.z, spider.forward.x);

            float angleXZPlane = -Mathf.Rad2Deg * (Mathf.Abs(rawAngleXZ) > Mathf.PI ?
                (rawAngleXZ > 0 ? rawAngleXZ + Mathf.PI : rawAngleXZ - Mathf.PI) : rawAngleXZ);

            spider.position += movementSpeed * Time.deltaTime * toRunner.normalized;
            spider.rotation *= Quaternion.Euler(new Vector3(0, angleXZPlane, 0).normalized * yawSpeed * Time.deltaTime).normalized;
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
}

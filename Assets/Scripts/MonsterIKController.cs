using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIKController : MonoBehaviour
{
    [SerializeField] private Leg[] legs;
    [SerializeField] private Transform spider;

    [SerializeField] private float stepTreshold;
    [SerializeField] private float stepHeight;
    [SerializeField] private float stepSpeed;

    private Leg currentMovingLeg = null;
    private int currentMovingLegIndex;
    private bool isMoving;
    private float lerp;

    private Dictionary<int, float> legsToMoveIndicesAndDistances;

    private void Start()
    {
        legsToMoveIndicesAndDistances = new Dictionary<int, float>();
    }

    void Update()
    {
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
                    currentLeg.nextPosition = hit.point;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyLegController : MonoBehaviour
{
    [SerializeField] private List<Leg> legs;
    [SerializeField] private Transform spiderEmpty;
    [SerializeField] private float stepSpeed;
    [SerializeField] private float stepHeight;
    [SerializeField] private float stepTreshold;
    [SerializeField] private float tilt;
    [SerializeField] private LayerMask layer;

    [HideInInspector] public float leftRightTilt;
    [HideInInspector] public float frontBackTilt;

    void Update()
    {
        float averageLeftSideY = 0;
        float averageRightSideY = 0;

        float averageFrontY = 0;
        float averageBackY = 0;

        for (int i = 0; i < legs.Count; i++)
        {
            Vector3 targetPos = legs[i].target.position;
            Vector3 tipPos = legs[i].tip.position;
            RaycastHit hit;

            if (legs[i].destinationPosition != Vector3.zero && legs[i].bezierTopPoint != Vector3.zero)
            {
                legs[i].target.position = Vector3.Lerp(
                Vector3.Lerp(legs[i].initialPosition, legs[i].bezierTopPoint, legs[i].lerp),
                Vector3.Lerp(legs[i].bezierTopPoint, legs[i].destinationPosition, legs[i].lerp),
                legs[i].lerp);

                legs[i].lerp += Time.deltaTime * stepSpeed;

                legs[i].lerp = Mathf.Clamp01(legs[i].lerp);

                if (legs[i].lerp < 1)
                    continue;
            }
            else if(legs[i].destinationPosition != Vector3.zero)
            {
                legs[i].target.position = Vector3.Lerp(legs[i].initialPosition, 
                    legs[i].destinationPosition, legs[i].lerp);
                legs[i].lerp += Time.deltaTime * stepSpeed;

                legs[i].lerp = Mathf.Clamp01(legs[i].lerp);

                if (legs[i].lerp < 1)
                    continue;
            }

            if ((targetPos - tipPos).magnitude >= stepTreshold && Vector3.Dot((tipPos - targetPos).normalized, spiderEmpty.up) <= 0.8f)
            {
                bool canTakeStep = true;

                for(int j = 0; j < legs[i].oppositeLegIndices.Length; j++)
                {
                    if (legs[legs[i].oppositeLegIndices[j]].lerp < 1 && legs[legs[i].oppositeLegIndices[j]].lerp > 0)
                    {
                        canTakeStep = false;
                        break;
                    }
                }

                if (!canTakeStep)
                    continue;

                Vector3 raycastFrom = new Vector3(tipPos.x, spiderEmpty.position.y, tipPos.z);

                if (Physics.Raycast(raycastFrom, Vector3.down, out hit, 10.0f ,layer) && legs[i].lerp >= 1)
                {
                    legs[i].tohitPos = hit.point - targetPos;
                    legs[i].initialPosition = legs[i].target.position;
                    legs[i].bezierTopPoint = targetPos + (legs[i].tohitPos.normalized * legs[i].tohitPos.magnitude / 2) 
                        + ((raycastFrom - hit.point).normalized * stepHeight);
                    legs[i].destinationPosition = hit.point;

                    legs[i].lerp = 0;
                }

                continue;
            }

            if(Physics.Raycast(new Vector3(targetPos.x, spiderEmpty.position.y, targetPos.z), 
                Vector3.down, out hit, 10.0f, layer) && legs[i].lerp < 1)
            {
                legs[i].destinationPosition = hit.point;
                legs[i].initialPosition = legs[i].target.position;
            }

            if (legs[i].isRight)
            {
                averageRightSideY += legs[i].target.position.y;
            }
            else if (!legs[i].isRight)
            {
                averageLeftSideY += legs[i].target.position.y;
            }

            if (legs[i].isFront)
            {
                averageFrontY += legs[i].target.position.y;
            }
            else if (!legs[i].isFront)
            {
                averageBackY += legs[i].target.position.y;
            }
        }

        averageLeftSideY /= legs.Count / 2;
        averageRightSideY /= legs.Count / 2;
        averageFrontY /= legs.Count / 2;
        averageBackY /= legs.Count / 2;

        leftRightTilt = (averageLeftSideY - averageRightSideY) * tilt;
        frontBackTilt = (averageFrontY - averageBackY) * tilt;
    }
}

[System.Serializable]
public class Leg
{
    public Transform tip;
    public Transform target;
    public int[] oppositeLegIndices;
    public bool isFront;
    public bool isRight;

    public float lerp;
    public Vector3 tohitPos;
    public Vector3 bezierTopPoint;
    public Vector3 initialPosition;
    public Vector3 destinationPosition;
}

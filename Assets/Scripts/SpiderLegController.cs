using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PROJECT_CONSCIENTIA
{
    public class SpiderLegController: MonoBehaviour
    {
        [SerializeField] private Transform legRaycastOrigin;
        [SerializeField] private Transform legTarget;
        [SerializeField] private Transform spiderBody;

        [SerializeField] private float stepTreshold;
        [SerializeField] private float stepHeight;
        [SerializeField] private float stepSpeed;

        [SerializeField] private bool rightLeg;
        [SerializeField] private bool frontLeg;
        [SerializeField] private float velocityPrediction;

        [SerializeField] private List<SpiderLegController> oppositeLegs;

        [Header("Debug")]
        [SerializeField] private float sphereRadius;

        private float lerp;

        private Vector3 toNextPosition;
        private Vector3 bezierTopPoint;
        private Vector3 nextPosition;

        private bool legMoving;
        private Vector3 spiderPosLastFrame;

        private void Start()
        {
            spiderPosLastFrame = spiderBody.position;
        }

        void Update()
        {
            lerp += Time.deltaTime * stepSpeed;

            MoveLeg();

            if (!legMoving)
                return;

            legTarget.position = 
                Vector3.Lerp(
            Vector3.Lerp(legTarget.position, bezierTopPoint, lerp),
            Vector3.Lerp(bezierTopPoint, nextPosition, lerp),
            lerp);

            if (legTarget.position == nextPosition)
            {
                legMoving = false;
            }

            spiderPosLastFrame = spiderBody.position;
        }

        private void MoveLeg()
        {
            RaycastHit stepHit;

            if (Physics.Raycast(legRaycastOrigin.position, -spiderBody.up, out stepHit))
            {
                if ((stepHit.point - legTarget.position).magnitude >= stepTreshold && 
                    !legMoving)
                {
                    nextPosition = (stepHit.point + (spiderBody.position - spiderPosLastFrame).normalized * velocityPrediction);
                    toNextPosition = nextPosition - legTarget.position;
                    bezierTopPoint = legTarget.position +
                        toNextPosition.normalized * toNextPosition.magnitude / 2 +
                        spiderBody.up * stepHeight;

                    lerp = 0;
                    legMoving = true;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(nextPosition, sphereRadius);
        }
    }
}

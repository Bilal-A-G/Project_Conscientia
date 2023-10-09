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

        [SerializeField] private float forwardsStepTreshold;
        [SerializeField] private float backwardsStepTreshold;
        [SerializeField] private float minLegSpacingFromBody;
        [SerializeField] private float stepHeight;
        [SerializeField] private float stepSpeed;

        [SerializeField] private float legExtension;
        [Range(0,1)][SerializeField] private float legSplay;

        [SerializeField] private float legDelay;
        [SerializeField] private bool rightLeg;
        [SerializeField] private bool frontLeg;

        [Header("Debug")]
        [SerializeField] private float sphereRadius;

        private float lerp;

        private Vector3 toNewTarget;
        private Vector3 bezierTopPoint;
        private RaycastHit nextStepHit;

        bool legMoving;
        bool evaluatingLegPos;

        void Update()
        {
            lerp += Time.deltaTime * stepSpeed;

            if (!evaluatingLegPos)
            {
                StartCoroutine(MoveLeg());
            }

            if (!legMoving)
                return;

            legTarget.position = Vector3.Lerp(
            Vector3.Lerp(legTarget.position, bezierTopPoint, lerp),
            Vector3.Lerp(bezierTopPoint, nextStepHit.point, lerp),
            lerp);

            if (legTarget.position == nextStepHit.point)
            {
                legMoving = false;
            }
        }

        IEnumerator MoveLeg()
        {
            evaluatingLegPos = true;

            yield return new WaitForSeconds(legDelay);

            legRaycastOrigin.position = spiderBody.position + Vector3.Lerp(
                (frontLeg ? spiderBody.forward : -spiderBody.forward) * legExtension,
                (rightLeg ? spiderBody.right : -spiderBody.right) * legExtension, legSplay) ;

            if (Physics.Raycast(legRaycastOrigin.position, -spiderBody.up, out nextStepHit))
            {
                RaycastHit wallHit;
                Vector3 toTarget = (nextStepHit.point - spiderBody.position).normalized;

                if (Physics.Raycast(spiderBody.position, toTarget, out wallHit))
                {
                    if (wallHit.transform.gameObject != nextStepHit.transform.gameObject)
                    {
                        nextStepHit = wallHit;
                    }
                }

                float stepTreshold = forwardsStepTreshold;

                if (Vector3.Dot((nextStepHit.point - legTarget.position).normalized, -spiderBody.forward) >= 0.8f)
                {
                    stepTreshold = backwardsStepTreshold;
                }

                if ((nextStepHit.point - legTarget.position).magnitude >= stepTreshold &&
                    (spiderBody.position - nextStepHit.point).magnitude >= minLegSpacingFromBody &&
                    !legMoving)
                {
                    toNewTarget = nextStepHit.point - legTarget.position;
                    bezierTopPoint = legTarget.position +
                        toNewTarget.normalized * toNewTarget.magnitude / 2 +
                        spiderBody.up * stepHeight;

                    lerp = 0;
                    legMoving = true;
                }
            }

            evaluatingLegPos = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(nextStepHit.point, sphereRadius);
        }
    }
}

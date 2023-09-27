using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHintController : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private float heightOffset;
    [SerializeField] private Transform hint;

    void Update()
    {
        hint.position = new Vector3(target.position.x, 
            centerOfMass.position.y + heightOffset, 
            target.position.z);
    }
}

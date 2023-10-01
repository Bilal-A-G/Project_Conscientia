using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHintController : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private Transform spider;
    [SerializeField] private float heightOffset;
    [SerializeField] private Transform hint;

    void Update()
    {
        hint.position = target.position + spider.up * heightOffset;
    }
}

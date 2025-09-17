using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotTester : MonoBehaviour
{
    public Transform target;
    public float dot;
    private void Update()
    {
        dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
    }
}

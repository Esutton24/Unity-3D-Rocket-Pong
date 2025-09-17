using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class TestTarget : MonoBehaviour, IRigidbody
{
    public enum TargetPathType { Circle, Square}
    public TargetPathType pathType;
    [SerializeField] float speed, radius;
    Rigidbody rb;
    float angle;
    Vector3 pos;
    Vector3 startPosition, nextPosition;

    private void Update()
    {
        switch (pathType)
        {
            case TargetPathType.Circle:
                Circle();
                break;
            default:
                Square();
                break;
        }
    }
    void Square()
    {
        if (nextPosition == null)
            NewPosition();
        else if (Vector3.Distance(Position, nextPosition) < .5f)
            NewPosition();
        rb.velocity = (nextPosition - startPosition).normalized * speed;
        void NewPosition()
        {
            if(nextPosition == null)
            {
                startPosition = Position;
                nextPosition = startPosition + Vector3.forward * radius;
                return;
            }
            Vector3 prevDirection = nextPosition - startPosition;
            prevDirection.Normalize();
            if (prevDirection.x > 0)
            {
                prevDirection.x = 0;
                prevDirection.z = -1;
            }
            else if( prevDirection.x < 0)
            {
                prevDirection.x = 0;
                prevDirection.z = 1;
            }
            else if(prevDirection.z > 1)
            {
                prevDirection.x = 1;
                prevDirection.z = 0;
            }
            else
            {
                prevDirection.x = -1;
                prevDirection.z = 0;
            }
            startPosition = transform.position;
            prevDirection.y = 0;
            nextPosition = startPosition + prevDirection * 5;
        }
    }
    void Circle()
    {
        angle = Time.time * speed;
        angle %= 360;
        angle *= Mathf.Deg2Rad;
        pos = Vector3.forward * Mathf.Cos(angle) + Vector3.right * Mathf.Sin(angle);
        pos.Normalize();
        pos *= radius;
        rb.velocity = pos;
    }
    public Vector3 Position => transform.position;

    public Vector3 Velocity => rb.velocity;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = Position;
    }
}


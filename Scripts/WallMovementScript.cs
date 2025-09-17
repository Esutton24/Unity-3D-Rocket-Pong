using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class WallMovementScript : MonoBehaviour, IRigidbody
{
    protected Rigidbody rb;
    protected bool rocketsOn { get; private set; }
    public Vector3 Position { get { return rb == null ? Vector3.zero : rb.position; } }
    public Vector3 Velocity { get { return rb == null ? Vector3.zero : rb.velocity; } }

    [SerializeField] private Vector3 addedForceDirection;
    [SerializeField] private float rocketForce, movementForce, gravityForce, rotationSensitivity, maxVelocity;
    [SerializeField] protected Settings gameSettings;
    Quaternion startRot;
    float time = 0;
    bool lerpRotOn = false;
    private float timeBeforeLerpFinish = 0.5f;
    Transform wallObject;
    WallRocket[] Rockets;
    public Vector2 minMaxRotX;
    public Vector2 minMaxRotY;
    Quaternion startLerpRot, endLerpRot;
    float Sensitivity { get { return (gameSettings? gameSettings.Sensitivity: 1) * rotationSensitivity; } }
    protected void SetMovementDirection(float xValue, float zValue)
    {
        Vector3 horizontal = xValue * transform.right,
            forward = zValue * transform.forward;
        addedForceDirection = (horizontal + forward).normalized;
    }
    protected void Rotate(Vector3 rotationAddition)
    {
        if (lerpRotOn)
            return;
        rotationAddition *= Sensitivity;
        float xRotation = wallObject.localEulerAngles.x + rotationAddition.x;
        float yRotation = wallObject.localEulerAngles.y + rotationAddition.y;
        wallObject.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
    protected void LerpRot(Quaternion newRot)
    {
        startLerpRot = wallObject.rotation;
        endLerpRot = newRot.normalized;
        endLerpRot.z = 0;
        lerpRotOn = true;
        time = 0;
    }
    protected void ResetRot()
    {
        LerpRot(startRot);
    }
    protected void SetRocket(bool on)
    {
        rocketsOn = on;
        foreach (WallRocket r in Rockets)
            r.ToggleRocketParticles(on);
    }
    protected virtual void Update()
    {
        if (PauseMenu.gamePaused) return;
        HandleRockets();
        HandleHorizontalMovement();
        HandleRotation();

        //Added Force Direction
        GameManager.DrawGizmos(GizmoDraw.Line, Position, Color.yellow, 0, transform.position + addedForceDirection * 2);
        //Velocity Direction
        GameManager.DrawGizmos(GizmoDraw.Line, Position, Color.green, 0, Position + Velocity);
    }
    private void HandleRotation()
    {
        if (lerpRotOn && time < timeBeforeLerpFinish)
        {
            float percent = time / timeBeforeLerpFinish;
            wallObject.rotation = Quaternion.Lerp(startLerpRot, endLerpRot, percent);
            time += Time.deltaTime;
            if(time > timeBeforeLerpFinish)
            {
                lerpRotOn = false;
                wallObject.rotation = endLerpRot;
            }
        }
    }
    private void HandleRockets()
    {
        float upVelocity = rb.velocity.y + (rocketsOn ? rocketForce : -gravityForce) * Time.deltaTime;
        rb.velocity = new Vector3 (rb.velocity.x, upVelocity, rb.velocity.z);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    }
    private void HandleHorizontalMovement()
    {
        Vector3 tempVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z) + addedForceDirection * Time.deltaTime * movementForce;
        rb.velocity = Vector3.ClampMagnitude (tempVelocity, maxVelocity) + rb.velocity.y * Vector3.up;
    }
    public void SetWallColor(Color newColor)
    {
        wallObject.GetComponent<MeshRenderer>().material.color = newColor;
    }
    protected virtual void Awake()
    {
        wallObject = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        startRot = wallObject.rotation;
        Rockets = GetComponentsInChildren<WallRocket>();
    }
    public float Gravity => gravityForce;
    public float RocketForce => rocketForce;
    public float MovementForce => movementForce;
}
public interface IRigidbody
{
    public Vector3 Position { get; }
    public Vector3 Velocity { get; }
}

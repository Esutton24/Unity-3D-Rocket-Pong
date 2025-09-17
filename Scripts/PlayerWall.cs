using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerWall : WallMovementScript
{
    private Vector3 _initialPlayerPosition;
    private Quaternion _initialPlayerRotation;

    private Vector3 currentMovement => _input == null? Vector3.zero : _input.Axis1Value.ToVector3();
    private Vector2 currentRotation
    {
        get { Vector2 inputRot = _input == null? Vector2.zero : _input.Axis2Value;
            return new Vector2(inputRot.y, inputRot.x); }
    }
    public float zvalue;
    bool spacePressed => _input == null? false : _input.Button1Value > 0.5f;
    [SerializeField] WallRocket rocketLeft, rocketRight;
    [SerializeField] InputManager _input;
    private void Start()
    {
        _initialPlayerPosition = transform.position;
        _initialPlayerRotation = transform.rotation;

    }
    protected override void Update()
    {
        GameManager.DrawGizmos(GizmoDraw.Line, Position, Color.red, 0, Position + currentMovement * 2 );
        SetMovementDirection(currentMovement.x, currentMovement.z);
        SetRocket(spacePressed);
        base.Update();
    }
    private void FixedUpdate()
    {
        Rotate(currentRotation.normalized);
    }
    public void clearRockets()
    {
        rocketLeft.clearParticles();
        rocketRight.clearParticles();
    }
    public void ResetPosition()
    {
        transform.position = _initialPlayerPosition;
        transform.rotation = _initialPlayerRotation;
    }
    private void OnEnable()
    {
        _input.Button2.performed += _ctx => ResetRot();
    }
    private void OnDisable()
    {
        _input.Button2.performed -= _ctx => ResetPosition();
    }
}

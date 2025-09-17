using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Users;
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private PlayerInput _input;
    private InputAction _axis1, _axis2, _button1, _button2, _pauseButton;
    public InputAction Axis1 { get { Init(); return _axis1; } }
    public InputAction Axis2 { get { Init(); return _axis2; } }
    public InputAction Button1 { get { Init(); return _button1; } }
    public InputAction Button2 { get { Init(); return _button2; } }
    public InputAction PauseButton { get { Init(); return _pauseButton; } }

    public System.Action Button1Performed;
    public System.Action Button2Performed;
    public System.Action PauseButtonPerformed;
    [SerializeField] Vector2 _a1Val, _a2Val;
    [SerializeField] float _b1Val, _b2Val, _pbVal;
    /*public Vector2 Axis1Value { get { return _axis1.ReadValue<Vector2>(); } }
    public Vector2 Axis2Value { get { return _axis2.ReadValue<Vector2>(); } }
    public float Button1Value { get { return _button1.ReadValue<float>(); } }
    public float Button2Value { get { return _button2.ReadValue<float>(); } }
    public float PauseButtonValue { get { return _pauseButton.ReadValue<float>(); } }*/

    public Vector2 Axis1Value => _a1Val;
    public Vector2 Axis2Value => _a2Val;
    public float Button1Value => _b1Val;
    public float Button2Value => _b2Val;
    public float PauseButtonValue => _pbVal;
    bool inited = false;
    private void Init()
    {
        if (inited) return;
        inited = true;
        _input = GetComponent<PlayerInput>();
        _axis1 = _input.actions["Axis1"];
        _axis2 = _input.actions["Axis2"];
        _button1 = _input.actions["Button1"];
        _button2 = _input.actions["Button2"];
        _pauseButton = _input.actions["Pause"];
        Button1Performed += throwaway;
        Button2Performed += throwaway;
        PauseButtonPerformed += throwaway;
        Button1.performed += _ctx => Button1Performed();
        Button2.performed += _ctx => Button2Performed();
        PauseButton.performed += _ctx => PauseButtonPerformed();
    }
    private void Update()
    {
        Init();
        _a1Val = _axis1.ReadValue<Vector2>();
        _a2Val = _axis2.ReadValue<Vector2>();
        _b1Val = _button1.ReadValue<float>();
        _b2Val = _button2.ReadValue<float>();
        _pbVal = _pauseButton.ReadValue<float>();
    }
    void throwaway() { }
}

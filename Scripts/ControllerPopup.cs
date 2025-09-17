using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerPopup : MonoBehaviour
{
    [SerializeField] Vector2 restingPosition, activePosition;
    [SerializeField] float popupSpeed;
    Transform myTransform;
    Coroutine popupCoroutine;
    TMP_Text messageText;
    
    public void Popup(string message, float stayDuration)
    {
        float increment = 0.001f;
        if (popupCoroutine != null) StopCoroutine(popupCoroutine);
        popupCoroutine = StartCoroutine(PopUp());
        IEnumerator PopUp()
        {
            messageText.SetText(message);
            float percent = 0;
            while(percent < 1)
            {
                percent += increment * popupSpeed;
                SetPosition();
                yield return new WaitForSecondsRealtime(increment);
            }
            percent = 1;
            SetPosition();
            yield return new WaitForSecondsRealtime(stayDuration);
            while (percent > 0)
            {
                percent -= Time.deltaTime * popupSpeed;
                SetPosition();
                yield return new WaitForSecondsRealtime(increment);
            }
            percent = 0;
            SetPosition();
            popupCoroutine = null;
            void SetPosition() => this.SetPosition(Vector2.Lerp(restingPosition, activePosition, percent));
        }
    }
    private void Awake()
    {
        messageText = GetComponentInChildren<TMP_Text>(true);
        myTransform = transform;
        SetPosition(restingPosition);
    }
    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
    }
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
    }
    void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        Popup(device.displayName + " " + change.ToString(), 3);
    }
    void SetPosition(Vector2 newPos) => myTransform.localPosition = newPos;
}

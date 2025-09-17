using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingCircle : MonoBehaviour
{
    MeshRenderer r;
    public Vector3 newSize = new Vector3(1, 1, 1), oldSize = new Vector3(1, 1, 1);
    Vector3 oldPos, newPos;
    Color newColor, oldColor;
    float lastTime;
    public float sizeChangeSpeed;
    Coroutine sizeChangeCoroutine;
    public bool isNewlyActive;
    public float percent = 0;
    public void SetProperties(float size, Quaternion newRot, Vector3 newPos ,Color ringColor, bool snap)
    {
        newSize = Vector3.one * size;
        if (snap)
        {
            //if (sizeChangeCoroutine != null) StopCoroutine(sizeChangeCoroutine);
            transform.localScale = newSize;
            r.material.color = newColor;
            transform.position = this.newPos = newPos;
            transform.rotation = newRot;
            return;
        }
        if(size != newSize.x)
        {
            oldSize = transform.localScale;
            if (sizeChangeCoroutine != null) StopCoroutine(sizeChangeCoroutine);
            sizeChangeCoroutine = StartCoroutine(changeProperties());
        }
        transform.rotation = newRot;
        transform.position = this.newPos = newPos;
        r.material.color = ringColor;
    }
    IEnumerator changeProperties()
    {
        percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * sizeChangeSpeed;
            transform.localScale = Vector3.Lerp(oldSize, newSize, percent);
            yield return null;
        }
        sizeChangeCoroutine = null;
    }
    public void SetActive(bool on)
    {
        if (r.enabled == on) return;
        r.enabled = on;
        isNewlyActive = on && !r.enabled;
        if (!on)
        {
            transform.localScale = Vector3.one * 0.25f;
        }
        
    }
    private void Awake()
    {
        r = GetComponent<MeshRenderer>();
    }
}

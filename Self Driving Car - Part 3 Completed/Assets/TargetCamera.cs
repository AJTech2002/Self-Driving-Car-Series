using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCamera : MonoBehaviour
{
    public float smoothTime = 0.5f;
    private Vector3 startPosition, startRotation;
    private Vector3 velocity;
    private GameObject lastTarget;
    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    public void UpdateTarget(GameObject target)
    {
        if(GameObject.ReferenceEquals(lastTarget, target)){
            transform.position = target.transform.position;
            transform.eulerAngles = target.transform.eulerAngles;
        }
        else{
            ChangeTarget(target);
        }
        
    }

    private void ChangeTarget(GameObject target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target.transform.position, ref velocity, smoothTime);
        transform.eulerAngles = target.transform.eulerAngles;
        lastTarget = target;
    }
}

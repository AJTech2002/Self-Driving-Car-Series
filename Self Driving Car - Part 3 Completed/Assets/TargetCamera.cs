using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class TargetCamera : MonoBehaviour
{
    [SerializeField] GeneticManager manager;
    private Vector3 startPosition, startRotation;
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    private void Reset()
    {
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    void LateUpdate()
    {
        (GameObject target, float currentGenerationTime) = manager.StatsForCamera();
        if(currentGenerationTime >= 3)
        {
            UpdateTarget(target);
            return;
        }
        Reset();
    }

    private void UpdateTarget(GameObject target)
    {
        if(target != null)
        {
            transform.position = target.transform.position;
            transform.eulerAngles = target.transform.eulerAngles;
        }
    }
}

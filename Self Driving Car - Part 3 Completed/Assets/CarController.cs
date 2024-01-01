using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private NNet network;
    private GeneticManager manager;
    private bool canMove = false;

    [Range(-1f,1f)]
    public float a,t;

    private float timeSinceStart = 0f;

    
    [Header("Status")]
    public float gas = 50f;
    public int laps = 0;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 2f;
    public float sensorMultiplier = 0.1f;
    public float lapsMultiplier = 30f;

    public Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    public float aSensor,bSensor,cSensor;

    private void Awake() {
        manager = GameObject.FindObjectOfType<GeneticManager>();
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    public void AssignNetwork(NNet net)
    {
        network = net;
    }

    private void OnCollisionEnter (Collision collision) {
        if(collision.gameObject.tag == "Wall")
        {
            Death();
        }
    }
    private void FixedUpdate() {
        if(canMove){
            
            InputSensors();
            lastPosition = transform.position;


            (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);


            MoveCar(a,t);

            timeSinceStart += Time.deltaTime;

            CalculateFitness();
        }

    }

    public void Reset() 
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        overallFitness = 0f;
        gas = 50f;
        laps = 0;
        lastPosition = startPosition;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void Death ()
    {
        overallFitness += laps*lapsMultiplier;
        network.fitness = overallFitness;
        canMove = false;
        manager.Death();
    }

    private void CalculateFitness() {

        totalDistanceTravelled += Vector3.Distance(transform.position,lastPosition);
        avgSpeed = totalDistanceTravelled/timeSinceStart;

        overallFitness = (totalDistanceTravelled*distanceMultipler)+(avgSpeed*avgSpeedMultiplier)+(((aSensor+bSensor+cSensor)/3)*sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40) {
            Death();
        }

        if (gas <= 0) {
            Death();
        }

    }

    private void InputSensors() {

        Vector3 a = (transform.forward+transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward-transform.right);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

        int LAYER_MASK = ~(1 << 3);
        LAYER_MASK &= ~(1 << 6);

        float TOTAL_VIEW = Mathf.Infinity;

        if (Physics.Raycast(r, out hit, TOTAL_VIEW, LAYER_MASK)) {
            aSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit, TOTAL_VIEW, LAYER_MASK)) {
            bSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit, TOTAL_VIEW, LAYER_MASK)) {
            cSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

    }

    private Vector3 inp;
    public void MoveCar (float v, float h) {
        inp = Vector3.Lerp(Vector3.zero,new Vector3(0,0,v*11.4f),0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
        ConsumeGas(v);
    }

    private void ConsumeGas(float acceleration)
    {
        float CONSUMPTION = 0.000416f;
        float ACCELERATION_CONSUMPTION = 0.02f;
        gas -= CONSUMPTION+(acceleration*ACCELERATION_CONSUMPTION);
    }

    public void AllowMoviment()
    {
        canMove = true;
    }

    public bool IsMoving()
    {
        return canMove;
    }

    public NNet GetNetwork() 
    {
        return network;
    }

}

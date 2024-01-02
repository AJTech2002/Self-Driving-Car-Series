using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private float[] sensors = new float[12];

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


            (a, t) = network.RunNetwork(sensors);


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

        overallFitness = (totalDistanceTravelled*distanceMultipler)+(avgSpeed*avgSpeedMultiplier)+((sensors.Sum()/12)*sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40) {
            Death();
        }

        if (gas <= 0) {
            Death();
        }

    }

    private void InputSensors() {

        int numRays = 12;
        int LAYER_MASK = ~(1 << 3);
        LAYER_MASK &= ~(1 << 6);
        float angleStep = 180f / numRays;
        for (int i = 0; i < numRays; i++)
        {
            float angle = i * angleStep;

            Vector3 direction = Quaternion.Euler(0, angle, 0) * (transform.right * -1);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, LAYER_MASK))
            {
                float sensor = hit.distance/20;
                sensors[i] = sensor;
                Debug.DrawRay(transform.position, direction*hit.distance, Color.red);
            }

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

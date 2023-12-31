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

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    private Vector3 lastPosition;
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

        //a = 0;
        //t = 0;


    }

    public void Reset() 
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void Death ()
    {
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

        if (overallFitness >= 1000) {
            Death();
        }

    }

    private void InputSensors() {

        Vector3 a = (transform.forward+transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward-transform.right);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

        int layerMask = 3;
        layerMask = ~~layerMask;

        if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
            aSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
            bSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
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
    }

    public void AllowMoviment()
    {
        canMove = true;
    }

    public NNet GetNetwork() 
    {
        return network;
    }

}

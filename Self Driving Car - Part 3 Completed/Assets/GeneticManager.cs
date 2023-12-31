using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using System;
using System.Reflection;
using Random = UnityEngine.Random;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public GameObject spawnPoint;
    private GameObject jeep;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;



    [Header("Public View")]
    public GameObject[] population;
    public int currentGeneration;
    public int collisedCars = 0;

    private void Awake()
    {
        jeep = Resources.Load<GameObject>("jeep");
    }

    private void LateUpdate()
    {
        UpdateRanking();
        UpdateCameraTarget();
    }

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new GameObject[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        StartPopulation();
    }

    private void FillPopulationWithRandomValues (GameObject[] cars, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            NNet network = InitialiaseNetwork();
            cars[startingIndex] = InstantiateCar(network);
            startingIndex++;
        }
    }

    public void Death ()
    {
        collisedCars++;
        if (collisedCars == initialPopulation) 
        { 
            RePopulate();
        }

    }

    public void UpdateRanking()
    {
        SortPopulation();
    }

    private void UpdateCameraTarget()
    {
        GameObject bestCarAlive = GetBestCarAlive();
        CarController controller = GetController(bestCarAlive);
        TargetCamera camera = GetCamera();
        if(controller.overallFitness > 1){
            foreach (Transform child in bestCarAlive.transform)
            {
                if (child.tag == "CameraTarget")
                {
                    GameObject bestTarget = child.gameObject;
                    camera.UpdateTarget(bestTarget);
                }
            }
        }
        else {
            camera.Reset();
        }
    }

    private GameObject GetBestCarAlive()
    {
        int position = 0;
        while (position < population.Length)
        {
            CarController controller = GetController(population[position]);
            if(controller.IsMoving())
            {
                return population[position];
            }
            position++;
        }
        return population[0];
        
    }

    
    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        UpdateRanking();

        GameObject[] newPopulation = PickBestPopulation();
        ResetBest(newPopulation);
        DeleteWorses(newPopulation);

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;
        collisedCars = 0;
        ClearLog();
        StartPopulation();
    }

    private GameObject[] PickBestPopulation()
    {

        GameObject[] newPopulation = new GameObject[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i];
            NNet actualNetwork = GetNetwork(population[i]);
            NNet newNetwork = GetNetwork(newPopulation[naturallySelected]);
            newNetwork.fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(actualNetwork.fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            NNet worseNetwork = GetNetwork(population[last]);

            int f = Mathf.RoundToInt(worseNetwork.fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;

    }

    private void ResetBest(GameObject[] newPopulation)
    {
        for(int i = 0; i < bestAgentSelection; i++)
        {
            GameObject car = newPopulation[i];
            CarController controller = GetController(car);
            controller.Reset();
        }
    }

    private void DeleteWorses(GameObject[] newPopulation)
    {
        GameObject[] carsToRemove = population.Except(newPopulation).ToArray();
        foreach(GameObject car in carsToRemove)
        {
            Destroy(car);
        }
    }

    private void Mutate (GameObject[] newPopulation)
    {

        for (int i = 0; i < naturallySelected; i++)
        {
            
            NNet network = GetNetwork(newPopulation[i]);
            for (int c = 0; c < network.weights.Count; c++)
            {

                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    network.weights[c] = MutateMatrix(network.weights[c]);
                }

            }

        }

    }

    Matrix<float> MutateMatrix (Matrix<float> A)
    {

        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;

    }

    private void Crossover (GameObject[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i+=2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            NNet Child1 = new NNet();
            NNet Child2 = new NNet();

            Child1.Initialise(LAYERS, NEURONS);
            Child2.Initialise(LAYERS, NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;

            NNet networkA = GetNetwork(population[AIndex]);
            NNet networkB = GetNetwork(population[BIndex]);

            for (int w = 0; w < Child1.weights.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = networkA.weights[w];
                    Child2.weights[w] = networkB.weights[w];
                }
                else
                {
                    Child2.weights[w] = networkA.weights[w];
                    Child1.weights[w] = networkB.weights[w];
                }

            }


            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = networkA.biases[w];
                    Child2.biases[w] = networkB.biases[w];
                }
                else
                {
                    Child2.biases[w] = networkA.biases[w];
                    Child1.biases[w] = networkB.biases[w];
                }

            }

            newPopulation[naturallySelected] = InstantiateCar(Child1);
            naturallySelected++;

            newPopulation[naturallySelected] = InstantiateCar(Child2);
            naturallySelected++;

        }
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(car => 
        {
            CarController controller = GetController(car);
            return controller.overallFitness;
        }).ToArray();

    }

    private void StartPopulation(){
        foreach(GameObject car in population)
        {
            CarController controller = GetController(car);
            controller.AllowMoviment();
        }
    }

    
    private NNet InitialiaseNetwork()
    {
        NNet network = new NNet();
        network.Initialise(LAYERS, NEURONS);
        return network;
    }

    private GameObject InstantiateCar(NNet network)
    {
        GameObject car = Instantiate(jeep, spawnPoint.transform.position, spawnPoint.transform.rotation);
        CarController controller = GetController(car);
        controller.AssignNetwork(network);
        return car;
    }

    private NNet GetNetwork(GameObject car)
    {
        CarController controller = GetController(car);
        NNet network = controller.GetNetwork();
        return network;
    }

    private CarController GetController(GameObject car)
    {
        CarController controller = car.GetComponent<CarController>();
        return controller;
    }

    private TargetCamera GetCamera()
    {
        Camera camera =  GameObject.FindObjectOfType<Camera>();
        TargetCamera cameraController = camera.GetComponent<TargetCamera>();
        return cameraController;
    }

    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

}

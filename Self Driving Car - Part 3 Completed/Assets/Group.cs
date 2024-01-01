
using UnityEngine;
using UnityEngine.UI;

public class Group : MonoBehaviour
{
    public Text overall;
    public Text acceleration;
    public Text gas;
    public Text laps;
    public Text carsAlive;

    public void UpdateStatus(CarController controller, int cars)
    {
        overall.text = "Overall: "+controller.overallFitness;
        acceleration.text = "Acceleration: "+controller.a;
        gas.text = "Gas: "+controller.gas;
        laps.text = "Laps: "+controller.laps;
        carsAlive.text = "Cars Alive: "+cars;
    }
}

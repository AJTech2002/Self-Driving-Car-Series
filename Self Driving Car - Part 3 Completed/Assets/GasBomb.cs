using UnityEngine;

public class Radar : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        GameObject car = collision.gameObject;
        CarController controller = car.GetComponent<CarController>();
        if(controller.IsMoving())
        {
            controller.gas += 27f;
            controller.laps++;
        }
    }
}

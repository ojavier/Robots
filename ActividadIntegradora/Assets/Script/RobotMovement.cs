using UnityEngine;

public class RobotMovement : MonoBehaviour
{
    // Definir variable y arreglo
    public float speed = 5f;
    private Vector3 targetPosition;

    // Este método es llamado al iniciar el programa
    void Start()
    {
        targetPosition = transform.position;
    }

    // Este método es llamado varias veces y define el movimiento hacia la posición deseada
    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    // Método para cambiar la posición objetivo
    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }
}

using UnityEngine;

public class RobotMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        // Movimiento hacia la posición objetivo
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }

        // Lógica para actualizar targetPosition
        // Aquí puedes agregar la lógica para cambiar la posición objetivo
        // Ejemplo: targetPosition = new Vector3(5, 0, 5);
    }

    // Método para cambiar la posición objetivo
    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }
}

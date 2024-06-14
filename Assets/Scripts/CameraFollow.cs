using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform; // Referencia al transform del jugador
    public Vector3 offset; // Desplazamiento de la cámara respecto al jugador

    private void LateUpdate()
    {
        if (playerTransform != null)
        {
            // Actualiza la posición de la cámara para que siga al jugador
            transform.position = playerTransform.position + offset;
        }
    }
}
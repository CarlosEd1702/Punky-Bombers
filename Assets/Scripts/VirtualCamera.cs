using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera2;

    private void Start()
    {
            SetupCamera();
    }

    private void SetupCamera()
    {
        // Espera a que se instancie el jugador
        StartCoroutine(WaitForPlayerInstantiation());
    }

    private IEnumerator WaitForPlayerInstantiation()
    {
        // Espera hasta que el jugador se instancie
        while (true)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Si el jugador se ha instanciado, configura la cámara para seguir al jugador
                Debug.Log("Player Found");
                virtualCamera.Follow = player.transform;
                virtualCamera2.Follow = player.transform;
                break;
            }
            yield return null;
        }
    }
}

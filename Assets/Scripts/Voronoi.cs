using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Voronoi : NetworkBehaviour
{
    [SerializeField] private List<GameObject> voronoi; // Lista de fragmentos de Voronoi
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TNT"))
        {
            // Desactivar kinemático y activar gravedad en los fragmentos de Voronoi
            foreach (var fragment in voronoi)
            {
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            // Iniciar coroutine para destruir fragmentos y el objeto padre después de 5 segundos
            StartCoroutine(DestroyVoronoiAfterDelay(5.0f));
        }
    }

    private IEnumerator DestroyVoronoiAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Destruir fragmentos de Voronoi
        foreach (var fragment in voronoi)
        {
            if (fragment != null)
            {
                Destroy(fragment);
            }
        }

        // Destruir el objeto padre
        Destroy(gameObject);
    }
}

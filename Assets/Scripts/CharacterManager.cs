using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private LayerMask whatToDetect;
    public Button btn;
    public GameObject Spawner;
    public Transform BombSpawner;
    public GameObject smokePrefab;
    public GameObject bombPrefab;
    public GameObject explosionPrefab;
    public GameObject fireTrailsPrefab;
    public VisualEffect fireEffect;
    public float bombRadius;
    private Transform bombPosition; // Almacena la posición de la bomba cuando se coloca
    private Vector3 bombVector;
    public float explosionRange; // Alcance de la explosión
    [SerializeField] private PlayerInput playerInput;

    public void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(SpawnBomb); // Llama a la función SpawnBomb al hacer clic en el botón
            Spawner.SetActive(false);
        }
    }

    public void SpawnBomb()
    {
        Debug.Log("Bomb Spawned");
        Spawner.SetActive(true);
        // Instancia smokePrefab y bombPrefab
        GameObject smokeInstance = Instantiate(smokePrefab, BombSpawner.position, BombSpawner.rotation);
        GameObject bombInstance = Instantiate(bombPrefab, BombSpawner.position, Quaternion.identity);
        // Almacena la posición y rotación de la bomba cuando se coloca
        bombVector = bombInstance.transform.position;
        //Debug.Log("Bomb Instancied " + bombVector);
        bombPosition = bombInstance.transform;
        // Inicia la coroutine para instanciar la explosión y los rastros de fuego después de 1 segundo
        StartCoroutine(SpawnExplosionAndFireTrails(smokeInstance, bombInstance));
    }

    IEnumerator SpawnExplosionAndFireTrails(GameObject smoke, GameObject bomb)
    {
        // Espera 1 segundo
        yield return new WaitForSeconds(3f);
        // Destruye smokePrefab y bombPrefab después de 1 segundo
        CheckCollision(bombVector);
        Destroy(smoke);
        Destroy(bomb);
        // Instancia explosionPrefab y fireTrailsPrefab en la posición y rotación de la bomba
        GameObject explosionInstance = Instantiate(explosionPrefab, bombPosition.position, bombPosition.rotation);
        GameObject fireTrailsInstance = Instantiate(fireTrailsPrefab, bombPosition.position, bombPosition.rotation);
        // Llama a la función para destruir los clones después de un tiempo determinado
        StartCoroutine(DestroyClones(explosionInstance, fireTrailsInstance));
    }

    IEnumerator DestroyClones(GameObject explosionClone, GameObject fireTrailsClone)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(5f); // Por ejemplo, esperamos 5 segundos
        // Destruye los clones de la explosión y los rastros de fuego
        Destroy(explosionClone);
        Destroy(fireTrailsClone);
    }

    void CheckCollision(Vector3 bombVector)
    {
        // Calcular la posición de inicio del raycast
        Vector3 raycastOrigin = bombVector;

        // Lanzar rayos en las cuatro direcciones principales
        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin, bombPosition.transform.forward, out hit, explosionRange))
        {
            Debug.Log("Check Collision Forward");
            DrawRaycastPath(raycastOrigin, bombPosition.transform.forward, hit.point);
            HandleHit(hit);
        }
        if (Physics.Raycast(raycastOrigin, -bombPosition.transform.forward, out hit, explosionRange))
        {
            Debug.Log("Check Collision Back");
            DrawRaycastPath(raycastOrigin, -bombPosition.transform.forward, hit.point);
            HandleHit(hit);
        }
        if (Physics.Raycast(raycastOrigin, bombPosition.transform.right, out hit, explosionRange))
        {
            Debug.Log("Check Collision Right");
            DrawRaycastPath(raycastOrigin, bombPosition.transform.right, hit.point);
            HandleHit(hit);
        }
        if (Physics.Raycast(raycastOrigin, -bombPosition.transform.right, out hit, explosionRange))
        {
            Debug.Log("Check Collision Left");
            DrawRaycastPath(raycastOrigin, -bombPosition.transform.right, hit.point);
            HandleHit(hit);
        }
    }

    void DrawRaycastPath(Vector3 origin, Vector3 direction, Vector3 hitPoint)
    {
        Vector3 currentPosition = origin;
        Vector3 step = direction.normalized * 0.1f; // Tamaño de paso para los puntos a lo largo del rayo

        while (Vector3.Distance(currentPosition, hitPoint) > step.magnitude)
        {
            InstantiateFireEffect(currentPosition);
            currentPosition += step;
        }
    }

    void InstantiateFireEffect(Vector3 position)
    {
        Instantiate(fireEffect, position, Quaternion.identity);
    }

    IEnumerator DestroyFireClones(GameObject fireClone)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(2f); // Por ejemplo, esperamos 5 segundos
        // Destruye los clones de la explosión y los rastros de fuego
        Destroy(fireClone);
    }


    void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Brick"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Brick"
            Debug.Log("Hit Brick: " + hit.collider.gameObject.name);
            Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.green, 2.0f); // Dibuja un rayo verde en la normal de la superficie
            Destroy(hit.collider.gameObject); // Destruir el ladrillo
        }
        else if (hit.collider.CompareTag("Map"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Map"
            Debug.Log("Hit Map: " + hit.collider.gameObject.name);
            // Aplicar algún efecto al golpear el "Map"
        }
        else if (hit.collider.CompareTag("Player"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Player"
            Debug.Log("Hit Player: " + hit.collider.gameObject.name);
            Instantiate(fireEffect, hit.transform.position, hit.transform.rotation);
            Destroy(hit.collider.gameObject);  
            // Aplicar algún efecto al golpear al jugador
        }
        else
        {
            // No se detectó colisión con objetos especiales
            Debug.Log("Hit something else: " + hit.collider.gameObject.name);
        }
    }
}
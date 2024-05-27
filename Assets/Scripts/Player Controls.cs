using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControls : NetworkBehaviour
{
    private GameObject player;
    private GameObject BombSpawner;
    private GameObject button;
    [SerializeField] private Button btn;
    private Transform bombPosition; // Almacena la posición de la bomba cuando se coloca
    private Vector3 bombVector; // Origen del Raycast

    [Header("Bomb")]
    [SerializeField] private Transform TransformSpawner;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float explosionRange;

/*    [Header("Brick Cube")]
    [SerializeField] public CubeSpawner _brickSpawn;*/

    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        BombSpawner = GameObject.FindGameObjectWithTag("Bomb Spawn");
        button = GameObject.FindGameObjectWithTag("A");
        btn = button.GetComponent<Button>();


        if (btn != null)
        {
            btn.onClick.AddListener(SpawnBomb); // Llama a la función SpawnBomb al hacer clic en el botón
        }
        else
        {
            Debug.Log("btn was not found");
        }
    }

    public void SpawnBomb()
    {
        Debug.Log("Bomb Spawned");
        // Instancia smokePrefab y bombPrefab
        GameObject smokeInstance = Instantiate(smokePrefab, TransformSpawner.position, TransformSpawner.rotation);
        GameObject bombInstance = Instantiate(bombPrefab, TransformSpawner.position, Quaternion.identity);
        // Almacena la posición y rotación de la bomba cuando se coloca
        bombVector = bombInstance.transform.position;
        //Debug.Log("Bomb Instancied " + bombVector);
        bombPosition = bombInstance.transform;
        // Verifica si bombPosition no es nulo antes de continuar
        if (bombPosition != null)
        {
            // Inicia la coroutine para instanciar la explosión y los rastros de fuego después de 1 segundo
            StartCoroutine(SpawnExplosion(smokeInstance, bombInstance));
        }
        else
        {
            Debug.LogError("Bomb position is null.");
        }
    }

    IEnumerator SpawnExplosion(GameObject smoke, GameObject bomb)
    {
        // Espera 1 segundo
        yield return new WaitForSeconds(3f);
        // Destruye smokePrefab y bombPrefab después de 1 segundo
        CheckCollision(bombVector);
        Destroy(smoke);
        Destroy(bomb);
        // Instancia explosionPrefab y fireTrailsPrefab en la posición y rotación de la bomba
        GameObject explosionInstance = Instantiate(explosionPrefab, bombPosition.position, bombPosition.rotation);
        // Llama a la función para destruir los clones después de un tiempo determinado
        StartCoroutine(DestroyClones(explosionInstance));
    }

    IEnumerator DestroyClones(GameObject explosionClone)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(2.5f); // Por ejemplo, esperamos 5 segundos
        // Destruye los clones de la explosión y los rastros de fuego
        Destroy(explosionClone);
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
        Vector3 step = direction.normalized * 1f; // Tamaño de paso para los puntos a lo largo del rayo

        while (Vector3.Distance(currentPosition, hitPoint) > step.magnitude)
        {
            InstantiateFireEffect(currentPosition);
            currentPosition += step;
        }
    }

    void InstantiateFireEffect(Vector3 position)
    {
        Instantiate(firePrefab, position, Quaternion.identity);

        StartCoroutine(DestroyFireClones(firePrefab));
    }
    IEnumerator DestroyFireClones(GameObject fireClone)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(2.5f);
        // Destruye los clones de la explosión y los rastros de fuego
        DestroyImmediate(fireClone, true);
    }
    public void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Brick"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Brick"
            Debug.Log("Hit Brick: " + hit.collider.gameObject.name);
            //Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.green, 2.0f); // Dibuja un rayo verde en la normal de la superficie
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
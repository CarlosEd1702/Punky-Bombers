using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerControlsRPC : NetworkBehaviour
{
    private GameObject button;
    [SerializeField] private Button btn;
    private Transform bombPosition; // Almacena la posición de la bomba cuando se coloca
    private Vector3 bombVector; // Origen del Raycast

    [Header("Bomb")]
    private Transform TransformSpawner; // El transform del jugador se asignará a esta variable
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float explosionRange;
    [SerializeField] private CharacterController controller;

    [Header("Flame")]
    [SerializeField] private GameObject Flame;

    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;

    void Start()
    {
        TransformSpawner = transform; // Asignar el transform del jugador
        button = GameObject.FindGameObjectWithTag("A");
        btn = button.GetComponent<Button>();

        if (IsOwner)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(() => SpawnBombServerRpc(TransformSpawner.position, TransformSpawner.rotation)); // Llama a la función SpawnBomb al hacer clic en el botón
            }
            else
            {
                Debug.Log("btn was not found");
            }
        }
    }

    private void Update()
    {
        if (IsOwner) { 
        
        }
    }

    [ServerRpc]
    public void SpawnBombServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log("SpawnBombServerRpc called");

        // Verifica si el TransformSpawner está definido
        if (TransformSpawner == null)
        {
            Debug.LogError("TransformSpawner is not set. Cancelling bomb spawn.");
            return;
        }

        Debug.Log($"TransformSpawner Position: {TransformSpawner.position}, Rotation: {TransformSpawner.rotation}");
        Debug.Log($"Requested Position: {position}, Rotation: {rotation}");

        // Ajustar la posición de Smoke Instance 
        Vector3 adjustedPosition = new Vector3(position.x, position.y + 1.5f, position.z);

        // Rotacion grados de Smoke Instance
        Quaternion adjustedRotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
        /*        Quaternion adjustedBombRotation = Quaternion.Euler(rotation.eulerAngles.x + 270, rotation.eulerAngles.y, rotation.eulerAngles.z);
        */
        Vector3 spawnPosition = controller.transform.position + new Vector3(0, 1f, 0);
        Quaternion spawnRotation = Quaternion.Euler(270, 0, 0);
        // Instancia smokePrefab y bombPrefab en la posición y rotación ajustadas
        GameObject smokeInstance = Instantiate(smokePrefab, adjustedPosition, adjustedRotation);
        /*        GameObject bombInstance = Instantiate(bombPrefab, adjustedPosition, adjustedBombRotation);
        */
        GameObject bombInstance = Instantiate(bombPrefab, spawnPosition, spawnRotation);

        NetworkObject smokeNetworkObject = smokeInstance.GetComponent<NetworkObject>();
        NetworkObject bombNetworkObject = bombInstance.GetComponent<NetworkObject>();

        smokeNetworkObject.Spawn();
        bombNetworkObject.Spawn();

        // Almacena la posición y rotación de la bomba cuando se coloca
        bombVector = bombInstance.transform.position;
        bombPosition = bombInstance.transform;

        Debug.Log($"Bomb Position: {bombPosition.position}");

        // Inicia la coroutine para instanciar la explosión y los rastros de fuego después de 1 segundo
        StartCoroutine(SpawnExplosion(smokeNetworkObject.NetworkObjectId, bombNetworkObject.NetworkObjectId));
    }

    IEnumerator SpawnExplosion(ulong smokeId, ulong bombId)
    {
        // Espera 3 segundos
        yield return new WaitForSeconds(3f);

        var smokeNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[smokeId];
        var bombNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bombId];

        CheckCollision(bombVector);
        Destroy(smokeNetworkObject.gameObject);
        Destroy(bombNetworkObject.gameObject);

        // Instancia explosionPrefab y fireTrailsPrefab en la posición y rotación de la bomba
        GameObject explosionInstance = Instantiate(explosionPrefab, bombPosition.position, bombPosition.rotation);
        NetworkObject explosionNetworkObject = explosionInstance.GetComponent<NetworkObject>();
        explosionNetworkObject.Spawn();

        // Llama a la función para destruir los clones después de un tiempo determinado
        StartCoroutine(DestroyClones(explosionNetworkObject.NetworkObjectId));
    }

    IEnumerator DestroyClones(ulong explosionId)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(2.5f); // Por ejemplo, esperamos 5 segundos

        var explosionNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[explosionId];
        Destroy(explosionNetworkObject.gameObject);
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
        GameObject fireInstance = Instantiate(firePrefab, position, Quaternion.identity);
        NetworkObject fireNetworkObject = fireInstance.GetComponent<NetworkObject>();
        fireNetworkObject.Spawn();

        StartCoroutine(DestroyFireClones(fireNetworkObject.NetworkObjectId));
    }

    IEnumerator DestroyFireClones(ulong fireId)
    {
        // Espera un tiempo antes de destruir los clones
        yield return new WaitForSeconds(2.5f);

        var fireNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[fireId];
        Destroy(fireNetworkObject.gameObject);
    }

    public void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Brick"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Brick"
            Debug.Log("Hit Brick: " + hit.collider.gameObject.name);
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
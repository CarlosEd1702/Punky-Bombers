using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;

public class PlayerControlsRPC : NetworkBehaviour
{
    [Header("UI")]
    private GameObject button;
    private GameObject buttonB;
    [SerializeField] private Button btn;
    [SerializeField] private Button btnB;

    [Header("Dash")]
    public float activeTime = 1f;
    public float meshRefrshRate = 0.1f;
    public float meshDestroyDelay = 0.5f;
    private bool isTrialActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    public Transform positionToSpawn;
    public Material mat;
    public Rigidbody rb;
    public float dashSpeed;
    public float dashTime;

    [Header("Bomb")]
    private Transform TransformSpawner; // El transform del jugador se asignará a esta variable
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float explosionRange;
    [SerializeField] private CharacterController controller;
    [SerializeField] private GameObject SpawnBomb;
    [SerializeField] private Transform T_SpawnBomb;
    private Transform bombPosition; // Almacena la posición de la bomba cuando se coloca
    private Vector3 bombVector; // Origen del Raycast

    [Header("Flame and TNT")]
    [SerializeField] private GameObject P_FlameTNT;

    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;
    
    [Header("Player")]
    [SerializeField] private CollectItems collectItems;

    private bool isBombActive = false; // Nueva variable para rastrear si una bomba está activa
    private bool isDashing = false;

    void Start()
    {
        TransformSpawner = transform; // Asignar el transform del jugador
        button = GameObject.FindGameObjectWithTag("A");
        btn = button.GetComponent<Button>();
        buttonB = GameObject.FindGameObjectWithTag("B");
        btnB = btn.GetComponent<Button>();

        SpawnBomb = GameObject.FindGameObjectWithTag("Bomb Spawn");
        T_SpawnBomb = SpawnBomb.GetComponent<Transform>();


        if (IsOwner)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(() => TrySpawnBomb()); // Llama a TrySpawnBomb al hacer clic en el botó
                btnB.onClick.AddListener(() => TryDash());
            }
            else
            {
                Debug.Log("btn was not found");
            }
        }
    }
    //------------------------------------
    private void TryDash()
    {
        if (!isDashing)
        {
            DashRpc(positionToSpawn.position, positionToSpawn.rotation);
        }
        else
        {
            Debug.Log("A Dash is already active. Please wait for used oter dash.");
        }
    }

    [ServerRpc]
    public void DashRpc(Vector3 position, Quaternion rotation)
    {
        
        
        StartCoroutine(Dash());
        StartCoroutine(ActiveTrial(activeTime));


    }
    IEnumerator Dash()
    {

        Vector3 dashDirection = transform.forward;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            if (rb != null)
            {
                rb.velocity = dashDirection * dashSpeed;

            }

            yield return null;
        }
    }
    IEnumerator ActiveTrial(float timeActive)
    {
        while(timeActive > 0 )
        {
            timeActive -= meshRefrshRate;
            
            if(skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for(int i = 0; i<skinnedMeshRenderers.Length; i++)
            {
                GameObject gameObject = new GameObject();

                gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                Destroy(gameObject, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefrshRate);
        }

        isTrialActive = false;

    }
    //-----------------------------------
    private void TrySpawnBomb()
    {
        if (!isBombActive)
        {
            SpawnBombServerRpc(TransformSpawner.position, TransformSpawner.rotation);
        }
        else
        {
            Debug.Log("A bomb is already active. Please wait for it to explode.");
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

        isBombActive = true; // Marca la bomba como activa

        // Ajustar la posición de Smoke Instance 
        Vector3 adjustedPosition = new Vector3(position.x, Mathf.Max(position.y, 1f), position.z); // Ajustar la altura Y a un valor mínimo 
        Quaternion adjustedRotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);

        // Vector3 spawnPosition = controller.transform.position + new Vector3(0, 2.5f, 0);
        Quaternion bombSpawnRotation = Quaternion.Euler(270, 0, 0);

        // Instancia smokePrefab y bombPrefab en la posición y rotación ajustadas
        GameObject smokeInstance = Instantiate(smokePrefab, adjustedPosition, adjustedRotation);
        GameObject bombInstance = Instantiate(bombPrefab, adjustedPosition, Quaternion.identity);

        NetworkObject smokeNetworkObject = smokeInstance.GetComponent<NetworkObject>();
        NetworkObject bombNetworkObject = bombInstance.GetComponent<NetworkObject>();

        smokeNetworkObject.Spawn();
        bombNetworkObject.Spawn();

        // Almacena la posición y rotación de la bomba cuando se coloca
        bombVector = bombInstance.transform.position;
        bombPosition = bombInstance.transform;

        Debug.Log($"Bomb Position: {bombPosition.position}");

        // Inicia la coroutine para instanciar el TNT y los rastros de fuego después de 1 segundo
        StartCoroutine(SpawnTNTAndFire(smokeNetworkObject.NetworkObjectId, bombNetworkObject.NetworkObjectId));
    }

    IEnumerator SpawnTNTAndFire(ulong smokeId, ulong bombId)
    {
        // Espera 3 segundos
        yield return new WaitForSeconds(3f);

        var smokeNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[smokeId];
        var bombNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bombId];

        CheckCollision(bombVector);
        Destroy(smokeNetworkObject.gameObject);
        Destroy(bombNetworkObject.gameObject);

        isBombActive = false; // Marca la bomba como no activa después de destruir los clones
    }

    void CheckCollision(Vector3 bombVector)
    {
        // Calcular la posición de inicio del raycast
        Vector3 raycastOrigin = bombVector;

        // Direcciones para los rayos en el plano XZ
        Vector3[] directions = {
            bombPosition.transform.forward,  // Hacia adelante
            -bombPosition.transform.forward, // Hacia atrás
            bombPosition.transform.right,    // Hacia la derecha
            -bombPosition.transform.right    // Hacia la izquierda
        };

        Color[] debugColors = {
            Color.blue,  // Color para adelante
            Color.red,   // Color para atrás
            Color.yellow, // Color para la derecha
            Color.white  // Color para la izquierda
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 direction = directions[i];
            float currentDistance = 0f;

            while (currentDistance < explosionRange)
            {
                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin + direction * currentDistance, direction, out hit, explosionRange - currentDistance))
                {
                    Debug.Log($"Check Collision {direction}");
                    HandleHit(hit);
                    Debug.DrawRay(raycastOrigin, direction * (currentDistance + hit.distance), debugColors[i], 2f); // Dibuja el rayo

                    // Instancia el fuego a lo largo del recorrido
                    float segmentLength = 1f; // Longitud de cada segmento
                    for (float j = currentDistance; j < currentDistance + hit.distance; j += segmentLength)
                    {
                        Vector3 firePosition = raycastOrigin + direction * j;
                        GameObject fireInstance = Instantiate(firePrefab, firePosition, Quaternion.identity);
                        fireInstance.GetComponent<NetworkObject>().Spawn();
                        StartCoroutine(DestroyFireAfterDelay(fireInstance, 1f)); // Destruir después de 1 segundo
                    }

                    currentDistance += hit.distance;
                    break; // Salir del bucle si se detecta una colisión
                }
                else
                {
                    Debug.DrawRay(raycastOrigin, direction * explosionRange, debugColors[i], 2f); // Dibuja el rayo completo si no hay colisión

                    // Instancia el fuego a lo largo del recorrido
                    float segmentLength = 1f; // Longitud de cada segmento
                    for (float j = currentDistance; j < explosionRange; j += segmentLength)
                    {
                        Vector3 firePosition = raycastOrigin + direction * j;
                        GameObject fireInstance = Instantiate(firePrefab, firePosition, Quaternion.identity);
                        fireInstance.GetComponent<NetworkObject>().Spawn();
                        StartCoroutine(DestroyFireAfterDelay(fireInstance, 1f)); // Destruir después de 1 segundo
                    }

                    currentDistance = explosionRange; // Terminar el bucle
                }
            }
        }
    }

    void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Brick"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Brick"
            Debug.Log("Hit Brick: " + hit.collider.gameObject.name);

            // Instanciar el TNT en la posición del ladrillo
            Instantiate(P_FlameTNT, hit.transform.position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();

            // Instanciar el fuego en la posición del ladrillo
            GameObject fireInstance = Instantiate(firePrefab, hit.transform.position, Quaternion.identity);
            fireInstance.GetComponent<NetworkObject>().Spawn();

            // Destruir todo el objeto "Brick" después de 2 segundos
            StartCoroutine(DestroyAfterDelay(hit.collider.gameObject, fireInstance, 2.0f));
        }
        else if (hit.collider.CompareTag("Player"))
        {
            // Se detectó una colisión con un objeto etiquetado como "Player"
            Debug.Log("Hit Player: " + hit.collider.gameObject.name);
            Destroy(hit.collider.gameObject);
        }
        else
        {
            // No se detectó colisión con objetos especiales
            Debug.Log("Hit something else: " + hit.collider.gameObject.name);
        }
    }

    IEnumerator DestroyFireAfterDelay(GameObject fireInstance, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (fireInstance != null)
        {
            Destroy(fireInstance);
        }
    }

    IEnumerator DestroyAfterDelay(GameObject target, GameObject fireInstance, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Verificar si el objeto objetivo y el efecto de fuego son válidos antes de destruirlos
        if (target != null)
        {
            Destroy(target);
        }

        if (fireInstance != null)
        {
            Destroy(fireInstance);
        }
        else
        {
            Debug.Log("Fire couldnt be able to destroy");
        }
    }
}

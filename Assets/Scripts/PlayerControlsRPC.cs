using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerControlsRPC : NetworkBehaviour
{   
    [Header("UI")]
    private GameObject button;
    [SerializeField] private Button btn;

    [Header("Bomb")] 
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float defaultExplosionRange = 1f; // Inicializar con 1
    private Transform TransformSpawner; // El transform del jugador se asignará a esta variable
    private GameObject bombSound;
    private AudioSource explosionSound;

    
    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovement; 
    
    [Header("Player Life")]
    [SerializeField] private PlayerLife playerLife;
    
    [Header("Collect Items")]
    [SerializeField] private CollectItems collectItems;

    private ItemsCounter itemsCounter;
    private List<GameObject> activeBombs = new List<GameObject>();

    void Start()
    {
        TransformSpawner = transform; // Asignar el transform del jugador
        button = GameObject.FindGameObjectWithTag("A");
        btn = button.GetComponent<Button>();
        bombSound = GameObject.FindGameObjectWithTag("Bomb Sound");
        explosionSound = bombSound.GetComponent<AudioSource>();

        itemsCounter = ItemsCounter.instancie;
        
        if (IsOwner)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(TrySpawnBomb); // Llama a TrySpawnBomb al hacer clic en el botón
            }
            else
            {
                Debug.Log("btn was not found");
            }
        }
    }

    private void TrySpawnBomb()
    {
        if (activeBombs.Count < itemsCounter.CurrentBooms)
        {
            SpawnBombServerRpc(TransformSpawner.position, TransformSpawner.rotation);
        }
        else
        {
            Debug.Log("Maximum number of active bombs reached. Please wait for a bomb to explode.");
        }
    }

    [ServerRpc]
    public void SpawnBombServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log("SpawnBombServerRpc called");

        if (TransformSpawner == null)
        {
            Debug.LogError("TransformSpawner is not set. Cancelling bomb spawn.");
            return;
        }

        // Ajustar la posición de Smoke Instance 
        Vector3 adjustedPosition = new Vector3(position.x, Mathf.Max(position.y, 1f), position.z); // Ajustar la altura Y a un valor mínimo 

        // Instancia smokePrefab y bombPrefab en la posición y rotación ajustadas
        GameObject smokeInstance = Instantiate(smokePrefab, adjustedPosition, rotation);
        GameObject bombInstance = Instantiate(bombPrefab, adjustedPosition, Quaternion.identity);

        NetworkObject smokeNetworkObject = smokeInstance.GetComponent<NetworkObject>();
        NetworkObject bombNetworkObject = bombInstance.GetComponent<NetworkObject>();

        smokeNetworkObject.Spawn();
        bombNetworkObject.Spawn();

        activeBombs.Add(bombInstance); // Añadir la bomba a la lista de bombas activas

        // Inicia la coroutine para instanciar el TNT y los rastros de fuego después de 3 segundos
        StartCoroutine(SpawnTNTAndFire(smokeNetworkObject.NetworkObjectId, bombNetworkObject.NetworkObjectId, bombInstance));
    }

    IEnumerator SpawnTNTAndFire(ulong smokeId, ulong bombId, GameObject bombInstance)
    {
        yield return new WaitForSeconds(3f);

        var smokeNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[smokeId];
        var bombNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bombId];

        float explosionRange = itemsCounter.CurrentFlame > 0 ? itemsCounter.CurrentFlame : defaultExplosionRange;
        CheckCollision(bombNetworkObject.transform.position, explosionRange);
        Destroy(smokeNetworkObject.gameObject);
        Destroy(bombNetworkObject.gameObject);
        
        PlayExplosionSoundClientRpc();
        
        activeBombs.Remove(bombInstance); // Eliminar la bomba de la lista de bombas activas
    }

    [ClientRpc]
    private void PlayExplosionSoundClientRpc()
    {
        explosionSound.Play();
    }

    void CheckCollision(Vector3 bombPosition, float explosionRange)
    {
        Vector3 raycastOrigin = bombPosition;
        Vector3[] directions = {
            Vector3.forward,  // Hacia adelante
            Vector3.back,     // Hacia atrás
            Vector3.right,    // Hacia la derecha
            Vector3.left      // Hacia la izquierda
        };

        Color[] debugColors = {
            Color.blue,  // Color para adelante
            Color.red,   // Color para atrás
            Color.yellow, // Color para la derecha
            Color.white  // Color para la izquierda
        };

        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastOrigin, direction, out hit, explosionRange))
            {
                HandleHit(hit);
                //Debug.DrawRay(raycastOrigin, direction * hit.distance, debugColors[Array.IndexOf(directions, direction)], 2f);
                CreateFireTrail(raycastOrigin, direction, hit.distance);
            }
            else
            {
                //Debug.DrawRay(raycastOrigin, direction * explosionRange, debugColors[Array.IndexOf(directions, direction)], 2f);
                CreateFireTrail(raycastOrigin, direction, explosionRange);
            }
        }
    }
    void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Brick"))
        {
            Debug.Log("Hit Brick: " + hit.collider.gameObject.name);
            GameObject fireInstance = Instantiate(firePrefab, hit.transform.position, Quaternion.identity);
            fireInstance.GetComponent<NetworkObject>().Spawn();
            StartCoroutine(DestroyAfterDelay(hit.collider.gameObject, fireInstance, 1.0f));
        }
        else if (hit.collider.CompareTag("Player"))
        {
            Debug.Log("Hit Player: " + hit.collider.gameObject.name);
            PlayerLife playerLife = hit.collider.GetComponent<PlayerLife>();
            if (playerLife != null)        {
                playerLife.HandlePlayerHit(); // Llamar al método para manejar el golpe al jugador
            }
        }
        else if (hit.collider.CompareTag("Shield Image"))
        {
            Debug.Log("Hit Shield: " + hit.collider.gameObject.name);
            collectItems.shieldIsActive.Value = false;
            hit.collider.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Hit something else: " + hit.collider.gameObject.name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateShieldImageServerRpc()
    {
        DeactivateShieldImageClientRpc();
    }

    [ClientRpc]
    private void DeactivateShieldImageClientRpc()
    {
        if (collectItems.ShieldImage != null)
        {
            collectItems.ShieldImage.SetActive(false);
            collectItems.shieldIsActive.Value = false;
        }
    }

    void CreateFire(Vector3 position)
    {
        GameObject fireInstance = Instantiate(firePrefab, position, Quaternion.identity);
        fireInstance.GetComponent<NetworkObject>().Spawn();
        StartCoroutine(DestroyFireAfterDelay(fireInstance, 1f));
    }

    void CreateFireTrail(Vector3 origin, Vector3 direction, float distance)
    {
        float segmentLength = 1f;
        for (float i = 0; i < distance; i += segmentLength)
        {
            Vector3 firePosition = origin + direction * i;
            CreateFire(firePosition);
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

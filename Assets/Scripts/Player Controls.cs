using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
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

        if (btn != null)
        {
            btn.onClick.AddListener(TrySpawnBomb); // Llama a TrySpawnBomb al hacer clic en el botón
        }
        else
        {
            Debug.Log("btn was not found");
        }
    }

    private void TrySpawnBomb()
    {
        if (activeBombs.Count < itemsCounter.CurrentBooms)
        {
            SpawnBomb(TransformSpawner.position, TransformSpawner.rotation);
        }
        else
        {
            Debug.Log("Maximum number of active bombs reached. Please wait for a bomb to explode.");
        }
    }
    
    public void SpawnBomb()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        SpawnBomb(position, rotation);
    }

    public void SpawnBomb(Vector3 position, Quaternion rotation) // Cambiado a public
    {
        Debug.Log("SpawnBomb called");

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

        activeBombs.Add(bombInstance); // Añadir la bomba a la lista de bombas activas

        // Inicia la coroutine para instanciar el TNT y los rastros de fuego después de 3 segundos
        StartCoroutine(SpawnTNTAndFire(smokeInstance, bombInstance));
    }

    IEnumerator SpawnTNTAndFire(GameObject smokeInstance, GameObject bombInstance)
    {
        yield return new WaitForSeconds(3f);

        float explosionRange = itemsCounter.CurrentFlame > 0 ? itemsCounter.CurrentFlame : defaultExplosionRange;
        CheckCollision(bombInstance.transform.position, explosionRange);
        Destroy(smokeInstance);
        Destroy(bombInstance);

        explosionSound.Play();

        activeBombs.Remove(bombInstance); // Eliminar la bomba de la lista de bombas activas
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

        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastOrigin, direction, out hit, explosionRange))
            {
                HandleHit(hit);
                CreateFireTrail(raycastOrigin, direction, hit.distance);
            }
            else
            {
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
            StartCoroutine(DestroyAfterDelay(hit.collider.gameObject, fireInstance, 1.0f));
        }
        else if (hit.collider.CompareTag("Player"))
        {
            Debug.Log("Hit Player: " + hit.collider.gameObject.name);
            PlayerLife playerLife = hit.collider.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
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

    void CreateFire(Vector3 position)
    {
        GameObject fireInstance = Instantiate(firePrefab, position, Quaternion.identity);
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
            Debug.Log("Fire couldn't be destroyed");
        }
    }
}

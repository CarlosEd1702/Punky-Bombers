using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerLife : NetworkBehaviour
{
    [SerializeField] private GameObject ShieldPrefab; // Prefab del escudo
    private GameObject shieldInstance; // Instancia del escudo
    [SerializeField] private CollectItems collectItems;
    private Animator animator; // Referencia al componente Animator
    [SerializeField] private PlayerMovement _playerMovement;

    private void Start()
    {
        // Asegúrate de que el collectItems esté correctamente asignado
        if (collectItems == null)
        {
            collectItems = GetComponent<CollectItems>();
            if (collectItems == null)
            {
                Debug.LogError("CollectItems script not found on the player object.");
            }
        }

        // Obtener referencia al componente Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the player object.");
        }

        // Asegúrate de que _playerMovement esté correctamente asignado
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
            if (_playerMovement == null)
            {
                Debug.LogError("PlayerMovement script not found on the player object.");
            }
        }
    }

    // Método para manejar cuando el jugador es alcanzado
    public void HandlePlayerHit()
    {
        if (IsOwner)
        {
            // Desactivar los inputs
            _playerMovement.DisablePlayerMovement();

            animator.SetBool("wasHit", true); // Activar la animación de muerte
            StartCoroutine(DeactivatePlayerAfterDeath());
        }
    }

    private IEnumerator DeactivatePlayerAfterDeath()
    {
        // Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 1f);

        // Desactivar el GameObject del jugador
        gameObject.SetActive(false);
    }
}

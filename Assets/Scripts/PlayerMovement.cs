using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public Vector3 movementDirection;
    public CharacterController controller;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Animator animator;
    private Vector2 input;
    private bool isJoystic = false;
    private bool isKeyboard = false;
    private bool isGamepad = false;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject GO_JS;
    [SerializeField] private VariableJoystick joystick;
    public float playerSpeed;
    [SerializeField] private float rotationSpeed;
    
    [Header("Ship")]
    [SerializeField] private GameObject shipPrefab;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        GO_JS = GameObject.FindGameObjectWithTag("Joystic");
        joystick = GO_JS.GetComponent<VariableJoystick>();
        shipPrefab.SetActive(false);

        if (IsOwner)
        {
            StartCoroutine(ReparentCameraCoroutine());

            if (controller == null) Debug.Log("Controller is null");
            else if (rb == null) Debug.Log("RB is null");
            else if (playerInput == null) Debug.Log("Player Input is null");
            else if (animator == null) Debug.Log("Animator is null");
            else
            {
                Debug.Log("Everything was found");
                EnableJoysticInput();
                EnableGamePadInput();
                EnableKeyboardInput();
            }
        }
    }

    private IEnumerator ReparentCameraCoroutine()
    {
        // Espera hasta que el NetworkObject esté completamente instanciado
        yield return new WaitUntil(() => NetworkObject.IsSpawned);

        Transform cameraContainer = transform.Find("CameraContainer");
        if (cameraContainer != null)
        {
            CameraManager.Instance.ReparentCamera(cameraContainer);
        }
        else
        {
            Debug.LogError("CameraContainer not found");
        }
    }

    public void EnableJoysticInput() { isJoystic = true; }
    public void EnableGamePadInput() { isGamepad = true; }
    public void EnableKeyboardInput() { isKeyboard = true; }

    void Update()
    {
        if (animator.GetBool("wasHit"))
        {
            DisablePlayerMovement();
            return; // Salir de Update si el jugador ha sido golpeado
        }

        // Lee la entrada del joystick
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        // Calcula la dirección de movimiento en base a la entrada del joystick
        Vector3 movementDirection = Vector3.zero;

        // Si estás utilizando un joystick, usa su entrada para la dirección de movimiento
        if (isJoystic)
        {
            movementDirection = new Vector3(joystick.Direction.x, 0.0f, joystick.Direction.y);
        }
        else if (isGamepad || isKeyboard)
        {
            movementDirection = new Vector3(input.x, 0.0f, input.y);
        }
        else
        {
            // Si no estás utilizando un joystick, usa las teclas de flecha para moverte
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        }

        // Normaliza la dirección del movimiento para que la velocidad sea constante en todas las direcciones
        if (movementDirection.sqrMagnitude > 0.1f)
        {
            movementDirection = CardinalDirection(movementDirection);
        }

        // Mueve el personaje en la dirección calculada
        controller.SimpleMove(movementDirection * playerSpeed);

        // Verifica si el jugador está moviéndose
        if (movementDirection.sqrMagnitude <= 0.1f)
        {
            // Si el jugador no se está moviendo, desactiva el efecto de fuego
            animator.SetBool("Running", false);
        }
        else
        {
            // Si el jugador se está moviendo, activa el efecto de fuego y actualiza la animación
            animator.SetBool("Running", true);

            // Calcula la rotación del jugador hacia la dirección de movimiento
            var targetDirection = Vector3.RotateTowards(controller.transform.forward, movementDirection, rotationSpeed * Time.deltaTime, 0.0f);
            controller.transform.rotation = Quaternion.LookRotation(targetDirection);
        }
    }

    private Vector3 CardinalDirection(Vector3 direction)
    {
        Vector3 cardinalDirection = Vector3.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            cardinalDirection = (direction.x > 0) ? Vector3.right : Vector3.left;
        }
        else
        {
            cardinalDirection = (direction.z > 0) ? Vector3.forward : Vector3.back;
        }

        return cardinalDirection;
    }

    public void DisablePlayerMovement()
    {
        playerInput.enabled = false; // Desactivar inputs de movimiento
        rb.isKinematic = true; // Hacer el Rigidbody kinemático
        animator.SetBool("Running", false); // Detener la animación de correr
    }
}

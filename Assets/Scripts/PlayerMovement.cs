using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public Vector3 movementDirection;
    public CharacterController controller;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Animator animator;
    private Vector2 input;
    private bool isJoystic = false;
    private bool isKeyboard = false;
    private bool isGamepad = false;
/*    private Canvas inputCanvas;*/
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject GO_JS;
    [SerializeField] private VariableJoystick joystick;
    public float playerSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private MeshTrail2 M_Dash;
    /* private bool groundedPlayer;*/
    /*    private float jumpHeight = 1.0f;
        private float gravityValue = -9.81f;*/

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        /*UI_P1 = GameObject.FindGameObjectWithTag("UI Player 1");*/
        GO_JS = GameObject.FindGameObjectWithTag("Joystic");
        joystick = GO_JS.GetComponent<VariableJoystick>();

        if (IsOwner)
        {
            if (controller == null)
            {
                Debug.Log("Controller is null");
            }
            else if (rb == null)
            {
                Debug.Log("RB is null");
            }
            else if (playerInput == null)
            {
                Debug.Log("Player Input is null");
            }
            else if (animator == null)
            {
                Debug.Log("Animator is null");
            }
            else
            {
                Debug.Log("Everything was found");
                EnableJoysticInput();
                EnableGamePadInput();
                EnableKeyboardInput();
            }
        }
    }

    public void EnableJoysticInput()
    {
        isJoystic = true;
    }
    public void EnableGamePadInput()
    {
        isGamepad = true;
    }

    public void EnableKeyboardInput()
    {
        isKeyboard = true;
    }

    void Update()
    {
        // Lee la entrada del joystick
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        // Calcula la dirección de movimiento en base a la entrada del joystick
        Vector3 movementDirection;

        // Si estás utilizando un joystick, usa su entrada para la dirección de movimiento
        if (isJoystic)
        {
            movementDirection = new Vector3(joystick.Direction.x, 0.0f, joystick.Direction.y);

        }
        else if (isGamepad || isKeyboard)
        {
            movementDirection = new Vector3(input.x, 0.0f, input.y);

        }
        else {
            // Si no estás utilizando un joystick, usa las teclas de flecha para moverte
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        }

        // Normaliza la dirección del movimiento para que la velocidad sea constante en todas las direcciones
        movementDirection.Normalize();

        // Mueve el personaje en la dirección calculada
        controller.SimpleMove(movementDirection * playerSpeed);

        // Verifica si el jugador está moviéndose
        if (movementDirection.sqrMagnitude <= 0)
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

}

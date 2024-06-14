using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Clock : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<float> timer = new NetworkVariable<float>(180, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    [SerializeField] private TextMeshProUGUI TimerText;
    
    [SerializeField] private GameObject TimeOver;
    [SerializeField] private GameObject clock;
    [SerializeField] private GameObject SpawnerSpikesController;

    private void Start()
    {
        if (IsServer)
        {
            timer.Value = 180; // Inicializa el temporizador en el servidor
        }
        
        TimeOver.SetActive(false);
        clock.SetActive(true);
        
        timer.OnValueChanged += OnTimerChanged; // Suscribirse al evento de cambio de valor
    }

    void Update()
    {
        if (IsServer)
        {
            timer.Value -= Time.deltaTime;

            if (timer.Value <= 60 && !SpawnerSpikesController.activeSelf)
            {
                SpawnerSpikesController.SetActive(true);
            }
            if (timer.Value <= 0)
            {
                timer.Value = 0; // Asegurarse de que el temporizador no sea negativo
                TimeOver.SetActive(true);
                Time.timeScale = 0;
                clock.SetActive(false);
            }
        }

        Time.timeScale = 1;
    }

    private void OnTimerChanged(float oldValue, float newValue)
    {
        TimerText.text = newValue.ToString("f0");

        if (newValue <= 0)
        {
            TimeOver.SetActive(true);
            clock.SetActive(false);
        }
    }

    // Llamamos al OnDestroy de la clase base sin modificarlo
    private void OnDestroy()
    {
        timer.OnValueChanged -= OnTimerChanged; // Desuscribirse del evento al destruir el objeto
        base.OnDestroy();
    }
}
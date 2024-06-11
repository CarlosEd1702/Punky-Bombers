using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerSelection : NetworkBehaviour
{
    private NetworkVariable<int> selectedCharacterIndex = new NetworkVariable<int>(-1);
    [SerializeField] private GameObject[] characters;
    [SerializeField] private GameObject btn_next;
    [SerializeField] private GameObject btn_prev;
    [SerializeField] private GameObject btn_play;
    private Button next;
    private Button prev;
    private Button play;

    private void Start()
    {
        if (IsOwner)
        {
            next = btn_next.GetComponent<Button>();
            prev = btn_prev.GetComponent<Button>();
            play = btn_play.GetComponent<Button>();
            
            // Suscribir los botones a los métodos de selección y inicio de juego
            next.clicked += OnNextClicked;
            prev.clicked += OnPrevClicked;
            play.clicked += OnPlayClicked;

            // Solo activar el primer personaje al inicio
            SelectCharacter(0);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Aquí podrías abrir la UI de selección de personaje o realizar otras configuraciones
            Debug.Log("Player has entered. Open character selection UI.");
        }
        // Asegurarse de que el personaje seleccionado esté visible para cada jugador
        selectedCharacterIndex.OnValueChanged += OnCharacterIndexChanged;
    }

    private void OnCharacterIndexChanged(int oldIndex, int newIndex)
    {
        // Actualizar la visibilidad de los personajes en base al índice seleccionado
        foreach (var character in characters)
        {
            character.SetActive(false);
        }

        if (newIndex >= 0 && newIndex < characters.Length)
        {
            characters[newIndex].SetActive(true);
        }
    }

    private void OnNextClicked()
    {
        if (IsOwner)
        {
            int newIndex = (selectedCharacterIndex.Value + 1) % characters.Length;
            SelectCharacter(newIndex);
        }
    }

    private void OnPrevClicked()
    {
        if (IsOwner)
        {
            int newIndex = selectedCharacterIndex.Value - 1;
            if (newIndex < 0)
            {
                newIndex += characters.Length;
            }
            SelectCharacter(newIndex);
        }
    }

    private void OnPlayClicked()
    {
        if (IsOwner)
        {
            StartGame();
        }
    }

    public void SelectCharacter(int index)
    {
        if (IsOwner)
        {
            SelectCharacterServerRpc(index);
        }
    }

    [ServerRpc]
    private void SelectCharacterServerRpc(int index)
    {
        // Guarda la selección del personaje del jugador en el servidor
        selectedCharacterIndex.Value = index;
        Debug.Log($"Character selected: {index}");
    }

    private void StartGame()
    {
        PlayerPrefs.SetInt("selectedCharacter", selectedCharacterIndex.Value);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}

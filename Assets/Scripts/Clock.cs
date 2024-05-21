using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Clock : MonoBehaviour
{
    public float TimeSpawnSpikes = 2;
    public float timer = 180;


    public TextMeshProUGUI TimerText;

    public GameObject TimeOver;
    public GameObject clock;
    public GameObject SpawnerSpikesController;

    //public GameObject SpikesPrefab;

    private void Start()
    {
        TimeOver.SetActive(false);
        clock.SetActive(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        TimerText.text = "" + timer.ToString("f0");

        if (timer <= 60)
        {
            SpawnerSpikesController.SetActive(true);
        }
        if(timer <= 0)
        {
            TimeOver.SetActive(true);
            Time.timeScale = 0;
            clock.SetActive(false);
        }
        Time.timeScale = 1;
    }

   

    /*private void invoke()
    {
        throw new NotImplementedException();
    }*/
}

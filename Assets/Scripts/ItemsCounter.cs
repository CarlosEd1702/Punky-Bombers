using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemsCounter : MonoBehaviour
{
    public static ItemsCounter instancie;

    public TMP_Text BoomsText;
    public TMP_Text FlameText;
   

   

    public int CurrentBooms = 1;
    public int CurrentFlame = 1;

    void Awake()
    {
        instancie = this;   
    }

    private void Start()
    {
        BoomsText.text = "Booms: " + CurrentBooms.ToString();
        FlameText.text = "Flame: " + CurrentFlame.ToString();

    }

    public void IncreaseBooms(int i)
    {
        CurrentBooms += 1;
        BoomsText.text = "Booms: " + CurrentBooms.ToString();
    }
    public void IncreaseFlame(int j)
    {
        CurrentFlame += 1;
        FlameText.text = "Flame: " + CurrentFlame.ToString();
    }

}

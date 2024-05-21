using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectItems : MonoBehaviour
{

    public GameObject TeleportController;
    public GameObject KeyImage;

    public int Boomb;
    public int Flame;

// Start is called before the first frame update
    void Start()
    {
        TeleportController.SetActive(false);
        KeyImage.SetActive(false);       
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Boomb")
        {
            Debug.Log("Boomb +1");
            Boomb = Boomb + 1;
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
            ItemsCounter.instancie.IncreaseBooms(Boomb);
        }

        if (other.gameObject.tag == "Flame")
        {
            Debug.Log("Flame +1");
            Flame = Flame + 1;
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
            ItemsCounter.instancie.IncreaseFlame(Flame);
        }

        if(other.gameObject.tag == "Key")
        {
            Debug.Log("Portales activados");
            TeleportController.SetActive(true);
            KeyImage.SetActive(true);
            
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}

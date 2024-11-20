using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominantHand : MonoBehaviour
{

    // If true, disable left hand
    public bool righty;

    // Start is called before the first frame update
    void Start()
    {
        if (righty) {
            gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.active = false;
        }
        else {
            gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.active = false;
        }
    }

    public void SwitchHands()
    {
        if (gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.active) {
            gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.active = false;
            gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.active = true;
        }
        else {
            gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.active = true;
            gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.active = false;
        }
    }
}

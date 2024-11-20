using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetDangerZone : MonoBehaviour
{

    private BoxCollider box;

    private AudioSource siren;

    // Start is called before the first frame update
    void Start()
    {
        box = GetComponent<BoxCollider>();
        siren = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Main Camera") {
            siren.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Main Camera") {
            siren.Stop();
        }
    }
}

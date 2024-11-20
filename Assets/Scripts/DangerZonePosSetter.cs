using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZonePosSetter : MonoBehaviour
{

    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPos()
    {
        gameObject.transform.position = cam.transform.position;
    }
}

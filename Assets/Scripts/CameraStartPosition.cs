using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStartPosition : MonoBehaviour
{

    public GameObject wall;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(wall.transform.position.x, transform.position.y, wall.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallOffset : MonoBehaviour
{

    private Vector3 wallDefaultPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveWall(float offset)
    {
        //transform.position = wallDefaultPos;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset);
    }
}

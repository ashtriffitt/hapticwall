using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;


public class TrialManager : MonoBehaviour
{
    public Array2DFloat offsetValues;

    public WallOffset wallOffset;

    public GameObject hands;

    public float trialTime;
    

    // Start is called before the first frame update
    IEnumerator Start()
    {
        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();


        for (int i = 0; i < offsetValues.GridSize.y; i++) {
            wallOffset.MoveWall(offsetValues.GetCell(0, i));
            // handOffset(offsetValues.getCell(1, i);
            yield return new WaitForSeconds(trialTime);
            Debug.Log("Next");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

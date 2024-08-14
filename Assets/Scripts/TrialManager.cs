using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;


public class TrialManager : MonoBehaviour
{
    public Array2DFloat offsetValues;

    public WallOffset wallOffset;

    public float trialTime;
    public float waitTime;

    public LeapXRServiceProvider leap;

    private Camera mainCamera;
    private Camera blackScreen;

    private int trialIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        blackScreen = GameObject.Find("Black Screen Camera").GetComponent<Camera>();


        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        StartCoroutine("ChangeTrial");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ChangeTrial()
    {

        wallOffset.MoveWall(offsetValues.GetCell(0, trialIndex));
        leap.deviceOffsetZAxis = offsetValues.GetCell(1, trialIndex);
        yield return new WaitForSeconds(trialTime);
        StartCoroutine("TrialChangeScreen");

        Debug.Log("Screen change");
    }

    IEnumerator TrialChangeScreen()
    {
        mainCamera.enabled = false;
        blackScreen.enabled = true;

        yield return new WaitForSeconds(waitTime);

        blackScreen.enabled = false;
        mainCamera.enabled = true;

        trialIndex++;
        StartCoroutine("ChangeTrial");
        Debug.Log("Next");
    }
}

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

    private Canvas questionnaire;

    private bool hasAnswered;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        blackScreen = GameObject.Find("Black Screen Camera").GetComponent<Camera>();

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();


        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        StartCoroutine("ChangeTrial");
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    IEnumerator ChangeTrial()
    {
        for (int i = 0; i < offsetValues.GridSize.y; i++) {

            yield return new WaitForSeconds(trialTime);

            // Camera off
            mainCamera.enabled = false;
            blackScreen.enabled = true;

            // Resets to original positions
            //wallOffset.MoveWall(wallOffset.wallDefaultPos.z);
            leap.deviceOffsetZAxis = 0;

            // Moves to new positions
            wallOffset.MoveWall(offsetValues.GetCell(0, trialIndex));
            leap.deviceOffsetZAxis = offsetValues.GetCell(1, trialIndex);

            // Enable questionnaire every two trials
            if (i % 2 != 0) {
                questionnaire.enabled = true;

                // WAIT FOR ANSWER - change this 
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));

                questionnaire.enabled = false;
                hasAnswered = false;
            }
            else {
                yield return new WaitForSeconds(waitTime);
            }

            // Camera on
            blackScreen.enabled = false;
            mainCamera.enabled = true;


            // Index increase, 
            Debug.Log("Next");
        }
    }

    private void ClickAnswer()
    {
        hasAnswered = true;
    }
}

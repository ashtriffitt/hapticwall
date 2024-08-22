using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;


public class TrialManager : MonoBehaviour
{
    public Array2DFloat offsetValues;

    private WallOffset wallOffset;

    public float trialTime;
    public float waitTime;

    [SerializeField]
    private LeapXRServiceProvider leap;

    private Camera mainCamera;
    private Camera blackScreen;

    //ivate int trialIndex = 0;

    private Canvas questionnaire;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        blackScreen = GameObject.Find("Black Screen Camera").GetComponent<Camera>();

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();


        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        // Starts trial loop
        //StartCoroutine("ChangeTrial");
        //wallOffset.MoveRealWall(1);
        StartCoroutine("ChangeTrial");
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    IEnumerator ChangeTrial()
    {
        yield return new WaitForSeconds(.5f);

        // Grab and save starting tracker position
        wallOffset.SetDefaultPos();


        // initial pos (wall + hands)
        // initialize real wall pos
        StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2,0)));
        wallOffset.MatchWalls();

        // Wall
        // wallOffset.MatchWalls();
        wallOffset.MoveVirtualWall(offsetValues.GetCell(0, 0));
        Debug.Log("Wall offset " + offsetValues.GetCell(0, 0));
        // Hands
        //leap.deviceOffsetZAxis = offsetValues.GetCell(1, 0) + .08f;
        Debug.Log("Hand offset" + offsetValues.GetCell(1, 0));

        
        for (int i = 1; i < offsetValues.GridSize.y; i++) {

            Debug.Log("i = " + i);
            yield return new WaitForSeconds(trialTime);

            // 'Start' of next trial
            // Enabling of Black/questionnaire screen
            mainCamera.enabled = false;
            blackScreen.enabled = true;

            // Move real wall to next spot but wait for wall to finish moving until continuing
            StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i)));

            // Resets Hands to original positions
            leap.deviceOffsetZAxis = 0;

            // Moves virtual wall to new position
            Debug.Log("Wall offset " + offsetValues.GetCell(0, i));
            wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i));
            
            // Hands
            leap.deviceOffsetZAxis = offsetValues.GetCell(1, i);
            Debug.Log("Hand offset" + offsetValues.GetCell(1, i));

            // Enable questionnaire every two trials
            if (i % 2 == 0) {
                questionnaire.enabled = true;

                // Wait until subject answers to go to next trial
                yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)));

                questionnaire.enabled = false;
            }
            else {
                // Wait for next trial
                yield return new WaitForSeconds(waitTime);
            }

            // Back to trial screen
            blackScreen.enabled = false;
            mainCamera.enabled = true;

            Debug.Log("New trial"); 
        } 
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(.5f);

        // Grab and save starting tracker position
        wallOffset.SetDefaultPos();
        wallOffset.MatchWalls();

        StartCoroutine(wallOffset.OffsetRealWall(.025f));
        wallOffset.MatchWalls();

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;

using UXF;

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

    private Canvas questionnaire;
    private AnswerTracker answerTracker;

    //public Session session;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        blackScreen = GameObject.Find("Black Screen Camera").GetComponent<Camera>();

        mainCamera.enabled = false;
        blackScreen.enabled = true;

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();
        answerTracker = GameObject.Find("Questionnaire").GetComponent<AnswerTracker>();


        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public IEnumerator ChangeTrial(Session session)
    {
       
        yield return new WaitForSeconds(.5f);

        // Grab and save starting tracker position
        wallOffset.SetDefaultPos();


        // initial pos (wall + hands)
        // initialize real wall pos
        // This shouldnt happen for default pos
        //yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2,0)));
        wallOffset.MatchWalls();

        // Wall
        wallOffset.MatchWalls();
        
        wallOffset.MoveVirtualWall(offsetValues.GetCell(0, 0));
        Debug.Log("Wall offset " + offsetValues.GetCell(0, 0));
        // Hands
        leap.deviceOffsetZAxis = offsetValues.GetCell(1, 0) + .08f;
        Debug.Log("Hand offset" + offsetValues.GetCell(1, 0));

        
        for (int i = 1; i < offsetValues.GridSize.y; i++) {

            Debug.Log("i = " + i);
            yield return new WaitForSeconds(trialTime);

            session.BeginNextTrial();

            // 'Start' of next trial
            // Enabling of Black/questionnaire screen

            // Change color of "blackout" screen
            CameraColor(i);

            mainCamera.enabled = false;
            blackScreen.enabled = true;

            // Move real wall to next spot but wait for wall to finish moving until continuing
            // This was a Coroutine ^ it was adding to the time inbetween trials
            yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i)));

            // Resets Hands to original positions
            leap.deviceOffsetZAxis = 0.8f;

            // Moves virtual wall to new position
            Debug.Log("Wall offset " + offsetValues.GetCell(0, i));
            wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i));
            
            // Hands
            leap.deviceOffsetZAxis = offsetValues.GetCell(1, i) + .08f;
            Debug.Log("Hand offset" + offsetValues.GetCell(1, i));

            // Enable questionnaire every two trials
            if (i % 2 == 0) {
                questionnaire.enabled = true;

                // Wait until subject answers to go to next trial
                yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)));

                // Store answer
                session.CurrentTrial.settings.SetValue("Questionnaire Answers", answerTracker.lastAnswer);

                questionnaire.enabled = false;
            }
            else {
                // Wait for next trial
                yield return new WaitForSeconds(waitTime);
            }
            // Save Trial data
            ManualSaveData(session);

            // Back to trial screen
            blackScreen.enabled = false;
            mainCamera.enabled = true;

            // End UXF trial / save trial data
            Debug.Log("New trial"); 
        } 
    }

    private IEnumerator PracticeTrial()
    {
        // Grab and save starting tracker position
        wallOffset.SetDefaultPos();


        // initial pos (wall + hands)
        // initialize real wall pos
        StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, 0)));
        wallOffset.MatchWalls();

        // Wall
        // wallOffset.MatchWalls();
        wallOffset.MoveVirtualWall(offsetValues.GetCell(0, 0));
        Debug.Log("Wall offset " + offsetValues.GetCell(0, 0));
        // Hands
        //leap.deviceOffsetZAxis = offsetValues.GetCell(1, 0) + .08f;
        Debug.Log("Hand offset" + offsetValues.GetCell(1, 0));

        yield return new WaitForSeconds(30);
    }


    private void ManualSaveData(Session session)
    {
        // Save data from previous trial
        // Real wall position
        // Virtual wall offset
        // Hand offset
        Debug.Log("Save data");

        session.CurrentTrial.settings.SetValue("Hand Offset", offsetValues.GetCell(1, session.currentTrialNum));
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValues.GetCell(0, session.currentTrialNum));
        session.CurrentTrial.settings.SetValue("Real Wall Position", offsetValues.GetCell(2, session.currentTrialNum));

        // Questionnaire response

        // Start next trial.
    }

    private void CameraColor(int index)
    {
        if (index % 2 == 0) {
            blackScreen.backgroundColor = Color.green;
        }
        else {
            blackScreen.backgroundColor = Color.blue;
        }
    }
}

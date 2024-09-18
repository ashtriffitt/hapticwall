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
    private int practiceTime;

    [SerializeField]
    private LeapXRServiceProvider leap;

    private Camera mainCamera;
    private Camera blackScreen;

    private Canvas questionnaire;
    private AnswerTracker answerTracker;

    private bool inTrial = false;
    private bool skip;

    private int trialMask;
    public LayerMask betweenMask;

    private AudioSource changeSound;

    [SerializeField]
    private Material blue;
    [SerializeField]
    public Material green;

    private Renderer wall;


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

        trialMask = mainCamera.cullingMask;

        changeSound = GetComponent<AudioSource>();

        wall = GameObject.Find("Wall").GetComponent<Renderer>();
        wall.material = blue;
    }

    // Update is called once per frame
    void Update()
    {
       // Skipping trial function
       if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) {
            
            // if WaitFor is active, end it.
            if (inTrial) {
                skip = true;
                inTrial = false;
                Debug.Log("skip");
            }
            // skips to inbetween trial screen
        }
    }

    public IEnumerator ChangeTrial(Session session)
    {
       
        yield return new WaitForSeconds(.5f);

        // Grab and save starting tracker position
        wallOffset.SetDefaultPos();

        // Wall
        wallOffset.MatchWalls();

        yield return new WaitForSeconds(practiceTime);

        // END OF PRACTICE TRIAL

        // Initialize first trial
        ChangeToBlackScreenCamera();
        wallOffset.MoveVirtualWall(offsetValues.GetCell(0, 0) / 100);
        leap.deviceOffsetZAxis = offsetValues.GetCell(1, 0) / 100 + .08f;


        //StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, 0) / 100));

        yield return new WaitForSeconds(waitTime);

        changeSound.Play();
        session.BeginNextTrial();
        ChangeToTrialCamera();

        for (int i = 1; i < offsetValues.GridSize.y; i++) {

            Debug.Log("i = " + i);
            CameraColor(i);
            //yield return new WaitForSeconds(trialTime);
            yield return StartCoroutine("WaitFor", session);

            session.BeginNextTrial();

            // 'Start' of next trial
            // Enabling of Black/questionnaire screen
            // Change color of "blackout" screen
            

            //mainCamera.enabled = false;
            // blackScreen.enabled = true;
            ChangeToBlackScreenCamera();

            // Move real wall to next spot but wait for wall to finish moving until continuing
            // This was a Coroutine ^ it was adding to the time inbetween trials // Replace so its always a consistent time inbetween trials
            //StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));

            // Resets Hands to original positions // Not needed, but doesnt do anything bad
            leap.deviceOffsetZAxis = 0.8f;

            // match wall
            wallOffset.MatchWalls();

            // Moves virtual wall to new position
            Debug.Log("Wall offset " + offsetValues.GetCell(0, i) / 100);
            wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
            
            // Hands
            leap.deviceOffsetZAxis = offsetValues.GetCell(1, i) / 100 + .08f;
            Debug.Log("Hand offset" + offsetValues.GetCell(1, i) / 100);

            // Enable questionnaire every two trials
            if (i % 2 == 0) {
                questionnaire.enabled = true;

                // Wait until subject answers to go to next trial

                yield return StartCoroutine(answerTracker.AnswerDelay());
                yield return new WaitUntil(() => (answerTracker.canAnswer == false));

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
            ChangeToTrialCamera();
            // Play sound
            changeSound.Play();

            // End UXF trial / save trial data
            Debug.Log("New trial"); 
        } 
    }

    // Trial timer without using a WaitForSeconds(). If skip key is pressed, exits timer;
    private IEnumerator WaitFor(Session session)
    {
        inTrial = true;
        Debug.Log("waiting");
        for (float timer = trialTime; timer >= 0; timer -= Time.deltaTime) {
            if (skip) { 
                skip = false; 
                yield break;
                session.CurrentTrial.settings.SetValue("Trial Time", trialTime - timer);
            }
            session.CurrentTrial.settings.SetValue("Trial Time", trialTime);
            yield return null;
        }
        inTrial = false;
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
    
    // Switches to camera for inbetween trials
    private void ChangeToBlackScreenCamera()
    {
        mainCamera.cullingMask = betweenMask;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    // Switches to camera for during trial
    private void ChangeToTrialCamera()
    {
        mainCamera.cullingMask = trialMask;
        mainCamera.clearFlags = CameraClearFlags.Skybox;
    }

    // Changes the background color of the camera to identify trials
    private void CameraColor(int index)
    {
        if (index % 2 == 0) {
            wall.material = green;
        }
        else {
            wall.material = blue;
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

}

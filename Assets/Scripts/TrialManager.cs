using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;
using TMPro;

using UXF;

public class TrialManager : MonoBehaviour
{
    public Array2DFloat offsetValues;

    private WallOffset wallOffset;

    [SerializeField]
    private DominantHand dominantHand;

    public float trialTime;
    public float minTrialTime;
    public float waitTime;
    [SerializeField]
    private int practiceTime;

    [SerializeField]
    private LeapXRServiceProvider leap;

    private Camera mainCamera;
    private Camera blackScreen;

    private Canvas questionnaire;

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

    [SerializeField]
    private ViveController viveController;

    public TextMeshProUGUI practiceText;
    public TextMeshProUGUI breakText;
    public TextMeshProUGUI endText;
    public TextMeshProUGUI trialNumberText;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        blackScreen = GameObject.Find("Black Screen Camera").GetComponent<Camera>();

        mainCamera.enabled = false;
        blackScreen.enabled = true;

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();

        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        trialMask = mainCamera.cullingMask;

        changeSound = GetComponent<AudioSource>();

        wall = GameObject.Find("Wall").GetComponent<Renderer>();
        wall.material = blue;

        ChangeToBlackScreenCamera();
    }

    // Update is called once per frame
    void Update()
    {
       // Skipping trial function
       if (viveController.triggerPressed) {
            
            // if WaitFor is active, end it.
            if (inTrial) {
                skip = true;
                //inTrial = false;
                Debug.Log("skip");
            }
            // skips to inbetween trial screen
        }
    }

    public IEnumerator ChangeTrial(Session session)
    {

        // Initialize first trial

        wallOffset.MatchWalls();

        ChangeToTrialCamera();

        trialNumberText.enabled = true;

        if (offsetValues.GetCell(2, 0) != 0) {
            StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, 0) / 100));
        }
        wallOffset.MoveVirtualWall(offsetValues.GetCell(0, 0) / 100);
        leap.deviceOffsetZAxis = offsetValues.GetCell(1, 0) / 100 + .08f;

        

        yield return new WaitForSeconds(waitTime);

        changeSound.Play();

        session.BeginNextTrial();

        ManualSaveData(session, 0);

        ChangeToTrialCamera();
        

        for (int i = 1; i < offsetValues.GridSize.y; i++) {

           // Debug.Log("i = " + i);
            WallColor(i);
            //yield return new WaitForSeconds(trialTime);
            yield return StartCoroutine(WaitFor(session, trialTime));

            // 'Start' of next trial
            // Enabling of Black/questionnaire screen
            // Change color of "blackout" screen

            ChangeToBlackScreenCamera();

            // Move real wall to next spot but wait for wall to finish moving until continuing
            // This was a Coroutine ^ it was adding to the time inbetween trials // Replace so its always a consistent time inbetween trials
            //StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));

            // Resets Hands to original positions // Not needed, but doesnt do anything bad
            leap.deviceOffsetZAxis = 0.8f;

            // match wall
            wallOffset.MatchWalls();

            // Moves virtual wall to new position
            wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
            
            // Hands
            leap.deviceOffsetZAxis = offsetValues.GetCell(1, i) / 100 + .08f;

            // Move real wall
            if (offsetValues.GetCell(2, i) != offsetValues.GetCell(2, i-1)) {
                StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));
            }
            else {
                StartCoroutine(wallOffset.DummyMovement());
            }

            // Enable questionnaire every two trials
            if (i % 2 == 0) {
                questionnaire.enabled = true;

                // Wait until subject answers to go to next trial

                //yield return StartCoroutine(answerTracker.AnswerDelay());
                yield return new WaitUntil(() => (viveController.leftClick || viveController.rightClick));

                // Store answer
                session.CurrentTrial.settings.SetValue("Questionnaire Answers", viveController.lastAnswer);

                questionnaire.enabled = false;

                yield return new WaitUntil(() => (wallOffset.wallMoving == false));
            }
            else {
                // Wait for next trial
                yield return new WaitForSeconds(waitTime);
            }
            // Save Trial data

            // BREAKS
            if (i % 60 == 0) {
                breakText.enabled = true;
                Debug.Log("Break time");

                dominantHand.SwitchHands();

                yield return StartCoroutine(WaitFor(session, 60));
                breakText.enabled = false;
            }
            

            // Back to trial screen
            ChangeToTrialCamera();
            // Play sound
            changeSound.Play();

            session.BeginNextTrial();

            ManualSaveData(session, i);

            // End UXF trial / save trial data
            Debug.Log("New trial");
            Debug.Log("Trial #" + i + " Wall Offset = " + offsetValues.GetCell(0, i) + " Hand Offset = " + offsetValues.GetCell(1, i) + " Wall Distance = " + offsetValues.GetCell(2, i));
        }

        ChangeToBlackScreenCamera();
        endText.enabled = true;

    }

    public IEnumerator Practice(Session session)
    {
        // See if there are any practice trials
        if (session.CurrentBlock.trials.Count > 0) {
            practiceText.enabled = true;

            session.BeginNextTrial();
            yield return new WaitForSeconds(.3f);

            wallOffset.SetDefaultPos();
            wallOffset.MatchWalls();

            ChangeToTrialCamera();

            yield return WaitFor(session, practiceTime);

            if (session.currentTrialNum < session.CurrentBlock.trials.Count) {
                ChangeToBlackScreenCamera();

                // Move wall and hands


                StartCoroutine(Practice(session));
            }
            else {

                ChangeToBlackScreenCamera();
                StartCoroutine(ChangeTrial(session));
            }

            practiceText.enabled = false;
        }
        else {
            StartCoroutine(ChangeTrial(session));
        }
    }

    // Trial timer without using a WaitForSeconds(). If skip key is pressed, exits timer;
    private IEnumerator WaitFor(Session session, float waitTime)
    {
        inTrial = true;
        Debug.Log("waiting");
        for (float timer = waitTime; timer >= 0; timer -= Time.deltaTime) {
            if (skip && (timer - Time.deltaTime < waitTime - minTrialTime)) {
                    Debug.Log(timer - Time.deltaTime);
                    inTrial = false;
                    skip = false;
                    session.CurrentTrial.settings.SetValue("Trial Time", trialTime - timer);
                    yield break;
            }
            else {
                skip = false;
            }
            session.CurrentTrial.settings.SetValue("Trial Time", trialTime);
            yield return null;
        }
        inTrial = false;
    }

    private void ManualSaveData(Session session, int index)
    {
        // Save data from previous trial
        // Real wall position
        // Virtual wall offset
        // Hand offset
        Debug.Log("Save data");

        session.CurrentTrial.settings.SetValue("Hand Offset", offsetValues.GetCell(1, index));
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValues.GetCell(0, index));
        session.CurrentTrial.settings.SetValue("Wall Distance", offsetValues.GetCell(2, index));

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

    // Changes the wall color to identify trials
    private void WallColor(int index)
    {
        if (index % 2 == 0) {
            wall.material = green;
            trialNumberText.text = "2";
        }
        else {
            
            wall.material = blue;
            trialNumberText.text = "1";
        }
    }
}

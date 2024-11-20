using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;
using TMPro;

using UXF;

public class TrialManagerMatching : MonoBehaviour
{
    public int currentTrialNum;

    public Array2DFloat offsetValues;

    private WallOffset wallOffset;

    [SerializeField]
    private float leapDeviceZOffset;

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

    [SerializeField]
    private bool canAdjust;
    [SerializeField]
    private bool canAdjustWall;
    [SerializeField]
    private bool canAdjustHands;

    [SerializeField]
    private float adjustAmt;

    private float wallHasBeenAdjusted;

    [SerializeField]
    private Vector3[] practiceTrials;

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

       if (viveController.downClick) {
            AdjustOffset(-adjustAmt);
        }
       else if (viveController.upClick) {
            AdjustOffset(adjustAmt);
        }
    }

    public IEnumerator ChangeTrial(Session session)
    {
        trialNumberText.enabled = true;

        canAdjust = false;

        for (int i = 0; i < offsetValues.GridSize.y; i++) {

            Debug.Log("Trial #" + i + " Wall Offset = " + offsetValues.GetCell(0, i) + " Hand Offset = " + offsetValues.GetCell(1, i) + " Wall Distance = " + offsetValues.GetCell(2, i));

            // Observation envrionment
            if ((i + 1) % 2 != 0) {
                Debug.Log("Observation");

                // Wait screen camera
                ChangeToBlackScreenCamera();

                // Set wall
                wallOffset.MatchWalls();
                //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
                // Set hands
                leap.deviceOffsetZAxis = (offsetValues.GetCell(1, i) / 100 + leapDeviceZOffset);
                // Set wall color and trial number text
                WallColor(i + 1);

                Debug.Log("Wall offset: " + offsetValues.GetCell(0, i) / 100);
                Debug.Log("Hand offset: " + (offsetValues.GetCell(1, i) / 100 + leapDeviceZOffset));

                //Debug.Log("Actual hand offset: " + leap.deviceOffsetZAxis);

                // Move real wall
                if (i != 0) {
                    if (offsetValues.GetCell(2, i) != offsetValues.GetCell(2, i - 1)) {
                        yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));
                    }
                    else {
                        yield return StartCoroutine(wallOffset.DummyMovement());
                    }
                }
                else {
                    yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));
                }

                wallOffset.MatchWalls();
                wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);

                // Wait time | Subbed for wall movement time instead
                //yield return new WaitForSeconds(waitTime);

                // Trial camera
                ChangeToTrialCamera();
                changeSound.Play();
                session.BeginNextTrial();

                // Let subject explore
                yield return StartCoroutine(WaitFor(session, trialTime));

                ManualSaveData(session, i);
                session.EndCurrentTrial();
            }
            // Enable adjustment every other trial
            else if ((i + 1) % 2 == 0) {
                Debug.Log("Adjustment");

                // Wait screen camera
                ChangeToBlackScreenCamera();

                // Set wall
                wallOffset.MatchWalls();
                //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
                // Set hands
                leap.deviceOffsetZAxis = (offsetValues.GetCell(1, i) / 100 + leapDeviceZOffset);
                // Set wall color and trial number text
                WallColor(i + 1);

                Debug.Log("Wall offset: " + offsetValues.GetCell(0, i) / 100);
                Debug.Log("Hand offset: " + (offsetValues.GetCell(1, i) / 100 + leapDeviceZOffset));

                Debug.Log("Actual hand offset: " + leap.deviceOffsetZAxis);

                // Move real wall
                if (offsetValues.GetCell(2, i) != offsetValues.GetCell(2, i - 1)) {
                    yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValues.GetCell(2, i) / 100));
                }
                else {
                    yield return StartCoroutine(wallOffset.DummyMovement());
                }

                wallOffset.MatchWalls();
                wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);

                canAdjust = true;

                CanAdjustWhat(new Vector3(offsetValues.GetCell(0, i - 1), offsetValues.GetCell(1, i - 1), offsetValues.GetCell(2, i - 1)));

                // Let subject adjust
                ChangeToTrialCamera();
                session.BeginNextTrial();
                yield return StartCoroutine(WaitFor(session, 100));

                // End adjustment
                canAdjust = false;
                ManualSaveAdjustmentData(session, i);
                session.EndCurrentTrial();
            }

            // BREAKS
            if (i % 32 == 0 && i != 0) {
                breakText.enabled = true;
                Debug.Log("Break time");

                dominantHand.SwitchHands();

                yield return StartCoroutine(WaitFor(session, 60));
                breakText.enabled = false;
            }

            // End UXF trial / save trial data
            Debug.Log("New trial");
           
        }

        ChangeToBlackScreenCamera();
        endText.enabled = true;

    }
    public IEnumerator Practice(Session session)
    {
        Debug.Log("Practice");
        session.BeginNextTrial();
        Debug.Log(session.CurrentBlock.trials.Count);

        currentTrialNum = session.currentTrialNum;

        // See if there are any practice trials
        if (session.CurrentBlock.trials.Count > 0) {

            // Non adjustment
            if (session.currentTrialNum % 2 != 0) {
                Debug.Log("env1");

                practiceText.enabled = true;


                yield return new WaitForSeconds(.3f);

                wallOffset.SetDefaultPos();
                wallOffset.MatchWalls();

                wallOffset.MoveVirtualWall((float)(practiceTrials[session.currentTrialNum - 1].x / 100));

                leap.deviceOffsetZAxis = (practiceTrials[session.currentTrialNum - 1].y / 100 + leapDeviceZOffset);

                yield return new WaitForSeconds(1);

                ChangeToTrialCamera();

                yield return WaitFor(session, practiceTime);
            }

            //Enviroment 1 ends
            //session.BeginNextTrial();

            // Adjustment
            if (session.currentTrialNum % 2 == 0) {
                Debug.Log("env2");

                ChangeToBlackScreenCamera();

                wallOffset.MatchWalls();

                wallOffset.MoveVirtualWall((float)(practiceTrials[session.currentTrialNum - 1].x / 100));

                leap.deviceOffsetZAxis = (practiceTrials[session.currentTrialNum - 1].y / 100 + leapDeviceZOffset);

                yield return new WaitForSeconds(1);

                canAdjust = true;

                CanAdjustWhat(practiceTrials[session.currentTrialNum - 2]);

                ChangeToTrialCamera();

                yield return WaitFor(session, practiceTime);

                
            }

            // Ontrigger press save data

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
            practiceText.enabled = true;
            Debug.Log("No Practice");
            ChangeToTrialCamera();
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

    private void ManualSaveAdjustmentData(Session session, int index)
    {
        // Save data from previous trial
        // Real wall position
        // Virtual wall offset
        // Hand offset
        Debug.Log("Save data");

        session.CurrentTrial.settings.SetValue("Adjustment Hand Offset", leap.deviceOffsetZAxis - leapDeviceZOffset);

        // Sort this
        session.CurrentTrial.settings.SetValue("Adjustment Wall Offset", wallHasBeenAdjusted * 100);

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

    private void CanAdjustWhat(Vector3 prior)
    {
        wallHasBeenAdjusted = 0;


        if (prior.x != 0) {
            canAdjustHands = true;
        }
        else {
            canAdjustHands = false;
        }

        if (prior.y != 0) {
            canAdjustWall = true;
        }
        else {
            canAdjustWall = false;
        }
    }

    // Adjusting both doesnt work right
    public void AdjustOffset(float amt)
    {
        Debug.Log("adjust");

        if (canAdjust) {
            if (canAdjustWall) {
                Debug.Log("Adjust wall");
                wallOffset.MoveVirtualWall(-amt);

                wallHasBeenAdjusted += amt;
            }

            if (canAdjustHands) {
                Debug.Log("adjust hands");
                leap.deviceOffsetZAxis += amt;
            }
        }
    }
}

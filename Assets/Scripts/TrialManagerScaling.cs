using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using Leap;
using TMPro;
using UnityEngine.UI;
using UXF;

public class TrialManagerScaling : MonoBehaviour
{
    public int currentTrialNum;

    public Array2DFloat offsetValuesScaling;
    public Array2DFloat offsetValuesMatching;

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

    private Canvas questionnaire;

    private bool inTrial = false;
    private bool skip;

    [SerializeField]
    private bool canAdjust;
    [SerializeField]
    private bool canAdjustWall;
    [SerializeField]
    private bool canAdjustHands;

    public bool canMoveSlider;

    [SerializeField]
    private float adjustAmt;

    private float wallHasBeenAdjusted;

    [SerializeField]
    private Vector3[] practiceTrialsScaling;

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

    [SerializeField]
    private Slider slider;

    public ExperimentGeneratorScaling gen;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();

        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        trialMask = mainCamera.cullingMask;

        changeSound = GetComponent<AudioSource>();

        wall = GameObject.Find("Wall").GetComponent<Renderer>();
       // wall.material = blue;

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

    // Scaling Block
    public IEnumerator ScalingBlock(Session session)
    {
        practiceText.enabled = false;
        // If there are practice trials you dont need this
        wallOffset.SetDefaultPos();
        //trialNumberText.enabled = true;

        for (int i = 0; i < offsetValuesScaling.GridSize.y; i++) {

            Debug.Log("Trial #" + i + " Wall Offset = " + offsetValuesScaling.GetCell(0, i) + " Hand Offset = " + offsetValuesScaling.GetCell(1, i) + " Wall Distance = " + offsetValuesScaling.GetCell(2, i));

            // Observation envrionment

                // Wait screen camera
                ChangeToBlackScreenCamera();

                // Set wall
                wallOffset.MatchWalls();
                //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
                // Set hands
                leap.deviceOffsetZAxis = (offsetValuesScaling.GetCell(1, i) / 100 + leapDeviceZOffset);
                // Set wall color and trial number text
                // WallColor(i + 1);

                Debug.Log("Wall offset: " + offsetValuesScaling.GetCell(0, i) / 100);
                Debug.Log("Hand offset: " + (offsetValuesScaling.GetCell(1, i) / 100 + leapDeviceZOffset));

                //Debug.Log("Actual hand offset: " + leap.deviceOffsetZAxis);

                // Move real wall
                if (i != 0) {
                    if (offsetValuesScaling.GetCell(2, i) != offsetValuesScaling.GetCell(2, i - 1)) {
                        yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValuesScaling.GetCell(2, i) / 100));
                    }
                    else {
                        yield return StartCoroutine(wallOffset.DummyMovement());
                    }
                }
                else {
                    yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValuesScaling.GetCell(2, i) / 100));
                }

                wallOffset.MatchWalls();
                wallOffset.MoveVirtualWall(offsetValuesScaling.GetCell(0, i) / 100);

                // Wait time | Subbed for wall movement time instead
                //yield return new WaitForSeconds(waitTime);

                // Trial camera
                ChangeToTrialCamera();

                canMoveSlider = true; 

                changeSound.Play();
                session.BeginNextTrial();

                // Let subject explore
                yield return StartCoroutine(WaitFor(session, trialTime));

                ManualSaveData(session, i);
                session.EndCurrentTrial();

                canMoveSlider = false;

            // Enable adjustment every other trial
       
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
        // Go to adjustment block
        //endText.enabled = true;

    }

    public IEnumerator AdjustmentBlock(Session session)
    {
        for (int i = 0; i < offsetValuesMatching.GridSize.y; i++) {

            Debug.Log("Trial #" + i + " Wall Offset = " + offsetValuesMatching.GetCell(0, i) + " Hand Offset = " + offsetValuesMatching.GetCell(1, i) + " Wall Distance = " + offsetValuesMatching.GetCell(2, i));

            // Observation envrionment

            // Wait screen camera
            ChangeToBlackScreenCamera();

            // Set wall
            wallOffset.MatchWalls();
            //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
            // Set hands
            leap.deviceOffsetZAxis = (offsetValuesMatching.GetCell(1, i) / 100 + leapDeviceZOffset);
            // Set wall color and trial number text
            // WallColor(i + 1);

            Debug.Log("Wall offset: " + offsetValuesMatching.GetCell(0, i) / 100);
            Debug.Log("Hand offset: " + (offsetValuesMatching.GetCell(1, i) / 100 + leapDeviceZOffset));

            //Debug.Log("Actual hand offset: " + leap.deviceOffsetZAxis);

            // Move real wall
            if (i != 0) {
                if (offsetValuesMatching.GetCell(2, i) != offsetValuesMatching.GetCell(2, i - 1)) {
                    yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValuesMatching.GetCell(2, i) / 100));
                }
                else {
                    yield return StartCoroutine(wallOffset.DummyMovement());
                }
            }
            else {
                yield return StartCoroutine(wallOffset.OffsetRealWall(offsetValuesMatching.GetCell(2, i) / 100));
            }

            wallOffset.MatchWalls();
            wallOffset.MoveVirtualWall(offsetValuesMatching.GetCell(0, i) / 100);

            // Wait time | Subbed for wall movement time instead
            //yield return new WaitForSeconds(waitTime);

            // CanAdjustWhat(offsetValues[i]);

            // Trial camera
            ChangeToTrialCamera();
            canAdjust = true;

            changeSound.Play();
            session.BeginNextTrial();

            // Let subject explore
            yield return StartCoroutine(WaitFor(session, trialTime));

            ManualSaveData(session, i);
            session.EndCurrentTrial();

            // Enable adjustment every other trial

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
        // Go to adjustment block
        //endText.enabled = true;
    }

    public IEnumerator Practice(Session session)
    {
        practiceTrialsScaling = gen.practiceEnvironmentsScaling;

        Debug.Log("Practice");
        session.BeginNextTrial();
        Debug.Log(session.CurrentBlock.trials.Count);

        currentTrialNum = session.currentTrialNum;

        // See if there are any practice trials
        if (session.CurrentBlock.trials.Count > 0) {

            // Non adjustment
            if (session.currentTrialNum < gen.numPracticeTrials / 2) {

                practiceText.enabled = true;

                yield return new WaitForSeconds(.3f);

                wallOffset.SetDefaultPos();
                wallOffset.MatchWalls();

                wallOffset.MoveVirtualWall((float)(practiceTrialsScaling[session.currentTrialNum - 1].x / 100));

                //leap.deviceOffsetZAxis = (practiceTrialsScaling[session.currentTrialNum - 1].y / 100 + leapDeviceZOffset);

                yield return new WaitForSeconds(1);

                ChangeToTrialCamera();

                slider.value = 50;
                canMoveSlider = true;
                

                yield return WaitFor(session, practiceTime);

                canMoveSlider = false;
            }
            else { 

            //Enviroment 1 ends
            //session.BeginNextTrial();

            // Adjustment

                ChangeToBlackScreenCamera();
                slider.gameObject.SetActive(false);

                wallOffset.MatchWalls();

                //wallOffset.MoveVirtualWall((float)(practiceTrials[session.currentTrialNum - 1].x / 100));

               // leap.deviceOffsetZAxis = (practiceTrials[session.currentTrialNum - 1].y / 100 + leapDeviceZOffset);

                yield return new WaitForSeconds(1);

                canAdjust = true;

               // CanAdjustWhat(practiceTrials[session.currentTrialNum - 2]);

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
                StartCoroutine(ScalingBlock(session));
            }

            practiceText.enabled = false;
        }
        else {
            practiceText.enabled = true;
            Debug.Log("No Practice");
            ChangeToTrialCamera();
            StartCoroutine(ScalingBlock(session));
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

        session.CurrentTrial.settings.SetValue("Hand Offset", offsetValuesScaling.GetCell(1, index));
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValuesScaling.GetCell(0, index));
        session.CurrentTrial.settings.SetValue("Wall Distance", offsetValuesScaling.GetCell(2, index));

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

        session.CurrentTrial.settings.SetValue("Hand Offset", offsetValuesMatching.GetCell(1, index));
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValuesMatching.GetCell(0, index));
        session.CurrentTrial.settings.SetValue("Wall Distance", offsetValuesMatching.GetCell(2, index));

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

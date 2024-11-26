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
    public List<ExperimentGeneratorScaling.AdjustmentValue> trialsAdjustment;

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
    public List<ExperimentGeneratorScaling.AdjustmentValue> practiceTrialsAdjustment;

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
    public TargetText targetText;

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

        trialsAdjustment = gen.block2;
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

        offsetValuesScaling = gen.trialsBlock1;

        for (int i = 0; i < offsetValuesScaling.GridSize.y; i++) {

            Debug.Log("Trial #" + i + " Wall Offset = " + offsetValuesScaling.GetCell(0, i) + " Hand Offset = " + offsetValuesScaling.GetCell(1, i) + " Wall Distance = " + offsetValuesScaling.GetCell(2, i));

            // Observation envrionment

            // Wait screen camera
            ChangeToBlackScreenCamera();

            // Set wall
            wallOffset.MatchWalls();
            wallOffset.MoveVirtualWall(offsetValuesScaling.GetCell(0, i) / 100);
            // Set hands
            leap.deviceOffsetZAxis = (offsetValuesScaling.GetCell(1, i) / 100 + leapDeviceZOffset);

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

            slider.value = 50;

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

            // Update equivalent adjustment trial with the target value
            FindOriginalIndexOfTrial(new Vector3(offsetValuesScaling.GetCell(0, i), offsetValuesScaling.GetCell(1, i), offsetValuesScaling.GetCell(2, i)), (int)slider.value);

            // Enable adjustment every other trial

            // BREAKS
            if (i % 26 == 0 && i != 0) {
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

    }

   /* public IEnumerator AdjustmentBlock(Session session)
     {
         for (int i = 0; i < trialsAdjustment.Count; i++) {

             Debug.Log("Trial #" + i + " Wall Offset = " + trialsAdjustment[i].offsets.x + " Hand Offset = " + trialsAdjustment[i].offsets.y + " Wall Distance = " + trialsAdjustment[i].offsets.z);

             // Observation envrionment

             // Wait screen camera
             ChangeToBlackScreenCamera();

             // Set wall
             wallOffset.MatchWalls();
             //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
             // Set hands
             leap.deviceOffsetZAxis = (trialsAdjustment[i].offsets.y / 100 + leapDeviceZOffset);
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
     } */

    public IEnumerator Practice(Session session)
    {
        wallOffset.SetDefaultPos();

        practiceTrialsScaling = gen.practiceEnvironmentsScaling;
        practiceTrialsAdjustment = gen.practiveEnvironmentsAdjustment;

        practiceText.enabled = true;

        Debug.Log("Practice");

        if (gen.numPracticeTrials > 0) {
            for (int i = 0; i < gen.numPracticeTrials; i++) {
                // See if there are any practice trials
                session.BeginNextTrial();

                Debug.Log("Trial #: " + session.currentTrialNum);

                currentTrialNum = session.currentTrialNum;

                // Non adjustment
                if (session.currentTrialNum <= gen.numPracticeTrials / 2) {
                    Debug.Log("Scaling practice");

                    yield return new WaitForSeconds(.3f);


                    wallOffset.MatchWalls();

                    wallOffset.MoveVirtualWall((float)(practiceTrialsScaling[session.currentTrialNum - 1].x / 100));

                    leap.deviceOffsetZAxis = (practiceTrialsScaling[session.currentTrialNum - 1].y / 100 + leapDeviceZOffset);

                    yield return new WaitForSeconds(1);

                    ChangeToTrialCamera();

                    slider.value = 50;
                    canMoveSlider = true;


                    yield return WaitFor(session, practiceTime);

                    canMoveSlider = false;

                    //session.BeginNextTrial();
                }
                else {
                    Debug.Log("Adjustment practice");

                    ChangeToBlackScreenCamera();
                    slider.gameObject.SetActive(false);
                    targetText.gameObject.GetComponent<TextMeshProUGUI>().enabled = true;

                    targetText.UpdateText(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 1)].target);

                    wallOffset.MatchWalls();

                    wallOffset.MoveVirtualWall((float)(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 1)].offsets.x / 100));
                    yield return StartCoroutine(wallOffset.OffsetRealWall(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 1)].offsets.z / 100));
                    wallOffset.MatchWalls();

                    leap.deviceOffsetZAxis = (practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 1)].offsets.y / 100 + leapDeviceZOffset);

                    yield return new WaitForSeconds(1);

                    canAdjust = true;

                    // Fix this
                    CanAdjustWhat(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 1)].modality);

                    ChangeToTrialCamera();

                    yield return WaitFor(session, practiceTime);

                    //session.BeginNextTrial();
                }

                // Ontrigger press save data

                if (session.currentTrialNum < session.CurrentBlock.trials.Count) {
                    ChangeToBlackScreenCamera();

                }
                else {

                    ChangeToBlackScreenCamera();
                    StartCoroutine(ScalingBlock(session));
                }


            }
        }
        else {
            practiceText.enabled = true;
            Debug.Log("No Practice");
            ChangeToTrialCamera();
            StartCoroutine(ScalingBlock(session));
        }
        practiceText.enabled = false;
    }

    // Trial timer without using a WaitForSeconds(). If skip key is pressed, exits timer;
    private IEnumerator WaitFor(Session session, float waitTime)
    {
        inTrial = true;
        // Debug.Log("waiting");
        for (float timer = waitTime; timer >= 0; timer -= Time.deltaTime) {
            if (skip && (timer - Time.deltaTime < waitTime - minTrialTime)) {
                //Debug.Log(timer - Time.deltaTime);

                // Replace adj target value
                // If in practice block
                if (session.currentBlockNum == 1 && session.currentTrialNum <= practiceTrialsScaling.Length) {

                    // Create new adjvalue (since structs are immutable) and replace it in the adjvalue list
                    CopyAndReplaceAdjValue(practiceTrialsAdjustment, practiceTrialsAdjustment[currentTrialNum - 1], currentTrialNum - 1, (int)slider.value);

                    //Debug.Log("Target: " + practiceTrialsAdjustment[currentTrialNum - 1].target);
                }


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

    // Save data from previous trial
    private void ManualSaveData(Session session, int index)
    {
        Debug.Log("Save data");

        session.CurrentTrial.settings.SetValue("Hand Offset", offsetValuesScaling.GetCell(1, index));
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValuesScaling.GetCell(0, index));
        session.CurrentTrial.settings.SetValue("Wall Distance", offsetValuesScaling.GetCell(2, index));
        session.CurrentTrial.settings.SetValue("Scale Value", slider.value); 

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

        //session.CurrentTrial.settings.SetValue("Hand Offset", offsetValuesMatching.GetCell(1, index));
        //session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", offsetValuesMatching.GetCell(0, index));
        //session.CurrentTrial.settings.SetValue("Wall Distance", offsetValuesMatching.GetCell(2, index));

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

    private void CanAdjustWhat(ExperimentGeneratorScaling.adjustModality modality)
    {
        wallHasBeenAdjusted = 0;


        if (modality == ExperimentGeneratorScaling.adjustModality.Hands) {
            canAdjustHands = true;
            Debug.Log("Adjust modality: Hands");
        }
        else {
            canAdjustHands = false;
        }

        if (modality == ExperimentGeneratorScaling.adjustModality.Wall) {
            canAdjustWall = true;
            Debug.Log("Adjust modality: Wall");
        }
        else {
            canAdjustWall = false;
        }

        if (modality == ExperimentGeneratorScaling.adjustModality.Both) {
            canAdjustWall = true;
            canAdjustHands = true;
            Debug.Log("Adjust modality: Both");
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

    private void CopyAndReplaceAdjValue(List<ExperimentGeneratorScaling.AdjustmentValue> belongsTo, ExperimentGeneratorScaling.AdjustmentValue toChange, int indexToReplace, int target)
    {
        ExperimentGeneratorScaling.AdjustmentValue copy = new ExperimentGeneratorScaling.AdjustmentValue(toChange.offsets, target, toChange.modality);
        belongsTo[indexToReplace] = copy;
    }

    private void FindOriginalIndexOfTrial(Vector3 offsets, int adjTarget)
    {
        for (int i = 0; i < trialsAdjustment.Count; i++) {
            if (offsets == trialsAdjustment[i].offsets) {
                CopyAndReplaceAdjValue(trialsAdjustment, trialsAdjustment[i], i, adjTarget);
                break;
            }
        }
    }

    // Changes the wall color to identify trials
    // Not used in current iteratiomn
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

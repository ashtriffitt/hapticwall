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
    private float handsHasBeenAdjusted;

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

    [SerializeField]
    private BallHands hands;

    [SerializeField]
    private AudioSource timeWarning;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        questionnaire = GameObject.Find("Questionnaire").GetComponent<Canvas>();

        wallOffset = GameObject.Find("Wall").GetComponent<WallOffset>();

        trialMask = mainCamera.cullingMask;

        changeSound = GetComponent<AudioSource>();

        wall = GameObject.Find("Wall").GetComponent<Renderer>();

        ChangeToBlackScreenCamera();

        trialsAdjustment = gen.block2;
    }

    // Update is called once per frame
    void Update()
    {

        // Skipping trial function
        if (viveController.triggerPressed || Input.GetKeyDown(KeyCode.Equals)) {

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
        session.EndCurrentTrial();

        yield return new WaitForSeconds(1);

        wallOffset.MatchWalls();
        hands.ResetHands();

        Debug.Log("0,0 environment");

        ChangeToTrialCamera();
        yield return WaitFor(session, 60);

        ChangeToBlackScreenCamera();


        yield return new WaitForSeconds(1);
        Debug.Log("Start scaling block");

        practiceText.enabled = false;
        // If there are practice trials you dont need this
        //wallOffset.SetDefaultPos();
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

            //leap.deviceOffsetZAxis = (offsetValuesScaling.GetCell(1, i) / 100 + leapDeviceZOffset);
            hands.ResetHands();
            hands.HandOffset(offsetValuesScaling.GetCell(1, i) / 100);

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

            slider.value = 0;

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
            if (i % 25 == 0 && i != 0) {
                breakText.enabled = true;
                Debug.Log("Break time");

                dominantHand.SwitchHands();

                ChangeToBlackScreenCamera();

                yield return StartCoroutine(WaitFor(session, 60));
                breakText.enabled = false;
            }

            // End UXF trial / save trial data
            Debug.Log("New trial");

        }

        ChangeToBlackScreenCamera();
        // Go to adjustment block
        StartCoroutine(PracticeAdjustment(session));

    }

    public IEnumerator AdjustmentBlock(Session session)
     {

        session.EndCurrentTrial();

        yield return new WaitForSeconds(1);

        wallOffset.MatchWalls();
        hands.ResetHands();

        Debug.Log("0,0 environment");

        ChangeToTrialCamera();
        yield return WaitFor(session, 60);

        ChangeToBlackScreenCamera();
        yield return new WaitForSeconds(1);

         for (int i = 0; i < trialsAdjustment.Count; i++) {

             //session.BeginNextTrial();
             Debug.Log("Trial #" + i + " Wall Offset = " + trialsAdjustment[i].offsets.x + " Hand Offset = " + trialsAdjustment[i].offsets.y + " Wall Distance = " + trialsAdjustment[i].offsets.z);

             // Observation envrionment

             // Wait screen camera
             ChangeToBlackScreenCamera();

             // Set wall
             wallOffset.MatchWalls();
            //wallOffset.MoveVirtualWall(offsetValues.GetCell(0, i) / 100);
            // Set hands

            hands.ResetHands();
            hands.HandOffset(trialsAdjustment[i].offsets.y / 100);
             //leap.deviceOffsetZAxis = (trialsAdjustment[i].offsets.y / 100 + leapDeviceZOffset);

             Debug.Log("Wall offset: " + trialsAdjustment[i].offsets.x / 100);
             Debug.Log("Hand offset: " + (trialsAdjustment[i].offsets.y / 100 + leapDeviceZOffset));

             // Move real wall
             if (i != 0) {
                 if (trialsAdjustment[i].offsets.z != trialsAdjustment[i - 1].offsets.z) {
                     yield return StartCoroutine(wallOffset.OffsetRealWall(trialsAdjustment[i].offsets.z / 100));
                 }
                 else {
                     yield return StartCoroutine(wallOffset.DummyMovement());
                 }
             }
             else {
                 yield return StartCoroutine(wallOffset.OffsetRealWall(trialsAdjustment[i].offsets.z / 100));
             }

             wallOffset.MatchWalls();
             wallOffset.MoveVirtualWall(trialsAdjustment[i].offsets.x / 100);

            if (trialsAdjustment[i].target == -1) {
                Debug.Log("-1 found");
                targetText.UpdateText(75);
            }
            else {
                targetText.UpdateText(trialsAdjustment[i].target);
            }

             CanAdjustWhat(trialsAdjustment[i].modality);

             

             // Trial camera
             ChangeToTrialCamera();
             canAdjust = true;

             changeSound.Play();
             session.BeginNextTrial();

             // Let subject explore
             yield return StartCoroutine(WaitFor(session, trialTime));

             ManualSaveAdjustmentData(session, i);
             session.EndCurrentTrial();

             canAdjust = false;


             // BREAKS
             if (i % 25 == 0 && i != 0) {
                 breakText.enabled = true;
                 Debug.Log("Break time");

                 ChangeToBlackScreenCamera();

                 dominantHand.SwitchHands();

                 yield return StartCoroutine(WaitFor(session, 300));
                 breakText.enabled = false;
             }

             // End UXF trial / save trial data
             Debug.Log("New trial");

         }

         ChangeToBlackScreenCamera();
        session.End();
         // Go to adjustment block
         //endText.enabled = true;
     } 

    public IEnumerator Practice(Session session)
    {
        slider.gameObject.SetActive(false);

        wallOffset.SetDefaultPos();

        practiceTrialsScaling = gen.practiceEnvironmentsScaling;
        practiceTrialsAdjustment = gen.practiveEnvironmentsAdjustment;

        practiceText.enabled = true;

        Debug.Log("Practice");

        // (0,0) practice scenario

        session.BeginNextTrial();

        Debug.Log("Trial #: " + session.currentTrialNum);

        currentTrialNum = session.currentTrialNum;

        wallOffset.MatchWalls();

        leap.deviceOffsetZAxis = leapDeviceZOffset;

        ChangeToTrialCamera();

        yield return WaitFor(session, 1000);

        ChangeToBlackScreenCamera();
        yield return new WaitForSeconds(1);

        // Wall offset
        session.BeginNextTrial();

        Debug.Log("Trial #: " + session.currentTrialNum);

        currentTrialNum = session.currentTrialNum;

        wallOffset.MatchWalls();
        wallOffset.MoveVirtualWall(-.1f);

        leap.deviceOffsetZAxis = leapDeviceZOffset;

        ChangeToTrialCamera();

        yield return WaitFor(session, 1000);

        ChangeToBlackScreenCamera();
        yield return new WaitForSeconds(1);

        // Hand offset

        session.BeginNextTrial();

        Debug.Log("Trial #: " + session.currentTrialNum);

        currentTrialNum = session.currentTrialNum;

        wallOffset.MatchWalls();

        hands.ResetHands();
        hands.HandOffset(-.1f);

        ChangeToTrialCamera();

        yield return WaitFor(session, 1000);

        ChangeToBlackScreenCamera();
        yield return new WaitForSeconds(1);

        // prop offset

        session.BeginNextTrial();

        Debug.Log("Trial #: " + session.currentTrialNum);

        currentTrialNum = session.currentTrialNum;

        wallOffset.MatchWalls();
        wallOffset.MoveVirtualWall(-.15f);

        hands.ResetHands();
        hands.HandOffset(.15f);

        ChangeToTrialCamera();

        yield return WaitFor(session, 1000);

        ChangeToBlackScreenCamera();
        yield return new WaitForSeconds(1);

        /*
        // Offset trial scenarios
        if (gen.numPracticeTrials > 0) {
            for (int i = 0; i < gen.numPracticeTrials - 1; i++) {
                // See if there are any practice trials
                session.BeginNextTrial();

                Debug.Log("Trial #: " + session.currentTrialNum);

                currentTrialNum = session.currentTrialNum;

                // Non adjustment // NOTE: the -2 and +2s encountered to set index exists due to the initial (0,0) practice trial.
                if (session.currentTrialNum <= 4) {
                    Debug.Log("Scaling practice");

                    yield return new WaitForSeconds(.3f);


                    wallOffset.MatchWalls();

                    wallOffset.MoveVirtualWall((float)(practiceTrialsScaling[session.currentTrialNum - 2].x / 100));

                    //leap.deviceOffsetZAxis = (practiceTrialsScaling[session.currentTrialNum - 2].y / 100 + leapDeviceZOffset);

                    hands.ResetHands();
                    hands.HandOffset(practiceTrialsScaling[session.currentTrialNum - 2].y / 100);

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

                    targetText.UpdateText(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].target);

                    wallOffset.MatchWalls();

                    wallOffset.MoveVirtualWall((float)(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].offsets.x / 100));
                    yield return StartCoroutine(wallOffset.OffsetRealWall(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].offsets.z / 100));
                    wallOffset.MatchWalls();

                    //leap.deviceOffsetZAxis = (practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].offsets.y / 100 + leapDeviceZOffset);

                    hands.ResetHands();
                    hands.HandOffset(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].offsets.y / 100);

                    yield return new WaitForSeconds(1);

                    canAdjust = true;

                    CanAdjustWhat(practiceTrialsAdjustment[session.currentTrialNum - (practiceTrialsScaling.Length + 2)].modality);

                    ChangeToTrialCamera();

                    yield return WaitFor(session, practiceTime);

                    canAdjust = false;

                    //session.BeginNextTrial();
                }

                // Ontrigger press save data

                if (session.currentTrialNum < session.CurrentBlock.trials.Count) {
                    ChangeToBlackScreenCamera();

                }
                else {

                    
                    ChangeToBlackScreenCamera();
                    
                }


            }
        }
        else {
            practiceText.enabled = true;
            Debug.Log("No Practice");
            ChangeToTrialCamera();
            StartCoroutine(ScalingBlock(session));
        }
        Debug.Log("Main block should start");
        ChangeToBlackScreenCamera();
        StartCoroutine(ScalingBlock(session));
        practiceText.enabled = false;
        */

        StartCoroutine(PracticeScaling(session));
    }

    private IEnumerator PracticeScaling(Session session)
    {
        slider.gameObject.SetActive(true);

        practiceTrialsScaling = gen.practiceEnvironmentsScaling;
        Debug.Log("Scaling practice");


        for (int i = 0; i < practiceTrialsScaling.Length; i++) {

            session.BeginNextTrial();

            Debug.Log("Trial #: " + session.currentTrialNum);
            Debug.Log("i" + i);

            currentTrialNum = session.currentTrialNum;
            yield return new WaitForSeconds(1);

            wallOffset.MatchWalls();

            wallOffset.MoveVirtualWall((float)(practiceTrialsScaling[i].x / 100));

            hands.ResetHands();
            hands.HandOffset(practiceTrialsScaling[i].y / 100);

            yield return new WaitForSeconds(1);

            ChangeToTrialCamera();

            slider.value = 0;
            canMoveSlider = true;

            yield return WaitFor(session, 1000);

            canMoveSlider = false;

            ChangeToBlackScreenCamera();
        }

        StartCoroutine(ScalingBlock(session));
    }

    private IEnumerator PracticeAdjustment(Session session)
    {
        practiceText.enabled = true;

        practiceTrialsAdjustment = gen.practiveEnvironmentsAdjustment;
        slider.gameObject.SetActive(false);

        targetText.gameObject.GetComponent<TextMeshProUGUI>().enabled = true;

        for (int i = 0; i < practiceTrialsAdjustment.Count; i++) {

            session.BeginNextTrial();

            Debug.Log("Adjustment practice");
            Debug.Log("Trial #: " + session.currentTrialNum);

            targetText.UpdateText(practiceTrialsAdjustment[i].target);

            wallOffset.MatchWalls();

            wallOffset.MoveVirtualWall((float)(practiceTrialsAdjustment[i].offsets.x / 100));
            yield return StartCoroutine(wallOffset.OffsetRealWall(practiceTrialsAdjustment[i].offsets.z / 100));

            //wallOffset.MatchWalls();

            hands.ResetHands();
            hands.HandOffset(practiceTrialsAdjustment[i].offsets.y / 100);

            yield return new WaitForSeconds(1);

            CanAdjustWhat(practiceTrialsAdjustment[i].modality);

            ChangeToTrialCamera();
            canAdjust = true;

            yield return WaitFor(session, 1000);

            canAdjust = false;

            ChangeToBlackScreenCamera();
        }

        practiceText.enabled = false;
        StartCoroutine(AdjustmentBlock(session));
    }


    // Trial timer without using a WaitForSeconds(). If skip key is pressed, exits timer;
    private IEnumerator WaitFor(Session session, float waitTime)
    {
        bool timerFlag = false;
        inTrial = true;
        // Debug.Log("waiting");

        float otherTimer = 0;

        for (float timer = waitTime; timer >= 0; timer -= Time.deltaTime) {

            otherTimer += Time.deltaTime;

            if (otherTimer > 35 && !timerFlag) {
                
                timeWarning.Play();
                timerFlag = true;
            }

            if (skip && (timer - Time.deltaTime < waitTime - minTrialTime)) {

                // Replace adj target value
                // If in practice block
                if (session.currentBlockNum == 1 && session.currentTrialNum <= practiceTrialsScaling.Length) {

                    // Create new adjvalue (since structs are immutable) and replace it in the adjvalue list
                    //CopyAndReplaceAdjValue(practiceTrialsAdjustment, practiceTrialsAdjustment[currentTrialNum - 1], currentTrialNum - 1, (int)slider.value);
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

        session.CurrentTrial.settings.SetValue("Adjustment Hand Offset", handsHasBeenAdjusted * 100);

        // Sort this
        session.CurrentTrial.settings.SetValue("Adjustment Wall Offset", wallHasBeenAdjusted * 100);

        session.CurrentTrial.settings.SetValue("Hand Offset", trialsAdjustment[index].offsets.y);
        session.CurrentTrial.settings.SetValue("Virtual Wall Offset From Real Wall", trialsAdjustment[index].offsets.x);
        session.CurrentTrial.settings.SetValue("Wall Distance", trialsAdjustment[index].offsets.z);

        session.CurrentTrial.settings.SetValue("Scale Value", trialsAdjustment[index].target);
        session.CurrentTrial.settings.SetValue("Adjustment Modality", trialsAdjustment[index].modality.ToString());
        
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
        handsHasBeenAdjusted = 0;


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

    public void AdjustOffset(float amt)
    {
        //Debug.Log("adjust");

        if (canAdjust) {
            if (canAdjustWall) {
                //Debug.Log("Adjust wall");
                wallOffset.MoveVirtualWall(-amt);

                wallHasBeenAdjusted += amt;
            }

            if (canAdjustHands) {
                //Debug.Log("adjust hands");
                hands.HandOffset(amt);
                //leap.deviceOffsetZAxis += amt;
                handsHasBeenAdjusted += amt;
            }
        }
    }

    private void CopyAndReplaceAdjValue(List<ExperimentGeneratorScaling.AdjustmentValue> belongsTo, ExperimentGeneratorScaling.AdjustmentValue toChange, int indexToReplace, int target)
    {
        Vector3 temp = new Vector3(toChange.offsets.x / 2, toChange.offsets.y / 2, toChange.offsets.z);
        ExperimentGeneratorScaling.AdjustmentValue copy = new ExperimentGeneratorScaling.AdjustmentValue(temp, target, toChange.modality);
        belongsTo[indexToReplace] = copy;
    }

    private void FindOriginalIndexOfTrial(Vector3 offsets, int adjTarget)
    {
        for (int i = 0; i < trialsAdjustment.Count; i++) {

            //Debug.Log("Offsets: " + offsets);
            //Debug.Log("trial offset" + trialsAdjustment[i].offsets);

            if (offsets == trialsAdjustment[i].offsets && trialsAdjustment[i].target < 0) {
                Debug.Log("Found copy");
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

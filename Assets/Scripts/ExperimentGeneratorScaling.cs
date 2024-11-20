using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UXF;

public class ExperimentGeneratorScaling : MonoBehaviour
{
    public int numTrialsBlock1;
    public int numTrialsBlock2;

    public int numPracticeTrials;

    public Array2DFloat trialsBlock1;
    public Array2DFloat trialsBlock2;

    [SerializeField]
    public List<int[]> arrayListBlock1 = new List<int[]>();
    [SerializeField]
    public List<float[]> arrayListBlock2 = new List<float[]>();

    [SerializeField]
    private TrialManagerScaling trialManager;

    public Vector3[] environments;

    public Vector3[] practiceEnvironmentsScaling;
    public List<AdjustmentValue> practiveEnvironmentsAdjustment;

    public GameObject dangerzones;

    public enum adjustModality {Wall, Hands, Both};

    public List<AdjustmentValue> block2 = new List<AdjustmentValue>();

    public struct AdjustmentValue
    {
        public Vector3 offsets;
        public int target;
        public adjustModality modality;

        public AdjustmentValue(Vector3 offsets, int target, adjustModality modality)
        {
            this.offsets = offsets;
            this.target = target;
            this.modality = modality;
        }
    }

    public void Generate(Session session)
    {
        // Turn on cameras
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        GameObject.Find("Black Screen Camera").GetComponent<Camera>().enabled = false;

        // Wall offset 0, hand offset 1, wall distance 2
        for (int i = 0; i < 50; i++) {
            arrayListBlock1.Add(new int[3]);
            arrayListBlock2.Add(new float[4]);

            block2.Add(new AdjustmentValue(new Vector3(0, 0, 0), 0, adjustModality.Wall));
        }

        //Debug.Log("array length" + arrayListBlock1.Count);

        // Place all environments in array 3 times
        int index = 0;
        for (int i = 0; i < arrayListBlock1.Count; i++) {

            arrayListBlock1[i][0] = (int)environments[index].x;
            arrayListBlock1[i][1] = (int)environments[index].y;
            arrayListBlock1[i][2] = (int)environments[index].z;

            if ((i + 1) % 2 == 0) {
                index++;
            }
        }

        List<Vector3> tempList = new List<Vector3>();

        // Set array 2
        for (int i = 0; i < arrayListBlock1.Count; i++) {
            var temp = arrayListBlock1[i];
            Vector3 tempVector = new Vector3(arrayListBlock1[i][0], arrayListBlock1[i][1], arrayListBlock1[i][2]);
            tempList.Add(tempVector);

            AdjustmentValue adj = new AdjustmentValue(new Vector3(0, 0, 0), 0, adjustModality.Wall);

            if (temp[0] != 0) {
                adj.offsets.x = temp[0] / 2;
            }
            if (temp[1] != 0) {
                adj.offsets.y = temp[1] / 2;
            }
            adj.offsets.z = temp[2];

            // If just wall offset
            if (temp[0] != 0 && temp[1] == 0) {
                adj.modality = adjustModality.Hands;
                // flip modality if needed
                if (i != 0 && tempVector == tempList[i - 1]  && block2[i - 1].modality == adjustModality.Hands) {
                    Debug.Log("Last modality: " + block2[i - 1].modality);
                    adj.modality = adjustModality.Both;
                }
            }
            // If just hands
            else if (temp[0] == 0 && temp[1] != 0) {
                adj.modality = adjustModality.Wall;
                // flip modality if needed
                if (i != 0 && tempVector == tempList[i - 1] && block2[i - 1].modality == adjustModality.Wall) {
                    Debug.Log("Last modality: " + block2[i - 1].modality);
                    adj.modality = adjustModality.Both;
                }
            }
            // If just prop offset
            else if (temp[0] != 0 && temp[1] != 0) {
                adj.modality = adjustModality.Wall;
                // flip modality if needed
                if (i != 0 && tempVector == tempList[i - 1] && block2[i - 1].modality == adjustModality.Wall) {
                    Debug.Log("Last modality: " + block2[i - 1].modality);
                    adj.modality = adjustModality.Hands;
                }
            }

            block2[i] = adj;

            Debug.Log("Offsets " + adj.offsets.x + " " + adj.offsets.y + " " + adj.offsets.z);
            Debug.Log("Modality " + adj.modality.ToString());

            // For setting target value: in scaling phase, after subject submits number, search block 2 for trial with the same offsets and insert the scaled number.

        }


        // Shuffle array list of trials
        arrayListBlock1.Shuffle();
        
        // Convert arraylist to readable grid.
        // This is unnecessary i think, can just do trialsBlcok1 = arrayListBlock1
        index = 0;

        for (int i = 0; i < trialsBlock1.GridSize.y; i++) {
            trialsBlock1.SetCell(0, i, arrayListBlock1[i][0]);
            trialsBlock1.SetCell(1, i, arrayListBlock1[i][1]);
            trialsBlock1.SetCell(2, i, arrayListBlock1[i][2]);
            Debug.Log(i);
        }

        //PRACTICE GENERATION

        practiveEnvironmentsAdjustment = new List<AdjustmentValue>();
        practiveEnvironmentsAdjustment.Add(new AdjustmentValue(new Vector3(-1, 0, 0), 0, adjustModality.Hands));
        practiveEnvironmentsAdjustment.Add(new AdjustmentValue(new Vector3(0, -2, 0), 0, adjustModality.Both));

        // Create practice block
        Block practice = session.CreateBlock(numPracticeTrials);

        // Create main block
        Block scaling = session.CreateBlock(numTrialsBlock1);
        Block adjustment = session.CreateBlock(numTrialsBlock2);

        // Set trial manager offset array = to this trial array.
        trialManager.offsetValuesScaling = trialsBlock1;

        if (numPracticeTrials > 0) {
            Debug.Log("Practice start");
            // Start practice block; this is where main block is called
            trialManager.StartCoroutine(trialManager.Practice(session));
        }
        else {
            trialManager.StartCoroutine(trialManager.ScalingBlock(session));
        }
    }

    // Set the danger zones to the initial subject position; it is delayed to let the headset position register
    public void TurnOnDangerZones()
    {
        StartCoroutine(DelayedTurnOnDangerZones());
    }

    private IEnumerator DelayedTurnOnDangerZones()
    {
        yield return new WaitForSeconds(1);
        dangerzones.active = true;
    }
}

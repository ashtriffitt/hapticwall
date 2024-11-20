using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UXF;

public class ExperimentGeneratorMatching : MonoBehaviour
{
    public int numTrials;

    public int numPracticeTrials;

    public Array2DFloat trials;

    [SerializeField]
    public List<int[]> arrayList = new List<int[]>();

    [SerializeField]
    private TrialManagerMatching trialManager;

    public Vector3[] environments;
    public Vector3[] adjustmentEnvironments;

    public GameObject dangerzones;


    public void Generate(Session session)
    {
        // Turn on cameras
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        GameObject.Find("Black Screen Camera").GetComponent<Camera>().enabled = false;

        // Wall offset 0, hand offset 1, wall distance 2
        for (int i = 0; i < 64; i++) {
            arrayList.Add(new int[6]);
        }

        Debug.Log("array length" + arrayList.Count);

        // Place all environments in array 4 times
        int index = 0;
        for (int i = 0; i < arrayList.Count; i++) {

            arrayList[i][0] = (int)environments[index].x;
            arrayList[i][1] = (int)environments[index].y;
            arrayList[i][2] = (int)environments[index].z;

            if ((i + 1) % 4 == 0) {
                index++;
            }
        }

        index = 0;
        // Fill in adjustment environment values
        for (int i = 0; i < arrayList.Count; i++) {
            if ((i + 1) % 2 == 0) {
                arrayList[i][5] = 7;
            }
        }

        // Shuffle array list of trials
        arrayList.Shuffle();

        // Convert arraylist to readable grid.
        index = 0;
        for (int i = 0; i < trials.GridSize.y; i += 2) {
            trials.SetCell(0, i, arrayList[index][0]);
            trials.SetCell(1, i, arrayList[index][1]);
            trials.SetCell(2, i, arrayList[index][2]);

            trials.SetCell(0, i + 1, arrayList[index][3]);
            trials.SetCell(1, i + 1, arrayList[index][4]);
            trials.SetCell(2, i + 1, arrayList[index][5]);

            index++;
        }

        // Create practice block
        Block practice = session.CreateBlock(numPracticeTrials);

        // Create main block
        Block main = session.CreateBlock(numTrials);

        // Set trial manager offset array = to this trial array.
        trialManager.offsetValues = trials;

        // Start practice block; this is where main block is called
        trialManager.StartCoroutine(trialManager.Practice(session));
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

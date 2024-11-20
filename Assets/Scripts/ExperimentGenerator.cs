using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    public int numTrials;

    public int numPracticeTrials;

    public Array2DFloat trials;

    [SerializeField]
    public List<int[]> arrayList = new List<int[]>();

    [SerializeField]
    private TrialManager trialManager;

    public Vector3[] environments;

    public GameObject dangerzones;


    public void Generate(Session session)
    {
        // Turn on cameras
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
       GameObject.Find("Black Screen Camera").GetComponent<Camera>().enabled = false;

        // Wall offset 0, hand offset 1, wall distance 2
        for (int i = 0; i < 100; i++) {
            arrayList.Add(new int[6]);
        }

        Debug.Log("array length" + arrayList.Count);

        int index = 0;
        for (int i = 0; i < arrayList.Count; i++) {

            if (index == 10) {
                index = 0;
            }

            arrayList[i][0] = (int)environments[index].x;
            arrayList[i][1] = (int)environments[index].y;
            arrayList[i][2] = (int)environments[index].z;

            arrayList[i][3] = (int)environments[(int)i / 10].x;
            arrayList[i][4] = (int)environments[(int)i / 10].y;
            arrayList[i][5] = (int)environments[(int)i / 10].z;
               
            index++;
            
            
        }

        // Remove trials comparing two identical environments
        for (int i = 0; i < arrayList.Count; i++) {
            if ((arrayList[i][0] == arrayList[i][3]) && (arrayList[i][1] == arrayList[i][4]) && (arrayList[i][2] == arrayList[i][5])) {
                arrayList.RemoveAt(i);
            }
        }

        // Shuffle array list of trials
        arrayList.Shuffle();

        

        
       /* for (int i = 0; i < trials.GridSize.y; i++) {
            trials.SetCell(0, i, arrayList[i][0]);
            trials.SetCell(1, i, arrayList[i][1]);
            trials.SetCell(2, i, arrayList[i][2]);
            trials.SetCell(3, i, arrayList[i][3]);
        } */

        
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

        trialManager.offsetValues = trials;

        // Start practice block
        // session.BeginNextTrial();
        //trialManager.StartCoroutine(trialManager.ChangeTrial(session)); // This will be replaced with 'Trial' / Or the initial trial methid

        trialManager.StartCoroutine(trialManager.Practice(session));
    }

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

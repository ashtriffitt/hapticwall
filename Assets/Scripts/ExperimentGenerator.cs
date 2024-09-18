using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    public int numTrials;

    public Array2DFloat trials;

    [SerializeField]
    private TrialManager trialManager;

    [SerializeField]
    private int[] handOffsetIntervals;
    [SerializeField]
    private int[] wallOffsetIntervals;
    [SerializeField]
    private int[] wallDistanceIntervals;


    public void Generate(Session session)
    {
        // Turn on cameras
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
       GameObject.Find("Black Screen Camera").GetComponent<Camera>().enabled = false;


        // Create practice session
        Block practice = session.CreateBlock(1);

        // Generate 3 blocks, one for each wall distance, each block is numTrials/3
        Block block1 = session.CreateBlock(numTrials / 3);
        Block block2 = session.CreateBlock(numTrials / 3);
        Block block3 = session.CreateBlock(numTrials / 3);

        // Populate block 1, randomly choose a wall distance to start at and then populate will all possible combinations of wall offset and hand offset.

        // Make triaL grid
        // Choose wall distance for first block
        // populate block
        int index = 0;
        for (int i = 0; i < trials.GridSize.y; i++) {
            // Randomly choose wall distance for this block
            trials.SetCell(2, i, wallDistanceIntervals[0]);

            // Cycle through wall offset values
            trials.SetCell(0, i, wallOffsetIntervals[i % 5]);

            // Change hand offset value every [threshold count] trials

            trials.SetCell(1, i, handOffsetIntervals[index]);
            if ((i + 1) % handOffsetIntervals.Length == 0 && i != 0) {
                index++;
            }
        }

        /* for (int i = 0; i < trials.GridSize.y; i++) {
            for (int j = 0; j < handOffsetIntervals.Length; j++) {
                trials.SetCell(0, j, wallOffsetIntervals[j]);
            }
        } */


        // Start practice block
        session.BeginNextTrial();
        trialManager.StartCoroutine(trialManager.ChangeTrial(session)); // This will be replaced with 'Trial' / Or the initial trial methid


        // Once every trial is over, call end trial and record data. Then start next trial. This is all you need to do with UXF.
    }
}

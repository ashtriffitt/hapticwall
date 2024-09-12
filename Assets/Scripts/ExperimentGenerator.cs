using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    public int numTrials;

    [SerializeField]
    private TrialManager trialManager;

    public void Generate(Session session)
    {
        // Turn on cameras
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        GameObject.Find("Black Screen Camera").GetComponent<Camera>().enabled = false;


        // Create practice session
        Block practice = session.CreateBlock(1);

        // Generate a single block with x trials
        Block mainBlock = session.CreateBlock(numTrials);

        // Start practice block
        session.BeginNextTrial();
        trialManager.StartCoroutine(trialManager.ChangeTrial(session)); // This will be replaced with 'Trial' / Or the initial trial methid


        // Once every trial is over, call end trial and record data. Then start next trial. This is all you need to do with UXF.
    }
}

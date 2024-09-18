using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerTracker : MonoBehaviour
{

    private Canvas canvas;
    public bool canAnswer;

    public int lastAnswer;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        // If questionnaire canvas is active, be able to record answer.
        if (canAnswer) {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                lastAnswer = 0;
                canAnswer = false;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                lastAnswer = 1;
                canAnswer = false;
            }
        }
    }

    public IEnumerator AnswerDelay()
    {
        yield return new WaitForSeconds(.3f);
        canAnswer = true;
    }
}

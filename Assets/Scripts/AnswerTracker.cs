using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerTracker : MonoBehaviour
{

    private Canvas canvas;

    public int lastAnswer;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canvas.isActiveAndEnabled) {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                lastAnswer = 0;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                lastAnswer = 1;
            }
        }
    }
}

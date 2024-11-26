using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViveController : MonoBehaviour
{
    [SerializeField]
    private InputActionMap controller;

    [SerializeField]
    private SliderControl slider;

    private Vector2 trackpadPos;
    private InputAction trackpad;

    [SerializeField]
    private bool trackpadClicked;
    private InputAction trackpadClick;
    private InputAction trackpadHold;

    private InputAction triggerPressedAction;
    public bool triggerPressed;

    public bool leftClick;
    public bool rightClick;
    public bool upClick;
    public bool downClick;

    public int lastAnswer;

    private bool trackpadHeld;
    

    // Start is called before the first frame update
    void Start()
    { 

        controller.Enable();

        trackpad = controller.FindAction("Trackpad");
        trackpad.Enable();

        trackpadClick = controller.FindAction("TrackpadClick");
        trackpadClick.Enable();
        
        triggerPressedAction = controller.FindAction("Trigger");
        triggerPressedAction.Enable();

        trackpadHold = controller.FindAction("TrackpadHold");
        trackpadHold.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        trackpadPos = trackpad.ReadValue<Vector2>();

        trackpadClicked = trackpadClick.triggered;

        triggerPressed = triggerPressedAction.triggered;

        if (trackpadClicked && trackpadPos.x < -.5f) {
            //Debug.Log("left" + trackpadPos.x);
            leftClick = true;
            lastAnswer = 0;
        }
        else {
            leftClick = false;
        }

        if (trackpadClicked && trackpadPos.x > .5f) {
            //Debug.Log("right" + trackpadPos.x);
            rightClick = true;
            lastAnswer = 1;
        }
        else {
            rightClick = false;
        }

        if(trackpadClicked && trackpadPos.y < -.5f) {
            //Debug.Log("down" + trackpadPos.y);
            downClick = true;
            
        }
        else {
            downClick = false;
        }

        if (trackpadClicked && trackpadPos.y > .5f) {
            //Debug.Log("up" + trackpadPos.y);
            upClick = true;
        }
        else {
            upClick = false;
        }

        trackpadHold.performed += ClickAndHold;
        trackpadHold.canceled += Released;

        if (trackpadHeld) {
            //Debug.Log("Held");
            if (trackpadPos.x > 0) {
                slider.Holding(1);
            }
            else {
                slider.Holding(-1);
            }
        }
    }

    private void Released(InputAction.CallbackContext ctx)
    {
        trackpadHeld = false;
    }

    private void ClickAndHold(InputAction.CallbackContext ctx)
    {
        trackpadHeld = true;
    }
}



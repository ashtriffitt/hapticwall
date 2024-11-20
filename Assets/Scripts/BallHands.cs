using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class BallHands : MonoBehaviour
{

    public CapsuleHand handR;
    public HandEnableDisable handEnableR;

    public CapsuleHand handL;
    public HandEnableDisable handEnableL;

    public GameObject ballHandR;
    public GameObject ballHandL;

    public float ballOffset;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // if hand is in frame then move static hands to hand pos
        if (handR.gameObject.active) {
            Vector3 tempR = handR._hand.PalmPosition;
            ballHandR.transform.position = new Vector3(tempR.x, tempR.y, tempR.z + ballOffset);
            ballHandR.transform.rotation = handR._hand.Rotation;
        }
        // if hand is in frame then move static hands to hand pos
        if (handL.gameObject.active) {
            Vector3 tempL = handL._hand.PalmPosition;
            ballHandL.transform.position = new Vector3(tempL.x, tempL.y, tempL.z + ballOffset);
            ballHandL.transform.rotation = handL._hand.Rotation;
        }


        /* Vector3 tempRotL = handL._hand.Rotation.eulerAngles;
        Vector3 tempRotR = handR._hand.Rotation.eulerAngles;
        //Vector3 tempRot = handL._hand.PalmNormal;
        //tempRot = new Vector3(-tempRot.x - 90, 0, 90);
        ballHandL.transform.eulerAngles = tempRotL;
        ballHandR.transform.eulerAngles = tempRotR; */

    }
}

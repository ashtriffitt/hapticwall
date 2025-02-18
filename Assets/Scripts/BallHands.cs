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

    private LineRenderer line;
    public LineRenderer fingerLine;

    public GameObject debugSphere;

    public GameObject actualHandR;
    public GameObject actualHandL;

    // default hand offset
    private float defaultHandZPos;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        //fingerLine = actualHandR.GetComponent<LineRenderer>();

        defaultHandZPos = actualHandR.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        
        // if hand is in frame then move static hands to hand pos
        if (handR.gameObject.active) {
            Vector3 tempR = handR._hand.PalmPosition;
            //Debug.Log("HandR pos " + tempR);
            //Debug.Log("HandR vector " + handR._hand.PalmNormal);

            Vector3 pointA = tempR;
            Vector3 pointB = pointA + (handR._hand.PalmNormal * 5);
            line.SetPosition(0, pointA);
            line.SetPosition(1, pointB);

            //debugSphere.transform.position = pointB;

            ballHandR.transform.position = new Vector3(tempR.x, tempR.y, tempR.z + ballOffset);
            //ballHandR.transform.rotation = handR._hand.Rotation;

            Vector3 target = Vector3.Normalize(pointA - pointB);
            //Debug.Log("Target " + target);

           

            Vector3 bonePoint = handR._hand.GetFinger(Finger.FingerType.MIDDLE).GetBone(Bone.BoneType.METACARPAL).NextJoint;
            //debugSphere.transform.position = bonePoint;

            Vector3 bonePointTemp = bonePoint - tempR;
            Vector3 bonePointB = pointA + (bonePointTemp * 5);
            debugSphere.transform.position = bonePointB;
            //fingerLine.SetPosition(0, pointA);
            //fingerLine.SetPosition(1, bonePointB);

            Vector3 myUp = (bonePointTemp);

            ballHandR.transform.rotation = Quaternion.LookRotation(target, myUp);

        }
        // if hand is in frame then move static hands to hand pos
        if (handL.gameObject.active) {
            Vector3 tempL = handL._hand.PalmPosition;
            ballHandL.transform.position = new Vector3(tempL.x, tempL.y, tempL.z + ballOffset);
            ballHandL.transform.localRotation = handL._hand.Rotation;
        }

    }

    public void HandOffset(float amt)
    {
        Debug.Log("amt " + amt);

        ballOffset -= amt;

       // actualHandR.transform.position = new Vector3(actualHandR.transform.position.x, actualHandR.transform.position.y, actualHandR.transform.position.z - amt);
        // actualHandL.transform.localPosition = new Vector3(actualHandL.transform.position.x, actualHandL.transform.position.y, actualHandL.transform.position.z - amt);

        //ballHandR.transform.localPosition = new Vector3(ballHandR.transform.l)
    }

    public void ResetHands()
    {
        ballOffset = 0;

        //actualHandR.transform.position = new Vector3(actualHandR.transform.position.x, actualHandR.transform.position.y, defaultHandZPos);
        //actualHandL.transform.position = new Vector3(actualHandL.transform.position.x, actualHandL.transform.position.y, defaultHandZPos);
    }
}

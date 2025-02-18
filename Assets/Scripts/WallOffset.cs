using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallOffset : MonoBehaviour
{

    public float wallOffsetZ; // The magic number that the wall should always be offset from the tracker by
    public float wallOffsetY;
    public float wallOffsetX;

    [SerializeField]
    private float startingTrackerPos;

    [SerializeField]
    private GameObject tracker;

    [SerializeField]
    private SampleUserPolling_ReadWrite arduino;

    public bool wallMoving;

    // Start is called before the first frame update
    void Start()
    {
       // arduino = GameObject.Find("SampleUserPolling").GetComponent<SampleUserPolling_ReadWrite>();
    }

    // Update is called once per frame  
    void Update()
    {
        
    }

    // Offsets the virtual wall from the real wall
    public void MoveVirtualWall(float offset)
    {
        //Debug.Log("Offset = " + offset);
        //Debug.Log("z transform of virtual wall = " + transform.position.z);
       // Debug.Log("z transform of real wall = " + tracker.transform.position.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, (transform.position.z + offset));
    }

    public IEnumerator OffsetRealWall(float offset)
    {
        wallMoving = true;

        //Debug.Log("Default tracker z pos = " + startingTrackerPos);
        float target = startingTrackerPos + offset;
        target = Mathf.Abs(target);
        //Debug.Log("Target z pos = " + target);

        // If wall is in front of target
        // Then move to desired position.

        if (target > Mathf.Abs(tracker.transform.position.z)) {

            while (target > Mathf.Abs(tracker.transform.position.z)) {
                arduino.MoveMotor(60);
                yield return new WaitForSeconds(.01f);
            }
            // Align both walls after real wall is done moving
            MatchWalls();
        }
        // If wall is in front of target.
        else {
            while (target < Mathf.Abs(tracker.transform.position.z)) {
                // Move wall towards target
                arduino.MoveMotor(-60);
                yield return new WaitForSeconds(.01f);
            }
            // Align both walls after real wall is done moving
            MatchWalls();
        }

        wallMoving = false;
    }

    public IEnumerator DummyMovement()
    {
        // Move the wall a bit and then bring it back to where it was

        wallMoving = true;

        float startingPos = tracker.transform.position.z;
        startingPos = Mathf.Abs(startingPos);
        float target = Mathf.Abs(tracker.transform.position.z) + .035f;

        //Debug.Log(startingPos + " start");
        //Debug.Log(target + " target");

        //Debug.Log("Moving back");

        // Now move the wall to the  raget, and once it reaches the target move it back to where it started.
        while (target > Mathf.Abs(tracker.transform.position.z)) {
            arduino.MoveMotor(100);
            yield return new WaitForSeconds(.02f);
        }

        //Debug.Log("Moving forward");

        while (startingPos < Mathf.Abs(tracker.transform.position.z)) {
            arduino.MoveMotor(-100);
            yield return new WaitForSeconds(.02f);
        }

        MatchWalls();
        wallMoving = false;
    }

    // Sets the starting "center" position of the wall/tracker.
    public void SetDefaultPos()
    {
        startingTrackerPos = tracker.transform.position.z;
        startingTrackerPos = Mathf.Abs(startingTrackerPos);

        
        //gameObject.transform.rotation = new Quaternion(tracker.transform.rotation.x, tracker.transform.rotation.y, tracker.transform.rotation.z, tracker.transform.rotation.w);
        gameObject.transform.eulerAngles = new Vector3(tracker.transform.rotation.eulerAngles.x, tracker.transform.rotation.eulerAngles.y, 0);

        //gameObject.transform.rotation.Euler(gameObject.transform.rotation.x, gameObject.transform.rotation.y, 0);
    }

    // Places the virtual wall to match the real wall.
    public void MatchWalls()
    {
        transform.position = new Vector3((tracker.transform.position.x - wallOffsetX), (tracker.transform.position.y - wallOffsetY), (tracker.transform.position.z - wallOffsetZ));
    }
}

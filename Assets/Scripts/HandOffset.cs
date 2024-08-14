using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class HandOffset : PostProcessProvider
{

    [SerializeField]
    private Transform headTransform;

    [SerializeField]
    private float projectionScale = 10f;

    [SerializeField]
    private float handMergeDistance = 0.35f;
    public override void ProcessFrame(ref Frame inputFrame)
    {
        // Calculate the position of the head and the basis to calculate shoulder position.
        if (headTransform == null) {
            headTransform = Camera.main.transform;
        }

        Vector3 headPos = headTransform.position;

        var shoulderBasis = Quaternion.LookRotation(
        Vector3.ProjectOnPlane(headTransform.forward, Vector3.up),
        Vector3.up);

        foreach (var hand in inputFrame.Hands) {
            // Approximate shoulder position with magic values.
            Vector3 shoulderPos = headPos
                                + (shoulderBasis * (new Vector3(0f, -0.13f, -0.1f)
                                + Vector3.left * 0.15f * (hand.IsLeft ? 1f : -1f)));

            // Calculate the projection of the hand if it extends beyond the
            // handMergeDistance.
            Vector3 shoulderToHand = hand.PalmPosition - shoulderPos;
            float handShoulderDist = shoulderToHand.magnitude;
            float projectionDistance = Mathf.Max(0f, handShoulderDist - handMergeDistance);
            float projectionAmount = 1f + (projectionDistance * projectionScale);

            hand.SetTransform(shoulderPos + shoulderToHand * projectionAmount,
                            hand.Rotation);
        }
    }
}
using GoogleARCore;
using UnityEngine;

public class TapToPlaceParent : MonoBehaviour
{
    bool placing = false;

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        // On each Select gesture, toggle whether the user is in placing mode.
        placing = !placing;

        //// If the user is in placing mode, display the spatial mapping mesh.
        //if (placing)
        //{
        //    SpatialMapping.Instance.DrawVisualMeshes = true;
        //}
        //// If the user is not in placing mode, hide the spatial mapping mesh.
        //else
        //{
        //    SpatialMapping.Instance.DrawVisualMeshes = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (placing)
        {
            //// Do a raycast into the world that will only hit the Spatial Mapping mesh.
            //var headPosition = Camera.main.transform.position;
            //var gazeDirection = Camera.main.transform.forward;

            //RaycastHit hitInfo;
            //if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
            //    30.0f, SpatialMapping.PhysicsRaycastMask))
            //{
            //    // Move this object's parent object to
            //    // where the raycast hit the Spatial Mapping mesh.
            //    this.transform.parent.position = hitInfo.point;

            //    // Rotate this object's parent object to face the user.
            //    Quaternion toQuat = Camera.main.transform.localRotation;
            //    toQuat.x = 0;
            //    toQuat.z = 0;
            //    this.transform.parent.rotation = toQuat;
            //}

            // Raycast against the center of the screen to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(.5f, .5f, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    //Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Move this object's parent object to
                    // where the raycast hit the Spatial Mapping mesh.
                    this.transform.parent.position = hit.Pose.position;

                    // Rotate this object's parent object to face the user.
                    Quaternion toQuat = Camera.main.transform.localRotation;
                    toQuat.x = 0;
                    toQuat.z = 0;
                    this.transform.parent.rotation = toQuat;

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make the parent model a child of the anchor.
                    this.transform.parent.parent = anchor.transform;
                }
            }
        }
    }
}
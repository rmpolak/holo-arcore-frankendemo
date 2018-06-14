using UnityEngine;

public class SphereCommands : MonoBehaviour
{

    Vector3 originalPosition;
    bool shouldSphereDrop = false;
    //bool placing = false;

    void Start()
    {
        originalPosition = this.transform.localPosition;
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        shouldSphereDrop = true;
        var rigidbody = this.GetComponent<Rigidbody>();

        // If the sphere has no Rigidbody component, add one to enable physics.
        if (!rigidbody)
        {
            rigidbody = this.gameObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else
        {
            //rigidbody.isKinematic = true;
            //Destroy(this.gameObject.GetComponent<Rigidbody>());

            //// Placing! Probably conflicts with other objects that use the same logic.
            //placing = !placing;

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
    }

    private void Update()
    {
        if (!this.GetComponent<Rigidbody>() && shouldSphereDrop)
        {
            var position = this.gameObject.transform.position;
            //this.gameObject.transform.position = new Vector3(position.x, position.y + .1f, position.z);
            this.gameObject.transform.Translate(new Vector3(0, .01f, 0), Space.World);
        }

        //// If the user is in placing mode,
        //// update the placement to match the user's gaze.

        //if (placing)
        //{
        //    // Do a raycast into the world (that will hit all (collision?) geometry).
        //    var headPosition = Camera.main.transform.position;
        //    var gazeDirection = Camera.main.transform.forward;

        //    RaycastHit hitInfo;
        //    if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
        //        30.0f))
        //    {
        //        // Move this object to where the raycast hit the Spatial Mapping mesh.
        //        this.transform.position = hitInfo.point;
                
        //        // Rotate this object to face the user.
        //        Quaternion toQuat = Camera.main.transform.localRotation;
        //        toQuat.x = 0;
        //        toQuat.z = 0;
        //        this.transform.rotation = toQuat;
        //    }
        //}
    }

    // Called by SpeechManager when the user says the "Reset world" command
    void OnReset()
    {
        shouldSphereDrop = false;
        //placing = false;
        // If the sphere has a Rigidbody component, remove it to disable physics.
        var rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            Destroy(rigidbody);
        }

        // Put the sphere back into its original local position.
        this.transform.localPosition = originalPosition;
    }

    // Called by SpeechManager when the user says the "Drop sphere" command
    void OnDrop()
    {
        // Just do the same logic as a Select gesture.
        OnSelect();
    }
}
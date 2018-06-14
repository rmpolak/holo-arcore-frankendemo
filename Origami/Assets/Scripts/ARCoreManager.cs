using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

/// <summary>
/// Controls the HelloAR example.
/// </summary>
public class ARCoreManager : MonoBehaviour
{
	/// <summary>
	/// The first-person camera being used to render the passthrough camera image (i.e. AR background).
	/// </summary>
	public Camera FirstPersonCamera;

	/// <summary>
	/// A prefab for tracking and visualizing detected planes.
	/// </summary>
	public GameObject DetectedPlanePrefab;

    // Last position hit with a raycast.
    public TrackableHit lastHit;

    // Last position in the real-world environment hit with a raycast.
    public TrackableHit lastEnvironmentHit;

	/// <summary>
	/// The rotation in degrees need to apply to model when the Andy model is placed.
	/// </summary>
	private const float k_ModelRotation = 180.0f;

	/// <summary>
	/// A list to hold all planes ARCore is tracking in the current frame. This object is used across
	/// the application to avoid per-frame allocations.
	/// </summary>
	private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

	/// <summary>
	/// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
	/// </summary>
	private bool m_IsQuitting = false;
    
	/// <summary>
	/// The Unity Update() method.
	/// </summary>
	public void Update()
	{
		_UpdateApplicationLifecycle();

		// If the player has not touched the screen, we are done with this update.
		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

        // Check whether a game object was selected.
        // Do a raycast into the world based on tap position.
        var tapRay = Camera.main.ScreenPointToRay(touch.position);
        //var tapRay = Camera.main.ScreenPointToRay(
        //    new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));

        RaycastHit hitInfo;

        if (Physics.Raycast(tapRay, out hitInfo))
        {
            // Let the object that was hit know it was selected.
            var hitObject = hitInfo.collider.gameObject;
            //if(hitObject is DetectedPlane)
            //{}
            hitObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
			TrackableHitFlags.FeaturePointWithSurfaceNormal;

		if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
		{
			// Use hit pose and camera pose to check if hittest is from the
			// back of the plane, if it is, no need to create the anchor.
			if ((hit.Trackable is DetectedPlane) &&
				Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
					hit.Pose.rotation * Vector3.up) < 0)
			{
				Debug.Log("Hit at back of the current DetectedPlane");
			}
			else
			{
                lastEnvironmentHit = hit;
                
				//// Instantiate Andy model at the hit pose.
				//var andyObject = Instantiate(AndyAndroidPrefab, hit.Pose.position, hit.Pose.rotation);

				//// Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
				//andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

				//// Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
				//// world evolves.
				//var anchor = hit.Trackable.CreateAnchor(hit.Pose);

				//// Make Andy model a child of the anchor.
				//andyObject.transform.parent = anchor.transform;
			}
		}
	}

	/// <summary>
	/// Check and update the application lifecycle.
	/// </summary>
	private void _UpdateApplicationLifecycle()
	{
		// Exit the app when the 'back' button is pressed.
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}

		// Only allow the screen to sleep when not tracking.
		if (Session.Status != SessionStatus.Tracking)
		{
			const int lostTrackingSleepTimeout = 15;
			Screen.sleepTimeout = lostTrackingSleepTimeout;
		}
		else
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		if (m_IsQuitting)
		{
			return;
		}

		// Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
		if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
		{
			_ShowAndroidToastMessage("Camera permission is needed to run this application.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
		else if (Session.Status.IsError())
		{
			_ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
	}

	/// <summary>
	/// Actually quit the application.
	/// </summary>
	private void _DoQuit()
	{
		Application.Quit();
	}

	/// <summary>
	/// Show an Android toast message.
	/// </summary>
	/// <param name="message">Message string to show in the toast.</param>
	private void _ShowAndroidToastMessage(string message)
	{
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null)
		{
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
			{
				AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
					message, 0);
				toastObject.Call("show");
			}));
		}
	}
}

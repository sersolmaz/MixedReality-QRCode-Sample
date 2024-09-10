using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

public class SpatialMeshController : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;

    private void Start()
    {


        // Get the first Mesh Observer available
        meshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (meshObserver == null)
        {
            Debug.LogError("Spatial mesh observer not found.");
        }

    }

    public void ToggleSpatialMeshOn()
    {
        if (meshObserver != null)
        {

            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
            Debug.Log("Occlude spatial mesh.");

        }
    }

    public void ToggleSpatialMeshVisible()
    {
        if (meshObserver != null)
        {
 
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
            Debug.Log("Visible spatial mesh.");
        }
    }

    public void ToggleSpatialMeshOff()
    {
        if (meshObserver != null)
        {

            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
            Debug.Log("Non-visible spatial mesh.");
        }

    }

    public void ToggleSpatialMeshResume()
    {
      
  
        meshObserver.Resume();
        CoreServices.SpatialAwarenessSystem.ResumeObservers();
        Debug.Log("Resume spatial mesh.");

    }


    public void ToggleSpatialMeshSuspended()
    {

        meshObserver.Suspend();
        CoreServices.SpatialAwarenessSystem.SuspendObservers();
        Debug.Log("Suspend spatial mesh.");

    }



}

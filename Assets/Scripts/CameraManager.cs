using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObject<CameraManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CameraManager");
                    instance = obj.AddComponent<CameraManager>();
                }
            }
            return instance;
        }
    }

    public void ReparentCamera(Transform cameraTransform)
    {
        cameraTransform.SetParent(this.transform);
    }

    private static T FindFirstObject<T>() where T : Object
    {
        return Object.FindFirstObjectByType<T>();
    }

    private static T[] FindAllObjects<T>() where T : Object
    {
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
    }
}
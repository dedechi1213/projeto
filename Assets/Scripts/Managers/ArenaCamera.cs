using UnityEngine;
[ExecuteAlways, RequireComponent(typeof(Camera))]
public class ArenaCamera : MonoBehaviour
{
    public float MinimumHalfHeight = 8.8f;
    public float MinimumHalfWidth = 14.8f;
    Camera view;
    void OnEnable() { Fit(); }
    void LateUpdate() { Fit(); }
    void OnPreCull() { Fit(); }
    public void Fit() {
        if(!view) view = GetComponent<Camera>();
        view.orthographic = true;
        view.orthographicSize = Mathf.Max(MinimumHalfHeight, MinimumHalfWidth / Mathf.Max(.1f,view.aspect));
    }
}


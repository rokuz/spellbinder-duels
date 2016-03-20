using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class HideInEditor : MonoBehaviour
{
    public bool visible = true;

    void Update()
    {
        if (Application.isEditor)
        {
            bool vis = visible || Application.isPlaying;
            CanvasRenderer canvasRenderer = gameObject.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
                canvasRenderer.SetAlpha(vis ? 1.0f : 0.0f);
            var canvasRenderers = gameObject.GetComponentsInChildren<CanvasRenderer>();
            foreach (CanvasRenderer r in canvasRenderers)
                r.SetAlpha(vis ? 1.0f : 0.0f);
        }
    }
}

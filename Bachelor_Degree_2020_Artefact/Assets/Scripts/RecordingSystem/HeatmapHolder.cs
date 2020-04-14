using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHolder : MonoBehaviour
{
    private List<MeshRenderer> childrenRenderers = new List<MeshRenderer>();

    public void SetColor(Color colorToSet)
    {
        foreach(MeshRenderer ren in childrenRenderers)
        {
            ren.material.color = colorToSet;
            ren.material.SetColor("_EmissionColor", colorToSet);
        }
    }

    public void UpdateRendererList()
    {
        childrenRenderers.Clear();
        foreach(Transform child in transform)
        {
            MeshRenderer ren = child.GetComponent<MeshRenderer>();
            if(ren != null)
            {
                childrenRenderers.Add(ren);
            }
        }
    }

    public void ClearHeatmap()
    {
        childrenRenderers.Clear();
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

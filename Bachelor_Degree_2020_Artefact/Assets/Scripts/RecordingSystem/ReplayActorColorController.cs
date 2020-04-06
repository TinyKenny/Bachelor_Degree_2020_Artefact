using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayActorColorController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color colorToSet)
    {
        if(meshRenderer != null)
        {
            meshRenderer.material.color = colorToSet;
        }
    }
}

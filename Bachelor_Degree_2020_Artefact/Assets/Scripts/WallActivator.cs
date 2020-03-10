using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WallActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] walls;
    

    private BoxCollider coll;
    private bool hasActivated = false;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TriggerEntered!");
            SetWallsActive(true);
        }
    }

    private void Awake()
    {
        SetWallsActive(false);
    }

    private void SetWallsActive(bool setActive)
    {
        foreach(GameObject gO in walls)
        {
            if (gameObject.Equals(gO) == false)
            {
                gO.SetActive(setActive);
            }
        }
    }
}

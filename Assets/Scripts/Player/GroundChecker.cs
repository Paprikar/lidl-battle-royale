using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField]
    private LayerMask Ground;
    public bool isGrounded = true;


    public List<int> collList = new List<int>(); // Debugging


    void OnTriggerEnter(Collider other)
    {
        if(!collList.Contains(other.gameObject.GetInstanceID()) && Ground == (Ground | 1 << other.gameObject.layer))
        {
            collList.Add(other.gameObject.GetInstanceID());

            if (!isGrounded)
            {
                isGrounded = true;
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (collList.Contains(other.gameObject.GetInstanceID()) && Ground == (Ground | 1 << other.gameObject.layer))
        {
            collList.Remove(other.gameObject.GetInstanceID());

            if (collList.Count == 0)
            {
                isGrounded = false;
            }
        }
    }
}

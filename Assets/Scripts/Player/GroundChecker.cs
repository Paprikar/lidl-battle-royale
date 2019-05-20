using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField]
    private LayerMask Ground;
    public bool isGrounded = true;


    public List<Collider> collList = new List<Collider>(); // Debugging


    void OnTriggerEnter(Collider other)
    {
        if(!collList.Contains(other) && Ground == (Ground | 1 << other.gameObject.layer))
        {
            collList.Add(other);
            isGrounded = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (collList.Contains(other) && Ground == (Ground | 1 << other.gameObject.layer))
        {
            collList.Remove(other);

            if (collList.Count == 0)
            {
                isGrounded = false;
            }
        }
    }
}

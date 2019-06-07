using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LidlBattleRoyale
{
    public class VelocityTracker : MonoBehaviour
    {
        public Vector3 velocity { get { return GetVelocity(); } }


        Vector3 m_PreviousPosition;
        Vector3 m_CurrentPosition;


        void Awake()
        {
            m_PreviousPosition = transform.position;
            m_CurrentPosition = transform.position;
        }


        void FixedUpdate()
        {
            m_PreviousPosition = m_CurrentPosition;
            m_CurrentPosition = transform.position;
        }


        Vector3 GetVelocity()
        {
            return (m_CurrentPosition - m_PreviousPosition) / Time.fixedDeltaTime;
        }
    }
}

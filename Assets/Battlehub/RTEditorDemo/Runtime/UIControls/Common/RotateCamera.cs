using UnityEngine;
using System.Collections;

namespace Battlehub.UIControls.Common.Demo
{
    public class RotateCamera : MonoBehaviour
    {
        private Vector3 m_rand;
        private float m_prevT;
        
        private void Start()
        {
            m_rand = Random.onUnitSphere;
        }

        private void Update()
        {
            if (Time.time - m_prevT > 10.0f)
            {
                m_rand = Random.onUnitSphere;
                m_prevT = Time.time;
            }

            transform.rotation *= Quaternion.AngleAxis(4 * Mathf.PI * Time.deltaTime, m_rand);
        }
    }
}


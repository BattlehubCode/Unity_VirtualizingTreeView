using System;
using UnityEngine;

namespace Battlehub.UIControls
{
    public class Repeater 
    {
        private float m_firstDelay;
        private float m_delay;

        private float m_nextT;
        private Action m_callback;

        public Repeater(float t, float initDelay, float firstDelay, float delay, Action callback)
        {
            m_nextT = t + initDelay;
            m_firstDelay = firstDelay;
            m_delay = delay;
            m_callback = callback;
        }

        public void Repeat(float t)
        {
            if(t >= m_nextT)
            {
                m_callback();
                if(m_firstDelay > 0)
                {
                    m_nextT += m_firstDelay;
                    m_firstDelay = 0;
                }
                else
                {
                    m_nextT += m_delay;
                }
            }
        }
    }
}


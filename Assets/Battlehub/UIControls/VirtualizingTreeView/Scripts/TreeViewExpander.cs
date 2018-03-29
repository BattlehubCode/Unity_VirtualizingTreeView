using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    [RequireComponent(typeof(Toggle))]
    public class TreeViewExpander : MonoBehaviour
    {
        public Graphic OffGraphic;
        private Toggle m_toggle;

        private bool m_canExpand;
        public bool CanExpand
        {
            get { return m_canExpand; }
            set
            {
                m_canExpand = value;
                if(!m_canExpand)
                {
                    if(m_toggle != null)
                    {
                        m_toggle.isOn = false;
                        m_toggle.enabled = false;
                    }
                    OffGraphic.enabled = false;
                }
                else
                {
                    if (m_toggle != null)
                    {
                        m_toggle.enabled = true;
                        if(!IsOn)
                        {
                            OffGraphic.enabled = true;
                        }
                    }
                }
            }
        }
        
        public bool IsOn
        {
            get { return m_toggle.isOn; }
            set
            {
                m_toggle.isOn = value && m_canExpand;   
            }
        }

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
            if(!m_canExpand)
            {
                m_toggle.isOn = false;
                m_toggle.enabled = false;
            }
            if(OffGraphic != null)
            {
                OffGraphic.enabled = !m_toggle.isOn && m_canExpand;
            }
            
            m_toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnEnable()
        {
            if(m_toggle != null)
            {
                if (OffGraphic != null)
                {
                    OffGraphic.enabled = !m_toggle.isOn && m_canExpand;
                }

                if (!m_canExpand)
                {
                    m_toggle.onValueChanged.RemoveListener(OnValueChanged);
                    m_toggle.isOn = true;
                    m_toggle.isOn = false;
                    m_toggle.onValueChanged.AddListener(OnValueChanged);
                    m_toggle.enabled = false;
                    
                }
            }
           
        }

        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnValueChanged(bool value)
        {
            if (!m_canExpand)
            {
                m_toggle.isOn = false;
                m_toggle.enabled = false;
            }
            if (OffGraphic != null)
            {
                OffGraphic.enabled = !value && m_canExpand;
            }
            
        }
    }
}

using System.Collections;
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
                UpdateState();
            }
        }

        public bool IsOn
        {
            get { return m_toggle.isOn; }
            set
            {
                m_toggle.isOn = value;
            }
        }

        private bool m_started;
        private void UpdateState()
        {
            if (m_started)
            {
                DoUpdateState();
            }
            else
            {
                StartCoroutine(CoUpdateState());
            }
        }

        private IEnumerator CoUpdateState()
        {
            yield return new WaitForEndOfFrame();
            DoUpdateState();
        }

        private void DoUpdateState()
        {
            if (CanExpand)
            {
                m_toggle.interactable = true;
                //m_toggle.enabled = true;

                if (IsOn)
                {
                    if (OffGraphic != null)
                    {
                        OffGraphic.enabled = false;
                    }
                }
                else
                {
                    if (OffGraphic != null)
                    {
                        OffGraphic.enabled = true;
                    }
                }
            }
            else
            {
                if (m_toggle != null)
                {
                    m_toggle.interactable = false;
                    //m_toggle.enabled = false;
                }
                if (OffGraphic != null)
                {
                    OffGraphic.enabled = false;
                }
            }
        }

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
            if (OffGraphic != null)
            {
                OffGraphic.enabled = false;
            }
            UpdateState();

            m_toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void Start()
        {
            m_started = true;
        }

        private void OnEnable()
        {
            if (m_toggle != null)
            {
                UpdateState();
            }

        }

        private void OnDestroy()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnValueChanged(bool value)
        {
            UpdateState();
        }
    }
}

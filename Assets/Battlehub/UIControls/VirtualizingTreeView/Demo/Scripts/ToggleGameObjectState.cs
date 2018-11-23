using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.UIControls
{
    public class ToggleGameObjectState : MonoBehaviour
    {

        private VirtualizingTreeViewItem m_treeViewItem;

        public bool State
        {
            get { return ((GameObject)m_treeViewItem.Item).activeSelf; }
            set { ((GameObject)m_treeViewItem.Item).SetActive(value); }
        }

        void Start()
        {
            m_treeViewItem = GetComponentInParent<VirtualizingTreeViewItem>();
        }
    }

}


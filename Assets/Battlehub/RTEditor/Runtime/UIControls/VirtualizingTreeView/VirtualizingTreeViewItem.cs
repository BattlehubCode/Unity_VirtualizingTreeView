using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{ 
    public class VirtualizingTreeViewItem : VirtualizingItemContainer
    {
        private TreeViewExpander m_expander;
        [SerializeField]
        private HorizontalLayoutGroup m_itemLayout = null;

        private Toggle m_toggle;
       
        private VirtualizingTreeView TreeView
        {
            get { return ItemsControl as VirtualizingTreeView; }
        }

        public float Indent
        {
            get { return m_treeViewItemData.Indent; }
        }

        public override object Item
        {
            get { return base.Item; }
            set
            {
                base.Item = value;

            
                m_treeViewItemData = (TreeViewItemContainerData)TreeView.GetItemContainerData(value);
                if (m_treeViewItemData == null)
                {
                    m_treeViewItemData = new TreeViewItemContainerData();
                    name = "Null";
                    return;
                }
                else
                {

                    UpdateIndent();
                    if (m_expander != null)
                    {
                        m_expander.CanExpand = m_treeViewItemData.CanExpand;
                        m_expander.IsOn = m_treeViewItemData.IsExpanded && m_treeViewItemData.CanExpand;
                    }

                    name = base.Item.ToString() + " " + m_treeViewItemData.ToString();
                }
            }
        }

        private TreeViewItemContainerData m_treeViewItemData;
        public TreeViewItemContainerData TreeViewItemData
        {
            get { return m_treeViewItemData; }
        }

        /// <summary>
        /// Parent TreeViewItem
        /// </summary>
        public TreeViewItemContainerData Parent
        {
            get { return m_treeViewItemData != null ? m_treeViewItemData.Parent : null; }
            set
            {
                if(m_treeViewItemData == null)
                {
                    return;
                }
                if (m_treeViewItemData.Parent == value)
                {
                    return;
                }

                
                m_treeViewItemData.Parent = value;             
                UpdateIndent();
            }
        }

        public void UpdateIndent()
        {
            if (Parent != null && TreeView != null && m_itemLayout != null)
            {
                m_treeViewItemData.Indent = Parent.Indent + TreeView.Indent;
                m_itemLayout.padding = new RectOffset(
                    m_treeViewItemData.Indent,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);

                int itemIndex = TreeView.IndexOf(Item);
                SetIndent(this, ref itemIndex);
            }
            else
            {
                ZeroIndent();
                int itemIndex = TreeView.IndexOf(Item);
                if(HasChildren)
                {
                    SetIndent(this, ref itemIndex);
                }   
            }
        }

        private void ZeroIndent()
        {
            if(m_treeViewItemData != null)
            {
                m_treeViewItemData.Indent = 0;
            }

            if (m_itemLayout != null)
            {
                m_itemLayout.padding = new RectOffset(
                    0,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);
            }
        }

        private void SetIndent(VirtualizingTreeViewItem parent, ref int itemIndex)
        {
            while (true)
            {
                object obj = TreeView.GetItemAt(itemIndex + 1);
                VirtualizingTreeViewItem child = (VirtualizingTreeViewItem)TreeView.GetItemContainer(obj);
                if (child == null)
                {
                    return;
                }

                if(child.Item == null)
                {
                    return;
                }

                if (child.Parent != parent.m_treeViewItemData)
                {
                    return;
                }

                child.m_treeViewItemData.Indent = parent.m_treeViewItemData.Indent + TreeView.Indent;
                if(child.m_itemLayout != null && child.m_itemLayout.padding != null)
                {
                    child.m_itemLayout.padding.left = child.m_treeViewItemData.Indent;
                }
                
                itemIndex++;
                SetIndent(child, ref itemIndex);
            }
        }

        /// <summary>
        /// Select or unselect TreeViewItem (this property is driven by tree view. If you set this property you might not get desired results)
        /// </summary>
        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if (base.IsSelected != value)
                {
                    if(m_toggle != null)
                    {
                        m_toggle.isOn = value;
                    }
                    base.IsSelected = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether item can be expanded (this property is driven by tree view. If you set this property you might not get desired results)
        /// </summary>
        public bool CanExpand
        {
            get { return m_treeViewItemData == null ? false : m_treeViewItemData.CanExpand; }
            set
            {
                if(m_treeViewItemData == null)
                {
                    return;
                }
                if (m_treeViewItemData.CanExpand != value)
                {
                    m_treeViewItemData.CanExpand = value;
                    if (m_expander != null)
                    {
                        m_expander.CanExpand = m_treeViewItemData.CanExpand;
                    }
                    if (!m_treeViewItemData.CanExpand)
                    {
                        IsExpanded = false;
                    }
                }
            }
        }

        /// <summary>
        /// Expand or Collapse treeview item
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                if (m_treeViewItemData == null)
                {
                    return false;
                }
                return m_treeViewItemData.IsExpanded;
            }
            set
            {
                if(m_treeViewItemData == null)
                {
                    return;
                }

                if (m_treeViewItemData.IsExpanded != value)
                {
                    if (m_expander != null)
                    {
                        m_expander.IsOn = value && CanExpand;
                    }
                    if (TreeView != null)
                    {
                        if (value && CanExpand)
                        {
                            TreeView.Internal_Expand(m_treeViewItemData.Item);
                        }
                        else
                        {
                            TreeView.Internal_Collapse(m_treeViewItemData.Item);
                        }
                    }
                }
            }
        }

        public bool HasChildren
        {
            get
            {
                if (m_treeViewItemData == null)
                {
                    return false;
                }
                return m_treeViewItemData.HasChildren(TreeView);
            }
        }

      

        /// <summary>
        /// Get First Child of TreeViewItem
        /// </summary>
        /// <returns>First Child</returns>
        public TreeViewItemContainerData FirstChild()
        {
            return m_treeViewItemData.FirstChild(TreeView);
        }


        /// <summary>
        /// Get Next Child Of TreeViewItem
        /// </summary>
        /// <param name="currentChild"></param>
        /// <returns>next treeview item after current child</returns>
        public TreeViewItemContainerData NextChild(TreeViewItemContainerData currentChild)
        {
            return m_treeViewItemData.NextChild(TreeView, currentChild);
        }

        /// <summary>
        /// Get Last Child of TreeViewItem
        /// </summary>
        /// <returns>Last Child</returns>
        public TreeViewItemContainerData LastChild()
        {
            return m_treeViewItemData.LastChild(TreeView);
        }

        /// <summary>
        /// Get Last Descendant Of TreeViewItem
        /// </summary>
        /// <returns>Last Descendant</returns>
        public TreeViewItemContainerData LastDescendant()
        {
            return m_treeViewItemData.LastDescendant(TreeView);
        }

        protected override void AwakeOverride()
        {
            m_toggle = GetComponent<Toggle>();
            m_toggle.interactable = false;
            m_toggle.isOn = IsSelected;

            m_expander = GetComponentInChildren<TreeViewExpander>();
            if (m_expander != null)
            {
                m_expander.CanExpand = CanExpand;
            }
        }

        protected override void StartOverride()
        {
            if (TreeView != null)
            {
                m_toggle.isOn = TreeView.IsItemSelected(Item);
                m_isSelected = m_toggle.isOn;
            }

            if (Parent != null)
            {
                m_treeViewItemData.Indent = Parent.Indent + TreeView.Indent;
                m_itemLayout.padding = new RectOffset(
                    m_treeViewItemData.Indent,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);
            }
            
            if (CanExpand && TreeView.AutoExpand)
            {
                IsExpanded = true;
            }
        }

        private void LateUpdate()
        {
            /// This is dirty fix of wrong selection after databinding (in runtime editor such buggy behavior can be reproduced by filtering hierarchy)
            /// TODO: replace with something better
            if (IsSelected != m_treeViewItemData.IsSelected)
            {
                IsSelected = m_treeViewItemData.IsSelected;
            }
        }
    }
}

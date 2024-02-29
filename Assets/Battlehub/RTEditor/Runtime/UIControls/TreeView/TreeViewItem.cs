using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class ParentChangedEventArgs : EventArgs
    {
        public TreeViewItem OldParent
        {
            get;
            private set;
        }

        public TreeViewItem NewParent
        {
            get;
            private set;
        }

        public ParentChangedEventArgs(TreeViewItem oldParent, TreeViewItem newParent)
        {
            OldParent = oldParent;
            NewParent = newParent;
        }
    }

    public class TreeViewItem : ItemContainer
    {
        public static event EventHandler<ParentChangedEventArgs> ParentChanged;

        private TreeViewExpander m_expander;
        [SerializeField]
        private HorizontalLayoutGroup m_itemLayout = null;

        private Toggle m_toggle;
        private TreeView m_treeView;
        private TreeView TreeView
        {
            get
            {
                if(m_treeView == null)
                {
                    m_treeView = GetComponentInParent<TreeView>();
                }

                return m_treeView;
            }
        }
        private int m_indent;

        /// <summary>
        /// Get TreeViewItem absolute indent
        /// </summary>
        public int Indent
        {
            get { return m_indent; }
        }

        private TreeViewItem m_parent;
        /// <summary>
        /// Parent TreeViewItem
        /// </summary>
        public TreeViewItem Parent
        {
            get { return m_parent; }
            set
            {
                if(m_parent == value)
                {
                    return;
                }

                TreeViewItem oldParent = m_parent;
                m_parent = value;
                if(m_parent != null && TreeView != null && m_itemLayout != null)
                {
                    m_indent = m_parent.m_indent + TreeView.Indent;
                    m_itemLayout.padding = new RectOffset(
                        m_indent,
                        m_itemLayout.padding.right,
                        m_itemLayout.padding.top,
                        m_itemLayout.padding.bottom);

                    int siblingIndex = transform.GetSiblingIndex();
                    SetIndent(this, ref siblingIndex);
                }
                else
                {
                    ZeroIndent();
                }

                if (TreeView != null)
                {
                    if (ParentChanged != null)
                    {
                        ParentChanged(this, new ParentChangedEventArgs(oldParent, m_parent));
                    }
                } 
            }
        }


        public void UpdateIndent()
        {
            if (m_parent != null && TreeView != null && m_itemLayout != null)
            {
                m_indent = m_parent.m_indent + TreeView.Indent;
                m_itemLayout.padding = new RectOffset(
                    m_indent,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);

                int siblingIndex = transform.GetSiblingIndex();
                SetIndent(this, ref siblingIndex);
            }
            else
            {
                ZeroIndent();
            }
        }

        private void ZeroIndent()
        {
            m_indent = 0;
            if (m_itemLayout != null)
            {
                m_itemLayout.padding = new RectOffset(
                    m_indent,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);
            }
        }

        private void SetIndent(TreeViewItem parent, ref int siblingIndex)
        {
            while(true)
            {   
                TreeViewItem child = (TreeViewItem)TreeView.GetItemContainer(siblingIndex + 1);
                if(child == null)
                {
                    return;
                }

                if(child.Parent != parent)
                {
                    return;
                }

                child.m_indent = parent.m_indent + TreeView.Indent;
                child.m_itemLayout.padding.left = child.m_indent;

                siblingIndex++;
                SetIndent(child, ref siblingIndex);
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
                    m_toggle.isOn = value;
                    base.IsSelected = value;
                }
            }
        }

        private bool m_canExpand;

        /// <summary>
        /// Gets or sets whether item can be expanded (this property is driven by tree view. If you set this property you might not get desired results)
        /// </summary>
        public bool CanExpand
        {
            get { return m_canExpand; }
            set
            {
                if(m_canExpand != value)
                {
                    m_canExpand = value;
                    if(m_expander != null)
                    {
                        m_expander.CanExpand = m_canExpand;
                    }
                    if (!m_canExpand)
                    {
                        IsExpanded = false;
                    }
                }
            }
        }

        private bool m_isExpanded;
        /// <summary>
        /// Expand or Collapse treeview item
        /// </summary>
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if(m_isExpanded != value)
                {
                    m_isExpanded = value && m_canExpand;
                    if(m_expander != null)
                    {
                        m_expander.IsOn = value && m_canExpand;
                    }
                    if(TreeView != null)
                    {
                        if (m_isExpanded)
                        {
                            TreeView.Expand(this);
                        }
                        else
                        {
                            TreeView.Collapse(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if TreeViewItem has children. DO NOT USE THIS PROPERTY DURING DRAG&DROP OPERATION
        /// </summary>
        public bool HasChildren
        {
            get
            {
                int index = transform.GetSiblingIndex();
                if(TreeView == null)
                {
                    return false;
                }

                TreeViewItem nextItem = (TreeViewItem)TreeView.GetItemContainer(index + 1);
                return nextItem != null && nextItem.Parent == this;
            }
        }

        /// <summary>
        /// Is treeviewitem is descendant of another element
        /// </summary>
        /// <param name="ancestor">treeview item to test</param>
        /// <returns>true if treeview item is descendant of ancestor</returns>
        public bool IsDescendantOf(TreeViewItem ancestor)
        {
            if (ancestor == null)
            {
                return true;
            }

            if(ancestor == this)
            {
                return false;
            }

            TreeViewItem testItem = this;
            while (testItem != null)
            {
                if (ancestor == testItem)
                {
                    return true;
                }

                testItem = testItem.Parent;
            }

            return false;
        }

        /// <summary>
        /// Get First Child of TreeViewItem
        /// </summary>
        /// <returns>First Child</returns>
        public TreeViewItem FirstChild()
        {
            if (!HasChildren)
            {
                return null;
            }

            int siblingIndex = transform.GetSiblingIndex();
            siblingIndex++;
            TreeViewItem child = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);

            Debug.Assert(child != null && child.Parent == this);

            return child;
        }

        /// <summary>
        /// Get Next Child Of TreeViewItem
        /// </summary>
        /// <param name="currentChild"></param>
        /// <returns>next treeview item after current child</returns>
        public TreeViewItem NextChild(TreeViewItem currentChild)
        {
            if(currentChild == null)
            {
                throw new ArgumentNullException("currentChild");
            }

            int siblingIndex = currentChild.transform.GetSiblingIndex();
            siblingIndex++;
            TreeViewItem nextChild = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
            while(nextChild != null && nextChild.IsDescendantOf(this))
            {
                if(nextChild.Parent == this)
                {
                    return nextChild;
                }

                siblingIndex++;
                nextChild = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
            }

            return null;
        }

        /// <summary>
        /// Get Last Child of TreeViewItem
        /// </summary>
        /// <returns>Last Child</returns>
        public TreeViewItem LastChild()
        {
            if (!HasChildren)
            {
                return null;
            }

            int siblingIndex = transform.GetSiblingIndex();

            TreeViewItem lastChild = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItem child = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
                if (child == null || !child.IsDescendantOf(this))
                {
                    return lastChild;
                }

                if (child.Parent == this)
                {
                    lastChild = child;
                }
            }
        }

        /// <summary>
        /// Get Last Descendant Of TreeViewItem
        /// </summary>
        /// <returns>Last Descendant</returns>
        public TreeViewItem LastDescendant()
        {
            if (!HasChildren)
            {
                return null;
            }

            int siblingIndex = transform.GetSiblingIndex();
            TreeViewItem lastDescendant = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItem child = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
                if (child == null || !child.IsDescendantOf(this)) 
                {
                    return lastDescendant;
                }

                lastDescendant = child;
            }
        }

        protected override void AwakeOverride()
        {
            m_toggle = GetComponent<Toggle>();
            m_toggle.interactable = false;
            m_toggle.isOn = IsSelected;

            m_expander = GetComponentInChildren<TreeViewExpander>();
            if (m_expander != null)
            {
                m_expander.CanExpand = m_canExpand;
            }
        }

        protected override void StartOverride()
        {
            if (TreeView != null)
            {
                //if (IsExpanded)
                //{
                //    TreeView.Expand(this);
                //}
                m_toggle.isOn = TreeView.IsItemSelected(Item);
                m_isSelected = m_toggle.isOn;
            }
            
            if (Parent != null)
            {
                m_indent = Parent.m_indent + TreeView.Indent;
                m_itemLayout.padding = new RectOffset(
                    m_indent,
                    m_itemLayout.padding.right,
                    m_itemLayout.padding.top,
                    m_itemLayout.padding.bottom);
            }

            if (CanExpand && TreeView.AutoExpand)
            {
                IsExpanded = true;
            }
        }

        public override void Clear()
        {
            base.Clear();
            m_parent = null;
            ZeroIndent();
            m_isSelected = false;
            m_toggle.isOn = m_isSelected;
            m_isExpanded = false;
            m_canExpand = false;
            m_expander.IsOn = false;
            m_expander.CanExpand = false;
          
        }
    }
}

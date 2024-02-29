using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.UIControls
{
    /// <summary>
    /// Data Item Expanding event arguments
    /// </summary>
    public class VirtualizingItemExpandingArgs : EventArgs
    {
        /// <summary>
        /// Data Item
        /// </summary>
        public object Item
        {
            get;
            private set;
        }

        /// <summary>
        /// Specify item's children using this property
        /// </summary>
        public IEnumerable Children
        {
            get;
            set;
        }

        public VirtualizingItemExpandingArgs(object item)
        {
            Item = item;
        }
    }


    public class VirtualizingItemCollapsedArgs : EventArgs
    {
        public object Item
        {
            get;
            private set;
        }
        public VirtualizingItemCollapsedArgs(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// TreeView data binding event arguments
    /// </summary>
    public class VirtualizingTreeViewItemDataBindingArgs : ItemDataBindingArgs
    {
        /// <summary>
        /// Set to true if data bound item has children
        /// </summary>
        public bool HasChildren
        {
            get;
            set;
        }
    }

    public class VirtualizingParentChangedEventArgs : EventArgs
    {
        public TreeViewItemContainerData OldParent
        {
            get;
            private set;
        }

        public TreeViewItemContainerData NewParent
        {
            get;
            private set;
        }

        public VirtualizingParentChangedEventArgs(TreeViewItemContainerData oldParent, TreeViewItemContainerData newParent)
        {
            OldParent = oldParent;
            NewParent = newParent;
        }
    }

    public class TreeViewItemContainerData : ItemContainerData
    {
        public static event EventHandler<VirtualizingParentChangedEventArgs> ParentChanging;
        public static event EventHandler<VirtualizingParentChangedEventArgs> ParentChanged;
        public static bool Internal_RaiseEvents = true;

        private TreeViewItemContainerData m_parent;
        public TreeViewItemContainerData Parent
        {
            get { return m_parent; }
            set
            {
                if(m_parent == value)
                {
                    return;
                }

                if(Internal_RaiseEvents)
                {
                    if (ParentChanging != null)
                    {
                        ParentChanging(this, new VirtualizingParentChangedEventArgs(m_parent, value));
                    }
                }
               

                TreeViewItemContainerData oldParent = m_parent;
                m_parent = value;

                if(Internal_RaiseEvents)
                {
                    if (ParentChanged != null)
                    {
                        ParentChanged(this, new VirtualizingParentChangedEventArgs(oldParent, m_parent));
                    }
                }
            }
        }


        public object ParentItem
        {
            get
            {
                if(m_parent == null)
                {
                    return null;
                }
                return m_parent.Item;
            }
        }

        /// <summary>
        /// Get TreeViewItem absolute indent
        /// </summary>
        public int Indent
        {
            get;
            set;
        }

        /// <summary>
        /// Can Expand TreeView item ?
        /// </summary>
        public bool CanExpand
        {
            get;
            set;
        }

        /// <summary>
        /// Is TreeViewItem expanded ?
        /// </summary>
        public bool IsExpanded
        {
            get;
            set;
        }

        /// <summary>
        /// Is treeviewitem is descendant of another element
        /// </summary>
        /// <param name="ancestor">treeview item to test</param>
        /// <returns>true if treeview item is descendant of ancestor</returns>
        public bool IsDescendantOf(TreeViewItemContainerData ancestor)
        {
            if (ancestor == null)
            {
                return true;
            }

            if (ancestor == this)
            {
                return false;
            }

            TreeViewItemContainerData testItem = this;
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
        /// Returns true if TreeViewItem has children. DO NOT USE THIS PROPERTY DURING DRAG&DROP OPERATION
        /// </summary>
        public bool HasChildren(VirtualizingTreeView treeView)
        {
            if(treeView == null)
            {
                return false;
            }
            int index = treeView.IndexOf(Item);

            TreeViewItemContainerData nextItem = (TreeViewItemContainerData)treeView.GetItemContainerData(index + 1);
            return nextItem != null && nextItem.Parent == this;
        }



        /// <summary>
        /// Get First Child of TreeViewItem
        /// </summary>
        /// <returns>First Child</returns>
        public TreeViewItemContainerData FirstChild(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);
            siblingIndex++;

            TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);

            Debug.Assert(child != null && child.Parent == this);

            return child;
        }


        /// <summary>
        /// Get Next Child Of TreeViewItem
        /// </summary>
        /// <param name="currentChild"></param>
        /// <returns>next treeview item after current child</returns>
        public TreeViewItemContainerData NextChild(VirtualizingTreeView treeView, TreeViewItemContainerData currentChild)
        {
            if (currentChild == null)
            {
                throw new ArgumentNullException("currentChild");
            }

            int siblingIndex = treeView.IndexOf(currentChild.Item);
            siblingIndex++;

            TreeViewItemContainerData nextChild = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
            while (nextChild != null && nextChild.IsDescendantOf(this))
            {
                if (nextChild.Parent == this)
                {
                    return nextChild;
                }

                siblingIndex++;
                nextChild = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
            }

            return null;
        }

        /// <summary>
        /// Get Last Child of TreeViewItem
        /// </summary>
        /// <returns>Last Child</returns>
        public TreeViewItemContainerData LastChild(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);

            TreeViewItemContainerData lastChild = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
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
        public TreeViewItemContainerData LastDescendant(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);
            TreeViewItemContainerData lastDescendant = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);

                if (child == null || !child.IsDescendantOf(this))
                {
                    return lastDescendant;
                }

                lastDescendant = child;
            }
        }

        public override string ToString()
        {
            return "Data: " + Item;
        }
    }

    public class VirtualizingTreeView : VirtualizingItemsControl<VirtualizingTreeViewItemDataBindingArgs>
    {
        /// <summary>
        /// Raised on item expanding
        /// </summary>
        public event EventHandler<VirtualizingItemExpandingArgs> ItemExpanding;
        public event EventHandler<VirtualizingItemExpandingArgs> ItemExpanded;

        public event EventHandler<VirtualizingItemCollapsedArgs> ItemCollapsing;
        public event EventHandler<VirtualizingItemCollapsedArgs> ItemCollapsed;

        /// <summary>
        /// Indent between parent and children
        /// </summary>
        public int Indent = 20;

        public bool CanReparent = true;
        protected override bool CanScroll
        {
            get { return base.CanScroll || CanReparent; }
        }

        /// <summary>
        /// Auto Expand items
        /// </summary>
        public bool AutoExpand = false;

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            TreeViewItemContainerData.ParentChanging += OnTreeViewItemParentChanging;
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();
            TreeViewItemContainerData.ParentChanging -= OnTreeViewItemParentChanging;
        }

        protected override ItemContainerData InstantiateItemContainerData(object item)
        {
            return new TreeViewItemContainerData
            {
                Item = item,
            };
        }

        /// <summary>
        /// Add data item as last child of parent
        /// </summary>
        /// <param name="parent">parent data item</param>
        /// <param name="item">data item to add</param>
        public void AddChild(object parent, object item)
        {
            if (parent == null)
            {
                Add(item);
            }
            else
            {
                TreeViewItemContainerData parentContainerData = (TreeViewItemContainerData)GetItemContainerData(parent);
                if (parentContainerData == null)
                {
                    return;
                }

                int index = -1;
                if (parentContainerData.IsExpanded)
                {
                    if (parentContainerData.HasChildren(this))
                    {
                        TreeViewItemContainerData lastDescendant = parentContainerData.LastDescendant(this);
                        index = IndexOf(lastDescendant.Item) + 1;
                    }
                    else
                    {
                        index = IndexOf(parentContainerData.Item) + 1;
                    }
                }
                else
                {
                    VirtualizingTreeViewItem parentContainer = (VirtualizingTreeViewItem)GetItemContainer(parent);
                    if(parentContainer != null)
                    {
                        parentContainer.CanExpand = true;
                    }
                    else
                    {
                        parentContainerData.CanExpand = true;
                    }
                }

                if (index > -1)
                {
                    TreeViewItemContainerData addedItemData = (TreeViewItemContainerData)Insert(index, item);
                    VirtualizingTreeViewItem addedTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(item);
                    if (addedTreeViewItem != null)
                    {
                        addedTreeViewItem.Parent = parentContainerData;
                    }
                    else
                    {
                        addedItemData.Parent = parentContainerData;
                    }
                }
            }
        }


        public override void Remove(object item)
        {
            throw new NotSupportedException("Use Remove Child instead");
        }

        public void RemoveChild(object parent, object item)
        {
            base.Remove(item);
            DataBindItem(parent);
        }

        [Obsolete("Use RemoveChild(object parent, object item) instead")]
        public void RemoveChild(object parent, object item, bool isLastChild)
        {
            if (parent == null)
            {
                base.Remove(item);
            }
            else
            {
                if (GetItemContainer(item) != null)
                {
                    base.Remove(item);
                }
                else
                {
                    //Parent item is not expanded (if isLastChild just remove parent expander)
                    if (isLastChild)
                    {
                        VirtualizingTreeViewItem parentContainer = (VirtualizingTreeViewItem)GetItemContainer(parent);
                        if (parentContainer)
                        {
                            parentContainer.CanExpand = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Change data item parent
        /// </summary>
        /// <param name="parent">new parent</param>
        /// <param name="item">data item</param>
        public void ChangeParent(object parent, object item)
        {
            if (IsDropInProgress)
            {
                return;
            }

            ItemContainerData dragItem = GetItemContainerData(item);
            if (parent == null)
            {
                if (dragItem == null)
                {
                    Add(item);
                }
                else
                {
                    ItemContainerData[] dragItems = new[] { dragItem };
                    if (CanDrop(dragItems, null))
                    {
                        Drop(dragItems, null, ItemDropAction.SetLastChild);
                    }
                }
            }
            else
            {
                ItemContainerData dropTarget = GetItemContainerData(parent);
                if (dropTarget == null)
                {
                    DestroyItems(new[] { item }, false);
                    return;
                }
                ItemContainerData[] dragItems = new[] { dragItem };
                if (CanDrop(dragItems, dropTarget))
                {
                    Drop(dragItems, dropTarget, ItemDropAction.SetLastChild);
                }
            }
        }

        /// <summary>
        /// Check wheter item is expanded
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsExpanded(object item)
        {
            TreeViewItemContainerData itemContainerData = (TreeViewItemContainerData)GetItemContainerData(item);
            if (itemContainerData == null)
            {
                return false;
            }
            return itemContainerData.IsExpanded;
        }

        public bool Expand(object item)
        {
            VirtualizingTreeViewItem tvi = GetTreeViewItem(item);
            if (tvi != null)
            {
                tvi.IsExpanded = true;
            }
            else
            {
                TreeViewItemContainerData containerData = (TreeViewItemContainerData)GetItemContainerData(item);
                if(containerData == null)
                {
                    Debug.LogWarning("Unable find container data for item " + item);
                    return false;
                }
                else
                {
                    Internal_Expand(item);
                }
            }
            return true;
        }

        /// <summary>
        /// To prevent Expand method call during drag drop operation 
        /// </summary>
        private bool m_expandSilently;
        public void Internal_Expand(object item)
        {
            TreeViewItemContainerData treeViewItemData = (TreeViewItemContainerData)GetItemContainerData(item);
            if(treeViewItemData == null)
            {
                throw new ArgumentException("TreeViewItemContainerData not found", "item");
            }
            if(treeViewItemData.IsExpanded)
            {
                return;
            }
            treeViewItemData.IsExpanded = true;

            if (m_expandSilently)
            {
                return;
            }

            if (ItemExpanding != null)
            {
                VirtualizingItemExpandingArgs args = new VirtualizingItemExpandingArgs(treeViewItemData.Item);
                ItemExpanding(this, args);

                IEnumerable children = args.Children == null ? null : args.Children.OfType<object>().ToArray();
                int itemIndex = IndexOf(treeViewItemData.Item);

                VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemData.Item);
                if(treeViewItem != null)
                {
                    treeViewItem.CanExpand = children != null;
                }
                else
                {
                    treeViewItemData.CanExpand = children != null;
                }

                if (treeViewItemData.CanExpand)
                {
                    TreeViewItemContainerData.Internal_RaiseEvents = false;
                    foreach (object childItem in children)
                    {
                        itemIndex++;

                        TreeViewItemContainerData childData = (TreeViewItemContainerData)Insert(itemIndex, childItem);
                        VirtualizingTreeViewItem childContainer = (VirtualizingTreeViewItem)GetItemContainer(childItem);
                        if (childContainer != null)
                        {
                            childContainer.Parent = treeViewItemData;
                        }
                        else
                        {
                            childData.Parent = treeViewItemData;
                        }
                    }
                    TreeViewItemContainerData.Internal_RaiseEvents = true;

                    UpdateSelectedItemIndex();
                }

                if (ItemExpanded != null)
                {
                    ItemExpanded(this, args);
                }
            }

           
        }

        public void Collapse(object item)
        {
            VirtualizingTreeViewItem tvi = GetTreeViewItem(item);
            if (tvi != null)
            {
                tvi.IsExpanded = false;
            }
            else
            {
                Internal_Collapse(item);
            }
        }

        public void Internal_Collapse(object item)
        {
            if(ItemCollapsing != null)
            {
                ItemCollapsing(this, new VirtualizingItemCollapsedArgs(item));
            }

            TreeViewItemContainerData treeViewItemData = (TreeViewItemContainerData)GetItemContainerData(item);
            if (treeViewItemData == null)
            {
                throw new ArgumentException("TreeViewItemContainerData not found", "item");
            }
            if (!treeViewItemData.IsExpanded)
            {
                return;
            }
            treeViewItemData.IsExpanded = false;

            int itemIndex = IndexOf(treeViewItemData.Item);
            List<object> itemsToDestroy = new List<object>();
            Collapse(treeViewItemData, itemIndex + 1, itemsToDestroy);

            if(itemsToDestroy.Count > 0)
            {
                bool unselect = false;
                base.DestroyItems(itemsToDestroy.ToArray(), unselect);
            }

            SelectedItems = SelectedItems;

            if (ItemCollapsed != null)
            {
                ItemCollapsed(this, new VirtualizingItemCollapsedArgs(item));
            }
        }

        private void Collapse(object[] items)
        {
            List<object> itemsToDestroy = new List<object>();
            
            for (int i = 0; i < items.Length; ++i)
            {
                int itemIndex = IndexOf(items[i]);
                if(itemIndex < 0)
                {
                    continue;
                }
                TreeViewItemContainerData itemData = (TreeViewItemContainerData)GetItemContainerData(itemIndex);
                Collapse(itemData, itemIndex + 1, itemsToDestroy);
            }

            if (itemsToDestroy.Count > 0)
            {
                bool unselect = false;
                base.DestroyItems(itemsToDestroy.ToArray(), unselect);
            }
        }

        private void Collapse(TreeViewItemContainerData item, int itemIndex, List<object> itemsToDestroy)
        {
            while (true)
            {
                TreeViewItemContainerData child = (TreeViewItemContainerData)GetItemContainerData(itemIndex);
                if (child == null || !child.IsDescendantOf(item))
                {
                    break;
                }

                itemsToDestroy.Add(child.Item);
                itemIndex++;
            }
        }

        public override void DataBindItem(object item, VirtualizingItemContainer itemContainer)
        {
            itemContainer.Clear();

            if (item != null)
            {
                VirtualizingTreeViewItemDataBindingArgs args = new VirtualizingTreeViewItemDataBindingArgs();
                args.Item = item;
                args.ItemPresenter = itemContainer.ItemPresenter == null ? itemContainer.gameObject : itemContainer.ItemPresenter;
                args.EditorPresenter = itemContainer.EditorPresenter == null ? itemContainer.gameObject : itemContainer.EditorPresenter;

                RaiseItemDataBinding(args);

                VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)itemContainer;
                treeViewItem.CanExpand = args.HasChildren;
                treeViewItem.CanEdit = CanEdit && args.CanEdit;
                treeViewItem.CanDrag = CanDrag && args.CanDrag;
                treeViewItem.CanBeParent = args.CanBeParent;
                treeViewItem.CanChangeParent = args.CanChangeParent;
                treeViewItem.CanSelect = args.CanSelect;
                treeViewItem.UpdateIndent();
            }
            else
            {
                VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)itemContainer;
                treeViewItem.CanExpand = false;
                treeViewItem.CanEdit = false;
                treeViewItem.CanDrag = false;
                treeViewItem.CanBeParent = false;
                treeViewItem.CanChangeParent = false;
                treeViewItem.CanSelect = false;
                treeViewItem.UpdateIndent();
            }
        }

        private void OnTreeViewItemParentChanging(object sender, VirtualizingParentChangedEventArgs e)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            TreeViewItemContainerData tvItem = (TreeViewItemContainerData)sender;
            TreeViewItemContainerData oldParent = e.OldParent;
            if (DropMarker.Action != ItemDropAction.SetLastChild && DropMarker.Action != ItemDropAction.None)
            {
                if (oldParent != null && !oldParent.HasChildren(this))
                {
                    VirtualizingTreeViewItem tvOldParent = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
                    if (tvOldParent != null)
                    {
                        tvOldParent.CanExpand = false;
                    }
                    else
                    {
                        oldParent.CanExpand = false;
                    }
                }
                return;
            }

            TreeViewItemContainerData tvDropTargetData = e.NewParent;
            VirtualizingTreeViewItem tvDropTarget = null;
            if (tvDropTargetData != null)
            {
                tvDropTarget = (VirtualizingTreeViewItem)GetItemContainer(tvDropTargetData.Item);
            }

            if (tvDropTarget != null)
            {
                if (tvDropTarget.CanExpand)
                {
                    tvDropTarget.IsExpanded = true;
                }
                else
                {
                    tvDropTarget.CanExpand = true;
                    try
                    {
                        m_expandSilently = true;
                        tvDropTarget.IsExpanded = true;
                    }
                    finally
                    {
                        m_expandSilently = false;
                    }
                }
            }
            else
            {
                if (tvDropTargetData != null)
                {
                    tvDropTargetData.CanExpand = true;
                    tvDropTargetData.IsExpanded = true;
                }
            }

            TreeViewItemContainerData dragItemChild = tvItem.FirstChild(this);
            TreeViewItemContainerData lastChild = null;
            if (tvDropTargetData != null)
            {
                lastChild = tvDropTargetData.LastChild(this);
                if (lastChild == null)
                {
                    lastChild = tvDropTargetData;
                }
            }
            else
            {

                lastChild = (TreeViewItemContainerData)LastItemContainerData();
            }

            if (lastChild != tvItem)
            {
                TreeViewItemContainerData lastDescendant = lastChild.LastDescendant(this);
                if (lastDescendant != null)
                {
                    lastChild = lastDescendant;
                }

                if (!lastChild.IsDescendantOf(tvItem))
                {
                    base.SetNextSiblingInternal(lastChild, tvItem);
                }
            }

            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }

            if (oldParent != null && !oldParent.HasChildren(this))
            {
                VirtualizingTreeViewItem tvOldParent = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
                if (tvOldParent != null)
                {
                    tvOldParent.CanExpand = false;
                }
                else
                {
                    oldParent.CanExpand = false;
                }
            }
        }

        private void MoveSubtree(TreeViewItemContainerData parent, TreeViewItemContainerData child)
        {
            int parentSiblingIndex = IndexOf(parent.Item);
            int siblingIndex = IndexOf(child.Item);
            bool incrementSiblingIndex = false;
            if (parentSiblingIndex < siblingIndex)
            {
                incrementSiblingIndex = true;
            }

            TreeViewItemContainerData prev = parent;
            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(prev.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
            while (child != null && child.IsDescendantOf(parent))
            {
                if (prev == child)
                {
                    break;
                }
                base.SetNextSiblingInternal(prev, child);

                tvItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
                if(tvItem != null)
                {
                    tvItem.UpdateIndent();
                }

                prev = child;
                if (incrementSiblingIndex)
                {
                    siblingIndex++;
                }
                child = (TreeViewItemContainerData)GetItemContainerData(siblingIndex);
            }
        }

        protected override bool CanDrop(ItemContainerData[] dragItems, ItemContainerData dropTarget)
        {
            if(base.CanDrop(dragItems, dropTarget))
            {
                TreeViewItemContainerData tvDropTarget = (TreeViewItemContainerData)dropTarget;
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItemContainerData dragItemData = (TreeViewItemContainerData)dragItems[i];
                    if (tvDropTarget == dragItemData || tvDropTarget != null && tvDropTarget.IsDescendantOf(dragItemData)) //disallow self parenting
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected override void Drop(ItemContainerData[] dragItems, ItemContainerData dropTarget, ItemDropAction action)
        {
            TreeViewItemContainerData tvDropTarget = (TreeViewItemContainerData)dropTarget;
            if (action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItemContainerData dragItemData = (TreeViewItemContainerData)dragItems[i];
                    if (tvDropTarget == null || tvDropTarget != dragItemData && !tvDropTarget.IsDescendantOf(dragItemData)) //disallow self parenting
                    {
                        SetParent(tvDropTarget, dragItemData);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    SetPrevSiblingInternal(tvDropTarget, dragItems[i]);
                }
            }
            else if (action == ItemDropAction.SetNextSibling)
            {
                for (int i = dragItems.Length - 1; i >= 0; --i)
                {
                    SetNextSiblingInternal(tvDropTarget, dragItems[i]);
                }
            }

            UpdateSelectedItemIndex();
        }

        protected override void SetNextSiblingInternal(ItemContainerData sibling, ItemContainerData nextSibling)
        {
            TreeViewItemContainerData tvSibling = (TreeViewItemContainerData)sibling;
            TreeViewItemContainerData lastDescendant = tvSibling.LastDescendant(this);
            if (lastDescendant == null)
            {
                lastDescendant = tvSibling;
            }
            TreeViewItemContainerData tvItemData = (TreeViewItemContainerData)nextSibling;
            TreeViewItemContainerData dragItemChild = tvItemData.FirstChild(this);

            base.SetNextSiblingInternal(lastDescendant, nextSibling);
            if (dragItemChild != null)
            {
                MoveSubtree(tvItemData, dragItemChild);
            }

            SetParent(tvSibling.Parent, tvItemData);

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(tvItemData.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
        }


        protected override void SetPrevSiblingInternal(ItemContainerData sibling, ItemContainerData prevSibling)
        {
            TreeViewItemContainerData tvSiblingData = (TreeViewItemContainerData)sibling;
            TreeViewItemContainerData tvItemData = (TreeViewItemContainerData)prevSibling;
            TreeViewItemContainerData tvDragItemChild = tvItemData.FirstChild(this);

            base.SetPrevSiblingInternal(sibling, prevSibling);

            if (tvDragItemChild != null)
            {
                MoveSubtree(tvItemData, tvDragItemChild);
            }

            SetParent(tvSiblingData.Parent, tvItemData);

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(tvItemData.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
        }

        private void SetParent(TreeViewItemContainerData parent, TreeViewItemContainerData child)
        {
            VirtualizingTreeViewItem tvDragItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
            if (tvDragItem != null)
            {
                tvDragItem.Parent = parent;
            }
            else
            {
                child.Parent = parent;
                UpdateIndent(child.Item);
            }
        }

        public void UpdateIndent(object obj)
        {
            VirtualizingTreeViewItem item = (VirtualizingTreeViewItem)GetItemContainer(obj);
            if (item == null)
            {
                return;
            }

            item.UpdateIndent();
        }

        protected override void DestroyItems(object[] items, bool unselect)
        {
            TreeViewItemContainerData[] itemContainers = items.Select(item => GetItemContainerData(item)).OfType<TreeViewItemContainerData>().ToArray();
            TreeViewItemContainerData[] parents = itemContainers.Where(container => container.Parent != null).Select(container => container.Parent).ToArray();

            Collapse(items);

            base.DestroyItems(items, unselect);

            foreach(TreeViewItemContainerData parent in parents)
            {
                if(!parent.HasChildren(this))
                {
                    VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)GetItemContainer(parent.Item);
                    if(treeViewItem != null)
                    {
                        treeViewItem.CanExpand = false;
                    }
                }
            }
        }

        public VirtualizingTreeViewItem GetTreeViewItem(object item)
        {
            return GetItemContainer(item) as VirtualizingTreeViewItem;
        }


        public void ScrollIntoView(object obj)
        {
            int index = IndexOf(obj);
            if (index < 0)
            {
                throw new InvalidOperationException(string.Format("item {0} does not exist or not visible", obj));
            }
            VirtualizingScrollRect scrollRect = GetComponentInChildren<VirtualizingScrollRect>();
            scrollRect.Index = index;
        }
    }

    public static class VirtualizingTreeViewExtension
    {
        public static void ExpandTo<T>(this VirtualizingTreeView treeView, T item, Func<T,T> getParent)
        {
            if (item == null)
            {
                return;
            }
            ItemContainerData containerData = treeView.GetItemContainerData(item);
            if (containerData == null)
            {
                treeView.ExpandTo(getParent(item), getParent);
                treeView.Expand(item);
            }
            else
            {
                treeView.Expand(item);
            }
        }

        public static void ExpandChildren<T>(this VirtualizingTreeView treeView, T item, Func<T, IEnumerable> getChildren)
        {
            IEnumerable children = getChildren(item);
            if (children != null)
            {
                treeView.Expand(item);

                foreach (T child in children)
                {
                    treeView.ExpandChildren(child, getChildren);
                }
            }
        }

        public static void ExpandAll<T>(this VirtualizingTreeView treeView, T item, Func<T,T> getParent, Func<T, IEnumerable> getChildren)
        {
            treeView.ExpandTo(getParent(item), getParent);
            treeView.ExpandChildren(item, getChildren);
        }

        public static void ItemDropStdHandler<T>(this VirtualizingTreeView treeView, ItemDropArgs e,
            Func<T, T> getParent,
            Action<T, T> setParent,
            Func<T, T, int> indexOfChild,
            Action<T, T> removeChild,
            Action<T, T, int> insertChild,
            Action<T, T> addChild = null) where T : class
        {

            T dropTarget = (T)e.DropTarget;
            //Set drag items as children of drop target
            if (e.Action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    T dragItem = (T)e.DragItems[i];
                    removeChild(dragItem, getParent(dragItem));
                    setParent(dragItem, dropTarget);
                    if(addChild != null)
                    {
                        addChild(dragItem, getParent(dragItem));
                    }
                    else
                    {
                        insertChild(dragItem, getParent(dragItem), 0);
                    }
                }
            }

            //Put drag items next to drop target
            else if (e.Action == ItemDropAction.SetNextSibling)
            {
                for (int i = e.DragItems.Length - 1; i >= 0; --i)
                {
                    T dragItem = (T)e.DragItems[i];
                    int dropTIndex = indexOfChild(dropTarget, getParent(dropTarget));
                    if (getParent(dragItem) != getParent(dropTarget))
                    {
                        removeChild(dragItem, getParent(dragItem));
                        setParent(dragItem, getParent(dropTarget));
                        insertChild(dragItem, getParent(dragItem), dropTIndex + 1);
                    }
                    else
                    {
                        int dragTIndex = indexOfChild(dragItem, getParent(dragItem));
                        if (dropTIndex < dragTIndex)
                        {
                            removeChild(dragItem, getParent(dragItem));
                            insertChild(dragItem, getParent(dragItem), dropTIndex + 1);
                        }
                        else
                        {
                            removeChild(dragItem, getParent(dragItem));
                            insertChild(dragItem, getParent(dragItem), dropTIndex);
                        }
                    }
                }
            }

            //Put drag items before drop target
            else if (e.Action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    T dragItem = (T)e.DragItems[i];
                    if (getParent(dragItem) != getParent(dropTarget))
                    {
                        removeChild(dragItem, getParent(dragItem));
                        setParent(dragItem, getParent(dropTarget));
                        insertChild(dragItem, getParent(dragItem), 0);
                    }

                    int dropTIndex = indexOfChild(dropTarget, getParent(dropTarget));
                    int dragTIndex = indexOfChild(dragItem, getParent(dragItem));
                    if (dropTIndex > dragTIndex)
                    {
                        removeChild(dragItem, getParent(dragItem));
                        insertChild(dragItem, getParent(dragItem), dropTIndex - 1);
                    }
                    else
                    {
                        removeChild(dragItem, getParent(dragItem));
                        insertChild(dragItem, getParent(dragItem), dropTIndex);
                    }
                }
            }
        }
    }
}

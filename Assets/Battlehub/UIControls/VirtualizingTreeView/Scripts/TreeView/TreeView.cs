using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
1.02 - RemoveChild method
*/


namespace Battlehub.UIControls
{
    /// <summary>
    /// Data Item Expanding event arguments
    /// </summary>
    public class ItemExpandingArgs : EventArgs
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

        public ItemExpandingArgs(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// TreeView data binding event arguments
    /// </summary>
    public class TreeViewItemDataBindingArgs : ItemDataBindingArgs
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

    public class TreeView : ItemsControl<TreeViewItemDataBindingArgs>
    {
        /// <summary>
        /// Raised on item expanding
        /// </summary>
        public event EventHandler<ItemExpandingArgs> ItemExpanding;
          
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
            TreeViewItem.ParentChanged += OnTreeViewItemParentChanged;
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();
            TreeViewItem.ParentChanged -= OnTreeViewItemParentChanged;
        }

        public TreeViewItem GetTreeViewItem(int siblingIndex)
        {
            return (TreeViewItem)GetItemContainer(siblingIndex);
        }

        public TreeViewItem GetTreeViewItem(object obj)
        {
            return (TreeViewItem)GetItemContainer(obj);
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
                TreeViewItem parentContainer = (TreeViewItem)GetItemContainer(parent);
                if(parentContainer == null)
                {
                    return;
                }

                int index = -1;
                if (parentContainer.IsExpanded)
                {
                    if (parentContainer.HasChildren)
                    {
                        TreeViewItem lastDescendant = parentContainer.LastDescendant();
                        index = IndexOf(lastDescendant.Item) + 1;
                    }
                    else
                    {
                        index = IndexOf(parentContainer.Item) + 1;
                    }
                }
                else
                {
                    parentContainer.CanExpand = true;
                }

                if(index > -1)
                {
                    TreeViewItem addedItem = (TreeViewItem)Insert(index, item);
                    addedItem.Parent = parentContainer;  
                }
            }
        }

        public override void Remove(object item)
        {
            throw new NotSupportedException("This method is not supported for TreeView use RemoveChild instead");
        }

        public void RemoveChild(object parent, object item, bool isLastChild)
        {
            if(parent == null)
            {
                base.Remove(item);
            }
            else
            {
                if(GetItemContainer(item) != null) 
                {
                    base.Remove(item);
                }
                else
                {
                    //Parent item is not expanded (if isLastChild just remove parent expander
                    if (isLastChild)
                    {
                        TreeViewItem parentContainer = (TreeViewItem)GetItemContainer(parent);
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
            if(IsDropInProgress)
            {
                return;
            }

            ItemContainer dragItem = GetItemContainer(item);
            if (dragItem == null)
            {
                return;
            }

            ItemContainer dropTarget = GetItemContainer(parent);
            ItemContainer[] dragItems = new[] { dragItem };
            if (CanDrop(dragItems, dropTarget))
            {
                Drop(dragItems, dropTarget, ItemDropAction.SetLastChild);
            }   
        }

   

        /// <summary>
        /// Check wheter item is expanded
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsExpanded(object item)
        {
            TreeViewItem itemContainer = (TreeViewItem)GetItemContainer(item);
            if(itemContainer == null)
            {
                return false;
            }
            return itemContainer.IsExpanded;
        }
        /// <summary>
        /// To prevent Expand method call during drag drop operation 
        /// </summary>
        private bool m_expandSilently;
        /// <summary>
        /// Expand Tree View Item
        /// </summary>
        /// <param name="item">tree view item to expand</param>
        public void Expand(TreeViewItem item)
        {
            if(m_expandSilently)
            {
                return;
            }

            if(ItemExpanding != null)
            {
                ItemExpandingArgs args = new ItemExpandingArgs(item.Item);
                ItemExpanding(this, args);

                IEnumerable children = args.Children;
                int containerIndex = item.transform.GetSiblingIndex();
                int itemIndex = IndexOf(item.Item);

                item.CanExpand = children != null;
                
                if (item.CanExpand)
                {
                    foreach(object childItem in children)
                    {
                        containerIndex++;
                        itemIndex++;

                        InsertItem(itemIndex, childItem);

                        TreeViewItem childContainer = (TreeViewItem)InstantiateItemContainer(containerIndex);
                        childContainer.Item = childItem;
                        childContainer.Parent = item;
                        
                        DataBindItem(childItem, childContainer);
                    }

                    UpdateSelectedItemIndex();
                }   
            }
        }

        /// <summary>
        /// Collapse tree view item
        /// </summary>
        /// <param name="item">tree view item to collapse</param>
        public void Collapse(TreeViewItem item)
        {
            int containerIndex = item.transform.GetSiblingIndex();
            int itemIndex = IndexOf(item.Item);
            Collapse(item, containerIndex + 1, itemIndex + 1);
        }

        private void Unselect(List<object> selectedItems, TreeViewItem item, ref int containerIndex, ref int itemIndex )
        {
            while (true)
            {
                TreeViewItem child = (TreeViewItem)GetItemContainer(containerIndex);
                if (child == null || child.Parent != item)
                {
                    break;
                }
                containerIndex++;
                itemIndex++;
                selectedItems.Remove(child.Item);
                Unselect(selectedItems, child, ref containerIndex, ref itemIndex);
            }
        }

        private void Collapse(TreeViewItem item, int containerIndex, int itemIndex)
        {
            while (true)
            {
                TreeViewItem child = (TreeViewItem)GetItemContainer(containerIndex);
                if (child == null || child.Parent != item)
                {
                    break;
                }

                Collapse(child, containerIndex + 1, itemIndex + 1);
                RemoveItemAt(itemIndex);
                DestroyItemContainer(containerIndex); 
            }
        }

   
        protected override ItemContainer InstantiateItemContainerOverride(GameObject container)
        {
            TreeViewItem itemContainer = container.GetComponent<TreeViewItem>();
            if (itemContainer == null)
            {
                itemContainer = container.AddComponent<TreeViewItem>();
                itemContainer.gameObject.name = "TreeViewItem";
            }
            return itemContainer;
        }

        protected override void DestroyItem(object item)
        {
            TreeViewItem itemContainer = (TreeViewItem)GetItemContainer(item);
            if(itemContainer != null)
            {
                Collapse(itemContainer);
                base.DestroyItem(item);
                if(itemContainer.Parent != null && !itemContainer.Parent.HasChildren)
                {
                    itemContainer.Parent.CanExpand = false;
                }
            }
        }

        public override void DataBindItem(object item, ItemContainer itemContainer)
        {
            TreeViewItemDataBindingArgs args = new TreeViewItemDataBindingArgs();
            args.Item = item;
            args.ItemPresenter = itemContainer.ItemPresenter == null ? gameObject : itemContainer.ItemPresenter;
            args.EditorPresenter = itemContainer.EditorPresenter == null ? gameObject : itemContainer.EditorPresenter;
            RaiseItemDataBinding(args);

            TreeViewItem treeViewItem = (TreeViewItem)itemContainer;
            treeViewItem.CanExpand = args.HasChildren;
            treeViewItem.CanEdit = args.CanEdit;
            treeViewItem.CanDrag = args.CanDrag;
            treeViewItem.CanDrop = args.CanDrop;
        }

        protected override bool CanDrop(ItemContainer[] dragItems, ItemContainer dropTarget)
        {
            if(!base.CanDrop(dragItems, dropTarget))
            {
                return false;
            }

            TreeViewItem tvDropTarget = (TreeViewItem)dropTarget;
            if (tvDropTarget == null)
            {
                return true;
            }

            foreach(ItemContainer dragItem in dragItems)
            {
                TreeViewItem tvDragItem = (TreeViewItem)dragItem;
                if (tvDropTarget.IsDescendantOf(tvDragItem))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnTreeViewItemParentChanged(object sender, ParentChangedEventArgs e)
        {
            TreeViewItem tvItem = (TreeViewItem)sender;
            if (!CanHandleEvent(tvItem))
            {
                return;
            }

            TreeViewItem oldParent = e.OldParent;
            if (DropMarker.Action != ItemDropAction.SetLastChild && DropMarker.Action != ItemDropAction.None)
            {
                if (oldParent != null && !oldParent.HasChildren)
                {
                    oldParent.CanExpand = false;
                }
                return;
            }
           
            TreeViewItem tvDropTarget = e.NewParent;
            if(tvDropTarget != null)
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
           
            TreeViewItem dragItemChild = tvItem.FirstChild();
            TreeViewItem lastChild = null;
            if(tvDropTarget != null)
            {
                lastChild = tvDropTarget.LastChild();
                if (lastChild == null)
                {
                    lastChild = tvDropTarget;
                }
            }
            else
            {
                lastChild = (TreeViewItem)LastItemContainer();
            }
          
            if(lastChild != tvItem)
            {
                TreeViewItem lastDescendant = lastChild.LastDescendant();
                if(lastDescendant != null)
                {
                    lastChild = lastDescendant;
                }

                if(!lastChild.IsDescendantOf(tvItem))
                {
                    base.SetNextSibling(lastChild, tvItem);
                }
            }
            
            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }

            if (oldParent != null && !oldParent.HasChildren)
            {
                oldParent.CanExpand = false;
            }
        }

        private void MoveSubtree(TreeViewItem parent, TreeViewItem child)
        {
            int parentSiblingIndex = parent.transform.GetSiblingIndex();
            int siblingIndex = child.transform.GetSiblingIndex();
            bool incrementSiblingIndex = false;
            if(parentSiblingIndex < siblingIndex)
            {
                incrementSiblingIndex = true;
            }

            TreeViewItem prev = parent;
            while (child != null && child.IsDescendantOf(parent))
            {
                if(prev == child)
                {
                    break;
                }
                base.SetNextSibling(prev, child);
                prev = child;
                if(incrementSiblingIndex)
                {
                    siblingIndex++;
                }
                child = (TreeViewItem)GetItemContainer(siblingIndex);
            }
        }

        protected override void Drop(ItemContainer[] dragItems, ItemContainer dropTarget, ItemDropAction action)
        {
            TreeViewItem tvDropTarget = (TreeViewItem)dropTarget;
            if (action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItem tvDragItem = (TreeViewItem)dragItems[i];
                    if(tvDropTarget != tvDragItem) //disallow self parenting
                    {
                        tvDragItem.Parent = tvDropTarget;
                    }
                }
            }
            else if (action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    SetPrevSibling(tvDropTarget, dragItems[i]);
                }
            }
            else if (action == ItemDropAction.SetNextSibling)
            {
                for (int i = dragItems.Length - 1; i >= 0; --i)
                {
                    SetNextSibling(tvDropTarget, dragItems[i]);
                }
            }

            UpdateSelectedItemIndex();
        }

        protected override void SetNextSibling(ItemContainer sibling, ItemContainer nextSibling)
        { 
            TreeViewItem tvSibling = (TreeViewItem)sibling;
            TreeViewItem lastDescendant = tvSibling.LastDescendant();
            if(lastDescendant == null)
            {
                lastDescendant = tvSibling;
            }
            TreeViewItem tvItem = (TreeViewItem)nextSibling;
            TreeViewItem dragItemChild = tvItem.FirstChild();

            base.SetNextSibling(lastDescendant, nextSibling);

            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }

            tvItem.Parent = tvSibling.Parent;

        }

        protected override void SetPrevSibling(ItemContainer sibling, ItemContainer prevSibling)
        {
            TreeViewItem tvSibling = (TreeViewItem)sibling;
            TreeViewItem tvItem = (TreeViewItem)prevSibling;
            TreeViewItem dragItemChild = tvItem.FirstChild();

            base.SetPrevSibling(sibling, prevSibling);

            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }

            tvItem.Parent = tvSibling.Parent;
        }

        public void FixScrollRect()
        {
            Canvas.ForceUpdateCanvases();
            RectTransform rt = Panel.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height - 0.01f);
        }

    }
}

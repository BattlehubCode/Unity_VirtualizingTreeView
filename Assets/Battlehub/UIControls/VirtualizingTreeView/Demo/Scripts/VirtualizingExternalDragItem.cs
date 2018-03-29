using UnityEngine;

using UnityEngine.EventSystems;


namespace Battlehub.UIControls
{
    public class VirtualizingExternalDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public VirtualizingTreeView TreeView;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            TreeView.ExternalBeginDrag(eventData.position);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            TreeView.ExternalItemDrag(eventData.position);
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (TreeView.DropTarget != null)
            {
                TreeView.AddChild(TreeView.DropTarget, new GameObject());
            }

            TreeView.ExternalItemDrop();           
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (TreeView.DropTarget != null)
            {
                GameObject dropTarget = (GameObject)TreeView.DropTarget;
                GameObject newDataItem = new GameObject();
                VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)TreeView.GetItemContainer(TreeView.DropTarget);

                if (TreeView.DropAction == ItemDropAction.SetLastChild)
                {
                    newDataItem.transform.SetParent(dropTarget.transform);
                    TreeView.AddChild(TreeView.DropTarget, newDataItem);    
                    treeViewItem.CanExpand = true;
                    treeViewItem.IsExpanded = true;
                }
                else if(TreeView.DropAction != ItemDropAction.None)
                {
                    int index;
                    if (TreeView.DropAction == ItemDropAction.SetNextSibling)
                    {
                        index = TreeView.IndexOf(dropTarget) + 1;
                    }
                    else
                    {
                        index = TreeView.IndexOf(dropTarget);
                    }

                    newDataItem.transform.SetParent(dropTarget.transform.parent);
                    newDataItem.transform.SetSiblingIndex(index);

                    TreeViewItemContainerData newTreeViewItemData = (TreeViewItemContainerData)TreeView.Insert(index, newDataItem);
                    VirtualizingTreeViewItem newTreeViewItem = (VirtualizingTreeViewItem)TreeView.GetItemContainer(newDataItem);
                    if(newTreeViewItem != null)
                    {
                        newTreeViewItem.Parent = treeViewItem.Parent;
                    }
                    else
                    {
                        newTreeViewItemData.Parent = treeViewItem.Parent;
                    }   
                }
               
            }

            TreeView.ExternalItemDrop();
        }

       
    }

}

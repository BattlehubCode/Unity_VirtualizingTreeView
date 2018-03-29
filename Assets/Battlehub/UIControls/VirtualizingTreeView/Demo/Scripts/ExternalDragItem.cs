using UnityEngine;

using UnityEngine.EventSystems;


namespace Battlehub.UIControls
{
    public class ExternalDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public TreeView TreeView;


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
                TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(TreeView.DropTarget);

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

                    TreeViewItem newTreeViewItem = (TreeViewItem)TreeView.Insert(index, newDataItem);
                    newTreeViewItem.Parent = treeViewItem.Parent;
                }
               
            }

            TreeView.ExternalItemDrop();
        }

       
    }

}

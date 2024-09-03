using UnityEngine;
namespace Battlehub.UIControls
{
    [RequireComponent(typeof(RectTransform))]
    public class VirtualizingTreeViewDropMarker : VirtualizingItemDropMarker
    {
        private bool m_useGrid;
        private VirtualizingTreeView m_treeView;
        private RectTransform m_siblingGraphicsRectTransform;
        public GameObject ChildGraphics;

        private bool m_canChangeDragItemParent;
        private bool m_canSetDragItemSiblingIndex;

        public override ItemDropAction Action
        {
            get { return base.Action; }
            set
            {
                base.Action = value;
                ChildGraphics.SetActive(base.Action == ItemDropAction.SetLastChild);
                SiblingGraphics.SetActive(base.Action != ItemDropAction.SetLastChild && base.Action != ItemDropAction.None);
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_treeView = GetComponentInParent<VirtualizingTreeView>();
            m_siblingGraphicsRectTransform = SiblingGraphics.GetComponent<RectTransform>();

            RectTransform rectTransform = (RectTransform)transform;
            VirtualizingScrollRect scrollRect = m_treeView.GetComponentInChildren<VirtualizingScrollRect>();
            if(scrollRect != null && scrollRect.UseGrid)
            {
                m_useGrid = true;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = new Vector2(1, 0);
            }
        }

        public override void SetTarget(VirtualizingItemContainer target)
        {
            base.SetTarget(target);

            m_canChangeDragItemParent = false;
            m_canSetDragItemSiblingIndex = false;

            if (target == null)
            {
                return;
            }

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)Target;
            if (tvItem != null)
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(tvItem.Indent, m_siblingGraphicsRectTransform.offsetMin.y);
            }
            else
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(0, m_siblingGraphicsRectTransform.offsetMin.y);
            }

            if(Target != null)
            {
                m_canChangeDragItemParent = true;
                m_canSetDragItemSiblingIndex = true;

                if(DragItems != null)
                {
                    ItemContainerData[] data = DragItems;
                    for (int i = 0; i < data.Length; ++i)
                    {
                        TreeViewItemContainerData treeViewItemData = (TreeViewItemContainerData)data[i];
                        if (!treeViewItemData.CanChangeParent)
                        {
                            object parentItem = tvItem.Parent != null ? tvItem.Parent.Item : null;
                            if (treeViewItemData.ParentItem != parentItem)
                            {
                                m_canSetDragItemSiblingIndex = false;
                            }

                            if (treeViewItemData.ParentItem != Target.Item)
                            {
                                m_canChangeDragItemParent = false;
                            }

                            if (!m_canSetDragItemSiblingIndex && !m_canChangeDragItemParent)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }


        public override void SetPosition(Vector2 position)
        {
            if (!m_canChangeDragItemParent && !m_canSetDragItemSiblingIndex)
            {
                Action = ItemDropAction.None;
                return;
            }

            RectTransform rt = Target.RectTransform;
            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)Target;

            Vector2 sizeDelta = m_rectTransform.sizeDelta;
            sizeDelta.y = rt.rect.height;
            if (m_useGrid)
            {
                sizeDelta.x = rt.rect.width;
            }
            m_rectTransform.sizeDelta = sizeDelta;

            Vector2 localPoint;
            Camera camera = null;
            if (ParentCanvas.renderMode == RenderMode.WorldSpace || ParentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                camera = m_treeView.Camera;
            }

            if (!m_treeView.CanReorder || !m_canSetDragItemSiblingIndex)
            {
                if (!Target.CanBeParent || !m_treeView.CanReparent)
                {
                    Action = ItemDropAction.None;
                    return;
                }

                Action = ItemDropAction.SetLastChild;
                RectTransform.position = rt.position;
            }
            else
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out localPoint))
                {
                    if(m_canChangeDragItemParent && m_treeView.CanReparent)
                    {
                        if (localPoint.y > -rt.rect.height / 4)
                        {
                            if (tvItem.Parent != null && !tvItem.Parent.CanBeParent)
                            {
                                Action = ItemDropAction.None;
                                return;
                            }

                            Action = ItemDropAction.SetPrevSibling;
                            RectTransform.position = rt.position;
                        }
                        else if (localPoint.y < rt.rect.height / 4 - rt.rect.height && !tvItem.HasChildren)
                        {
                            if (tvItem.Parent != null && !tvItem.Parent.CanBeParent)
                            {
                                Action = ItemDropAction.None;
                                return;
                            }

                            Action = ItemDropAction.SetNextSibling;
                            RectTransform.position = rt.TransformPoint(Vector3.down * rt.rect.height);
                        }
                        else
                        {
                            if (!tvItem.CanBeParent)
                            {
                                Action = ItemDropAction.None;
                                return;
                            }

                            Action = ItemDropAction.SetLastChild;
                            RectTransform.position = rt.position;
                        }
                    }
                    else
                    {
                        if (localPoint.y > -rt.rect.height / 4)
                        {
                            if (tvItem.Parent != null && !tvItem.Parent.CanBeParent)
                            {
                                Action = ItemDropAction.None;
                                return;
                            }


                            Action = ItemDropAction.SetPrevSibling;
                            RectTransform.position = rt.position;
                        }
                        else if (localPoint.y < rt.rect.height / 2 && !tvItem.HasChildren)
                        {
                            if (tvItem.Parent != null && !tvItem.Parent.CanBeParent)
                            {
                                Action = ItemDropAction.None;
                                return;
                            }

                            Action = ItemDropAction.SetNextSibling;
                            RectTransform.position = rt.TransformPoint(Vector3.down * rt.rect.height);
                        }
                    }
                  
                }
            }
        }
    }
}


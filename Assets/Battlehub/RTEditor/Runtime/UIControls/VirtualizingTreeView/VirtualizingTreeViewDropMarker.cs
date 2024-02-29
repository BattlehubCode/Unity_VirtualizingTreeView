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
        public override ItemDropAction Action
        {
            get { return base.Action; }
            set
            {
                base.Action = value;
                ChildGraphics.SetActive(base.Action == ItemDropAction.SetLastChild);
                SiblingGraphics.SetActive(base.Action != ItemDropAction.SetLastChild);
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

        public override void SetTarget(VirtualizingItemContainer item)
        {
            base.SetTarget(item);
            if(item == null)
            {
                return;
            }

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)item;
            if (tvItem != null)
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(tvItem.Indent, m_siblingGraphicsRectTransform.offsetMin.y);
            }
            else
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(0, m_siblingGraphicsRectTransform.offsetMin.y);
            }
        }


        public override void SetPosition(Vector2 position)
        {
            if (Item == null)
            {
                return;
            }

            if (!m_treeView.CanReparent || !Item.CanChangeParent)
            {
                base.SetPosition(position);
                return;
            }

            RectTransform rt = Item.RectTransform;
            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)Item;

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

            if (!m_treeView.CanReorder)
            {
                if (!tvItem.CanBeParent)
                {
                    return;
                }

                Action = ItemDropAction.SetLastChild;
                RectTransform.position = rt.position;
            }
            else
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out localPoint))
                {
                    if (localPoint.y > -rt.rect.height / 4)
                    {
                        Action = ItemDropAction.SetPrevSibling;
                        RectTransform.position = rt.position;
                    }
                    else if (localPoint.y < rt.rect.height / 4 - rt.rect.height && !tvItem.HasChildren)
                    {
                        Action = ItemDropAction.SetNextSibling;
                        RectTransform.position = rt.TransformPoint(Vector3.down * rt.rect.height);
                    }
                    else
                    {
                        if (!tvItem.CanBeParent)
                        {
                            return;
                        }

                        Action = ItemDropAction.SetLastChild;
                        RectTransform.position = rt.position;
                    }
                }
            }
        }
    }
}


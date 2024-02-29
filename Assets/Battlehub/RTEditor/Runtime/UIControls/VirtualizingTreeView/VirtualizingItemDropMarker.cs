using System;
using UnityEngine;
namespace Battlehub.UIControls
{
    [RequireComponent(typeof(RectTransform))]
    public class VirtualizingItemDropMarker : MonoBehaviour
    {
        private VirtualizingItemsControl m_itemsControl;

        private Canvas m_parentCanvas;
        protected Canvas ParentCanvas
        {
            get { return m_parentCanvas; }
        }

        public GameObject SiblingGraphics;
        private ItemDropAction m_action;
        public virtual ItemDropAction Action
        {
            get { return m_action; }
            set
            {
                m_action = value;
            }
        }

        protected RectTransform m_rectTransform;
        public RectTransform RectTransform
        {
            get { return m_rectTransform; }
        }
        private VirtualizingItemContainer m_target;
        public VirtualizingItemContainer Target
        {
            get { return m_target; }
        }

        private ItemContainerData[] m_dragItems;
        protected ItemContainerData[] DragItems
        {
            get { return m_dragItems; }
        }

        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            SiblingGraphics.SetActive(true);
            m_parentCanvas = GetComponentInParent<Canvas>();
            m_itemsControl = GetComponentInParent<VirtualizingItemsControl>();
            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {

        }

        public virtual void SetTarget(VirtualizingItemContainer item)
        {
            gameObject.SetActive(item != null);
            
            m_target = item;
            if(m_target == null)
            {
                Action = ItemDropAction.None;
            }
        }

        public virtual void SetDragItems(ItemContainerData[] dragItems)
        {
            m_dragItems = dragItems;
        }

        public virtual void SetPosition(Vector2 position)
        {
            if (m_target == null)
            {
                return;
            }

            if(!m_itemsControl.CanReorder)
            {
                return;
            }

            RectTransform rt = Target.RectTransform;
            Vector2 sizeDelta = m_rectTransform.sizeDelta;
            sizeDelta.y = rt.rect.height;
            m_rectTransform.sizeDelta = sizeDelta;

            Vector2 localPoint;

            Camera camera = null;
            if (ParentCanvas.renderMode == RenderMode.WorldSpace || ParentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                camera = m_itemsControl.Camera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out localPoint))
            {
                if (localPoint.y > -rt.rect.height / 2)
                {
                    Action = ItemDropAction.SetPrevSibling;
                    RectTransform.position = rt.position;
                }
                else 
                {
                    Action = ItemDropAction.SetNextSibling;
                    RectTransform.position = rt.position;
                    RectTransform.localPosition = RectTransform.localPosition - new Vector3(0, rt.rect.height * ParentCanvas.scaleFactor, 0);
                }
            }
        }

        #region Obsolete

        [Obsolete] //12.02.2021
        protected VirtualizingItemContainer Item
        {
            get { return m_target; }
        }

        #endregion

    }
}


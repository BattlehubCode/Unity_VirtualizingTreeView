using UnityEngine;
namespace Battlehub.UIControls
{
    public enum ItemDropAction
    {
        None,
        SetLastChild,
        SetPrevSibling,
        SetNextSibling
    }

    [RequireComponent(typeof(RectTransform))]
    public class ItemDropMarker : MonoBehaviour
    {
        private Canvas m_parentCanvas;
        private ItemsControl m_itemsControl;

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

        private RectTransform m_rectTransform;
        public RectTransform RectTransform
        {
            get { return m_rectTransform; }
        }
        private ItemContainer m_item;
        protected ItemContainer Item
        {
            get { return m_item; }
        }
        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            SiblingGraphics.SetActive(true);
            m_parentCanvas = GetComponentInParent<Canvas>();
            m_itemsControl = GetComponentInParent<ItemsControl>();
            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {

        }

        public virtual void SetTraget(ItemContainer item)
        {
            gameObject.SetActive(item != null);
            
            m_item = item;
            if(m_item == null)
            {
                Action = ItemDropAction.None;
            }
        }

        public virtual void SetPosition(Vector2 position)
        {
            if(m_item == null)
            {
                return;
            }

            if(!m_itemsControl.CanReorder)
            {
                return;
            }


            RectTransform rt = Item.RectTransform;
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

                    //RectTransform.position = rt.position - new Vector3(0, rt.rect.height, 0);
                }
            }
        }
       
    }
}


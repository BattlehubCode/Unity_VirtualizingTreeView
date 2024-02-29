using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Battlehub.UIControls
{
    public delegate void ItemEventHandler(ItemContainer sender, PointerEventData eventData);

    [RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
    public class ItemContainer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public bool CanDrag = true;
        public bool CanEdit = true;
        public bool CanDrop = true;

        public static event EventHandler Selected;
        public static event EventHandler Unselected;
        public static event ItemEventHandler PointerDown;
        public static event ItemEventHandler PointerUp;
        public static event ItemEventHandler DoubleClick;
        public static event ItemEventHandler PointerEnter;
        public static event ItemEventHandler PointerExit;
        public static event ItemEventHandler BeginDrag;
        public static event ItemEventHandler Drag;
        public static event ItemEventHandler Drop;
        public static event ItemEventHandler EndDrag;
        public static event EventHandler BeginEdit;
        public static event EventHandler EndEdit;

        public GameObject ItemPresenter;
        public GameObject EditorPresenter;

        private LayoutElement m_layoutElement;
        public LayoutElement LayoutElement
        {
            get { return m_layoutElement; }
        }

        private RectTransform m_rectTransform;
        public RectTransform RectTransform
        {
            get { return m_rectTransform; }
        }

        protected bool m_isSelected;
        public virtual bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    if (m_isSelected)
                    {
                        if (Selected != null)
                        {
                            Selected(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        if (Unselected != null)
                        {
                            Unselected(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        [SerializeField]
        private bool m_isEditing;
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if(m_isEditing != value && m_isSelected)
                {
                    m_isEditing = value && m_isSelected;

                    if(EditorPresenter != ItemPresenter)
                    {
                        if (EditorPresenter != null)
                        {
                            EditorPresenter.SetActive(m_isEditing);
                        }

                        if (ItemPresenter != null)
                        {
                            ItemPresenter.SetActive(!m_isEditing);
                        }
                    }
                  
                    if(m_isEditing)
                    {
                        if(BeginEdit != null)
                        {
                            BeginEdit(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        if (EndEdit != null)
                        {
                            EndEdit(this, EventArgs.Empty);
                        }
                    }
                    
                }
            }
        }

        private ItemsControl m_itemsControl;
        private ItemsControl ItemsControl
        {
            get
            {
                if (m_itemsControl == null)
                {
                    m_itemsControl = GetComponentInParent<ItemsControl>();
                }

                return m_itemsControl;
            }
        }

        public object Item
        {
            get;
            set;
        }

        private bool m_canBeginEdit;
        private IEnumerator m_coBeginEdit;

        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_layoutElement = GetComponent<LayoutElement>();
            if (ItemPresenter == null)
            {
                ItemPresenter = gameObject;
            }

            if (EditorPresenter == null)
            {
                EditorPresenter = gameObject;
            }
            AwakeOverride();
        }
        private void Start()
        {
            StartOverride();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            m_coBeginEdit = null;
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride()
        {
            
        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        public virtual void Clear()
        {
            m_isEditing = false;
            if (EditorPresenter != ItemPresenter)
            {
                if (EditorPresenter != null)
                {
                    EditorPresenter.SetActive(m_isEditing);
                }

                if (ItemPresenter != null)
                {
                    ItemPresenter.SetActive(!m_isEditing);
                }
            }
            m_isSelected = false;
            Item = null;
        }

        private IEnumerator CoBeginEdit()
        {
            yield return new WaitForSeconds(0.5f);
            m_coBeginEdit = null;
            IsEditing = CanEdit;
        }

      
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            m_canBeginEdit = m_isSelected && ItemsControl != null && ItemsControl.SelectedItemsCount == 1 && ItemsControl.CanEdit;
            if (PointerDown != null)
            {
                PointerDown(this, eventData);
            }
        }


        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if(eventData.clickCount == 2)
            {
                if(DoubleClick != null)
                {
                    DoubleClick(this, eventData);
                }

                if(CanEdit)
                {
                    if (m_coBeginEdit != null)
                    {
                        StopCoroutine(m_coBeginEdit);
                        m_coBeginEdit = null;
                    }
                }
            }
            else
            {
                if(m_canBeginEdit)
                {
                    if (m_coBeginEdit == null)
                    {
                        m_coBeginEdit = CoBeginEdit();
                        StartCoroutine(m_coBeginEdit);
                    }
                }

                if (PointerUp != null)
                {
                    PointerUp(this, eventData);
                }
            }
            
             
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
                return;
            }

            m_canBeginEdit = false;

            if (BeginDrag != null)
            {
                BeginDrag(this, eventData);
            }
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            //if (!CanDrag)
            //{
            //    ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dropHandler);
            //    return;
            //}
            if (Drop != null)
            {
                Drop(this, eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!CanDrag)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
                return;
            }
            if (Drag != null)
            {
                Drag(this, eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!CanDrag)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
                return;
            }
            if (EndDrag != null)
            {
                EndDrag(this, eventData);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(PointerEnter != null)
            {
                PointerEnter(this, eventData);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if(PointerExit != null)
            {
                PointerExit(this, eventData);
            }
        }

    }

}

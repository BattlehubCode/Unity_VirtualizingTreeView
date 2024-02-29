using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public delegate void VirtualizingItemEventHandler(VirtualizingItemContainer sender, PointerEventData eventData);

    /// <summary>
    /// Visual representation of data item
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
    public class VirtualizingItemContainer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IPointerClickHandler
    {
        [HideInInspector]
        public bool CanDrag = true;
        [HideInInspector]
        public bool CanEdit = true;
        [HideInInspector]
        public bool CanBeParent = true;
        [HideInInspector]
        public bool CanSelect = true;
        [HideInInspector]
        public bool CanChangeParent = true;
        
        public static event EventHandler Selected;
        public static event EventHandler Unselected;
        public static event VirtualizingItemEventHandler PointerDown;
        public static event VirtualizingItemEventHandler PointerUp;
        public static event VirtualizingItemEventHandler DoubleClick;
        public static event VirtualizingItemEventHandler Click;
        public static event VirtualizingItemEventHandler PointerEnter;
        public static event VirtualizingItemEventHandler PointerExit;
        public static event VirtualizingItemEventHandler BeginDrag;
        public static event VirtualizingItemEventHandler Drag;
        public static event VirtualizingItemEventHandler Drop;
        public static event VirtualizingItemEventHandler EndDrag;
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

        private bool m_isEditing;
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if(Item == null)
                {
                    return;
                }

                if (m_isEditing != value && m_isSelected)
                {
                    m_isEditing = value && m_isSelected;

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

                    if (m_isEditing)
                    {
                        if (BeginEdit != null)
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

        private VirtualizingItemsControl m_itemsControl;
        protected VirtualizingItemsControl ItemsControl
        {
            get
            {
                if (m_itemsControl == null)
                {
                    m_itemsControl = GetComponentInParent<VirtualizingItemsControl>();
                    if(m_itemsControl == null)
                    {
                        Transform parent = transform.parent;
                        while(parent != null)
                        {
                            m_itemsControl = parent.GetComponent<VirtualizingItemsControl>();
                            if(m_itemsControl != null)
                            {
                                break;
                            }

                            parent = parent.parent;
                        }
                    }
                }

                return m_itemsControl;
            }
        }

        private object m_item;
        public virtual object Item
        {
            get { return m_item; }
            set
            {
                m_item = value;

                if(m_isEditing)
                {
                    if(EditorPresenter != null)
                    {
                        EditorPresenter.SetActive(m_item != null);
                    }
                    
                }
                else
                {
                    if(ItemPresenter != null)
                    {
                        ItemPresenter.SetActive(m_item != null);
                    }
                }   

                if(m_item == null)
                {
                    IsSelected = false;
                }
            }
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

            ItemsControl.UpdateContainerSize(this);
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
                    EditorPresenter.SetActive(m_item != null && m_isEditing);
                }

                if (ItemPresenter != null)
                {
                    ItemPresenter.SetActive(m_item != null && !m_isEditing);
                }
            }

            if(m_item == null)
            {
                IsSelected = false;
            }
            //m_isSelected = false;
           // Item = null;
        }

        private IEnumerator CoBeginEdit()
        {
            yield return new WaitForSeconds(0.5f);
            m_coBeginEdit = null;
            if(m_itemsControl.IsSelected && m_itemsControl.IsFocused)
            {
                IsEditing = CanEdit;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            m_canBeginEdit = m_isSelected && ItemsControl != null && ItemsControl.SelectedItemsCount == 1 && ItemsControl.CanEdit;

            if(!CanSelect)
            {
                return;
            }
            if (PointerDown != null)
            {
                PointerDown(this, eventData);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {            
            if (eventData.clickCount == 2)
            {
                if (DoubleClick != null)
                {
                    DoubleClick(this, eventData);
                }

                if (CanEdit && eventData.button == PointerEventData.InputButton.Left)
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
                if (m_canBeginEdit && eventData.button == PointerEventData.InputButton.Left)
                {
                    if (m_coBeginEdit == null)
                    {
                        m_coBeginEdit = CoBeginEdit();
                        StartCoroutine(m_coBeginEdit);
                    }
                }

                if (!CanSelect)
                {
                    return;
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
            if (PointerEnter != null)
            {
                PointerEnter(this, eventData);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (PointerExit != null)
            {
                PointerExit(this, eventData);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(Click != null)
            {
                Click(this, eventData);
            }
        }
    }
}

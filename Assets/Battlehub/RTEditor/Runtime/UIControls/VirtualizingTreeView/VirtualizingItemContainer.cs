using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Battlehub.UIControls
{
    public delegate void VirtualizingItemEventHandler(VirtualizingItemContainer sender, PointerEventData eventData);

    /// <summary>
    /// Visual representation of data item
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
    public class VirtualizingItemContainer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IPointerClickHandler
    {
        [HideInInspector, SerializeField, FormerlySerializedAs("CanDrag")]
        private bool m_canDrag = true;
        public virtual bool CanDrag
        {
            get { return m_canDrag; }
            set { m_canDrag = value; }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("CanBeParent")]
        private bool m_canBeParent = true;
        public virtual bool CanBeParent
        {
            get { return m_canBeParent; }
            set { m_canBeParent = value; }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("CanChangeParent")]
        private bool m_canChangeParent = true;
        public virtual bool CanChangeParent
        {
            get { return m_canChangeParent; }
            set { m_canChangeParent = value; }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("CanEdit")]
        private bool m_canEdit = true;
        public virtual bool CanEdit
        {
            get { return m_canEdit; }
            set { m_canEdit = value; }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("CanSelect")]
        private bool m_canSelect = true;
        public virtual bool CanSelect
        {
            get { return m_canSelect; }
            set { m_canSelect = value; }
        }

        public static event EventHandler Selected;
        public static event EventHandler Unselected;
        public static event VirtualizingItemEventHandler PointerDown;
        public static event VirtualizingItemEventHandler Hold;
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

        private bool m_wasEditing;
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
                    m_wasEditing = m_isEditing;
                    if(m_wasEditing)
                    {
                        if(m_coResetWasEditing != null)
                        {
                            StopCoroutine(m_coResetWasEditing);
                        }
                        m_coResetWasEditing = CoResetWasEditing();
                        if (gameObject.activeSelf)
                        {
                            StartCoroutine(m_coResetWasEditing);
                        }
                        else
                        {
                            ResetWasEditing();
                        }
                    }

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

        public event EventHandler ItemChanged;
        private object m_item;
        public virtual object Item
        {
            get { return m_item; }
            set
            {
                object oldItem = m_item;
                if (m_item != value)
                {
                    m_item = value;
                    ItemChanged?.Invoke(this, EventArgs.Empty);
                }

                if (m_item == null)
                {
                    IsSelected = false;
                }
            }
        }


        private bool m_hold;
        private IEnumerator m_coHold;

        private bool m_canBeginEdit;
        private IEnumerator m_coBeginEdit;

        private IEnumerator m_coResetWasEditing;
        private IEnumerator CoResetWasEditing()
        {
            yield return new WaitForSeconds(0.5f);
            ResetWasEditing();
        }

        private void ResetWasEditing()
        {
            m_wasEditing = false;
            m_coResetWasEditing = null;
        }

        private bool m_isDragInProgress;
        
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
            m_coResetWasEditing = null;
            m_coHold = null;
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride()
        {

        }

        protected virtual void StartOverride()
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

        protected virtual void OnDestroyOverride()
        {

        }

        public virtual void Clear()
        {
            m_isEditing = false;
            //if (EditorPresenter != ItemPresenter && m_isStarted)
            //{
            //    if (EditorPresenter != null)
            //    {
            //        EditorPresenter.SetActive(m_item != null && m_isEditing);
            //    }

            //    if (ItemPresenter != null)
            //    {
            //        ItemPresenter.SetActive(m_item != null && !m_isEditing);
            //    }
            //}

            if(m_item == null)
            {
                IsSelected = false;
            }
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
            OnPointerDownOverride(eventData);
        }

        private protected virtual void OnPointerDownOverride(PointerEventData eventData)
        {
            m_canBeginEdit = m_isSelected && ItemsControl != null && ItemsControl.SelectedItemsCount == 1 && ItemsControl.CanEdit;

            if (!CanSelect)
            {
                return;
            }

            if (PointerDown != null)
            {
                PointerDown(this, eventData);
            }

            if (m_coHold != null)
            {
                StopCoroutine(m_coHold);
            }

            m_coHold = CoHold(eventData);
            StartCoroutine(m_coHold);
        }

        private IEnumerator CoHold(PointerEventData eventData)
        {
            yield return new WaitForSeconds(0.5f);

            if (Hold != null)
            {
                Hold(this, eventData);
                m_hold = true;
                m_coHold = null;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpOverride(eventData);
        }

        private protected virtual void OnPointerUpOverride(PointerEventData eventData)
        {
            bool hold = m_hold;
            m_hold = false;

            if (m_coHold != null)
            {
                StopCoroutine(m_coHold);
                m_coHold = null;
            }

            if (eventData.clickCount == 2)
            {
                OnDoubleClick(eventData);
            }
            else
            {
                VirtualizingItemsControl control = GetComponentInParent<VirtualizingItemsControl>();
                if (control != null && control.InputProvider != null && control.InputProvider.TouchCount == 1 && control.InputProvider.GetTouch(0).tapCount == 2)
                {
                    OnDoubleClick(eventData);
                }
                else
                {
                    if (!hold)
                    {
                        if (m_canBeginEdit && eventData.button == PointerEventData.InputButton.Left && !m_wasEditing)
                        {
                            if (m_coBeginEdit == null)
                            {
                                m_coBeginEdit = CoBeginEdit();
                                StartCoroutine(m_coBeginEdit);
                            }
                        }
                    }

                    m_wasEditing = false;
                    if (m_coResetWasEditing != null)
                    {
                        StopCoroutine(m_coResetWasEditing);
                        m_coResetWasEditing = null;
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

        private void OnDoubleClick(PointerEventData eventData)
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

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragOverride(eventData);
        }

        private protected virtual void OnBeginDragOverride(PointerEventData eventData)
        {
            if (!CanDrag)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
                return;
            }

            m_canBeginEdit = false;
            m_isDragInProgress = true;

            if (BeginDrag != null)
            {
                BeginDrag(this, eventData);
            }
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            m_isDragInProgress = false;
            if (Drop != null)
            {
                Drop(this, eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!m_isDragInProgress)
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
            OnEndDragOverride(eventData);
        }

        private protected virtual void OnEndDragOverride(PointerEventData eventData)
        {
            if (!m_isDragInProgress)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
                return;
            }

            m_isDragInProgress = false;
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
            OnPointerClickOverride(eventData);
        }

        private protected virtual void OnPointerClickOverride(PointerEventData eventData)
        {
            if (eventData.clickCount == 1)
            {
                if (Click != null)
                {
                    Click(this, eventData);
                }
            }
        }
    }
}

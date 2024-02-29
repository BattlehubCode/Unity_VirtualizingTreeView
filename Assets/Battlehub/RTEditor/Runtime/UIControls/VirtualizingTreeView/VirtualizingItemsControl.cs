

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class VirtualizingItemsControl<TDataBindingArgs> : VirtualizingItemsControl where TDataBindingArgs : ItemDataBindingArgs, new()
    {
        /// <summary>
        /// Raised when item data binding required
        /// </summary>
        public event EventHandler<TDataBindingArgs> ItemDataBinding;


        /// <summary>
        /// Raised when itemContainer enters editing mode and EditorPresenter became visible 
        /// </summary>
        public event EventHandler<TDataBindingArgs> ItemBeginEdit;

        /// <summary>
        /// Raised when itemContainer exits editing mode and ItemPresenter became visible 
        /// </summary>
        public event EventHandler<TDataBindingArgs> ItemEndEdit;

        protected override void OnItemBeginEdit(object sender, EventArgs e)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            VirtualizingItemContainer itemContainer = (VirtualizingItemContainer)sender;
            if (ItemBeginEdit != null)
            {
                TDataBindingArgs args = new TDataBindingArgs();
                args.Item = itemContainer.Item;
                args.ItemPresenter = itemContainer.ItemPresenter == null ? itemContainer.gameObject : itemContainer.ItemPresenter;
                args.EditorPresenter = itemContainer.EditorPresenter == null ? itemContainer.gameObject : itemContainer.EditorPresenter;
                ItemBeginEdit(this, args);
            }
        }

        protected override void OnItemEndEdit(object sender, EventArgs e)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            VirtualizingItemContainer itemContainer = (VirtualizingItemContainer)sender;
            if (ItemBeginEdit != null)
            {
                TDataBindingArgs args = new TDataBindingArgs();
                args.Item = itemContainer.Item;
                args.ItemPresenter = itemContainer.ItemPresenter == null ? itemContainer.gameObject : itemContainer.ItemPresenter;
                args.EditorPresenter = itemContainer.EditorPresenter == null ? itemContainer.gameObject : itemContainer.EditorPresenter;
                ItemEndEdit(this, args);
            } 
            IsSelected = true;
        }

        public override void DataBindItem(object item, VirtualizingItemContainer itemContainer)
        {
            TDataBindingArgs args = new TDataBindingArgs();
            args.Item = item;
            args.ItemPresenter = itemContainer.ItemPresenter == null ? itemContainer.gameObject : itemContainer.ItemPresenter;
            args.EditorPresenter = itemContainer.EditorPresenter == null ? itemContainer.gameObject : itemContainer.EditorPresenter;

            itemContainer.Clear();

            if(item != null)
            {
                RaiseItemDataBinding(args);

                itemContainer.CanEdit = args.CanEdit;
                itemContainer.CanDrag = args.CanDrag;
                itemContainer.CanBeParent = args.CanBeParent;
                itemContainer.CanChangeParent = args.CanChangeParent;
                itemContainer.CanSelect = args.CanSelect;
            }
            else
            {
                itemContainer.CanEdit = false;
                itemContainer.CanDrag = false;
                itemContainer.CanBeParent = false;
                itemContainer.CanChangeParent = false;
                itemContainer.CanSelect = false;
            }
        }

        protected void RaiseItemDataBinding(TDataBindingArgs args)
        {
            if (ItemDataBinding != null)
            {
                ItemDataBinding(this, args);
            }
        } 
    }

    public class ItemContainerData
    {
        public bool IsSelected
        {
            get;
            set;
        }


        public object Item
        {
            get;
            set;
        }
    }

    public class PointerEventArgs : EventArgs
    {
        public PointerEventData Data
        {
            get;
            private set;
        }

        public PointerEventArgs(PointerEventData data)
        {
            Data = data;
        }
    }

    public class VirtualizingItemsControl : Selectable, IBeginDragHandler,  IDropHandler, IEndDragHandler, IUpdateSelectedHandler, IUpdateFocusedHandler, IPointerClickHandler
    {
        /// <summary>
        /// Raised on begin drag
        /// </summary>
        public event EventHandler<ItemArgs> ItemBeginDrag;

        /// <summary>
        /// Reased on before item dropped
        /// </summary>
        public event EventHandler<ItemDropCancelArgs> ItemBeginDrop;

        /// <summary>
        /// Raised when pointer enter new drop target
        /// </summary>
        public event EventHandler<ItemDropCancelArgs> ItemDragEnter;

        /// <summary>
        /// Raised when pointer leaves drop area.
        /// </summary>
        public event EventHandler ItemDragExit;

        /// <summary>
        /// Fired while dragging an item
        /// </summary>
        public event EventHandler<ItemArgs> ItemDrag;

        /// <summary>
        /// Raised on after item dropped
        /// </summary>
        public event EventHandler<ItemDropArgs> ItemDrop;


        /// <summary>
        /// Raised when drag operation completed
        /// </summary>
        public event EventHandler<ItemArgs> ItemEndDrag;

        /// <summary>
        /// Raised on selection changed
        /// </summary>
        public event EventHandler<SelectionChangedArgs> SelectionChanged;

        /// <summary>
        /// triggered after double clicking on an item
        /// </summary>
        public event EventHandler<ItemArgs> ItemDoubleClick;

        /// <summary>
        /// Raised on item click
        /// </summary>
        public event EventHandler<ItemArgs> ItemClick;

        /// <summary>
        /// Raise before items removed
        /// </summary>
        public event EventHandler<ItemsCancelArgs> ItemsRemoving;

        /// <summary>
        /// Raised when item removed
        /// </summary>
        public event EventHandler<ItemsRemovedArgs> ItemsRemoved;

        /// <summary>
        /// Raised when IsFocused value changed
        /// </summary>
        public event EventHandler IsFocusedChanged;
        public event EventHandler Submit;
        public event EventHandler Cancel;
        public event EventHandler<PointerEventArgs> Click;
        public event EventHandler<PointerEventArgs> PointerEnter;
        public event EventHandler<PointerEventArgs> PointerExit;

        [SerializeField]
        public EventSystem m_eventSystem;

        public InputProvider InputProvider;
        private bool m_isDragInProgress;
        public bool SelectOnPointerUp = false;
        public bool SelectUsingLeftButtonOnly = false;
        public bool CanUnselectAll = true;
        public bool CanSelectAll = true;
        public bool CanMultiSelect = true;
        public bool CanEdit = true;
        public bool CanRemove = true;
        private bool m_prevCanDrag;
        public bool CanDrag = true;
        public bool CanReorder = true;
        protected virtual bool CanScroll
        {
            get { return CanReorder; }
        }

        public bool ExpandChildrenWidth = true;
        public bool ExpandChildrenHeight;

        
        private bool m_isDropInProgress;
        /// <summary>
        /// Protection against stack overflow
        /// </summary>
        public bool IsDropInProgress
        {
            get { return m_isDropInProgress; }
        }

        private List<object> m_selectionBackup;
        /// <summary>
        /// Navigate VirtualizingItems if is focused
        /// </summary>
        private bool m_isFocused;
        
        public bool IsFocused
        {
            get { return m_isFocused; }
            set
            {
                if(m_isFocused != value)
                {
                    m_isFocused = value;

                    if(IsFocusedChanged != null)
                    {
                        IsFocusedChanged(this, EventArgs.Empty);
                    }
                    
                    if(m_isFocused)
                    {
                        if (SelectedIndex == -1 && m_scrollRect.ItemsCount > 0 && !CanUnselectAll)
                        {
                            SelectedIndex = 0;
                        }
                    }
                }   
            }
        }

        private bool m_isSelected;

        public bool IsSelected
        {
            get { return m_isSelected; }
            protected set
            {
                if(m_isSelected != value)
                {
                    m_isSelected = value;
                
                    if (m_isSelected)
                    {
                        m_selectionBackup = m_selectedItems;
                    }
                    else
                    {
                        m_selectionBackup = null;
                    }

                    if (!m_isSelected)
                    {
                        IsFocused = false;
                    }
                }
            }
        }

        /// <summary>
        /// Root canvas
        /// </summary>
        private Canvas m_canvas;
        private CanvasGroup m_canvasGroup;

        /// <summary>
        /// Raycasting camera (used with world space canvas)
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// ScrollView scroll speed during drag drop operaiton
        /// </summary>
        public float ScrollSpeed = 100;
        public Vector4 ScrollMargin = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
        private enum ScrollDir
        {
            None,
            Up,
            Down,
            Left,
            Right
        }
        private ScrollDir m_scrollDir;

        private PointerEnterExitListener m_pointerEventsListener;

        /// <summary>
        /// ScrollView RectTransformChange listener
        /// </summary>
        private RectTransformChangeListener m_rtcListener;

        /// <summary>
        /// Viewport effective width
        /// </summary>
        private float m_width;

        /// <summary>
        /// Viewport effective height
        /// </summary>
        private float m_height;

        private VirtualizingItemDropMarker m_dropMarker;
        private Repeater m_repeater;

        private bool m_externalDragOperation;

        /// <summary>
        /// drop target
        /// </summary>
        private VirtualizingItemContainer m_dropTarget;

        public object DropTarget
        {
            get
            {
                if (m_dropTarget == null)
                {
                    return null;
                }
                return m_dropTarget.Item;
            }
        }

        public void ClearTarget()
        {
            m_dropMarker.SetTarget(null);
        }

        public ItemDropAction DropAction
        {
            get
            {
                if (m_dropMarker == null)
                {
                    return ItemDropAction.None;
                }
                return m_dropMarker.Action;
            }
        }

        /// <summary>
        /// drag items
        /// </summary>
        private ItemContainerData[] m_dragItems;
        private object[] m_dragItemsData;
        public object[] DragItems
        {
            get { return m_dragItems; }
        }

        protected VirtualizingItemDropMarker DropMarker
        {
            get { return m_dropMarker; }
        }

        /// <summary>
        /// Selected Items Count
        /// </summary>
        public int SelectedItemsCount
        {
            get
            {
                if (m_selectedItems == null)
                {
                    return 0;
                }

                return m_selectedItems.Count;
            }
        }

        /// <summary>
        /// SelectedItems property setter is not reenterant
        /// </summary>
        private bool m_selectionLocked;
        private List<object> m_selectedItems;
        private HashSet<object> m_selectedItemsHS;
        public bool IsItemSelected(object obj)
        {
            if (m_selectedItemsHS == null)
            {
                return false;
            }
            return m_selectedItemsHS.Contains(obj);
        }


        /// <summary>
        /// Selected DataItems
        /// </summary>
        public virtual IEnumerable SelectedItems
        {
            get { return m_selectedItems; }
            set
            {
                if (m_selectionLocked)
                {
                    return;
                }
                m_selectionLocked = true;

                IList oldSelectedItems = m_selectedItems;
                if (value != null)
                {
                    m_selectedItems = value.OfType<object>().ToList();
                    m_selectedItemsHS = new HashSet<object>(m_selectedItems);

                    for (int i = m_selectedItems.Count - 1; i >= 0; --i)
                    {
                        object item = m_selectedItems[i];

                        ItemContainerData selectData;
                        if(m_itemContainerData.TryGetValue(item, out selectData))
                        {
                            selectData.IsSelected = true;
                        }

                        VirtualizingItemContainer container = GetItemContainer(item);
                        if (container != null)
                        {
                            container.IsSelected = true;
                        }
                    }
                    if (m_selectedItems.Count == 0)
                    {
                        m_selectedIndex = -1;
                    }
                    else
                    {
                        m_selectedIndex = IndexOf(m_selectedItems[0]);
                    }
                }
                else
                {
                    m_selectedItems = null;
                    m_selectedItemsHS = null;
                    m_selectedIndex = -1;
                }

                List<object> unselectedItems = new List<object>();
                if (oldSelectedItems != null)
                {
                    for (int i = 0; i < oldSelectedItems.Count; ++i)
                    {
                        object oldItem = oldSelectedItems[i];
                        if (m_selectedItemsHS == null || !m_selectedItemsHS.Contains(oldItem))
                        {
                            ItemContainerData unselectData;
                            if (m_itemContainerData.TryGetValue(oldItem, out unselectData))
                            {
                                unselectData.IsSelected = false;
                            }
                            unselectedItems.Add(oldItem);
                            VirtualizingItemContainer container = GetItemContainer(oldItem);
                            if (container != null)
                            {
                                container.IsSelected = false;
                            }
                        }
                    }
                }

                bool selectionChanged = oldSelectedItems == null && m_selectedItems != null || oldSelectedItems != null && m_selectedItems == null ||
                    oldSelectedItems != null && m_selectedItems != null && oldSelectedItems.Count != m_selectedItems.Count;
                if(!selectionChanged && oldSelectedItems != null)
                {
                    for(int i = 0; i < oldSelectedItems.Count; ++i)
                    {
                        if(m_selectedItems[i] != oldSelectedItems[i])
                        {
                            selectionChanged = true;
                            break;
                        }
                    }
                }

                if(selectionChanged)
                {
                    if (SelectionChanged != null)
                    {
                        object[] selectedItems = m_selectedItems == null ? new object[0] : m_selectedItems.ToArray();
                        SelectionChanged(this, new SelectionChangedArgs(unselectedItems.ToArray(), selectedItems));
                    }
                }
                
                m_selectionLocked = false;
            }
        }

        public object SelectedItem
        {
            get
            {
                if (m_selectedItems == null || m_selectedItems.Count == 0)
                {
                    return null;
                }
                return m_selectedItems[0];
            }
            set
            {
                SelectedIndex = IndexOf(value);
            }
        }

        private int m_selectedIndex = -1;
        /// <summary>
        /// First selected item index (-1 if no items selected)
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                if (SelectedItem == null)
                {
                    return -1;
                }

                return m_selectedIndex;
            }
            set
            {
                if (m_selectedIndex == value)
                {
                    return;
                }

                if (m_selectionLocked)
                {
                    return;
                }
                m_selectionLocked = true;

                if (SelectedItem != null)
                {
                    ItemContainerData unselectData;
                    if(m_itemContainerData.TryGetValue(SelectedItem, out unselectData))
                    {
                        unselectData.IsSelected = false;
                    }                    
                }
                VirtualizingItemContainer oldItemContainer = GetItemContainer(SelectedItem);
                if (oldItemContainer != null)
                {
                    oldItemContainer.IsSelected = false;
                }

                m_selectedIndex = value;
                object newItem = null;
                if (m_selectedIndex >= 0 && m_selectedIndex < m_scrollRect.ItemsCount)
                {
                    newItem = m_scrollRect.Items[m_selectedIndex];
                    if (newItem != null)
                    {
                        ItemContainerData selectData;
                        if (m_itemContainerData.TryGetValue(newItem, out selectData))
                        {
                            selectData.IsSelected = true;
                        }
                    }
                    
                    VirtualizingItemContainer selectedItemContainer = GetItemContainer(newItem);
                    if (selectedItemContainer != null)
                    {
                        selectedItemContainer.IsSelected = true;
                    }
                }

                object[] newItems = newItem != null ? new[] { newItem } : new object[0];
                object[] oldItems = m_selectedItems == null ? new object[0] : m_selectedItems.Except(newItems).ToArray();
                for (int i = 0; i < oldItems.Length; ++i)
                {
                    object oldItem = oldItems[i];
                    if(oldItem != null)
                    {
                        ItemContainerData unselectData;
                        if (m_itemContainerData.TryGetValue(oldItem, out unselectData))
                        {
                            unselectData.IsSelected = false;
                        }
                    }
                    VirtualizingItemContainer container = GetItemContainer(oldItem);
                    if (container != null)
                    {
                        container.IsSelected = false;
                    }
                }

                m_selectedItems = newItems.ToList();
                m_selectedItemsHS = new HashSet<object>(m_selectedItems);
                if (SelectionChanged != null)
                {
                    SelectionChanged(this, new SelectionChangedArgs(oldItems, newItems));
                }

                m_selectionLocked = false;
            }
        }

        private Dictionary<object, ItemContainerData> m_itemContainerData = new Dictionary<object, ItemContainerData>();

        public int ItemsCount
        {
            get { return m_scrollRect.ItemsCount; }
        }

        public IEnumerable Items
        {
            get { return m_scrollRect.Items; }
            set
            {
                SetItems(value, true);
            }
        }

        public int VisibleItemIndex
        {
            get { return m_scrollRect.RoundedIndex; }
        }

        public int VisibleItemsCount
        {
            get { return m_scrollRect.VisibleItemsCount; }
        }

        public void SetItems(IEnumerable value, bool updateSelection)
        {
            if (value == null)
            {
                SelectedItems = null;

                m_scrollRect.Items = null;
                m_scrollRect.verticalNormalizedPosition = 1;
                m_scrollRect.horizontalNormalizedPosition = 0;
                m_itemContainerData = new Dictionary<object, ItemContainerData>();
            }
            else
            {
                List<object> items = value.OfType<object>().ToList();
                if(updateSelection)
                {
                    if (m_selectedItemsHS != null)
                    {
                        //SelectedItems = items.Where(item => m_selectedItemsHS.Contains(item)).ToArray();
                        m_selectedItems = items.Where(item => m_selectedItemsHS.Contains(item)).ToList();
                        if (m_selectedItems.Count > 0)
                        {
                            m_selectedIndex = IndexOf(m_selectedItems.First());
                        }
                        else
                        {
                            m_selectedIndex = -1;
                        }
                        
                    }
                    else
                    {
                        m_selectedIndex = -1;
                    }
                }

                m_itemContainerData = new Dictionary<object, ItemContainerData>();
                for (int i = 0; i < items.Count; ++i)
                {
                    m_itemContainerData.Add(items[i], InstantiateItemContainerData(items[i]));
                }

                if(m_scrollRect != null)
                {
                    m_scrollRect.Items = items;
                    if (updateSelection && !CanUnselectAll)
                    {
                        if (IsFocused && SelectedIndex == -1 && m_selectedItems != null && m_selectedItems.Count > 0)
                        {
                            SelectedIndex = 0;
                        }
                    }
                }
            }
        }


        protected virtual ItemContainerData InstantiateItemContainerData(object item)
        {
            ItemContainerData data = new ItemContainerData();
            data.Item = item;
            return data;
        }

        private VirtualizingScrollRect m_scrollRect;
        protected override void Awake()
        {
            base.Awake();            
            m_scrollRect = GetComponent<VirtualizingScrollRect>();
            if(m_scrollRect == null)
            {
                Debug.LogError("Scroll Rect is required");
            }
            
            m_scrollRect.ItemDataBinding += OnScrollRectItemDataBinding;
            m_dropMarker = GetComponentInChildren<VirtualizingItemDropMarker>(true);
            
            m_rtcListener = GetComponentInChildren<RectTransformChangeListener>();
            if (m_rtcListener != null)
            {
                m_rtcListener.RectTransformChanged += OnViewportRectTransformChanged;
            }

            m_pointerEventsListener = GetComponentInChildren<PointerEnterExitListener>();
            if(m_pointerEventsListener != null)
            {
                m_pointerEventsListener.PointerEnter += OnViewportPointerEnter;
                m_pointerEventsListener.PointerExit += OnViewportPointerExit;
            }

            if (Camera == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if(canvas != null)
                {
                    Camera = canvas.worldCamera;
                }
                
            }

            m_prevCanDrag = CanDrag;
            OnCanDragChanged();

           
            AwakeOverride();
        }

        protected override void Start()
        {
            base.Start();
            if (m_eventSystem == null)
            {
                m_eventSystem = EventSystem.current;
            }

            if (InputProvider == null)
            {
                InputProvider = GetComponent<InputProvider>();
                if(InputProvider == null)
                {
                    InputProvider = CreateInputProviderOverride();
                }
            }
            m_canvas = GetComponentInParent<Canvas>();
            m_canvasGroup = GetComponentInParent<CanvasGroup>();
            StartOverride();
        }

        protected virtual InputProvider CreateInputProviderOverride()
        {
            return gameObject.AddComponent<VirtualizingItemsControlInputProvider>();
        }

        private void Update()
        {
            if(m_canvasGroup != null && !m_canvasGroup.interactable)
            {
                return;
            }

            if (m_scrollDir != ScrollDir.None)
            {
                float verDelta = (m_scrollRect.content.rect.height - m_scrollRect.viewport.rect.height);
                float verOffset = 0;
                if (verDelta > 0)
                {
                    verOffset = (ScrollSpeed / 10.0f) * (1.0f / verDelta);
                }

                float horDelta = (m_scrollRect.content.rect.width - m_scrollRect.viewport.rect.width);
                float horOffset = 0;
                if (horDelta > 0)
                {
                    horOffset = (ScrollSpeed / 10.0f) * (1.0f / horDelta);
                }

                if (m_scrollDir == ScrollDir.Up)
                {
                    m_scrollRect.verticalNormalizedPosition += verOffset;
                    if (m_scrollRect.verticalNormalizedPosition > 1)
                    {
                        m_scrollRect.verticalNormalizedPosition = 1;
                        m_scrollDir = ScrollDir.None;
                    }
                }
                else if (m_scrollDir == ScrollDir.Down)
                {
                    m_scrollRect.verticalNormalizedPosition -= verOffset;
                    if (m_scrollRect.verticalNormalizedPosition < 0)
                    {
                        m_scrollRect.verticalNormalizedPosition = 0;
                        m_scrollDir = ScrollDir.None;
                    }
                }
                else if (m_scrollDir == ScrollDir.Left)
                {
                    m_scrollRect.horizontalNormalizedPosition -= horOffset;
                    if (m_scrollRect.horizontalNormalizedPosition < 0)
                    {
                        m_scrollRect.horizontalNormalizedPosition = 0;
                        m_scrollDir = ScrollDir.None;
                    }
                }
                if (m_scrollDir == ScrollDir.Right)
                {

                    m_scrollRect.horizontalNormalizedPosition += horOffset;
                    if (m_scrollRect.horizontalNormalizedPosition > 1)
                    {
                        m_scrollRect.horizontalNormalizedPosition = 1;
                        m_scrollDir = ScrollDir.None;
                    }
                }
            }

            if(InputProvider != null && IsSelected)
            {
                if (InputProvider.IsDeleteButtonDown && CanRemove)
                {
                    RemoveSelectedItems();
                }

                else if (InputProvider.IsSelectAllButtonDown && InputProvider.IsFunctionalButtonPressed && CanSelectAll)
                {
                    SelectedItems = m_scrollRect.Items;
                }
                else if(InputProvider.IsSubmitButtonDown)
                {
                    IsFocused = !IsFocused;
                    if(!IsFocused)
                    {
                        if(Submit != null)
                        {
                            Submit(this, EventArgs.Empty);
                        }
                    }
                }
                else if(InputProvider.IsCancelButtonDown)
                {
                    SelectedItems = m_selectionBackup;
                    IsFocused = false;
                    if(Cancel != null)
                    {
                        Cancel(this, EventArgs.Empty);
                    }
                }

                if(IsFocused)
                {
                    if(!Mathf.Approximately(InputProvider.VerticalAxis, 0))
                    {
                        if(InputProvider.IsVerticalButtonDown)
                        {
                            m_repeater = new Repeater(Time.time, 0, 0.4f, 0.05f, () =>
                            {
                                float ver = InputProvider.VerticalAxis;
                                if (ver < 0)
                                {
                                    if (m_scrollRect.Index + m_scrollRect.VisibleItemsCount > SelectedIndex + m_scrollRect.ContainersPerGroup)
                                    {
                                        if (SelectedIndex + m_scrollRect.ContainersPerGroup < m_scrollRect.ItemsCount)
                                        {
                                            SelectedIndex += m_scrollRect.ContainersPerGroup;
                                        }
                                    }
                                    else
                                    {
                                        m_scrollRect.Index += m_scrollRect.ContainersPerGroup;
                                        if(m_scrollRect.Index + m_scrollRect.VisibleItemsCount > SelectedIndex + m_scrollRect.ContainersPerGroup)
                                        {
                                            if(SelectedIndex + m_scrollRect.ContainersPerGroup < m_scrollRect.ItemsCount)
                                            {
                                                SelectedIndex += m_scrollRect.ContainersPerGroup;
                                            }
                                        }
                                    }
                                }
                                else if (ver > 0)
                                {
                                    if (m_scrollRect.Index < (SelectedIndex - (m_scrollRect.ContainersPerGroup - 1)))
                                    {
                                        SelectedIndex -= m_scrollRect.ContainersPerGroup;
                                    }
                                    else
                                    {
                                        m_scrollRect.Index -= m_scrollRect.ContainersPerGroup;
                                        if (m_scrollRect.Index < (SelectedIndex - (m_scrollRect.ContainersPerGroup - 1)))
                                        {
                                            SelectedIndex -= m_scrollRect.ContainersPerGroup;
                                        }
                                    }
                                }
                            });
                        }
                        if(m_repeater != null)
                        {
                            m_repeater.Repeat(Time.time);
                        }
                    }
                    else if (!Mathf.Approximately(InputProvider.HorizontalAxis, 0))
                    {
                        if(m_scrollRect.UseGrid)
                        {
                            if (InputProvider.IsHorizontalButtonDown)
                            {
                                m_repeater = new Repeater(Time.time, 0, 0.4f, 0.05f, () =>
                                {
                                    float hor = InputProvider.HorizontalAxis;
                                    if (hor > 0)
                                    {
                                        if (m_scrollRect.Index + m_scrollRect.VisibleItemsCount > SelectedIndex + m_scrollRect.ContainersPerGroup)
                                        {
                                            SelectedIndex++;
                                        }
                                        else
                                        {
                                            m_scrollRect.Index++;
                                            if (m_scrollRect.Index + m_scrollRect.VisibleItemsCount > SelectedIndex + 1)
                                            {
                                                if (SelectedIndex < m_scrollRect.ItemsCount - 1)
                                                {
                                                    SelectedIndex++;
                                                }
                                            }
                                        }
                                    }
                                    else if (hor < 0)
                                    {
                                        if (m_scrollRect.Index < SelectedIndex)
                                        {
                                            SelectedIndex--;
                                        }
                                        else
                                        {
                                            m_scrollRect.Index--;
                                            if (m_scrollRect.Index < SelectedIndex)
                                            {
                                                SelectedIndex--;
                                            }
                                        }
                                    }
                                });
                            }
                            m_repeater.Repeat(Time.time);
                        }
                    }
                    else
                    {
                        m_repeater = null;
                    }
                }
                

            }

            if (m_prevCanDrag != CanDrag)
            {
                OnCanDragChanged();
                m_prevCanDrag = CanDrag;
            }

            UpdateOverride();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            VirtualizingItemContainer.Selected += OnItemSelected;
            VirtualizingItemContainer.Unselected += OnItemUnselected;
            VirtualizingItemContainer.PointerUp += OnItemPointerUp;
            VirtualizingItemContainer.PointerDown += OnItemPointerDown;
            VirtualizingItemContainer.PointerEnter += OnItemPointerEnter;
            VirtualizingItemContainer.PointerExit += OnItemPointerExit;
            VirtualizingItemContainer.DoubleClick += OnItemDoubleClick;
            VirtualizingItemContainer.Click += OnItemClick;
            VirtualizingItemContainer.BeginEdit += OnItemBeginEdit;
            VirtualizingItemContainer.EndEdit += OnItemEndEdit;
            VirtualizingItemContainer.BeginDrag += OnItemBeginDrag;
            VirtualizingItemContainer.Drag += OnItemDrag;
            VirtualizingItemContainer.Drop += OnItemDrop;
            VirtualizingItemContainer.EndDrag += OnItemEndDrag;
           
            OnEnableOverride();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            VirtualizingItemContainer.Selected -= OnItemSelected;
            VirtualizingItemContainer.Unselected -= OnItemUnselected;
            VirtualizingItemContainer.PointerUp -= OnItemPointerUp;
            VirtualizingItemContainer.PointerDown -= OnItemPointerDown;
            VirtualizingItemContainer.PointerEnter -= OnItemPointerEnter;
            VirtualizingItemContainer.PointerExit -= OnItemPointerExit;
            VirtualizingItemContainer.DoubleClick -= OnItemDoubleClick;
            VirtualizingItemContainer.Click -= OnItemClick;
            VirtualizingItemContainer.BeginEdit -= OnItemBeginEdit;
            VirtualizingItemContainer.EndEdit -= OnItemEndEdit;
            VirtualizingItemContainer.BeginDrag -= OnItemBeginDrag;
            VirtualizingItemContainer.Drag -= OnItemDrag;
            VirtualizingItemContainer.Drop -= OnItemDrop;
            VirtualizingItemContainer.EndDrag -= OnItemEndDrag;

            IsFocused = false;
            
            OnDisableOverride();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(m_scrollRect != null)
            {
                m_scrollRect.ItemDataBinding -= OnScrollRectItemDataBinding;
            }

            if (m_rtcListener != null)
            {
                m_rtcListener.RectTransformChanged -= OnViewportRectTransformChanged;
            }

            if (m_pointerEventsListener != null)
            {
                m_pointerEventsListener.PointerEnter -= OnViewportPointerEnter;
                m_pointerEventsListener.PointerExit -= OnViewportPointerExit;
            }

            OnDestroyOverride();
        }

        /// <summary>
        /// Lifecycle method overrides
        /// </summary>
        protected virtual void AwakeOverride()
        {

        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void UpdateOverride()
        {

        }

        protected virtual void OnEnableOverride()
        {

        }

        protected virtual void OnDisableOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        /// <summary>
        /// Called when item selected
        /// </summary>
        private void OnItemSelected(object sender, EventArgs e)
        {
            if (m_selectionLocked)
            {
                return;
            }

            if (!CanHandleEvent(sender))
            {
                return;
            }

            VirtualizingItemContainer.Unselected -= OnItemUnselected;
            if (CanMultiSelect)
            {
                if (InputProvider.IsFunctionalButtonPressed)
                {
                    IList selectedItems = m_selectedItems != null ? m_selectedItems.ToList() : new List<object>();
                    selectedItems.Add(((VirtualizingItemContainer)sender).Item);
                    SelectedItems = selectedItems;
                }
                else if (InputProvider.IsFunctional2ButtonPressed)
                {
                    SelectRange((VirtualizingItemContainer)sender);
                }
                else
                {
                    SelectedIndex = IndexOf(((VirtualizingItemContainer)sender).Item);
                }
            }
            else
            {
                SelectedIndex = IndexOf(((VirtualizingItemContainer)sender).Item);
            }
            VirtualizingItemContainer.Unselected += OnItemUnselected;
        }

        /// <summary>
        /// Select Data Items Range between current selection and itemContainer inclusive
        /// </summary>
        /// <param name="itemContainer">last item container to be included in range selection</param>
        private void SelectRange(VirtualizingItemContainer itemContainer)
        {
            if (m_selectedItems != null && m_selectedItems.Count > 0)
            {
                List<object> selectedItems = new List<object>();
                int firstItemIndex = IndexOf(m_selectedItems[0]);

                object item = itemContainer.Item;
                int lastItemIndex = IndexOf(item);

                int minIndex = Mathf.Min(firstItemIndex, lastItemIndex);
                int maxIndex = Math.Max(firstItemIndex, lastItemIndex);

                selectedItems.Add(m_selectedItems[0]);
                for (int i = minIndex; i < firstItemIndex; ++i)
                {
                    selectedItems.Add(m_scrollRect.Items[i]);
                }
                for (int i = firstItemIndex + 1; i <= maxIndex; ++i)
                {
                    selectedItems.Add(m_scrollRect.Items[i]);
                }
                SelectedItems = selectedItems;

            }
            else
            {
                SelectedIndex = IndexOf(itemContainer.Item);
            }
        }

        /// <summary>
        /// Called when item unselected
        /// </summary>
        private void OnItemUnselected(object sender, EventArgs e)
        {
            if (m_selectionLocked)
            {
                return;
            }

            if (!CanHandleEvent(sender))
            {
                return;
            }

            IList selectedItems = m_selectedItems != null ? m_selectedItems.ToList() : new List<object>();
            selectedItems.Remove(((VirtualizingItemContainer)sender).Item);
            SelectedItems = selectedItems;
        }

        private void TryToSelect(VirtualizingItemContainer sender)
        {
            if (CanMultiSelect && InputProvider.IsFunctional2ButtonPressed)
            {
                if (sender.Item != null)
                {
                    SelectRange(sender);
                }
            }
            else if (CanMultiSelect && InputProvider.IsFunctionalButtonPressed)
            {
                if (sender.Item != null)
                {
                    sender.IsSelected = !sender.IsSelected;
                }
            }
            else
            {
                if (sender.Item != null)
                {
                    sender.IsSelected = true;
                }
                else if (CanUnselectAll)
                {
                    SelectedIndex = -1;
                }
            }

            m_eventSystem.SetSelectedGameObject(gameObject);
            IsFocused = true;
        }

        private void OnItemPointerDown(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            if (m_externalDragOperation)
            {
                return;
            }

            m_dropMarker.SetTarget(null);
            m_dragItems = null;
            m_dragItemsData = null;
            m_isDropInProgress = false;

            if (SelectUsingLeftButtonOnly && eventData.button != PointerEventData.InputButton.Left || sender.IsSelected && eventData.button == PointerEventData.InputButton.Right)
            {
                return;
            }

            if (!SelectOnPointerUp)
            {
                TryToSelect(sender);
            }
        }

        private void OnItemPointerUp(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            if (m_externalDragOperation)
            {
                return;
            }

            if (m_dragItems != null)
            {
                return;
            }

            if (SelectUsingLeftButtonOnly && eventData.button != PointerEventData.InputButton.Left || sender.IsSelected && eventData.button == PointerEventData.InputButton.Right)
            {
                return;
            }

            if (SelectOnPointerUp)
            {
                if (!m_isDropInProgress && !m_isDragInProgress)
                {
                    TryToSelect(sender);
                }
            }

            if (!InputProvider.IsFunctional2ButtonPressed && !InputProvider.IsFunctionalButtonPressed)
            {
                if (m_selectedItems != null && m_selectedItems.Count > 1)
                {
                    if (SelectedItem == sender.Item)
                    {
                        SelectedItem = null;
                    }
                    SelectedItem = sender.Item;
                }
            }
        }

        private void OnItemPointerEnter(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            m_dropTarget = sender;

            ItemDropCancelArgs args = null;
            if (m_dragItems != null && m_dragItems.Length > 0)
            {
                args = new ItemDropCancelArgs(m_dragItemsData, m_dropTarget.Item, m_dropMarker.Action, m_externalDragOperation, eventData);
                if (ItemDragEnter != null)
                {
                    ItemDragEnter(this, args);
                }
            }
            
            if (m_dragItems != null || m_externalDragOperation)
            {
                if (m_scrollDir == ScrollDir.None)
                {
                    if(args == null || !args.Cancel)
                    {
                        m_dropMarker.SetTarget(m_dropTarget);
                    }
                    else
                    {
                        m_dropMarker.SetTarget(null);
                    }
                }
            }
        }

        private void OnItemPointerExit(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }
            m_dropTarget = null;
            if (m_dragItems != null || m_externalDragOperation)
            {
                m_dropMarker.SetTarget(null);
            }

            if(m_dragItems != null)
            {
                if (ItemDragExit != null)
                {
                    ItemDragExit(this, EventArgs.Empty);
                }
            }
        }

        private void OnItemDoubleClick(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            if(sender.Item != null)
            {
                if (ItemDoubleClick != null)
                {
                    ItemDoubleClick(this, new ItemArgs(new[] { sender.Item }, eventData));
                }
            }
        }

        private void OnItemClick(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if(!CanHandleEvent(sender))
            {
                return;
            }
            
            if(sender.Item == null)
            {
                if(Click != null)
                {
                    Click(this, new PointerEventArgs(eventData));
                }
            }
            else
            {
                if (ItemClick != null)
                {
                    ItemClick(this, new ItemArgs(new[] { sender.Item }, eventData));
                }
            }
            
        }

        private void OnItemBeginDrag(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            if (m_selectedItems != null && m_selectedItems.Contains(sender.Item))
            {
                m_dragItems = GetDragItems();
            }
            else
            {
                m_dragItems = new[] { m_itemContainerData[sender.Item] };
            }

            m_dragItemsData = m_dragItems.Select(di => di.Item).ToArray();
            if (ItemBeginDrag != null)
            {
                ItemBeginDrag(this, new ItemArgs(m_dragItemsData, eventData));
            }
        
            if (m_dropTarget != null)
            {
                ItemDropCancelArgs args = new ItemDropCancelArgs(m_dragItemsData, m_dropTarget.Item, m_dropMarker.Action, m_externalDragOperation, eventData);
                if (ItemDragEnter != null)
                {
                    ItemDragEnter(this, args);
                }

                if(!args.Cancel)
                {
                    m_dropMarker.SetTarget(m_dropTarget);
                    m_dropMarker.SetPosition(eventData.position);
                }   
            }
            else
            {
                m_dropMarker.SetTarget(null);
            }

        }

        private void OnItemDrag(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }
            ExternalItemDrag(eventData.position);
            if(ItemDrag != null)
            {
                ItemDrag(this, new ItemArgs(m_dragItemsData, eventData));
            }

            float viewportHeight = m_scrollRect.viewport.rect.height;
            float viewportWidth = m_scrollRect.viewport.rect.width;

            Camera camera = null;
            if (m_canvas.renderMode == RenderMode.WorldSpace || m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                camera = Camera;
            }

            if (CanScroll)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollRect.viewport, eventData.position, camera, out localPoint))
                {
                    if (localPoint.y > 0 && localPoint.y < ScrollMargin.y)
                    {
                        m_scrollDir = ScrollDir.Up;
                        m_dropMarker.SetTarget(null);
                    }
                    else if (localPoint.y < -viewportHeight && localPoint.y > -(viewportHeight + ScrollMargin.w))
                    {
                        m_scrollDir = ScrollDir.Down;
                        m_dropMarker.SetTarget(null);
                    }
                    else if (localPoint.x < 0 && localPoint.x >= -ScrollMargin.x)
                    {
                        m_scrollDir = ScrollDir.Left;
                    }
                    else if (localPoint.x >= viewportWidth && localPoint.x < viewportWidth + ScrollMargin.z)
                    {
                        m_scrollDir = ScrollDir.Right;
                    }
                    else
                    {
                        m_scrollDir = ScrollDir.None;
                    }
                }
            }
            else
            {
                m_scrollDir = ScrollDir.None;
            }
        }

        private void OnItemDrop(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            m_isDropInProgress = true; //Prevent ChangeParent operation
            try
            {
                if (m_dragItems != null && m_dropTarget != null && CanDrop(m_dragItems, GetItemContainerData(m_dropTarget.Item)))
                {
                    bool cancel = false;
                    if (ItemBeginDrop != null)
                    {
                        ItemDropCancelArgs args = new ItemDropCancelArgs(m_dragItemsData, m_dropTarget.Item, m_dropMarker.Action, false, eventData);
                        if (ItemBeginDrop != null)
                        {
                            ItemBeginDrop(this, args);
                            cancel = args.Cancel;
                        }
                    }

                    if (!cancel)
                    {
                        object[] dragItems = m_dragItems != null ? m_dragItemsData : null;
                        object dropTarget = m_dropTarget != null ? m_dropTarget.Item : null;

                        ItemContainerData containerData = GetItemContainerData(dropTarget);
                        Drop(m_dragItems, containerData, m_dropMarker.Action);

                        if (ItemDrop != null)
                        {
                            if (dragItems != null && dropTarget != null && m_dropMarker != null)
                            {
                                ItemDrop(this, new ItemDropArgs(dragItems, dropTarget, m_dropMarker.Action, false, eventData));
                            }
                        }

                        DataBindVisible();
                      
                    }
                }
                RaiseEndDrag(eventData);
            }
            finally
            {
                m_isDropInProgress = false;
            }
        }

        private void OnItemEndDrag(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (m_dropTarget != null)
            {
                OnItemDrop(sender, eventData);
            }
            else
            {
                if (!CanHandleEvent(sender))
                {
                    return;
                }
                RaiseEndDrag(eventData);
            }
        }

        private void RaiseEndDrag(PointerEventData eventData)
        {
            if (m_dragItems != null)
            {
                if (ItemEndDrag != null)
                {
                    ItemEndDrag(this, new ItemArgs(m_dragItemsData, eventData));
                }
                m_dropMarker.SetTarget(null);
                m_dragItems = null;
                m_dragItemsData = null;
                m_scrollDir = ScrollDir.None;
            }
        }

        /// <summary>
        /// Viewport transform change event handler
        /// </summary>
        private void OnViewportRectTransformChanged()
        {
            if (ExpandChildrenHeight || ExpandChildrenWidth)
            {
                Rect viewportRect = m_scrollRect.viewport.rect;
                if (viewportRect.width != m_width || viewportRect.height != m_height)
                {
                    m_width = viewportRect.width;
                    m_height = viewportRect.height;

                    SetContainersSize();
                }
            }
        }
        private void OnViewportPointerEnter(object sender, PointerEventArgs e)
        {
            if(PointerEnter != null)
            {
                PointerEnter(this, e);
            }
        }

        private void OnViewportPointerExit(object sender, PointerEventArgs e)
        {
            if(PointerExit != null)
            {
                PointerExit(this, e);
            }
        }

        private void SetContainersSize()
        {
            m_scrollRect.ForEachContainer(c =>
            {
                VirtualizingItemContainer container = c.GetComponent<VirtualizingItemContainer>();
                UpdateContainerSize(container);
            });
        }

        public void UpdateContainerSize(VirtualizingItemContainer container)
        {
            if (container != null && container.LayoutElement != null)
            {
                if (ExpandChildrenWidth)
                {
                    container.LayoutElement.minWidth = m_width;
                }
                if (ExpandChildrenHeight)
                {
                    container.LayoutElement.minHeight = m_height;
                }
            }
        }

        /// <summary>
        /// Update all data containers with ItemsControl CanDrag field value
        /// </summary>
        private void OnCanDragChanged()
        {
            m_scrollRect.ForEachContainer(c =>
            {
                VirtualizingItemContainer container = c.GetComponent<VirtualizingItemContainer>();
                if(container != null)
                {
                    container.CanDrag = CanDrag;
                }
            });
        }

        /// <summary>
        /// Whether event can be handled by ItemsControl
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        protected bool CanHandleEvent(object sender)
        {
            if (sender is ItemContainerData)
            {
                ItemContainerData data = (ItemContainerData)sender;
                ItemContainerData ownData;
                if (m_itemContainerData.TryGetValue(data.Item, out ownData))
                {
                    return data == ownData;
                }
                return false;
            }

            VirtualizingItemContainer itemContainer = sender as VirtualizingItemContainer;
            if (!itemContainer)
            {
                return false;
            }
            return m_scrollRect.IsParentOf(itemContainer.transform);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            m_isDragInProgress = true;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            m_isDragInProgress = false;
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (!CanReorder)
            {
                return;
            }

            if (m_dragItems == null)
            {
                GameObject go = eventData.pointerDrag;
                if (go != null)
                {
                    ItemContainer itemContainer = go.GetComponent<ItemContainer>();
                    if (itemContainer != null && itemContainer.Item != null)
                    {
                        object item = itemContainer.Item;
                        if (ItemDrop != null)
                        {
                            ItemDrop(this, new ItemDropArgs(new[] { item }, null, ItemDropAction.SetLastChild, true, eventData));
                        }
                    }
                }
                return;
            }

            if (m_scrollRect.ItemsCount > 0)
            {
                RectTransform rt = m_scrollRect.LastContainer();
                if (rt != null)
                {
                    m_dropTarget = rt.GetComponent<VirtualizingItemContainer>();
                    if (m_dropTarget.Item == m_scrollRect.Items[m_scrollRect.Items.Count - 1])
                    {
                        m_dropMarker.Action = ItemDropAction.SetNextSibling;
                    }
                    else
                    {
                        m_dropTarget = null;
                    }
                }
            }

            if(m_dropTarget != null)
            {
                m_isDropInProgress = true; //Prevent ChangeParent operation
                try
                {
                    ItemContainerData data = GetItemContainerData(m_dropTarget.Item);
                    if (CanDrop(m_dragItems, data))
                    {
                        Drop(m_dragItems, data, m_dropMarker.Action);
                        if (ItemDrop != null)
                        {
                            ItemDrop(this, new ItemDropArgs(m_dragItemsData, m_dropTarget.Item, m_dropMarker.Action, false, eventData));
                        }
                    }

                    /*
                    m_dropMarker.SetTarget(null);
                    m_dragItems = null;
                    m_dragItemsData = null;
                    */
                }
                finally
                {
                    m_isDropInProgress = false;
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (CanUnselectAll)
            {
                SelectedIndex = -1;
            }

            m_eventSystem.SetSelectedGameObject(gameObject);
            IsFocused = true;
        }

        /// <summary>
        /// OnItemBeginEdit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnItemBeginEdit(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// OnItemEndEdit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnItemEndEdit(object sender, EventArgs e)
        {

        }

        public virtual void DataBindItem(object item)
        {
            VirtualizingItemContainer itemContainer = GetItemContainer(item);
            if(itemContainer != null)
            {
                DataBindItem(item, itemContainer);
            }
        }

        public virtual void DataBindVisible()
        {
            if(m_scrollRect == null)
            {
                return;
            }

            for(int i = VisibleItemIndex; i < VisibleItemIndex + VisibleItemsCount && i < m_scrollRect.ItemsCount; ++i)
            {
                object item = m_scrollRect.Items[i];
                DataBindItem(item);
            }
        }

        public virtual void DataBindItem(object item, VirtualizingItemContainer itemContainer)
        {
        }
        
        private void OnScrollRectItemDataBinding(RectTransform container, object item)
        {
            VirtualizingItemContainer itemContainer = container.GetComponent<VirtualizingItemContainer>();
            itemContainer.Item = item;
          
            if(item != null)
            {
                m_selectionLocked = true;
                ItemContainerData itemContainerData = m_itemContainerData[item];
                itemContainerData.IsSelected = IsItemSelected(item);

                itemContainer.IsSelected = itemContainerData.IsSelected;
                itemContainer.CanDrag = CanDrag;
                m_selectionLocked = false;
            }
            
            DataBindItem(item, itemContainer);

            if(m_scrollRect.ItemsCount == 1)
            {
                SetContainersSize();
            }
        }

        /// <summary>
        /// Index of dataitem
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int IndexOf(object obj)
        {
            if (m_scrollRect.Items == null)
            {
                return -1;
            }

            if (obj == null)
            {
                return -1;
            }

            return m_scrollRect.Items.IndexOf(obj);
        }

        public virtual void SetIndex(object obj, int newIndex)
        {
            int index = IndexOf(obj);
            if (index == -1)
            {
                return;
            }

            if (index == m_selectedIndex)
            {
                m_selectedIndex = newIndex;
            }

            if (index < newIndex)
            {
                m_scrollRect.SetNextSibling(GetItemAt(newIndex), obj);
            }
            else
            {
                m_scrollRect.SetPrevSibling(GetItemAt(newIndex), obj);
            }
        }


        /// <summary>
        /// Get Last Item Container
        /// </summary>
        /// <returns>last item container</returns>
        public ItemContainerData LastItemContainerData()
        {
            if (m_scrollRect.Items == null || m_scrollRect.ItemsCount == 0)
            {
                return null;
            }

            return GetItemContainerData(m_scrollRect.Items[m_scrollRect.ItemsCount - 1]);
        }

        public VirtualizingItemContainer GetItemContainer(object item)
        {
            if(item == null)
            {
                return null;
            }

            
            RectTransform container = m_scrollRect.GetContainer(item);
            if(container == null)
            {
                return null;
            }
            return container.GetComponent<VirtualizingItemContainer>();
        }


        public ItemContainerData GetItemContainerData(object item)
        {
            if(item == null)
            {
                return null;
            }

            ItemContainerData itemContainerData = null;
            m_itemContainerData.TryGetValue(item, out itemContainerData);

            return itemContainerData;
        }

        public ItemContainerData GetItemContainerData(int siblingIndex)
        {
            if(siblingIndex < 0 || m_scrollRect.Items.Count <= siblingIndex)
            {
                return null;
            }

            object item = m_scrollRect.Items[siblingIndex];
            return m_itemContainerData[item];
        }

        protected virtual bool CanDrop(ItemContainerData[] dragItems, ItemContainerData dropTarget)
        {
            if (dropTarget == null)
            {
                return true;
            }

            if (dragItems == null)
            {
                return false;
            }

            if (dragItems.Contains(dropTarget.Item))
            {
                return false;
            }

            return true;
        }

        protected ItemContainerData[] GetDragItems()
        {
            ItemContainerData[] dragItems = new ItemContainerData[m_selectedItems.Count];
            if (m_selectedItems != null)
            {
                for (int i = 0; i < m_selectedItems.Count; ++i)
                {
                    dragItems[i] = m_itemContainerData[m_selectedItems[i]];
                }
            }

            return dragItems.OrderBy(di => IndexOf(di.Item)).ToArray();
        }

        protected virtual void Drop(ItemContainerData[] dragItems, ItemContainerData dropTargetData, ItemDropAction action)
        {
            if (action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    ItemContainerData dragItemData = dragItems[i];
                    SetPrevSiblingInternal(dropTargetData, dragItemData);
                }
            }
            else if (action == ItemDropAction.SetNextSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    ItemContainerData dragItem = dragItems[i];
                    SetNextSiblingInternal(dropTargetData, dragItem);
                }
            }

            UpdateSelectedItemIndex();
        }

        protected virtual void SetNextSiblingInternal(ItemContainerData sibling, ItemContainerData nextSibling)
        {
            m_scrollRect.SetNextSibling(sibling.Item, nextSibling.Item);
        }

        protected virtual void SetPrevSiblingInternal(ItemContainerData sibling, ItemContainerData prevSibling)
        {
            m_scrollRect.SetPrevSibling(sibling.Item, prevSibling.Item);
        }

        protected void UpdateSelectedItemIndex()
        {
            m_selectedIndex = IndexOf(SelectedItem);
        }

        public void ExternalBeginDrag(Vector3 position)
        {
            if (!CanDrag)
            {
                return;
            }

            m_externalDragOperation = true;

            if (m_dropTarget == null)
            {
                return;
            }

            if (m_dragItems != null || m_externalDragOperation)
            {
                if (m_scrollDir == ScrollDir.None)
                {
                    m_dropMarker.SetTarget(m_dropTarget);
                }
                else
                {
                    m_dropMarker.SetTarget(null);
                }
            }
        }

        public void ExternalItemDrag(Vector3 position)
        {
            if (!CanDrag)
            {
                return;
            }

            if (m_dropTarget != null)
            {
                m_dropMarker.SetPosition(position);
            }
        }

        public void ExternalItemDrop()
        {
            if (!CanDrag)
            {
                return;
            }

            m_externalDragOperation = false;
            m_dropMarker.SetTarget(null);
        }

        public void RemoveSelectedItems()
        {
            if (m_selectedItems == null)
            {
                return;
            }

            object[] selectedItems;
            if (ItemsRemoving != null)
            {
                ItemsCancelArgs args = new ItemsCancelArgs(m_selectedItems.ToList());
                ItemsRemoving(this, args);
                if (args.Items == null)
                {
                    selectedItems = new object[0];
                }
                else
                {
                    selectedItems = args.Items.ToArray();
                }
            }
            else
            {
                selectedItems = m_selectedItems.ToArray();
            }


            if (selectedItems.Length == 0)
            {
                return;
            }

            DestroyItems(selectedItems, true);


            if (ItemsRemoved != null)
            {
                ItemsRemoved(this, new ItemsRemovedArgs(selectedItems));
            }
        }


        protected virtual void DestroyItems(object[] items, bool unselect)
        {
            if(unselect)
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    object item = items[i];
                    if (m_selectedItems != null && m_selectedItems.Contains(item))
                    {
                        //NOTE: Selection Changed Event is not raised 
                        m_selectedItems.Remove(item);
                        m_selectedItemsHS.Remove(item);
                        if (m_selectedItems.Count == 0)
                        {
                            m_selectedIndex = -1;
                        }
                        else
                        {
                            m_selectedIndex = IndexOf(m_selectedItems[0]);
                        }
                    }
                }
            }
           
            m_scrollRect.RemoveItems(items.Select( item => IndexOf(item)).ToArray());
            for(int i = 0; i < items.Length; ++i)
            {
                object item = items[i];
                m_itemContainerData.Remove(item);
            }
        }


        public ItemContainerData Add(object item)
        {
            return Insert(m_scrollRect.ItemsCount, item);
        }
        /// <summary>
        /// Insert data item (NOTE: if you insert more than one item consider using Items property)
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="item">data item</param>
        /// <returns>Item container for data item</returns>
        public virtual ItemContainerData Insert(int index, object item)
        {
            if(m_itemContainerData.ContainsKey(item))
            {
                return m_itemContainerData[item];
            }

            ItemContainerData itemContainerData = InstantiateItemContainerData(item);
            m_itemContainerData.Add(item, itemContainerData);
            m_scrollRect.InsertItem(index, item);
            return itemContainerData;
        }

        /// <summary>
        /// Set next sibling
        /// </summary>
        /// <param name="sibling"></param>
        /// <param name="nextSibling"></param>
        public void SetNextSibling(object sibling, object nextSibling)
        {
            ItemContainerData itemContainer = GetItemContainerData(sibling);
            if (itemContainer == null)
            {
                return;
            }

            ItemContainerData nextSiblingItemContainer = GetItemContainerData(nextSibling);
            if (nextSiblingItemContainer == null)
            {
                return;
            }

            Drop(new[] { nextSiblingItemContainer }, itemContainer, ItemDropAction.SetNextSibling);
        }


        /// <summary>
        /// Set previous sibling
        /// </summary>
        /// <param name="sibling"></param>
        /// <param name="nextSibling"></param>
        public void SetPrevSibling(object sibling, object prevSibling)
        {
            ItemContainerData itemContainer = GetItemContainerData(sibling);
            if (itemContainer == null)
            {
                return;
            }

            ItemContainerData prevSiblingItemContainer = GetItemContainerData(prevSibling);
            if (prevSiblingItemContainer == null)
            {
                return;
            }

            Drop(new[] { prevSiblingItemContainer }, itemContainer, ItemDropAction.SetPrevSibling);
        }

        protected virtual void Remove(object[] items)
        {
            items = items.Where(item => m_scrollRect.Items.Contains(item)).ToArray();
            if(items.Length == 0)
            {
                return;
            }

            if (ItemsRemoving != null)
            {
                ItemsCancelArgs args = new ItemsCancelArgs(items.ToList());
                ItemsRemoving(this, args);
                if (args.Items == null)
                {
                    items = new object[0];
                }
                else
                {
                    items = args.Items.ToArray();
                }
            }


            if (items.Length == 0)
            {
                return;
            }

            DestroyItems(items, true);

            if (ItemsRemoved != null)
            {
                ItemsRemoved(this, new ItemsRemovedArgs(items));
            }
        }

        public virtual void Remove(object item)
        {
            Remove(new[] { item });
        }

        public object GetItemAt(int index)
        {
            if(index < 0 || index >= m_scrollRect.Items.Count)
            {
                return null;
            }

            return m_scrollRect.Items[index];
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            IsSelected = true;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            IsSelected = false;
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            if(!IsFocused)
            {
                return;
            }
            
            eventData.Use();
        }

        public void OnUpdateFocused(BaseEventData eventData)
        {
            if (!IsFocused)
            {
                return;
            }

            eventData.Use();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(Click != null)
            {
                Click(this, new PointerEventArgs(eventData));
            }
        }
    }
}
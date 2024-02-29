using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    /// <summary>
    /// Virtualize Items Horizontally or Vertically
    /// </summary>
    public enum VirtualizingMode
    {
        Horizontal,
        Vertical, //NOTE: Only vertical mode was tested
    }

    public delegate void DataBindAction(RectTransform container, object item);

    /// <summary>
    /// This class re-use 'ui containers' to represent large data item collections. 
    /// 'ui containers' created for small visible portion of data items
    /// </summary>
    public class VirtualizingScrollRect : ScrollRect
    {
        /// <summary>
        /// raised if data item should be bound to ui container
        /// </summary>
        public event DataBindAction ItemDataBinding;

        /// <summary>
        /// ui container prefab (represents visible data item)
        /// </summary>
        public RectTransform ContainerPrefab;

        /// <summary>
        /// ui containers parent
        /// </summary>
        [SerializeField]
        private RectTransform m_virtualContent = null;

        private HorizontalOrVerticalLayoutGroup m_layoutGroup;

        /// <summary>
        /// Listen for virtualContent dimensions change
        /// </summary>
        private RectTransformChangeListener m_virtualContentTransformChangeListener;

        /// <summary>
        /// virtualizing mode (horizontal or vertical(default))
        /// </summary>
        [SerializeField]
        private VirtualizingMode m_mode = VirtualizingMode.Vertical;

        [SerializeField]
        private bool m_useGrid = false;
        public bool UseGrid
        {
            get { return m_useGrid; }
        }
        [SerializeField]
        private Vector2 m_gridSpacing = Vector2.zero;

        private GridLayoutGroup m_gridLayoutGroup;
        public int ContainersPerGroup
        {
            get
            {
                if(m_useGrid)
                {
                    return m_gridLayoutGroup.constraintCount;
                }
                return 1;
            }
            set
            {
                if(m_useGrid)
                {
                    m_gridLayoutGroup.constraintCount = value;
                    scrollSensitivity = ContainerSize;// * ContainersPerGroup;
                    UpdateContentSize();
                }
            }
        }
        

        /// <summary>
        /// linked list of ui containers
        /// </summary>
        private LinkedList<RectTransform> m_containers = new LinkedList<RectTransform>();

        /// <summary>
        /// data items
        /// </summary>
        private IList m_items;
        public IList Items
        {
            get { return m_items; }
            set
            { 
                if (m_items != value)
                {
                    m_items = value;
                    DataBind(RoundedIndex);
                    UpdateContentSize();
                }
            }
        }

        /// <summary>
        /// data items count
        /// </summary>
        public int ItemsCount
        {
            get
            {
                if(Items == null)
                {
                    return 0;
                }
                return Items.Count;
            }
        }

        private int RoundedItemsCount
        {
            get
            {
                return Mathf.CeilToInt(((float)ItemsCount) / ContainersPerGroup) * ContainersPerGroup;
                //return Mathf.RoundToInt(((float)ItemsCount) / ContainersPerGroup) * ContainersPerGroup;
            }
        }


        /// <summary>
        /// normalized index [0, 1] (basically the same as to normalizedPosition)
        /// </summary>
        private float m_normalizedIndex;
        private float NormalizedIndex
        {
            get
            {
                return m_normalizedIndex;
            }
            set
            {
                if (value == m_normalizedIndex)
                {
                    return;
                }

                OnNormalizedIndexChanged(value);
            }
        }

        public int Index
        {
            get { return RoundedIndex + LocalGroupIndex; }
            set { RoundedIndex = value; }
        }

        private int LocalGroupIndex
        {
            get
            {
                if (RoundedItemsCount == 0)
                {
                    return 0;
                }

                int index = Mathf.RoundToInt(NormalizedIndex * Mathf.Max((RoundedItemsCount - VisibleItemsCount), 0));
                index = index % ContainersPerGroup;
                return index;
            }
        }

        /// <summary>
        /// index of first visible item
        /// </summary>
        public int RoundedIndex
        {
            get
            {
                if (RoundedItemsCount == 0)
                {
                    return 0;
                }

                float magicScrollFix = (0.5f / RoundedItemsCount);

                int index = Mathf.RoundToInt((NormalizedIndex + magicScrollFix) * Mathf.Max((RoundedItemsCount - VisibleItemsCount), 0));

                index = index / ContainersPerGroup * ContainersPerGroup;

                return index;
            }
            set
            {
                if (value < 0 || value >= RoundedItemsCount)
                {
                    return;
                }
                NormalizedIndex = EvalNormalizedIndex(value);
                if(m_mode == VirtualizingMode.Vertical)
                {
                    verticalNormalizedPosition = 1 - NormalizedIndex;
                }
                else
                {
                    horizontalNormalizedPosition = NormalizedIndex;
                }
            }
        }

       

        /// <summary>
        /// This method will convert index to normalized index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float EvalNormalizedIndex(int index)
        {
            int delta = RoundedItemsCount - VisibleItemsCount;
            if (delta <= 0)
            {
                return 0;
            }

            float normalizedIndex = ((float)index) / delta;
            return normalizedIndex;
        }

        /// <summary>
        /// visible items count
        /// </summary>
        public int VisibleItemsCount
        {
            get
            {
                return Mathf.Min(ItemsCount, PossibleItemsCount);
            }
        }

        /// <summary>
        /// Maximum possible visible items count
        /// </summary>
        private int PossibleItemsCount
        {
            get
            {
                if (ContainerSize < 0.00001f)
                {
                    return 0;
                }
                //return Mathf.FloorToInt(Size / ContainerSize) * ContainersPerGroup;
                return Mathf.RoundToInt(Size / ContainerSize) * ContainersPerGroup;
            }
        }

        /// <summary>
        /// size of ui container (width in horziontal mode and height in vertical mode)
        /// </summary>
        private float ContainerSize
        {
            get
            {
                if (m_mode == VirtualizingMode.Horizontal)
                {
                    return Mathf.Max(0, ContainerPrefab.rect.width + (m_useGrid ? m_gridSpacing.x : m_layoutGroup.spacing));
                }
                else if(m_mode == VirtualizingMode.Vertical)
                {
                    return Mathf.Max(0, ContainerPrefab.rect.height + (m_useGrid ? m_gridSpacing.y : m_layoutGroup.spacing));
                }

                throw new System.InvalidOperationException("Unable to eval container size in non-virtualizing mode");
            }
        }

        /// <summary>
        /// size of visible items viewport
        /// </summary>
        private float Size
        {
            get
            {
                if (m_mode == VirtualizingMode.Horizontal)
                {
                    return Mathf.Max(0, m_virtualContent.rect.width);
                }
                return Mathf.Max(0, m_virtualContent.rect.height);
            }
        }


        protected override void Awake()
        {
            base.Awake();

            if(m_virtualContent == null)
            {
                return;
            }

            m_virtualContentTransformChangeListener = m_virtualContent.GetComponent<RectTransformChangeListener>();
            m_virtualContentTransformChangeListener.RectTransformChanged += OnVirtualContentTransformChaged;

            UpdateVirtualContentPosition();

            if(m_useGrid)
            {
                LayoutGroup layoutGroup = m_virtualContent.GetComponent<LayoutGroup>();
                if (layoutGroup != null && !(layoutGroup is GridLayoutGroup))
                {
                    DestroyImmediate(layoutGroup);
                }

                GridLayoutGroup gridLayout = m_virtualContent.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                {
                    gridLayout = m_virtualContent.gameObject.AddComponent<GridLayoutGroup>();                   
                }

                gridLayout.cellSize = ContainerPrefab.rect.size;
                gridLayout.childAlignment = TextAnchor.UpperLeft;
                gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
                gridLayout.spacing = m_gridSpacing;
                if (m_mode == VirtualizingMode.Vertical)
                {
                    gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
                    gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                }
                else
                {
                    gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
                    gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                }
                m_gridLayoutGroup = gridLayout;
            }
            else
            {
                if (m_mode == VirtualizingMode.Horizontal)
                {
                    //In horizontal mode we destroy VerticalLayoutGroup or GridLayoutGroup if exists

                    LayoutGroup layoutGroup = m_virtualContent.GetComponent<LayoutGroup>();
                    if (layoutGroup != null && !(layoutGroup is HorizontalLayoutGroup))
                    {
                        DestroyImmediate(layoutGroup);
                    }

                    // Create HorizontalLayoutGroup if does not exists

                    HorizontalLayoutGroup horizontalLayout = m_virtualContent.GetComponent<HorizontalLayoutGroup>();
                    if (horizontalLayout == null)
                    {
                        horizontalLayout = m_virtualContent.gameObject.AddComponent<HorizontalLayoutGroup>();
                    }

                    // Setup HorizontalLayoutGroup behavior to arrange ui containers correctly

                    horizontalLayout.childControlHeight = true;
                    horizontalLayout.childControlWidth = false;
                    horizontalLayout.childForceExpandWidth = false;

                    m_layoutGroup = horizontalLayout;
                }
                else
                {
                    //In horizontal mode we destroy HorizontalLayoutGroup or GridLayoutGroup if exists

                    LayoutGroup layoutGroup = m_virtualContent.GetComponent<LayoutGroup>();
                    if (layoutGroup != null && !(layoutGroup is VerticalLayoutGroup))
                    {
                        DestroyImmediate(layoutGroup);
                    }

                    // Create VerticalLayoutGroup if does not exists

                    VerticalLayoutGroup verticalLayout = m_virtualContent.GetComponent<VerticalLayoutGroup>();
                    if (verticalLayout == null)
                    {
                        verticalLayout = m_virtualContent.gameObject.AddComponent<VerticalLayoutGroup>();
                    }

                    // Setup VerticalLayoutGroup behavior to arrange ui containers correctly

                    verticalLayout.childControlWidth = true;
                    verticalLayout.childControlHeight = false;
                    verticalLayout.childForceExpandHeight = false;

                    m_layoutGroup = verticalLayout;
                }
            }

            // Set ScrollSensitivity to be exactly the same as ContainerSize

            scrollSensitivity = ContainerSize;// * ContainersPerGroup;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(m_virtualContentTransformChangeListener != null)
            {
                m_virtualContentTransformChangeListener.RectTransformChanged -= OnVirtualContentTransformChaged;
            }
        }

        private void OnVirtualContentTransformChaged()
        {
            if (m_containers.Count == 0)
            {
                DataBind(RoundedIndex);
                UpdateContentSize();
            }

            if (m_mode == VirtualizingMode.Horizontal)
            {
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_virtualContent.rect.height);
                if (m_useGrid)
                {
                    if ((m_gridLayoutGroup.cellSize.y + m_gridSpacing.y) < 0.00001f)
                    {
                        Debug.LogError("cellSize is too small");
                    }

                    RectTransform parent = (RectTransform)m_virtualContent.parent;

                    if(verticalScrollbarVisibility == ScrollbarVisibility.Permanent)
                    {
                        ContainersPerGroup = Mathf.FloorToInt(parent.rect.height / Mathf.Max(0.00001f, m_gridLayoutGroup.cellSize.y + m_gridSpacing.y));
                    }
                    else
                    {
                        ContainersPerGroup = Mathf.RoundToInt(parent.rect.height / Mathf.Max(0.00001f, m_gridLayoutGroup.cellSize.y + m_gridSpacing.y));
                    }
                    
                }
            }
            else if(m_mode == VirtualizingMode.Vertical)
            {
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_virtualContent.rect.width);
                if (m_useGrid)
                {
                    if((m_gridLayoutGroup.cellSize.x + m_gridSpacing.x) < 0.00001f)
                    {
                        Debug.LogError("cellSize is too small");
                    }

                    RectTransform parent = (RectTransform)m_virtualContent.parent;

                    if(horizontalScrollbarVisibility == ScrollbarVisibility.Permanent)
                    {
                        //ContainersPerGroup = Mathf.FloorToInt(parent.rect.width / Mathf.Max(0.00001f, m_gridLayoutGroup.cellSize.x + m_gridSpacing.x));
                        ContainersPerGroup = Mathf.RoundToInt(parent.rect.width / Mathf.Max(0.00001f, m_gridLayoutGroup.cellSize.x + m_gridSpacing.x));
                    }
                    else
                    {
                        ContainersPerGroup = Mathf.RoundToInt(parent.rect.width / Mathf.Max(0.00001f, m_gridLayoutGroup.cellSize.x + m_gridSpacing.x));
                    }
                }
            }
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            UpdateVirtualContentPosition();
            if(m_mode == VirtualizingMode.Vertical && axis == 1)
            {
                //ScrollView creates top to bottom vertical scrollbar by default, so value inverted here
                NormalizedIndex = 1 - value;
            }
            else if(m_mode == VirtualizingMode.Horizontal && axis == 0)
            {
                NormalizedIndex = value;
            }
        }
        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            UpdateVirtualContentPosition();
            if (m_mode == VirtualizingMode.Vertical)
            {
                //ScrollView creates top to bottom vertical scrollbar by default, so value inverted here
                NormalizedIndex = 1 - verticalNormalizedPosition;
            }
            else if (m_mode == VirtualizingMode.Horizontal)
            {
                NormalizedIndex = horizontalNormalizedPosition;
            }
        }        

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            //Databind if there should be more(or less) visible items than instantiated ui containers
            if(isActiveAndEnabled)
            {
                StartCoroutine(CoRectTransformDimensionsChange());
            }
        }

        IEnumerator CoRectTransformDimensionsChange()
        {
            yield return new WaitForEndOfFrame();
            if (VisibleItemsCount != m_containers.Count)
            {
                DataBind(RoundedIndex);
            }

            OnVirtualContentTransformChaged();
        }

        /// <summary>
        ///this is implementation of ordinary scrolling perpindicular to virtualizing direction (scroll x - in vertical mode, scroll y - in horizontal mode)   
        /// </summary>
        private void UpdateVirtualContentPosition()
        {
            if (m_virtualContent != null)
            {
                if(m_mode == VirtualizingMode.Horizontal)
                {
                    m_virtualContent.anchoredPosition = new Vector2(0, content.anchoredPosition.y);
                }
                else if(m_mode == VirtualizingMode.Vertical)
                {
                    m_virtualContent.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
                }
            }
        }

        /// <summary>
        /// Update content size according to amount of space occupied by all data items if they were all visible. 
        /// This is required to force default behavior of ScrollRect while rendering only required portion of data items.
        /// </summary>
        private void UpdateContentSize()
        {
            if (m_mode == VirtualizingMode.Horizontal)
            {
                content.sizeDelta = new Vector2(Mathf.CeilToInt(((float)RoundedItemsCount) / ContainersPerGroup) * ContainerSize, content.sizeDelta.y);
            }
            else if (m_mode == VirtualizingMode.Vertical)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.CeilToInt(((float)RoundedItemsCount) / ContainersPerGroup) * ContainerSize);
            }
        }


        /// <summary>
        /// handles normailzed index change
        /// </summary>
        /// <param name="newValue"></param>
        private void OnNormalizedIndexChanged(float newValue)
        {
            //clamp newValue to be in [0,1]
            newValue = Mathf.Clamp01(newValue);
            
            //store index
            int prevFirstItemIndex = RoundedIndex;

            //store normalized index
            float prevNormalizedIndex = m_normalizedIndex;

            //update normalized index
            m_normalizedIndex = newValue;

            //get updated index
            int firstItemIndex = RoundedIndex;

            if (firstItemIndex < 0 || firstItemIndex >= RoundedItemsCount)
            {
                m_normalizedIndex = prevNormalizedIndex;
                return;
            }

            //if previous index is not equal to updated index then scroll
            if (prevFirstItemIndex != firstItemIndex)
            {
                int delta = firstItemIndex - prevFirstItemIndex;
                bool scrollDown = delta > 0;

                delta = Mathf.Abs(delta);
                if (delta > VisibleItemsCount)
                {
                    //if delta is too big then it is faster to databind all ui containers at specified offset rather than scrolling one by one
                    DataBind(firstItemIndex);
                }
                else
                {
                    if (scrollDown) // scrolling down
                    {
                        for (int i = 0; i < delta; ++i)
                        {
                            LinkedListNode<RectTransform> first = m_containers.First;
                            RectTransform recycledContainer;
                            if (m_containers.Count > 1)
                            {
                                m_containers.RemoveFirst();

                                int lastSiblingIndex = m_containers.Last.Value.transform.GetSiblingIndex();

                                m_containers.AddLast(first);

                                recycledContainer = first.Value;
                                recycledContainer.SetSiblingIndex(lastSiblingIndex + 1);
                            }
                            else
                            {
                                recycledContainer = first.Value;
                            }

                            if (ItemDataBinding != null && Items != null)
                            {
                                int index = prevFirstItemIndex + VisibleItemsCount;
                                if(index < ItemsCount)
                                {
                                    object item = Items[prevFirstItemIndex + VisibleItemsCount];
                                    ItemDataBinding(recycledContainer, item);
                                }
                                else
                                {
                                    ItemDataBinding(recycledContainer, null);
                                }
                            }
                            prevFirstItemIndex++;
                        }
                    }
                    else //scrolling up
                    {
                        for (int i = 0; i < delta; ++i)
                        {
                            LinkedListNode<RectTransform> last = m_containers.Last;
                            RectTransform recycledContainer;
                            if (m_containers.Count > 1)
                            {
                                m_containers.RemoveLast();

                                int firstSiblingIndex = m_containers.First.Value.transform.GetSiblingIndex();

                                m_containers.AddFirst(last);

                                recycledContainer = last.Value;
                                recycledContainer.SetSiblingIndex(firstSiblingIndex);
                            }
                            else
                            {
                                recycledContainer = last.Value;
                            }

                            prevFirstItemIndex--;

                            if (ItemDataBinding != null && Items != null)
                            {
                                if (prevFirstItemIndex < ItemsCount)
                                {
                                    object item = Items[prevFirstItemIndex];
                                    ItemDataBinding(recycledContainer, item);
                                }
                                else
                                {
                                    ItemDataBinding(recycledContainer, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DataBind(int firstItemIndex)
        {
            int delta = VisibleItemsCount - m_containers.Count;
            if (delta < 0)
            {
                for (int i = 0; i < -delta; ++i)
                {
                    Destroy(m_containers.Last.Value.gameObject);
                    m_containers.RemoveLast();
                }
            }
            else
            {
                for (int i = 0; i < delta; ++i)
                {
                    RectTransform container = Instantiate(ContainerPrefab, m_virtualContent);
                    m_containers.AddLast(container);
                }
            }

            if (ItemDataBinding != null && Items != null)
            {
                int i = 0;

                foreach (RectTransform container in m_containers.ToArray())
                {
                    int index = firstItemIndex + i;
                    if(index < Items.Count)
                    {
                        ItemDataBinding(container, Items[firstItemIndex + i]);
                    }
                    else
                    {
                        ItemDataBinding(container, null);
                    }
                    i++;
                }
            }
        }
       
        public bool IsParentOf(Transform child)
        {
            if(m_virtualContent == null)
            {
                return false;
            }

            return child.IsChildOf(m_virtualContent);
        }

        public void InsertItem(int index, object item, bool raiseItemDataBindingEvent = true)
        {
            int firstIndex = RoundedIndex;
            int lastIndex = firstIndex + VisibleItemsCount - 1;

            m_items.Insert(index, item);

            UpdateContentSize();
            UpdateScrollbar(firstIndex);

            if (PossibleItemsCount >= m_items.Count)
            {
                if(m_containers.Count < VisibleItemsCount)
                {
                    RectTransform container = Instantiate(ContainerPrefab, m_virtualContent);
                    m_containers.AddLast(container);
                    lastIndex++;
                }
            }

            if (firstIndex <= index && index <= lastIndex)
            {
                RectTransform container = m_containers.Last.Value;
                m_containers.RemoveLast();

                if (index == firstIndex)
                {
                    m_containers.AddFirst(container);
                    container.SetSiblingIndex(0);
                }
                else
                {
                    RectTransform prevContainer = m_containers.ElementAtOrDefault(index - firstIndex - 1);
                    LinkedListNode<RectTransform> prevNode = m_containers.Find(prevContainer);
                    m_containers.AddAfter(prevNode, container);
                    container.SetSiblingIndex(index - firstIndex);
                }

                if (raiseItemDataBindingEvent)
                {
                    if (ItemDataBinding != null)
                    {
                        ItemDataBinding(container, item);
                    }
                }
            }
            else
            {
                if(index < firstIndex)
                {
                    UpdateScrollbar(firstIndex + 1);
                }
            }
        }

        public void RemoveItems(int[] indices, bool raiseItemDataBindingEvent = true)
        {
            int index = RoundedIndex;

            indices = indices.OrderBy(i => i).ToArray();
            for(int i = indices.Length - 1; i >= 0; --i)
            {
                int removeAtIndex = indices[i];
                if (removeAtIndex < 0 || removeAtIndex >= m_items.Count)
                {
                    continue;
                }

                m_items.RemoveAt(removeAtIndex);

                if(removeAtIndex < index)
                {
                    index--;
                }
            }

            if(index + VisibleItemsCount >= RoundedItemsCount)
            {
                index = Mathf.Max(0, RoundedItemsCount - VisibleItemsCount);
            }

            UpdateContentSize();
            UpdateScrollbar(index);
            DataBind(index);
            OnVirtualContentTransformChaged();
        }

        public void SetNextSibling(object sibling, object nextSibling)
        { 
            if(sibling == nextSibling)
            {
                return;
            }

            int siblingIndex = m_items.IndexOf(sibling);
            int nextSiblingIndex = m_items.IndexOf(nextSibling);

            Debug.Assert(siblingIndex != nextSiblingIndex);

            int firstIndex = RoundedIndex;
            int lastIndex = firstIndex + VisibleItemsCount - 1;

            bool isNextSiblingInView = firstIndex <= nextSiblingIndex && nextSiblingIndex <= lastIndex;

            int insertIndex = siblingIndex;
            if (nextSiblingIndex > siblingIndex)
            {
                insertIndex++;
            }

            int nextSiblingContainerIndex = nextSiblingIndex - firstIndex;
            int insertContainerIndex = insertIndex - firstIndex;

            //bool isInsertIndexInView = firstIndex <= insertIndex && (isNextSiblingInView ? insertIndex <= lastIndex : insertIndex < lastIndex);
            bool isInsertIndexInView = firstIndex <= insertIndex && (nextSiblingContainerIndex >= 0 ? insertIndex <= lastIndex : insertIndex < lastIndex);

            m_items.RemoveAt(nextSiblingIndex);
            m_items.Insert(insertIndex, nextSibling);

            if(isInsertIndexInView)
            {
                if(isNextSiblingInView)
                {
                    RectTransform nextContainer = m_containers.ElementAt(nextSiblingContainerIndex);
                    m_containers.Remove(nextContainer);

                    if (insertContainerIndex == 0)
                    {
                        m_containers.AddFirst(nextContainer);
                        nextContainer.SetSiblingIndex(0);
                    }
                    else
                    {
                        RectTransform prevContainer = m_containers.ElementAt(insertContainerIndex - 1);

                        LinkedListNode<RectTransform> prevNode = m_containers.Find(prevContainer);
                        m_containers.AddAfter(prevNode, nextContainer);
                    }

                    nextContainer.SetSiblingIndex(insertContainerIndex);
                    if (ItemDataBinding != null)
                    {
                        ItemDataBinding(nextContainer, nextSibling);
                    }
                }
                else
                {
                    RectTransform lastContainer = m_containers.Last.Value;
                    m_containers.RemoveLast();

                    if (insertContainerIndex == 0)
                    {
                        m_containers.AddFirst(lastContainer);
                    }
                    else
                    {
                        RectTransform prevContainer = nextSiblingContainerIndex < 0 ?
                            m_containers.ElementAt(insertContainerIndex) :
                            m_containers.ElementAt(insertContainerIndex - 1);

                        LinkedListNode<RectTransform> prevNode = m_containers.Find(prevContainer);
                        m_containers.AddAfter(prevNode, lastContainer);
                    }

 
                    if(nextSiblingContainerIndex < 0)
                    {
                        UpdateScrollbar(firstIndex - 1);

                        lastContainer.SetSiblingIndex(insertContainerIndex + 1);
                    }
                    else
                    {
                        lastContainer.SetSiblingIndex(insertContainerIndex);
                    }

                    if (ItemDataBinding != null)
                    {
                        ItemDataBinding(lastContainer, nextSibling);
                    }
                }
            }
            else
            {
                if (isNextSiblingInView) //in view to out of view
                {
                    if (insertIndex < firstIndex)
                    {
                        RectTransform nextContainer = m_containers.ElementAt(nextSiblingContainerIndex);
                        m_containers.Remove(nextContainer);

                        m_containers.AddFirst(nextContainer);
                        nextContainer.SetSiblingIndex(0);

                        if (ItemDataBinding != null)
                        {
                            ItemDataBinding(nextContainer, m_items[firstIndex]);
                        }
                    }
                    else if (insertIndex > lastIndex)
                    {
                        RectTransform nextContainer = m_containers.ElementAt(nextSiblingContainerIndex);
                        m_containers.Remove(nextContainer);

                        m_containers.AddLast(nextContainer);
                        nextContainer.SetSiblingIndex(m_containers.Count - 1);

                        if (ItemDataBinding != null)
                        {
                            ItemDataBinding(nextContainer, m_items[lastIndex]);
                        }
                    }
                }
                else //out of view to out of view
                {
                    if (nextSiblingContainerIndex < 0)
                    {
                        UpdateScrollbar(firstIndex - 1);
                    }
                  
                    
                }
            }
        }

        public void SetPrevSibling(object sibling, object prevSibling)
        {
            int index = m_items.IndexOf(sibling);
            index--;
            if(index >= 0)
            {
                sibling = m_items[index];
                SetNextSibling(sibling, prevSibling);
            }
            else
            {
                RectTransform lastContainer = GetContainer(prevSibling);

                int prevSiblingIndex = m_items.IndexOf(prevSibling);
                m_items.RemoveAt(prevSiblingIndex);
                m_items.Insert(0, prevSibling);

                if(lastContainer == null)
                {
                    lastContainer = m_containers.Last.Value;
                    m_containers.RemoveLast();
                }
                else
                {
                    m_containers.Remove(lastContainer);
                }

                m_containers.AddFirst(lastContainer);

                lastContainer.SetSiblingIndex(0);

                if(ItemDataBinding != null)
                {
                    ItemDataBinding(lastContainer, prevSibling);
                }
            }
        }

        public RectTransform GetContainer(object obj)
        {
            if(m_items == null)
            {
                return null;
            }            
            
            int index = m_items.IndexOf(obj);
            if(index < 0)
            {
                return null;
            }

            int firstIndex = RoundedIndex;
            int lastIndex = firstIndex + VisibleItemsCount - 1;
            if(firstIndex <= index && index <= lastIndex)
            {
                int elementIndex = index - firstIndex;
                if(elementIndex < 0 || elementIndex >= m_containers.Count)
                {
                    return null;
                }
                return m_containers.ElementAtOrDefault(index - firstIndex);
            }

            return null;
        }

        public RectTransform FirstContainer()
        {
            if(m_containers.Count == 0)
            {
                return null;
            }

            return m_containers.First.Value;
        }

        public void ForEachContainer(System.Action<RectTransform> action)
        {
            if(action == null)
            {
                return;
            }

            foreach(RectTransform container in m_containers)
            {
                action(container);
            }
        }

        public RectTransform LastContainer()
        {
            if(m_containers.Count == 0)
            {
                return null;
            }

            return m_containers.Last.Value;
        }
        private void UpdateScrollbar(int index)
        {
            m_normalizedIndex = EvalNormalizedIndex(index);
            if (m_mode == VirtualizingMode.Vertical)
            {
                verticalNormalizedPosition = 1 - m_normalizedIndex;
            }
            else if (m_mode == VirtualizingMode.Horizontal)
            {
                horizontalNormalizedPosition = m_normalizedIndex;
            }
        }  
    }
}


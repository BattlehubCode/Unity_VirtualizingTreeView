using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class DataItem
    {
        public string Name;

        public DataItem Parent;

        public List<DataItem> Children;

        public DataItem(string name)
        {
            Name = name;
            Children = new List<DataItem>();
        }
    }

    /// <summary>
    /// In this demo we use game objects hierarchy as data source (each data item is game object)
    /// You can use any hierarchical data with treeview.
    /// </summary>
    public class VirtualizingTreeViewDemo_AddItems : MonoBehaviour
    {
        public VirtualizingTreeView TreeView;

        private List<DataItem> m_dataItems;

  

        private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
            //object dropTarget = e.DropTarget;
            //if(e.Action == ItemDropAction.SetNextSibling || e.Action == ItemDropAction.SetPrevSibling)
            //{
            //    e.Cancel = true;
            //}

        }

        private void Start()
        {
            TreeView.ItemDataBinding += OnItemDataBinding;
            TreeView.SelectionChanged += OnSelectionChanged;
            TreeView.ItemsRemoved += OnItemsRemoved;
            TreeView.ItemExpanding += OnItemExpanding;
            TreeView.ItemBeginDrag += OnItemBeginDrag;

            TreeView.ItemDrop += OnItemDrop;
            TreeView.ItemBeginDrop += OnItemBeginDrop;
            TreeView.ItemEndDrag += OnItemEndDrag;

            m_dataItems = new List<DataItem>();
            for (int i = 0; i < 5; ++i)
            {
                DataItem dataItem = new DataItem("DataItem " + i);
                m_dataItems.Add(dataItem);
            }

            TreeView.Items = m_dataItems;

        }

        private void OnDestroy()
        {
            TreeView.ItemDataBinding -= OnItemDataBinding;
            TreeView.SelectionChanged -= OnSelectionChanged;
            TreeView.ItemsRemoved -= OnItemsRemoved;
            TreeView.ItemExpanding -= OnItemExpanding;
            TreeView.ItemBeginDrag -= OnItemBeginDrag;
            TreeView.ItemBeginDrop -= OnItemBeginDrop;
            TreeView.ItemDrop -= OnItemDrop;
            TreeView.ItemEndDrag -= OnItemEndDrag;
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            //get parent data item (game object in our case)
            DataItem dataItem = (DataItem)e.Item;
            if (dataItem.Children.Count > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    DataItem item = new DataItem("Added OnItemExpanding " + i);
                    item.Parent = dataItem;
                    dataItem.Children.Add(item);
                }

                //Populate children collection
                e.Children = dataItem.Children;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            #if UNITY_EDITOR
            //Do something on selection changed (just syncronized with editor's hierarchy for demo purposes)
            UnityEditor.Selection.objects = e.NewItems.OfType<GameObject>().ToArray();
            #endif
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            //Destroy removed dataitems
            for (int i = 0; i < e.Items.Length; ++i)
            {
                DataItem dataItem = (DataItem)e.Items[i];
                if(dataItem.Parent != null)
                {
                    dataItem.Parent.Children.Remove(dataItem);
                }
            }
        }

        /// <summary>
        /// This method called for each data item during databinding operation
        /// You have to bind data item properties to ui elements in order to display them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            DataItem dataItem = e.Item as DataItem;
            if (dataItem != null)
            {   
                //We display dataItem.name using UI.Text 
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.Name;

                //Load icon from resources
                Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
                icon.sprite = Resources.Load<Sprite>("cube");

                //And specify whether data item has children (to display expander arrow if needed)

                e.HasChildren = dataItem.Children.Count > 0;
                
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            //Could be used to change cursor
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            if(e.DropTarget == null)
            {
                return;
            }

            DataItem dropT = ((DataItem)e.DropTarget);
            
            //Set drag items as children of drop target
            if (e.Action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    DataItem dragT = ((DataItem)e.DragItems[i]);
                    if(dragT.Parent != null)
                    {
                        dragT.Parent.Children.Remove(dragT);
                    }
                    else
                    {
                        m_dataItems.Remove(dragT);
                    }
                    dragT.Parent = dropT;
                    dragT.Parent.Children.Add(dragT);
                }
            }

            //Put drag items next to drop target
            else if (e.Action == ItemDropAction.SetNextSibling)
            {
                for (int i = e.DragItems.Length - 1; i >= 0; --i)
                {
                    DataItem dragT = ((DataItem)e.DragItems[i]);
                    int dropTIndex = dropT.Parent != null ? dropT.Parent.Children.IndexOf(dropT) : m_dataItems.IndexOf(dropT);
                    if (dragT.Parent != dropT.Parent)
                    {
                        if (dragT.Parent != null)
                        {
                            dragT.Parent.Children.Remove(dragT);
                        }
                        else
                        {
                            m_dataItems.Remove(dragT);
                        }
                        dragT.Parent = dropT;
                        dragT.Parent.Children.Insert(dropTIndex + 1, dragT); 
                    }
                    else
                    {
                        int dragTIndex = dragT.Parent != null ? dragT.Parent.Children.IndexOf(dragT) : m_dataItems.IndexOf(dragT);
                        if (dropTIndex < dragTIndex)
                        {
                            if(dragT.Parent != null)
                            {
                                dragT.Parent.Children.Remove(dragT);
                                dragT.Parent.Children.Insert(dropTIndex + 1, dragT);
                            }
                            else
                            {
                                m_dataItems.Remove(dragT);
                                m_dataItems.Insert(dropTIndex + 1, dragT);
                            }
                        }
                        else
                        {
                            if (dragT.Parent != null)
                            {
                                dragT.Parent.Children.Remove(dragT);
                                dragT.Parent.Children.Insert(dropTIndex, dragT);
                            }
                            else
                            {
                                m_dataItems.Remove(dragT);
                                m_dataItems.Insert(dropTIndex, dragT);
                            }
                        }
                    } 
                }
            }

            //Put drag items before drop target
            else if (e.Action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    DataItem dragT = ((DataItem)e.DragItems[i]);
                    if (dragT.Parent != dropT.Parent)
                    {
                        if (dragT.Parent != null)
                        {
                            dragT.Parent.Children.Remove(dragT);
                        }
                        else
                        {
                            m_dataItems.Remove(dragT);
                        }
                        dragT.Parent = dropT.Parent;
                        dragT.Parent.Children.Add(dragT);
                    }

                    int dropTIndex = dropT.Parent != null ? dropT.Parent.Children.IndexOf(dropT) : m_dataItems.IndexOf(dropT);
                    int dragTIndex = dragT.Parent != null ? dragT.Parent.Children.IndexOf(dragT) : m_dataItems.IndexOf(dragT);
                    if (dropTIndex > dragTIndex)
                    {
                        if (dragT.Parent != null)
                        {
                            dragT.Parent.Children.Remove(dragT);
                            dragT.Parent.Children.Insert(dropTIndex - 1, dragT);
                        }
                        else
                        {
                            m_dataItems.Remove(dragT);
                            m_dataItems.Insert(dropTIndex - 1, dragT);
                        }
                    }
                    else
                    {
                        if (dragT.Parent != null)
                        {
                            dragT.Parent.Children.Remove(dragT);
                            dragT.Parent.Children.Insert(dropTIndex - 1, dragT);
                        }
                        else
                        {
                            m_dataItems.Remove(dragT);
                            m_dataItems.Insert(dropTIndex, dragT);
                        }
                    }
                }
            }
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {            
        }


        
        public void AddItem()
        {
            DataItem parent = (DataItem)TreeView.SelectedItem;
            if(parent == null)
            {
                parent = TreeView.Items.OfType<DataItem>().First();
            }

            for (int i = 0; i < 1; i++)
            {
                DataItem item = new DataItem("New Item " + i);
                item.Parent = parent;
                parent.Children.Add(item);
            }

            VirtualizingItemContainer itemContainer = TreeView.GetItemContainer(parent);
            if(itemContainer != null)
            {
                //Update arrow visiblity
                TreeView.DataBindItem(parent, itemContainer);
                TreeView.Expand(parent);
            }
        }

    }
}

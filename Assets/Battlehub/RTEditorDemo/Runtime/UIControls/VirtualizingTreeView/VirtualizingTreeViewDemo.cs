using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    /// <summary>
    /// In this demo we use game objects hierarchy as data source (each data item is game object)
    /// You can use any hierarchical data with treeview.
    /// </summary>
    public class VirtualizingTreeViewDemo : MonoBehaviour
    {
        public VirtualizingTreeView TreeView;

        public int GameObjectNum = 1000;

        public static bool IsPrefab(Transform This)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                throw new InvalidOperationException("Does not work in edit mode");
            }
            return This.gameObject.scene.buildIndex < 0;
        }

        private void Awake()
        {
            for (int i = 0; i < GameObjectNum; ++i)
            {
                GameObject go = new GameObject();
                go.name = "Instantiated GameObject" + i;
            }
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

            var items = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            TreeView.Items = items;
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
            GameObject gameObject = (GameObject)e.Item;
            if(gameObject.transform.childCount > 0)
            {
                //get children
                List<GameObject> children = new List<GameObject>();

                for (int i = 0; i < gameObject.transform.childCount; ++i)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;

                    children.Add(child);
                }
                
                //Populate children collection
                e.Children = children;
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
                GameObject go = (GameObject)e.Items[i];
                if(go != null)
                {
                    Destroy(go);
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
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                //We display dataItem.name using UI.Text 
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = dataItem.name;

                //Load icon from resources
                Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
                icon.sprite = Resources.Load<Sprite>("IconNew");

                //And specify whether data item has children (to display expander arrow if needed)
                
                e.HasChildren = dataItem.transform.childCount > 0;
                
            }
        }

       
        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            //Could be used to change cursor
        }

        private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {            
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            if (e.DropTarget == null)
            {
                return;
            }

            Transform dropT = ((GameObject)e.DropTarget).transform;

            //Set drag items as children of drop target
            if (e.Action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    dragT.SetParent(dropT, true);
                    dragT.SetAsLastSibling();
                }
            }

            //Put drag items next to drop target
            else if (e.Action == ItemDropAction.SetNextSibling)
            {
                for (int i = e.DragItems.Length - 1; i >= 0; --i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    int dropTIndex = dropT.GetSiblingIndex();
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                        dragT.SetSiblingIndex(dropTIndex + 1);
                    }
                    else
                    {
                        int dragTIndex = dragT.GetSiblingIndex();
                        if (dropTIndex < dragTIndex)
                        {
                            dragT.SetSiblingIndex(dropTIndex + 1);
                        }
                        else
                        {
                            dragT.SetSiblingIndex(dropTIndex);
                        }
                    }
                }
            }

            //Put drag items before drop target
            else if (e.Action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int dropTIndex = dropT.GetSiblingIndex();
                    int dragTIndex = dragT.GetSiblingIndex();
                    if (dropTIndex > dragTIndex)
                    {
                        dragT.SetSiblingIndex(dropTIndex - 1);
                    }
                    else
                    {
                        dragT.SetSiblingIndex(dropTIndex);
                    }
                }
            }
        }

    }
}

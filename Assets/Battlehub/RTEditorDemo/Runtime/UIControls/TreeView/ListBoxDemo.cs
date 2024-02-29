using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class ListBoxDemo : MonoBehaviour
    {
        public ListBox ListBox;

        public static bool IsPrefab(Transform This)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                throw new InvalidOperationException("Does not work in edit mode");
            }
            return This.gameObject.scene.buildIndex < 0;
        }

        private void Start()
        {
            if (!ListBox)
            {
                Debug.LogError("Set ListBox field");
                return;
            }

            ListBox.ItemDataBinding += OnItemDataBinding;
            ListBox.SelectionChanged += OnSelectionChanged;
            ListBox.ItemsRemoved += OnItemsRemoved;
            ListBox.ItemBeginDrag += OnItemBeginDrag;
            ListBox.ItemDrop += OnItemDrop;
            ListBox.ItemEndDrag += OnItemEndDrag;

            IEnumerable<GameObject> items = Resources.FindObjectsOfTypeAll<GameObject>().Where(go => !IsPrefab(go.transform) && go.transform.parent == null);
            ListBox.Items = items.OrderBy(t => t.transform.GetSiblingIndex());

        }

        private void OnDestroy()
        {
            if (!ListBox)
            {
                return;
            }
            ListBox.ItemDataBinding -= OnItemDataBinding;
            ListBox.SelectionChanged -= OnSelectionChanged;
            ListBox.ItemsRemoved -= OnItemsRemoved;
            ListBox.ItemBeginDrag -= OnItemBeginDrag;
            ListBox.ItemDrop -= OnItemDrop;
            ListBox.ItemEndDrag -= OnItemEndDrag;
        }

        private void OnItemExpanding(object sender, ItemExpandingArgs e)
        {
            GameObject gameObject = (GameObject)e.Item;
            if (gameObject.transform.childCount > 0)
            {
                GameObject[] children = new GameObject[gameObject.transform.childCount];
                for (int i = 0; i < children.Length; ++i)
                {
                    children[i] = gameObject.transform.GetChild(i).gameObject;
                }
                e.Children = children;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.objects = e.NewItems.OfType<GameObject>().ToArray();
#endif
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            for (int i = 0; i < e.Items.Length; ++i)
            {
                GameObject go = (GameObject)e.Items[i];
                if (go != null)
                {
                    Destroy(go);
                }
            }
        }

        private void OnItemDataBinding(object sender, ItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.name;
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            //Debug.Log("OnItemBeginDrag");
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            Transform dropT = ((GameObject)e.DropTarget).transform;
            if (e.Action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    dragT.SetParent(dropT, true);
                    dragT.SetAsLastSibling();
                }
            }
            else if (e.Action == ItemDropAction.SetNextSibling)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int siblingIndex = dropT.GetSiblingIndex();
                    dragT.SetSiblingIndex(siblingIndex + 1);
                }
            }
            else if (e.Action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int siblingIndex = dropT.GetSiblingIndex();
                    dragT.SetSiblingIndex(siblingIndex);
                }
            }
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
        }
    }
}

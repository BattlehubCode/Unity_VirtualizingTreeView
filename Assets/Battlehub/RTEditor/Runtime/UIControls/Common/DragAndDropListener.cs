using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls.Common
{
    public delegate void DragAndDropEvent(PointerEventData eventData);

    public class DragAndDropListener : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public event DragAndDropEvent InitializePotentialDrag;
        public event DragAndDropEvent BeginDrag;
        public event DragAndDropEvent Drag;
        public event DragAndDropEvent Drop;
        public event DragAndDropEvent EndDrag;

        public bool InProgress
        {
            get;
            private set;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if(InitializePotentialDrag != null)
            {
                InitializePotentialDrag(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InProgress = true;
            if (BeginDrag != null)
            {
                BeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(Drag != null)
            {
                Drag(eventData);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            InProgress = false;
            if(Drop != null)
            {
                Drop(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InProgress = false;
            if(EndDrag != null)
            {
                EndDrag(eventData);
            }
        }
    }
}


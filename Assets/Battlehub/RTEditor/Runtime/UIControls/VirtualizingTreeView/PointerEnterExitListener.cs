using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public class PointerEnterExitListener : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
    {
        public event EventHandler<PointerEventArgs> PointerEnter;
        public event EventHandler<PointerEventArgs> PointerExit;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke(this, new PointerEventArgs(eventData));
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke(this, new PointerEventArgs(eventData));
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
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
           
            if(PointerEnter != null)
            {
                PointerEnter(this, new PointerEventArgs(eventData));
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            
            if (PointerExit != null)
            {
                PointerExit(this, new PointerEventArgs(eventData));
            }
        }
    }
}


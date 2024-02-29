using Battlehub.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Battlehub.UIControls
{
    public class LayoutElementResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public LayoutElement Target;
        public RectTransform Parent;
        public LayoutElement SecondaryTarget;

        public Texture2D CursorTexture;
        public float XSign = 1;
        public float YSign = 0;
        public float MaxSize;
        public bool HasMaxSize;

        private bool m_pointerInside;
        private bool m_pointerDown;
        private float m_midX;
        private float m_midY;

        private CursorHelper m_cursorHelper = new CursorHelper();

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (Parent != null && SecondaryTarget != null)
            {
                if(XSign != 0)
                {
                    Target.flexibleWidth = Mathf.Clamp01(Target.flexibleWidth);
                    SecondaryTarget.flexibleWidth = Mathf.Clamp01(SecondaryTarget.flexibleWidth);
                }
                
                if(YSign != 0)
                {
                    Target.flexibleHeight = Mathf.Clamp01(Target.flexibleHeight);
                    SecondaryTarget.flexibleHeight = Mathf.Clamp01(SecondaryTarget.flexibleHeight);
                }

                m_midY = Target.flexibleHeight / (Target.flexibleHeight + SecondaryTarget.flexibleHeight);
                m_midY *= Math.Max((Parent.rect.height - Target.minHeight - SecondaryTarget.minHeight), 0);
                m_midX = Target.flexibleWidth / (Target.flexibleWidth + SecondaryTarget.flexibleWidth);
                m_midX *= Math.Max((Parent.rect.width - Target.minWidth - SecondaryTarget.minWidth), 0);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if(Parent != null && SecondaryTarget != null)
            {
                if (XSign != 0)
                {
                    float newMidX = m_midX + eventData.delta.x * Math.Sign(XSign);
                    
                    float targetFlexibleWidth = newMidX / (Parent.rect.width - Target.minWidth - SecondaryTarget.minWidth);
                    Target.flexibleWidth = targetFlexibleWidth;
                    SecondaryTarget.flexibleWidth = (1 - targetFlexibleWidth);
                    m_midX = newMidX;
                }

                if (YSign != 0)
                {
                    float newMidY = m_midY + eventData.delta.y * Math.Sign(YSign);
                    float targetFlexibleHeight = newMidY / (Parent.rect.height - Target.minHeight - SecondaryTarget.minHeight);
                    Target.flexibleHeight = targetFlexibleHeight;
                    SecondaryTarget.flexibleHeight = (1 - targetFlexibleHeight);
                    m_midY = newMidY;
                }

                if (XSign != 0)
                {
                    Target.flexibleWidth = Mathf.Clamp01(Target.flexibleWidth);
                    SecondaryTarget.flexibleWidth = Mathf.Clamp01(SecondaryTarget.flexibleWidth);
                }

                if (YSign != 0)
                {
                    Target.flexibleHeight = Mathf.Clamp01(Target.flexibleHeight);
                    SecondaryTarget.flexibleHeight = Mathf.Clamp01(SecondaryTarget.flexibleHeight);
                }
            }
            else
            {
                if (XSign != 0)
                {
                    Target.preferredWidth += eventData.delta.x * Math.Sign(XSign);
                    if (HasMaxSize)
                    {
                        if (Target.preferredWidth > MaxSize)
                        {
                            Target.preferredWidth = MaxSize;
                        }
                    }
                }

                if (YSign != 0)
                {
                    Target.preferredHeight += eventData.delta.y * Math.Sign(YSign);
                    if (HasMaxSize)
                    {
                        if (Target.preferredHeight > MaxSize)
                        {
                            Target.preferredHeight = MaxSize;
                        }
                    }
                }
            }
          
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            m_pointerInside = true;
            m_cursorHelper.SetCursor(this, CursorTexture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_pointerInside = false;
            if (!m_pointerDown)
            {
                m_cursorHelper.ResetCursor(this);
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            m_pointerDown = true;
            if(Target.preferredWidth < -1)
            {
                Target.preferredWidth = Target.minWidth;
            }

            if(Target.preferredHeight < -1)
            {
                Target.preferredHeight = Target.minHeight;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            m_pointerDown = false;
            if(!m_pointerInside)
            {
                m_cursorHelper.ResetCursor(this);
            }
        }
    }
}


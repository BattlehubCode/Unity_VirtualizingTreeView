using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class VirtualizingListBoxItem : VirtualizingItemContainer
    {
        [SerializeField]
        private Graphic m_selectionGraphics;

        [SerializeField]
        private Graphic[] m_presenterGraphics;

        [SerializeField]
        private Color m_selectionNormalColor = Color.gray;

        [SerializeField]
        private Color m_selectionFocusedColor = new Color32(0x0B, 0x7E, 0xF0, 0xFF);

        [SerializeField]
        private Color[] m_presenterSelectedColor;

        [SerializeField]
        private Color[] m_presenterNormalColor;

        
        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if (base.IsSelected != value)
                {
                    m_selectionGraphics.enabled = value;
                    base.IsSelected = value;

                    for(int i = 0; i < m_presenterGraphics.Length; ++i)
                    {
                        m_presenterGraphics[i].color = base.IsSelected ?
                             m_presenterSelectedColor[i] :
                             m_presenterNormalColor[i];
                    }
                }
            }
        }

        protected override void AwakeOverride()
        {
            if(m_selectionGraphics == null)
            {
                m_selectionGraphics = GetComponent<Graphic>();
            }
            
            m_selectionGraphics.enabled = IsSelected;
            ItemsControl.IsFocusedChanged += OnIsFocusedChanged;
        }

        protected override void StartOverride()
        {
            base.StartOverride();
            UpdateGraphicsColor();
        }

        protected override void OnDestroyOverride()
        {
            if(ItemsControl != null)
            {
                ItemsControl.IsFocusedChanged -= OnIsFocusedChanged;
            }
            base.OnDestroyOverride();
        }

        private void OnIsFocusedChanged(object sender, System.EventArgs e)
        {
            UpdateGraphicsColor();   
        }

        private void UpdateGraphicsColor()
        {
            m_selectionGraphics.color = ItemsControl.IsFocused ?
                m_selectionFocusedColor :
                m_selectionNormalColor;
        }
    }
}


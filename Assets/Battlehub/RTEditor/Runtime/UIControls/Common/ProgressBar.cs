using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image[] Images = null;

        [SerializeField]
        private Outline[] Outlines = null;

        private float[] m_alpha;
     
        [SerializeField]
        private float Speed = 1.0f;

        public bool IsInProgress = true;
       
        private void Start()
        {
            m_alpha = new float[Images.Length];

            for (int i = 0; i < Images.Length; ++i)
            {
                m_alpha[i] = ((float)i) / (Images.Length);

                Color color = Images[i].color;
                color.a = m_alpha[i];
                Images[i].color = color;

                Color effectColor = Outlines[i].effectColor;
                effectColor.a = m_alpha[i]; 
                Outlines[i].effectColor = effectColor;


            }
        }

        private void FixedUpdate()
        {
            if(!IsInProgress)
            {
                return;
            }
            for (int i = 0; i < Images.Length; ++i)
            {
                Images[i].color = UpdateAlpha(Images[i].color, i);
                Outlines[i].effectColor = UpdateAlpha(Outlines[i].effectColor, i);
            }
        }

        private Color UpdateAlpha(Color color, int index)
        {
            m_alpha[index] -= Time.deltaTime * Speed;
            if (m_alpha[index] < 0.0f)
            {
                m_alpha[index] = 1.0f;
            }
            color.a = Mathf.Clamp01(m_alpha[index]);
            return color;
        }
    }
}


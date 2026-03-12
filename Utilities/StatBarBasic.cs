using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nebula
{
    public class StatBarBasic : MonoBehaviour
    {
        public string Name = "No Name Given";
        [SerializeField] private Image _fill;

        public enum StatBarBasicFillAxis{ X, Y, Z }
        [SerializeField] private StatBarBasicFillAxis _fillAxis = StatBarBasicFillAxis.X;

        public void FillPercent(float fillFraction)
        {
            if(fillFraction > 1f) fillFraction = 1f;
            if(fillFraction < 0f) fillFraction = 0f;
            switch (_fillAxis)
            {
                case StatBarBasicFillAxis.X:
                    _fill.rectTransform.localScale = new Vector3(fillFraction, 1f, 1f);
                    break;
                case StatBarBasicFillAxis.Y:
                    _fill.rectTransform.localScale = new Vector3(1f, fillFraction, 1f);
                    break;
                case StatBarBasicFillAxis.Z:
                    _fill.rectTransform.localScale = new Vector3(1f, 1f, fillFraction);
                    break;
            }
        }
    }
}
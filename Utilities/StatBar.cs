using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nebula
{
    public struct StatBarConfig
    {
        public string Name;
        public Vector2 Size;
        public float Yoffset;
        public StatBarConfig(string name = "No Name Given", Vector2? size = null, float yoffset = 0.0f)
        {
            Name = name;
            Size = (size != null) ? (Vector2)size : new Vector2(1f, 1f);
            Yoffset = yoffset;
        }
    }
    public class StatBar : MonoBehaviour
    {
        public StatBarConfig Config = new StatBarConfig();
        private GameObject _canvasObject;
        private Canvas _canvas;
        private GameObject _fillObject;
        private Image _fill;
        private Slider _slider;
        private void Start()
        {
            // Create a canvas to put UI onto.
            _canvasObject = new GameObject("Stat Bar: " + Config.Name);
            _canvasObject.transform.SetParent(this.transform);

            _canvas = _canvasObject.AddComponent<Canvas>();
            _canvas.GetComponent<RectTransform>().localPosition = Vector3.zero;
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;
            RectTransform canvasRectTransform = _canvas.GetComponent<RectTransform>();
            canvasRectTransform.sizeDelta = Config.Size;
            canvasRectTransform.anchoredPosition = new Vector3(0.0f, Config.Yoffset, 0.0f);

            // Create a fill image show bar fill amount
            _fillObject = new GameObject("Fill: " + Config.Name);
            _fillObject.transform.SetParent(_canvasObject.transform);
            _fill = _fillObject.AddComponent<Image>();
            _fill.rectTransform.offsetMin = new Vector2(0f, 0f);
            _fill.rectTransform.offsetMax = new Vector2(0f, 0f);

            // Create a slider to control fill image on the canvas object.
            _slider = _canvasObject.AddComponent<Slider>();
            _slider.interactable = false;
            _slider.transition = Selectable.Transition.None;
            _slider.fillRect = _fill.rectTransform;
            //_slider.navigation = Navigation.Mode.None;
        }

        public void SetMaxHealth(int maxHealth)
        {
            _slider.maxValue = maxHealth;
            _slider.value = maxHealth;
        }

        public void UpdateHealth(int health)
        {
            _slider.value = health;
        }
    }
}
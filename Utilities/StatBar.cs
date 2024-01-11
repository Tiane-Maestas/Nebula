using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nebula
{
    public struct StatBarText
    {
        public bool Visible;
        public int FontSize;
        public enum Position { Left, Middle, Right };
        public Position Anchor;
        public StatBarText(int fontSize = 2, Position anchor = StatBarText.Position.Left, bool visible = true)
        {
            Visible = visible;
            FontSize = fontSize;
            Anchor = anchor;
        }
    }
    public struct StatBarConfig
    {
        public string Name;
        public Vector2 Size;
        public Vector2 Position;
        public Color Color;
        public bool FillFromLeft;
        public StatBarText Text;
        public StatBarConfig(string name = "No Name Given", Vector2? size = null, Vector2? position = null, Color? color = null, bool fillFromLeft = true, StatBarText? text = null)
        {
            Name = name;
            Size = (size != null) ? (Vector2)size : new Vector2(1f, 1f);
            Position = (position != null) ? (Vector2)position : new Vector2(0f, 0f);
            Color = (color != null) ? (Color)color : new Color(1, 0, 0, 1); // Default color is red.
            FillFromLeft = fillFromLeft;
            Text = (text != null) ? (StatBarText)text : new StatBarText(visible: false);
        }
    }
    public class StatBar : MonoBehaviour
    {
        public StatBarConfig Config = new StatBarConfig(); // If you ever update Config after making the object you must call "UpdateConfig"
        private Canvas _canvas;
        private Image _fill;
        private Slider _slider;
        private TextMeshPro _statText = null;
        private void Start()
        {
            // Create a canvas to put UI onto.
            GameObject canvasObject = new GameObject("Stat Bar: " + Config.Name);
            canvasObject.transform.SetParent(this.transform);

            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.GetComponent<RectTransform>().localPosition = Vector3.zero;
            _canvas.renderMode = RenderMode.WorldSpace; // Todo: Test in 3D but might want todo LookAt() on transform to camera.
            _canvas.worldCamera = Camera.main;

            // Create a fill image show bar fill amount
            GameObject fillObject = new GameObject("Fill: " + Config.Name);
            fillObject.transform.SetParent(canvasObject.transform);
            _fill = fillObject.AddComponent<Image>();
            _fill.rectTransform.offsetMin = new Vector2(0f, 0f);
            _fill.rectTransform.offsetMax = new Vector2(0f, 0f);

            // Create a slider to control fill image on the canvas object.
            _slider = canvasObject.AddComponent<Slider>();
            _slider.interactable = false;
            _slider.transition = Selectable.Transition.None;
            _slider.fillRect = _fill.rectTransform;
            //_slider.navigation = Navigation.Mode.None; // Todo: Check this.

            // Add text to display fill amount.
            if (Config.Text.Visible)
            {
                GameObject textObject = new GameObject("Text: " + Config.Name);
                textObject.transform.SetParent(canvasObject.transform);
                _statText = textObject.AddComponent<TextMeshPro>();
                _statText.autoSizeTextContainer = true;
                _statText.enableWordWrapping = false;
                _statText.text = "";
            }

            UpdateConfig();
        }

        // Updates everything but gameobject names.
        public void UpdateConfig()
        {
            // Update Canvas
            RectTransform canvasRectTransform = _canvas.GetComponent<RectTransform>();
            canvasRectTransform.sizeDelta = Config.Size;
            canvasRectTransform.anchoredPosition = new Vector3(Config.Position.x, Config.Position.y, 0.0f);

            // Update Fill
            _fill.color = Config.Color;

            // Update Slider
            if (Config.FillFromLeft)
                _slider.direction = Slider.Direction.LeftToRight;
            else
                _slider.direction = Slider.Direction.RightToLeft;

            // Text Update
            if (Config.Text.Visible)
            {
                _statText.alignment = TextAlignmentOptions.Center;
                if (Config.Text.Anchor == StatBarText.Position.Left)
                    _statText.GetComponent<RectTransform>().localPosition = new Vector3(-Config.Size.x / 2.0f, 0.0f, -1.0f);
                else if (Config.Text.Anchor == StatBarText.Position.Middle)
                    _statText.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, -1.0f);
                else
                    _statText.GetComponent<RectTransform>().localPosition = new Vector3(Config.Size.x / 2.0f, 0.0f, -1.0f);
                _statText.fontSize = Config.Text.FontSize;
            }
        }

        public void SetMaxValue(int maxValue)
        {
            _slider.maxValue = maxValue;
            _slider.value = maxValue;

            if (Config.Text.Visible) _statText.text = "" + _slider.value;
        }

        public void UpdateValue(int value)
        {
            _slider.value = value;

            if (Config.Text.Visible) _statText.text = "" + _slider.value;
        }
    }
}
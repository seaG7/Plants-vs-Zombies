using System;
using Data.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class PlantCard : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _selectionBorder; 

        private PlantData _data;
        private Action<PlantData> _onClick;

        public void Initialize(PlantData data, Action<PlantData> onClick)
        {
            _data = data;
            _onClick = onClick;

            _iconImage.sprite = data.icon;
            _costText.text = data.cost.ToString();
            
            _button.onClick.AddListener(OnClick);
            SetSelected(false);
        }

        private void OnClick() => _onClick?.Invoke(_data);

        public void SetSelected(bool isSelected)
        {
            if (_selectionBorder != null)
                _selectionBorder.enabled = isSelected;
        }

        public void SetAffordable(bool isAffordable)
        {
            _button.interactable = isAffordable;
            _iconImage.color = isAffordable ? Color.white : Color.gray;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}
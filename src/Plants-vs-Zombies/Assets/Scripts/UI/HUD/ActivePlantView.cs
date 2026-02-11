using System;
using Core.Interfaces;
using Data.Enums;
using Infrastructure.Providers.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.HUD
{
    /// <summary>
    /// Represents an active plant in the hotbar. Clickable for possession switching.
    /// </summary>
    public class ActivePlantView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Slider _reloadSlider;
        [SerializeField] private TextMeshProUGUI _hotkeyText;
        [SerializeField] private Image _selectionBorder;
        [SerializeField] private Button _selectButton; 

        private IPossessablePlant _plant;
        private IStaticDataProvider _staticData;
        private int _hotkeyIndex;
        private Action<int> _onClicked;

        [Inject]
        public void Construct(IStaticDataProvider staticData)
        {
            _staticData = staticData;
        }

        public void Initialize(IPossessablePlant plant, int index, PlantType type, Action<int> onClicked)
        {
            _plant = plant;
            _hotkeyIndex = index + 1;
            _onClicked = onClicked;

            _hotkeyText.text = _hotkeyIndex.ToString();
            
            var data = _staticData.GetPlantData(type);
            if (data != null) _icon.sprite = data.icon;

            if (_selectButton == null) 
                _selectButton = GetComponent<Button>();
                
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => _onClicked?.Invoke(_hotkeyIndex));
            }
        }

        private void Update()
        {
            if (_plant == null || _plant.Equals(null))
            {
                Destroy(gameObject);
                return;
            }

            _reloadSlider.value = _plant.GetReloadProgress();
        }

        public void SetSelected(bool isSelected)
        {
            _selectionBorder.enabled = isSelected;
        }
    }
}
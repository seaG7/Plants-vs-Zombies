using Core.Interfaces;
using Data.Enums;
using Infrastructure.Providers.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.HUD
{
    public class ActivePlantView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Slider _reloadSlider;
        [SerializeField] private TextMeshProUGUI _hotkeyText;
        [SerializeField] private Image _selectionBorder;

        private IPossessablePlant _plant;
        private IStaticDataProvider _staticData;

        [Inject]
        public void Construct(IStaticDataProvider staticData)
        {
            _staticData = staticData;
        }

        public void Initialize(IPossessablePlant plant, int index, PlantType type)
        {
            _plant = plant;
            _hotkeyText.text = (index + 1).ToString();
            
            var data = _staticData.GetPlantData(type);
            if (data != null) _icon.sprite = data.icon;
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
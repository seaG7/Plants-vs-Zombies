using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Mobile
{
    public class MobileControlsView : MonoBehaviour
    {
        [Header("Joystick Asset")]
        [SerializeField] private Joystick _joystick; 

        [Header("Buttons")]
        [SerializeField] private Button _fireButton;
        [SerializeField] private Button _exitPlantButton;
        
        public Vector2 InputVector => _joystick != null ? _joystick.Direction : Vector2.zero;

        public bool IsFirePressed { get; private set; }

        private bool _exitTriggered;

        private void Start()
        {
            SetupButton(_fireButton, (pressed) => IsFirePressed = pressed);
            _exitPlantButton.onClick.AddListener(() => _exitTriggered = true);
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            if (!isVisible && _joystick != null)
            {
                _joystick.OnPointerUp(null);
                IsFirePressed = false;
                _exitTriggered = false;
            }
        }

        public bool ConsumeExitPress()
        {
            if (_exitTriggered)
            {
                _exitTriggered = false;
                return true;
            }
            return false;
        }

        private void SetupButton(Button btn, System.Action<bool> onStateChanged)
        {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();
            
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((_) => onStateChanged(true));
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((_) => onStateChanged(false));
            trigger.triggers.Add(up);
        }
    }
}
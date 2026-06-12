using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public class UIToggleButton : UIBaseInteractive, IPointerClickHandler
    {
        [SerializeField] private Image avatar;
        [SerializeField] private Image fill; 
        [SerializeField] private bool isOn = true;
        [SerializeField] private Transform posOn;
        [SerializeField] private Transform posOff;

        private float pressedY { get; set; }
        public Toggle.ToggleEvent OnValueChange;


        private void Start()
        {
            if (avatar == null)
            {
                avatar = GetComponent<Image>();
            }

            onFingerDown.AddListener(OnFingerDown);
            onFingerUp.AddListener(OnFingerUp);

            ApplyState(false);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            IsOn = !IsOn;
            OnValueChange?.Invoke(IsOn);
        }

        public bool IsOn
        {
            get { return isOn; }
            set
            {
                if (isOn == value) return;
                isOn = value;
                ApplyState(true);
            }
        }

        private void ApplyState(bool animate)
        {
            if (avatar == null) return;

            if (fill != null)
            {
                fill.gameObject.SetActive(isOn);
            }

            Transform target = isOn ? posOn : posOff;
            if (target == null) return;

            if (animate)
            {
                LeanTween.cancel(avatar.gameObject);

                LeanTween.moveLocal(avatar.gameObject, target.localPosition, 0.15f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setIgnoreTimeScale(true);
            }
            else
            {
                avatar.rectTransform.localPosition = target.localPosition;
            }
        }

        private void OnFingerDown()
        {
            if (avatar == null) return;

            LeanTween.cancel(avatar.gameObject);

            pressedY = avatar.transform.localPosition.y;

            LeanTween.moveLocalY(avatar.gameObject, pressedY - 5, 0.1f)
                .setIgnoreTimeScale(true);

            LeanTween.scale(avatar.gameObject, Vector3.one * 0.98f, 0.1f)
                .setIgnoreTimeScale(true);
        }

        private void OnFingerUp()
        {
            if (avatar == null) return;

            LeanTween.cancel(avatar.gameObject);

            LeanTween.moveLocalY(avatar.gameObject, pressedY, 0.1f)
                .setIgnoreTimeScale(true);

            LeanTween.scale(avatar.gameObject, Vector3.one, 0.1f)
                .setIgnoreTimeScale(true);
        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }
    }
}
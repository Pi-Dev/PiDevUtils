using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiDev.Utilities.UI
{
    public class MenuNavigationItem : MonoBehaviour, IPointerEnterHandler
    {
        [Header("Disable Item")]
        public bool ignoreItem;

        [Header("Nav override")]
        public Transform navUp;
        public Transform navDown, navLeft, navRight;
        public bool onlyIfDefaultNavFails;

        [Header("Behaviour")]
        public List<MonoBehaviour> activeIfNavigated;
        public List<MonoBehaviour> activeIfNotNavigated;
        public Material TextMaterial, TextSelectedMaterial;
        public bool fireNavInEventEvenIfContextNotActive = false;
        public UnityEvent OnNavigatedIn;
        public UnityEvent OnNavigatedOut;
        public UnityEvent onTriggered;

        [Header("Debug")]
        public MenuNavigationSystemContext menu;

        public void NavigatedIn()
        {
            if (ignoreItem) return;
            OnNavigatedIn?.Invoke();
            foreach (var item in activeIfNavigated) item.enabled = true;
            foreach (var item in activeIfNotNavigated) item.enabled = false;
        }
        public void NavigatedOut()
        {
            if (ignoreItem) return;
            OnNavigatedOut?.Invoke();
            foreach (var item in activeIfNotNavigated) item.enabled = true;
            foreach (var item in activeIfNavigated) item.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (fireNavInEventEvenIfContextNotActive)
            {
                NavigatedIn();
            }
            else
            {
                if (!menu) menu = GetComponentInParent<MenuNavigationSystemContext>();
                if (menu && menu.followMouse)
                    menu.HighlightItem(transform);
            }
        }

    }
}
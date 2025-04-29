using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiDev.Utilities.UI
{

    public class MenuNavigationSystemContext : MonoBehaviour
    {
        [Header("General Setup")]
        public MenuNavigationSystem.NavMode navigationMode = MenuNavigationSystem.NavMode.Vertical;
        public bool followMouse = true;
        public bool activateOnEnable = false;
        [Tooltip("The wait time before next Input Action")]
        public float stepsDelayTime = 0.2f;
        [Tooltip("if not autoDetectItems, manually set")]
        public bool autoDetectItems = true;
        public bool requireItemComponent = false;
        public List<Transform> menuItems;
        public int currentIndex = 0;
        public int defaultIndex = -1;

        public MenuNavigationSystem.BackButtonAction backAction;
        public RectTransform BackTriggerItem;

        public UnityEvent OnContextActivated;
        public UnityEvent OnContextDeactivated;

        [Header("User Interface")]
        public RectTransform cursor;
        public bool CursorAlwaysVisible;
        public Vector2 cursorExtentsSize;
        public Vector3 cursorOffset;
        public RectTransform menuHighlight;
        public Vector2 highlightMenuExtentsSize;
        public Vector2 highlightMenuOffset;
        public float scrollAreaUpperBoundMargin;
        public float scrollAreaLowerBoundMargin;

        CanvasGroup cursorCanvas;
        CanvasGroup highlightCanvas;
        float cursorTargetAlpha = 0;
        int prevIndex = -2;

        private void Awake()
        {
            // Fix anchors for positioning
            if (cursor)
            {
                //cursor.anchorMax = Vector3.zero;
                //cursor.anchorMin = Vector3.zero;
                cursorCanvas = cursor.gameObject.GetOrAddComponent<CanvasGroup>();
                cursorCanvas.alpha = 0;
            }
            if (menuHighlight)
            {
                menuHighlight.anchorMax = Vector3.zero;
                menuHighlight.anchorMin = Vector3.zero;
                highlightCanvas = menuHighlight.gameObject.GetOrAddComponent<CanvasGroup>();
                highlightCanvas.alpha = 0;
            }

            if(autoDetectItems) RebuildItems();
        }

        public void RebuildItems()
        {
            menuItems.Clear();
            if (!requireItemComponent)
            {
                foreach (Transform t in transform)
                {
                    if (!t.gameObject.activeInHierarchy) continue;
                    // Don't pick the cursor graphics as items!
                    if (t == cursor) continue;
                    if (t == menuHighlight) continue;

                    var itemComp = t.GetComponent<MenuNavigationItem>();
                    if (itemComp && itemComp.ignoreItem) continue;

                    if ((requireItemComponent && itemComp) || !requireItemComponent)
                    {
                        var item = t.gameObject.GetOrAddComponent<MenuNavigationItem>();
                        item.menu = this;
                        menuItems.Add(t);
                    }
                }
            }
            else
            {
                var items = transform.GetComponentsInChildren<MenuNavigationItem>();
                foreach (var t in items)
                {
                    if (!t.gameObject.activeInHierarchy) continue;
                    // Don't pick the cursor graphics as items!
                    if (t.transform == cursor) continue;
                    if (t.transform == menuHighlight) continue;

                    if (t && t.ignoreItem) continue;

                    menuItems.Add(t.transform);
                }
            }
            menuItems.RemoveAll(o => o == null);
        }

        private void OnEnable()
        {
            if (MenuNavigationSystem.instance == null) return;

            if (activateOnEnable) Activate();

            if (autoDetectItems) RebuildItems();
        }

        bool isActive;

        public void Activate()
        {
            isActive = true;
            if (defaultIndex != -1) currentIndex = defaultIndex;
            if (MenuNavigationSystem.PushContext(this))
                OnContextActivated?.Invoke();

            if (autoDetectItems) RebuildItems();
        }

        public void Deactivate()
        {
            isActive = false;
            if (MenuNavigationSystem.RemoveContext(this))
                OnContextDeactivated?.Invoke();
        }

        private void OnDisable()
        {
            if (activateOnEnable)
            {
                Deactivate();
            }
        }

        public float cursorLerpFactor = 0.4f;
        public string menuTag;

        void Update()
        {
            if (menuItems.Count == 0) return;
            menuItems.RemoveAll(o => o == null);
            if (currentIndex != -1 && (currentIndex >= menuItems.Count || currentIndex < -1)) currentIndex = menuItems.Count - 1;
            if (currentIndex == -1) currentIndex = 0;
            var item = menuItems[currentIndex];

            // Selectables?
            if (prevIndex != currentIndex)
            {
                if (prevIndex > 0 && prevIndex < menuItems.Count) menuItems[prevIndex].GetComponent<MenuNavigationItem>()?.NavigatedOut();
                menuItems[currentIndex].GetComponent<MenuNavigationItem>()?.NavigatedIn();
                HighlightItem(currentIndex);
                prevIndex = currentIndex;
            }

            // cursor
            if (cursor && !MenuNavigationSystem.ContextsChangedThisFrame)
            {
                float lerpf = cursorLerpFactor;
                cursor.position = Vector3.Lerp(cursor.position, item.position + cursorOffset, lerpf);
                cursor.sizeDelta = Vector2.Lerp(cursor.sizeDelta, item.GetComponent<RectTransform>().sizeDelta + cursorExtentsSize, lerpf);
                if (isActive && MenuNavigationSystem.stack.Count > 0)
                {
                    cursorCanvas.alpha = CursorAlwaysVisible ? 1 : MenuNavigationSystem.stack[0] == this ? 1 : 0.6f;
                }
                else cursorCanvas.alpha = CursorAlwaysVisible ? 1 : 0;
            }
        }

        // obviously Unity hates us
        void HandleScrollAreas(Transform selected)
        {
            if (!selected) return;
            var sr = selected.GetComponentInParent<ScrollRect>();
            if (sr)
            {
                var contentPanel = sr.content;
                if (selected.transform.parent != contentPanel.transform) return;
                var selectedRectTransform = selected.GetComponent<RectTransform>();
                var scrollRectTransform = sr.GetComponent<RectTransform>();

                var Padding = new Rect(0, 0, 100, 100);
                Canvas.ForceUpdateCanvases();

                var objPosition = (Vector2)sr.transform.InverseTransformPoint(selectedRectTransform.position);
                var scrollHeight = sr.GetComponent<RectTransform>().rect.height;
                var objHeight = selectedRectTransform.rect.height;

                float ubound = scrollHeight / 2 - scrollAreaUpperBoundMargin;
                float dbound = -scrollHeight / 2 + scrollAreaLowerBoundMargin;
                //Debug.DrawLine(new Vector3(-10000, ubound, 0), new Vector3(10000, ubound, 0), new Color(1, 0.5f, 0.5f, 1));
                //Debug.DrawLine(new Vector3(-10000, dbound, 0), new Vector3(10000, dbound, 0), Color.red);

                float itemdbound = objPosition.y - objHeight / 2;
                float itemubound = objPosition.y + objHeight / 2;
                //Debug.DrawLine(new Vector3(-10000, itemdbound, 0), new Vector3(10000, itemdbound, 0), Color.blue);
                //Debug.DrawLine(new Vector3(-10000, itemubound, 0), new Vector3(10000, itemubound, 0), Color.green);

                if (itemdbound < dbound)
                {
                    sr.content.DOAnchorPosY(sr.content.anchoredPosition.y + dbound - itemdbound, 0.2f).SetUpdate(true);
                    //sr.content.anchoredPosition += new Vector2(0, dbound - itemdbound);
                }
                else if (itemubound > ubound)
                {
                    sr.content.DOAnchorPosY(sr.content.anchoredPosition.y - (itemubound - ubound), 0.2f).SetUpdate(true);
                    //sr.content.anchoredPosition += new Vector2(0, -(itemubound - ubound));
                }
            }
        }

        public void HighlightItem(int index)
        {
            MenuNavigationSystem.instance.UIMove.Play(transform.position);

            if (index < 0 || index >= menuItems.Count) return;
            currentIndex = index;
            var item = menuItems[index];
            if (!item) return;
            var sel = item.GetComponent<ISelectHandler>();
            if (sel != null) EventSystem.current.SetSelectedGameObject(item.gameObject);
            HandleScrollAreas(item);
        }

        public void HighlightItem(Transform transform)
        {
            for (int i = 0; i < menuItems.Count; i++)
                if (menuItems[i] == transform)
                    currentIndex = i;
            HighlightItem(currentIndex);
        }
    }
}
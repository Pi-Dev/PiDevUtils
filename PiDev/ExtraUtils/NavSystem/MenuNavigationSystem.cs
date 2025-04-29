using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PiDev.Utilities.UI
{
    public class MenuNavigationSystem : MonoBehaviour
    {
#if false  // Change these to fit your project for legacy Input system
        public bool PressNavConfirm() => Input.GetButtonDown("Submit");
        public bool PressNavBack() => Input.GetButtonDown("Cancel");
        public bool NavUp() => Input.GetAxis("Vertical") > 0;
        public bool NavDown() => Input.GetAxis("Vertical") < 0;
        public bool NavLeft() => Input.GetAxis("Horizontal") < 0;
        public bool NavRight() => Input.GetAxis("Horizontal") > 0;
        [Header("If using New Input, edit this script")] // on cursorTransitionTime
#else
        [Header("If using legacy input system, edit this script")]
        public InputActionReference moveAction;
        public InputActionReference confirmAction;
        public InputActionReference backAction;

        public bool PressNavConfirm() => confirmAction?.action?.WasPressedThisFrame() == true;
        public bool PressNavBack() => backAction?.action?.WasPressedThisFrame() == true;
        public bool NavUp() => moveAction?.action?.ReadValue<Vector2>().y > 0.5f;
        public bool NavDown() => moveAction?.action?.ReadValue<Vector2>().y < -0.5f;
        public bool NavLeft() => moveAction?.action?.ReadValue<Vector2>().x < -0.5f;
        public bool NavRight() => moveAction?.action?.ReadValue<Vector2>().x > 0.5f;
#endif

        public float cursorTransitionTime = 0.2f;

        public static MenuNavigationSystem instance;
        public SoundBankSet UIMove;
        public float gridModeResolveDistance = 10;
        public static bool ContextsChangedThisFrame = false;

        public enum NavMode
        {
            Horizontal, // Move with left/right, Up/down do nothing or trigger actions
            Vertical,   // Move with up/down, left/right do nothing or trigger actions
            Grid        // Move with all directions
        }

        public enum BackButtonAction
        {
            None,
            DeactivateContext,
            TriggerItem
        }

        public static void PopContext()
        {
            if (stack.Count > 0)
            {
                stack.RemoveAt(0);
                if (stack.Count > 0)
                {
                    var c = stack[0];
                    c.HighlightItem(c.currentIndex);
                }
            }
        }

        public void PopContextForUI()
        {
            if (stack.Count > 0)
            {
                stack.RemoveAt(0);
                if (stack.Count > 0)
                {
                    var c = stack[0];
                    c.HighlightItem(c.currentIndex);
                }
            }
        }

        public static bool PushContext(MenuNavigationSystemContext c)
        {
            RemoveContext(c); // promote
            stack.Insert(0, c);
            c.HighlightItem(c.currentIndex);
            ContextsChangedThisFrame = true;
            return true;
        }

        public static bool RemoveContext(MenuNavigationSystemContext c)
        {
            stack.RemoveAll(x => x == null);
            bool wasRemoved = stack.Remove(c);
            if (stack.Count > 0) stack[0].HighlightItem(stack[0].currentIndex);
            ContextsChangedThisFrame = true;
            return wasRemoved;
        }

        public static void RemoveNamedContexts(string name)
        {
            ContextsChangedThisFrame = true;
            stack.RemoveAll(x => x.menuTag == name);
        }
        public static void RemoveAllContexts()
        {
            ContextsChangedThisFrame = true;
            stack.Clear();
        }

        public static List<MenuNavigationSystemContext> stack = new List<MenuNavigationSystemContext>();

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (moveAction != null)
                moveAction.action.Enable();
            if (confirmAction != null)
                confirmAction.action.Enable();
            if (backAction != null)
                backAction.action.Enable();
        }

        public void TriggerMenuItem(Transform item)
        {
            if (ContextsChangedThisFrame) return;
            if (item == null) return;

            var navItem = item?.GetComponentInChildren<MenuNavigationItem>();
            if (navItem) navItem.onTriggered?.Invoke();

            var menuContext = item?.GetComponentInChildren<MenuNavigationSystemContext>();
            if (menuContext) { menuContext.Activate(); return; }

            // ET/Button/Toggle will all call with no chain breaking
            var eventTrigger = item?.GetComponentInChildren<EventTrigger>();
            if (eventTrigger) eventTrigger.OnPointerClick(new PointerEventData(EventSystem.current));
            var button = item?.GetComponentInChildren<Button>();
            if (button) button.onClick.Invoke();

            var toggle = item?.GetComponentInChildren<Toggle>();
            if (toggle) toggle.isOn = !toggle.isOn;

            item?.gameObject.SendMessage("OnMenuItemActivated", null, SendMessageOptions.DontRequireReceiver);
        }

        float nextTick = 0;
        void Update()
        {
            if (stack.Count == 0) return;
            if (Time.realtimeSinceStartup < nextTick) return;

            // get the menu context
            var mc = stack.First();
            if (mc.menuItems.Count == 0) return;
            mc.menuItems.RemoveAll(i => i == null);
            if (mc.currentIndex >= mc.menuItems.Count)
                mc.currentIndex = mc.menuItems.Count - 1;

            if(ContextsChangedThisFrame)
            {
                ContextsChangedThisFrame = false;
                return;
            }

            if (PressNavConfirm())
            {
                TriggerMenuItem(mc.menuItems[mc.currentIndex]);
            }
            else if (PressNavBack())
            {
                switch (mc.backAction)
                {
                    case BackButtonAction.DeactivateContext:
                        mc.Deactivate();
                        break;
                    case BackButtonAction.TriggerItem:
                        TriggerMenuItem(mc.BackTriggerItem);
                        break;
                }
            }

            // We'll use walk/climb here form the custom input manager
            var h = (NavLeft() ? -1 : 0) + (NavRight() ? 1 : 0);
            var v = (NavUp() ? 1 : 0) + (NavDown() ? -1 : 0);
            int hh = Mathf.RoundToInt(h);
            int vv = Mathf.RoundToInt(v);
            if (hh == 0 && vv == 0) return;

            MenuNavigationItem currentNavItem = null;
            if (mc.currentIndex != -1 && mc.menuItems[mc.currentIndex] != null)
                currentNavItem = mc.menuItems[mc.currentIndex].GetComponent<MenuNavigationItem>();

            Transform target = null;
            if (currentNavItem != null)
            {
                // Nav Override: onlyIfDefaultNavFails = false
                if (!currentNavItem.onlyIfDefaultNavFails)
                {
                    if (hh == -1 && currentNavItem.navLeft != null)
                        target = currentNavItem.navLeft;
                    else if (hh == 1 && currentNavItem.navRight != null)
                        target = currentNavItem.navRight;
                    else if (vv == 1 && currentNavItem.navUp != null)
                        target = currentNavItem.navUp;
                    else if (vv == -1 && currentNavItem.navDown != null)
                        target = currentNavItem.navDown;
                }
            }

            //target overridden? if yes use it else ise nav mode Horizontal/vertical/grid
            if (target != null)
            {
                for (int i = 0; i < mc.menuItems.Count; i++)
                    if (mc.menuItems[i].transform == target)
                    { mc.currentIndex = i; break; }
            }
            else if (mc.navigationMode == NavMode.Vertical)
            {
                int d = Mathf.RoundToInt(v);
                if (d != 0)
                {
                    mc.currentIndex -= d;
                    if (mc.currentIndex < 0) mc.currentIndex = mc.menuItems.Count - 1;
                    if (mc.currentIndex >= mc.menuItems.Count) mc.currentIndex = 0;
                    nextTick = Time.realtimeSinceStartup + mc.stepsDelayTime;
                    //UIMove.Play(transform.position);
                }
            }
            else if (mc.navigationMode == NavMode.Horizontal)
            {
                int d = Mathf.RoundToInt(h);
                if (d != 0)
                {
                    mc.currentIndex += d;
                    if (mc.currentIndex < 0) mc.currentIndex = mc.menuItems.Count - 1;
                    if (mc.currentIndex >= mc.menuItems.Count) mc.currentIndex = 0;
                    nextTick = Time.realtimeSinceStartup + mc.stepsDelayTime;
                    //UIMove.Play(transform.position);
                }
            }
            else if (mc.navigationMode == NavMode.Grid)
            {
                int x = Mathf.RoundToInt(h);
                int y = Mathf.RoundToInt(v);
                if (mc.menuItems.Count == 0)
                {
                    mc.cursor.gameObject.SetActive(false);
                    return;
                }
                else mc.cursor.gameObject.SetActive(true);
                if (mc.currentIndex < 0) mc.currentIndex = mc.menuItems.Count - 1;
                if (mc.currentIndex >= mc.menuItems.Count) mc.currentIndex = 0;

                // we should perform lookup / point in rect or rect in rect
                Rect ownRect = mc.menuItems[mc.currentIndex].GetComponent<RectTransform>().GetWorldSpaceRect();

                ownRect.x += x * ownRect.width;
                ownRect.y += y * ownRect.height;
                Vector2 ownPoint = ownRect.center;

                for (int i = 0; i < mc.menuItems.Count; i++)
                {
                    if (i == mc.currentIndex) continue;
                    if (mc.menuItems[i].GetComponent<RectTransform>().GetWorldSpaceRect().Contains(ownPoint))
                    {
                        mc.currentIndex = i;
                        nextTick = Time.realtimeSinceStartup + mc.stepsDelayTime;
                        //UIMove.Play(transform.position);
                        return;
                    }
                }
            }

            // Reached here? Then Nav mode failed to resolve and is fallback nav target is set, it will be used
            if (currentNavItem != null && currentNavItem.onlyIfDefaultNavFails)
            {
                if (hh == -1 && currentNavItem.navLeft != null)
                    target = currentNavItem.navLeft;
                else if (hh == 1 && currentNavItem.navRight != null)
                    target = currentNavItem.navRight;
                else if (vv == 1 && currentNavItem.navUp != null)
                    target = currentNavItem.navUp;
                else if (vv == -1 && currentNavItem.navDown != null)
                    target = currentNavItem.navDown;

                if (target != null)
                    for (int i = 0; i < mc.menuItems.Count; i++)
                        if (mc.menuItems[i].transform == target)
                        {
                            mc.currentIndex = i;
                            nextTick = Time.realtimeSinceStartup + mc.stepsDelayTime;
                            return;
                        }
            }

        }

        public static float GetStepsDelayTime()
        {
            if (stack.Count > 0) return stack[0].stepsDelayTime;
            return instance.cursorTransitionTime;
        }

        public static void RefreshMenus(bool force = false)
        {
            foreach (var s in stack)
                if (s.autoDetectItems || force)
                    s.RebuildItems();
        }
    }
}
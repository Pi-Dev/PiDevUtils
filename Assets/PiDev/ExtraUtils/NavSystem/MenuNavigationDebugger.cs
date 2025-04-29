using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace PiDev.Utilities.UI
{
    public class MenuNavigationDebugger : MonoBehaviour
    {
        Text text;
        MenuNavigationSystem ms = null;

        // Use this for initialization
        void Start()
        {
            text = GetComponent<Text>();
        }

        void Update()
        {
            if (ms == null) ms = MenuNavigationSystem.instance;
            string s = "";
            if (ms && MenuNavigationSystem.stack.Count > 0)
            {
                var c = MenuNavigationSystem.stack.First();
                s += string.Format("<b>{0}</b>\n\n", c.name);
                for (int i = 0; i < c.menuItems.Count; i++)
                {
                    var item = c.menuItems[i];
                    s += i == c.currentIndex ? "-> " : "   ";
                    s += (item ? item.name : "[Null item]") + "\n";
                }
                s += "\n\n----------STACK----------\n";
                foreach (var i in MenuNavigationSystem.stack)
                    s += i?.name + "\n";

                s += "\n\n----------INPUT----------\n";
                s += ms.moveAction?.action?.ReadValue<Vector2>().ToString();
                text.text = s;
            }
            else text.text = "<b>MenuNavigationSystem</b> not active";
        }
    }
}
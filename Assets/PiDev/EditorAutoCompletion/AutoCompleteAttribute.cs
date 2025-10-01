using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PiDev.Utilities.AutoCompletion
{
    public class AutoComplete : PropertyAttribute
    {
        public virtual IEnumerable<string> GetItems() => items ?? Array.Empty<string>();
        public virtual IEnumerable<string> GetItems(string filter) => GetItems().Where(x=>x.Contains(filter, StringComparison.OrdinalIgnoreCase));

        public string[] items;
        public AutoComplete() { }
        public AutoComplete(params string[] items) => this.items = items;
    }
}

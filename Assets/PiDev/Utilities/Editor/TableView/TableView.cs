/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * Based on SimpleEditorTableView by Red Games (https://github.com/redclock)
 * Added support for sorting and drag-and-drop reordering.
 *
 * The MIT License (MIT)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * A generic, reorderable and sortable Unity Editor table component with customizable columns.
 * Supports custom drawing per column, sortable column modes, drag-and-drop row reordering, and selection tracking.
 * Designed to be reused in editor tools and inspectors for lists and serialized data displays.
 *
 * ============= Usage =============
 * var table = new TableView<MyDataType>();
 * table.AddColumn("Name", 100, (rect, item) => GUI.Label(rect, item.name));
 * inside OnGUI: table.Render(myItemsArray);
 */

#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;


namespace PiDev.Utilities.Editor
{
    public class TableView<T>
    {
        private MultiColumnHeaderState multiColumnHeaderState;
        private MultiColumnHeader multiColumnHeader;
        private MultiColumnHeaderState.Column[] columns;
        private readonly Color lighterColor = Color.white * 0.0f;
        private readonly Color darkerColor = Color.white * 0.04f;

        private Vector2 scrollPos;
        private bool columnResized;
        private bool sortingDirty;

        public int SelectedRowIndex { get; private set; } = -1;
        public int SelectedColumnIndex { get; private set; } = -1;
        //public T SelectedItem { get; private set; }

        // Assign this action to enable drag/drop
        public Action<int, int> OnReorder;
        private int dragFromIndex = -1;

        private Rect? dropLineRect;
        public bool AllowRowReordering = false;

        public delegate void DrawItem(Rect rect, T item);

        public class ColumnDefinition
        {
            public string baseLabel;
            public MultiColumnHeaderState.Column column;
            public DrawItem onDraw;
            public List<(string label, Comparison<T> comparer)> sortModes;
            public int currentSortIndex = -1;

            public ColumnDefinition SetMaxWidth(float maxWidth)
            {
                column.maxWidth = maxWidth;
                return this;
            }

            public ColumnDefinition SetTooltip(string tooltip)
            {
                column.headerContent.tooltip = tooltip;
                return this;
            }

            public ColumnDefinition SetAutoResize(bool autoResize)
            {
                column.autoResize = autoResize;
                return this;
            }

            public ColumnDefinition SetAllowToggleVisibility(bool allow)
            {
                column.allowToggleVisibility = allow;
                return this;
            }
        }

        private readonly List<ColumnDefinition> columnDefinitions = new();

        private T[] cachedInput;
        private T[] cachedSorted;
        private int currentSortColumnIndex = -1;

        public void ClearColumns()
        {
            columnDefinitions.Clear();
            columnResized = true;
        }

        public ColumnDefinition AddColumn(
            string title,
            int minWidth,
            DrawItem onDrawItem,
            bool allowToggleVisibility = false,
            List<(string label, Comparison<T> comparer)> sortModes = null)
        {
            ColumnDefinition def = new()
            {
                baseLabel = title,
                column = new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = allowToggleVisibility,
                    autoResize = true,
                    minWidth = minWidth,
                    maxWidth = int.MaxValue,
                    canSort = sortModes != null,
                    sortingArrowAlignment = TextAlignment.Left, // No visual arrow
                    headerContent = new GUIContent(title),
                    headerTextAlignment = TextAlignment.Left,
                },
                onDraw = onDrawItem,
                sortModes = sortModes,
                currentSortIndex = (sortModes != null && sortModes.Count > 0) ? 0 : -1
            };

            columnDefinitions.Add(def);
            columnResized = true;
            return def;
        }

        private void Rebuild()
        {
            columns = columnDefinitions.Select(def => def.column).ToArray();
            multiColumnHeaderState = new MultiColumnHeaderState(columns);
            multiColumnHeader = new MultiColumnHeader(multiColumnHeaderState);
            multiColumnHeader.visibleColumnsChanged += (hdr) => hdr.ResizeToFit();
            multiColumnHeader.sortingChanged += OnHeaderSortingChanged;
            multiColumnHeader.ResizeToFit();
            columnResized = false;
        }

        private void OnHeaderSortingChanged(MultiColumnHeader hdr)
        {
            int colIndex = hdr.sortedColumnIndex;
            if (colIndex < 0 || colIndex >= columnDefinitions.Count)
                return;

            var clickedDef = columnDefinitions[colIndex];
            if (clickedDef.sortModes != null && clickedDef.sortModes.Count > 0)
            {
                // Cycle sort index
                clickedDef.currentSortIndex = (clickedDef.currentSortIndex + 1) % clickedDef.sortModes.Count;

                // Update internal tracking
                currentSortColumnIndex = colIndex;
                sortingDirty = true;
                GUI.FocusControl(null);

                // Suppress Unity's internal arrow
                try { hdr.SetSorting(-1, true); } catch { }

                // Repaint window
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        //bool refocus = false;
        public void Render(T[] data, Rect rect = new Rect(), float maxHeight = float.MaxValue, float rowHeight = -1, bool inspectorMode = false, string controlIdPrefix = "")
        {

            // var w = EditorWindow.focusedWindow;
            //Debug.Log($"FW={w?.titleContent.text} && {GUI.GetNameOfFocusedControl()}");
            try
            {
                if (multiColumnHeader == null || columnResized) Rebuild();

                bool shouldResort = sortingDirty ||
                                    cachedSorted == null ||
                                    data.Length != cachedSorted.Length;

                if (shouldResort)
                {
                    Debug.Log("TableView sorted");
                    cachedInput = data;
                    cachedSorted = (T[])data.Clone();
                    UpdateSorting(cachedSorted);
                    sortingDirty = false;
                    GUI.FocusControl(null);
                    GUIUtility.keyboardControl = 0;
                }

                DrawTableGUI(cachedSorted, rect, maxHeight, rowHeight, inspectorMode, controlIdPrefix);

            }
            catch (ExitGUIException e) { throw e; }
            catch (ObjectDisposedException e)
            {
                /* BUG: Deleting items disposes the Serialized Property */
                // HandleUtility.Repaint();
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        int controlId;
        private void DrawTableGUI(T[] sortedData, Rect rect, float maxHeight, float rowHeight, bool useInspectorMode, string controlIdPrefix)
        {
            // Handle navigation
            if (Event.current.rawType == EventType.KeyDown)
            {
                string focusedName = GUI.GetNameOfFocusedControl();
                if (!focusedName.StartsWith(controlIdPrefix)) return;
                var kc = Event.current.keyCode;
                bool editingText = EditorGUIUtility.editingTextField;
                Debug.Log($"editingText={editingText} && focusedName:{GUI.GetNameOfFocusedControl()}");

                bool isMovement = kc == KeyCode.UpArrow || kc == KeyCode.DownArrow || kc == KeyCode.LeftArrow || kc == KeyCode.RightArrow;
                bool escapeWithArrows = editingText && (kc == KeyCode.UpArrow || kc == KeyCode.DownArrow);
                bool ignore = editingText && (kc == KeyCode.LeftArrow || kc == KeyCode.RightArrow);
                bool isCell = TryParseCellCoordinates(focusedName, out int CellColumn, out int CellRow);

                if (isMovement && !(ignore || (!escapeWithArrows && editingText)) && isCell)
                {
                    if (kc == KeyCode.UpArrow) CellRow--;
                    else if (kc == KeyCode.DownArrow) CellRow++;
                    else if (kc == KeyCode.LeftArrow) CellColumn--;
                    else if (kc == KeyCode.RightArrow) CellColumn++;

                    CellRow = Mathf.Clamp(CellRow, 0, sortedData.Length - 1);
                    CellColumn = Mathf.Clamp(CellColumn, 0, columnDefinitions.Count - 1);

                    // Do something with new x/y
                    SelectedRowIndex = CellRow;
                    SelectedColumnIndex = CellColumn;
                    //SelectedItem = sortedData[CellRow];
                    var cellName = $"{controlIdPrefix}TableCell[{CellColumn},{CellRow}]";
                    GUI.FocusControl(cellName);
                    //EditorGUI.FocusTextInControl(cellName);
                    HandleUtility.Repaint();
                    return;
                }

                if (kc == KeyCode.KeypadEnter || kc == KeyCode.Return)
                {
                    var cellName = $"{controlIdPrefix}TableCell[{CellColumn},{CellRow}]";
                    GUI.FocusControl(cellName);
                    //EditorGUI.FocusTextInControl(cellName);
                    HandleUtility.Repaint();
                }
            }

            float rowWidth = Mathf.Max(rect.width - 8f, multiColumnHeaderState.widthOfAllVisibleColumns);
            if (rowHeight < 0) rowHeight = EditorGUIUtility.singleLineHeight;

            if (!useInspectorMode)
            {
                Rect headerRect = GUILayoutUtility.GetRect(rowWidth, rowHeight);
                multiColumnHeader.OnGUI(headerRect, xScroll: scrollPos.x);
                DrawActiveSortModeLabel(useInspectorMode);
            }

            float sumWidth = rowWidth;
            float sumHeight = rowHeight * sortedData.Length + GUI.skin.horizontalScrollbar.fixedHeight;

            Rect scrollViewPos = GUILayoutUtility.GetRect(0, sumWidth, 0, maxHeight);
            Rect viewRect = new Rect(0, 0, sumWidth, sumHeight);

            scrollPos = GUI.BeginScrollView(scrollViewPos, scrollPos, viewRect, false, false);
            if (useInspectorMode)
            {
                multiColumnHeader.OnGUI(new Rect(0, 0, rowWidth, EditorGUIUtility.singleLineHeight), 0);
                DrawActiveSortModeLabel(useInspectorMode);
            }

            EditorGUILayout.BeginVertical();
            Rect? DrawDropRect = null;

            for (int row = 0; row < sortedData.Length; row++)
            {
                Rect rowRect = new Rect(0, rowHeight * (row + (useInspectorMode ? 1 : 0)), rowWidth, rowHeight);

                Color rowColor = row == SelectedRowIndex ? GetSelectionColor() :
                                 (row % 2 == 0 ? darkerColor : lighterColor);
                EditorGUI.DrawRect(rowRect, rowColor);

                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    SelectedRowIndex = row;
                    //SelectedItem = sortedData[row];
                }

                if (AllowRowReordering)
                {
                    Event e = Event.current;

                    if (e.type == EventType.MouseDrag && rowRect.Contains(e.mousePosition) && dragFromIndex == -1)
                    {
                        dragFromIndex = row;
                        controlId = GUIUtility.GetControlID(FocusType.Passive);
                        GUIUtility.hotControl = controlId;
                        e.Use();
                    }

                    if (dragFromIndex >= 0 && rowRect.Contains(e.mousePosition))
                    {
                        // Live drop preview
                        DrawDropRect = rowRect; // new Rect(rowRect.x, rowRect.yMin - 2, rowRect.width, 4);

                        // Finalize drop here immediately
                        if (e.type == EventType.MouseUp && dragFromIndex != row)
                        {
                            OnReorder?.Invoke(dragFromIndex, row);
                            dragFromIndex = -1;
                            e.Use();
                        }
                        else if (e.type == EventType.MouseUp) // Cancel drop on same row
                        {
                            dragFromIndex = -1;
                            e.Use();
                        }
                    }
                }

                for (int col = 0; col < columns.Length; col++)
                {
                    if (multiColumnHeader.IsColumnVisible(col))
                    {
                        int visibleCol = multiColumnHeader.GetVisibleColumnIndex(col);
                        Rect cellRect = multiColumnHeader.GetCellRect(visibleCol, rowRect);
                        GUI.color = row == SelectedRowIndex ? Color.Lerp(GetSelectionColor(), Color.white, 0.8f) : Color.white;
                        GUI.SetNextControlName($"{controlIdPrefix}TableCell[{col},{row}]");
                        if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                            SelectedColumnIndex = col;

                        EditorGUI.BeginChangeCheck();
                        columnDefinitions[col].onDraw(cellRect, sortedData[row]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            //Debug.Log($"Row {row} changed: {sortedData[row].GetType().Name}");
                            if (sortedData[row] is SerializedProperty prop)
                            {
                                //Debug.Log($" => prop.serializedObject.ApplyModifiedProperties");
                                prop.serializedObject.ApplyModifiedProperties();
                            }
                            else if (sortedData[row] is ScriptableObject so)
                            {
                                //Debug.Log($" => EditorUtility.SetDirty(ScriptableObject);");
                                EditorUtility.SetDirty(so);
                            }
                            else if (sortedData[row] is UnityEngine.Object obj)
                            {
                                //Debug.Log($" => EditorUtility.SetDirty(UnityEngine.Object);");
                                EditorUtility.SetDirty(obj);
                            }
                        }
                    }
                }
            }

            if (DrawDropRect.HasValue)
            {
                GUI.color = Color.green;
                GUI.Box(DrawDropRect.Value, "", EditorStyles.helpBox);
                GUI.color = Color.white;
                EditorWindow.focusedWindow?.Repaint();
            }

            GUI.color = Color.white;

            // Always cancel drag when mouse is released, even if not over a row
            if (Event.current.rawType == EventType.MouseUp && GUIUtility.hotControl == controlId && dragFromIndex >= 0)
            {
                dragFromIndex = -1;
                Event.current.Use(); // optional, prevents propagation
            }
            EditorGUILayout.EndVertical();
            if (dropLineRect.HasValue)
            {
                EditorGUI.DrawRect(dropLineRect.Value, new Color(1f, 0.6f, 0f, 0.5f));
            }
            GUI.EndScrollView(handleScrollWheel: true);
        }

        private void DrawActiveSortModeLabel(bool useInspectorMode)
        {
            if (multiColumnHeader == null || currentSortColumnIndex < 0 || currentSortColumnIndex >= columnDefinitions.Count)
                return;

            var def = columnDefinitions[currentSortColumnIndex];
            if (def.sortModes == null || def.currentSortIndex < 0)
                return;

            string modeLabel = def.sortModes[def.currentSortIndex].label;

            Rect rect = multiColumnHeader.GetColumnRect(currentSortColumnIndex);

            Rect labelRect = new Rect(
                rect.x + 4,
                rect.y + 4 + (useInspectorMode ? 0 : EditorGUIUtility.singleLineHeight),
                rect.width - 8,
                rect.height - 4
            );

            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.UpperRight,
                fontStyle = FontStyle.Italic
            };

            Color originalColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.35f); // Transparent white
            GUI.Label(labelRect, modeLabel, style);
            GUI.color = originalColor;
        }

        private void UpdateSorting(T[] data)
        {
            if (currentSortColumnIndex < 0 || currentSortColumnIndex >= columnDefinitions.Count)
                return;

            var def = columnDefinitions[currentSortColumnIndex];
            if (def.sortModes == null || def.currentSortIndex < 0)
                return;

            var (_, comparer) = def.sortModes[def.currentSortIndex];
            if (comparer != null)
                Array.Sort(data, comparer);
        }

        private static Color GetSelectionColor()
        {
#if UNITY_2021_1_OR_NEWER
            return GUI.skin.settings.selectionColor;
#else
            return EditorGUIUtility.isProSkin
                ? new Color(62f / 255f, 95f / 255f, 150f / 255f, 1f)
                : new Color(0.24f, 0.49f, 0.90f, 0.4f);
#endif
        }

        public static bool TryParseCellCoordinates(string input, out int col, out int row)
        {
            col = -1;
            row = -1;

            var match = Regex.Match(input, @"\d+\s*[:,;]\s*\d+");
            if (!match.Success)
                return false;

            var parts = Regex.Split(match.Value, @"\s*[:,;]\s*");
            return parts.Length == 2 &&
                   int.TryParse(parts[0], out col) &&
                   int.TryParse(parts[1], out row);
        }

        public static List<(string, Comparison<SerializedProperty>)> CreateSortOptionsForField(string fieldName, SerializedPropertyType propType)
        {
            List<(string, Comparison<SerializedProperty>)> sortModes = null;
            switch (propType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                    sortModes = new()
                        {
                            ("▲", (a, b) => a.FindPropertyRelative(fieldName).intValue
                                             .CompareTo(b.FindPropertyRelative(fieldName).intValue)),
                            ("▼", (a, b) => b.FindPropertyRelative(fieldName).intValue
                                             .CompareTo(a.FindPropertyRelative(fieldName).intValue)),
                            ("", null)
                        };
                    break;

                case SerializedPropertyType.Float:
                    sortModes = new()
                        {
                            ("▲", (a, b) => a.FindPropertyRelative(fieldName).floatValue
                                             .CompareTo(b.FindPropertyRelative(fieldName).floatValue)),
                            ("▼", (a, b) => b.FindPropertyRelative(fieldName).floatValue
                                             .CompareTo(a.FindPropertyRelative(fieldName).floatValue)),
                            ("", null)
                        };
                    break;

                case SerializedPropertyType.String:
                    sortModes = new()
                        {
                            ("▲", (a, b) => string.Compare(
                                a.FindPropertyRelative(fieldName).stringValue,
                                b.FindPropertyRelative(fieldName).stringValue, StringComparison.Ordinal)),
                            ("▼", (a, b) => string.Compare(
                                b.FindPropertyRelative(fieldName).stringValue,
                                a.FindPropertyRelative(fieldName).stringValue, StringComparison.Ordinal)),
                            ("", null)
                        };
                    break;

                case SerializedPropertyType.ObjectReference:
                    sortModes = new()
                        {
                            ("▲", (a, b) => string.Compare(
                                a.FindPropertyRelative(fieldName).objectReferenceValue?.name,
                                b.FindPropertyRelative(fieldName).objectReferenceValue?.name, StringComparison.Ordinal)),
                            ("▼", (a, b) => string.Compare(
                                b.FindPropertyRelative(fieldName).objectReferenceValue?.name,
                                a.FindPropertyRelative(fieldName).objectReferenceValue?.name, StringComparison.Ordinal)),
                            ("", null)
                        };
                    break;
            }
            return sortModes;
        }
    }
}
#endif
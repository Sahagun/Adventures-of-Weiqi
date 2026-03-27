using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace EditorAttributes.Editor
{
    [CustomPropertyDrawer(typeof(TabGroupAttribute))]
    public class TabGroupDrawer : GroupDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var tabGroupAttribute = attribute as TabGroupAttribute;

            string selectedTabSaveKey = CreatePropertySaveKey(property, "SelectedTab");
            string[] propertyNames = GetPropertyDisplayNames(property, tabGroupAttribute);

            ValueTabView tabView = new(EditorPrefs.GetInt(selectedTabSaveKey));
            tabView.SelectedTabChanged += (selectedTabIndex) => EditorPrefs.SetInt(selectedTabSaveKey, selectedTabIndex);

            ApplyBoxStyle(tabView);

            for (int i = 0; i < propertyNames.Length; i++)
            {
                string propertyName = propertyNames[i];
                string fieldName = tabGroupAttribute.FieldsToGroup[i];
                VisualElement groupProperty = CreateGroupProperty(fieldName, property);
                VisualElement tabContent = new();

                ApplyBoxStyle(tabContent);
                tabContent.Add(groupProperty);
                tabView.AddTab(propertyName, tabContent);
            }

            return tabView;
        }

        private string[] GetPropertyDisplayNames(SerializedProperty property, TabGroupAttribute tabGroupAttribute)
        {
            List<string> stringList = new();

            foreach (var field in tabGroupAttribute.FieldsToGroup)
            {
                SerializedProperty fieldProperty = FindNestedProperty(property, GetSerializedPropertyName(field, property));

                stringList.Add(fieldProperty == null ? field : fieldProperty.displayName);
            }

            return stringList.ToArray();
        }

        private sealed class ValueTabView : VisualElement
        {
            private readonly VisualElement tabHeader = new();
            private readonly VisualElement tabContent = new();
            private readonly List<Button> tabButtons = new();
            private readonly List<VisualElement> tabPages = new();
            private readonly Color activeTabColor = EditorExtension.GLOBAL_COLOR / 2f;

            private int selectedTabIndex;

            public event Action<int> SelectedTabChanged;

            public ValueTabView(int selectedTabIndex)
            {
                this.selectedTabIndex = Mathf.Max(0, selectedTabIndex);

                style.flexDirection = FlexDirection.Column;

                tabHeader.style.flexDirection = FlexDirection.Row;
                tabHeader.style.flexWrap = Wrap.Wrap;

                tabContent.style.flexDirection = FlexDirection.Column;

                Add(tabHeader);
                Add(tabContent);
            }

            public void AddTab(string title, VisualElement page)
            {
                int tabIndex = tabButtons.Count;
                Button button = new(() => SetSelectedTab(tabIndex, true))
                {
                    text = title
                };

                button.style.flexGrow = 1f;

                tabButtons.Add(button);
                tabHeader.Add(button);

                page.style.display = DisplayStyle.None;

                tabPages.Add(page);
                tabContent.Add(page);

                SetSelectedTab(selectedTabIndex, false);
            }

            private void SetSelectedTab(int tabIndex, bool notify)
            {
                if (tabButtons.Count == 0)
                    return;

                selectedTabIndex = Mathf.Clamp(tabIndex, 0, tabButtons.Count - 1);

                for (int i = 0; i < tabButtons.Count; i++)
                {
                    bool isActive = i == selectedTabIndex;

                    tabButtons[i].style.backgroundColor = isActive ? new StyleColor(activeTabColor) : new StyleColor(StyleKeyword.Null);
                    tabButtons[i].style.unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal;
                    tabPages[i].style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (notify)
                    SelectedTabChanged?.Invoke(selectedTabIndex);
            }
        }
    }
}

using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using EditorAttributes.Editor.Utility;

namespace EditorAttributes.Editor
{
    [CustomPropertyDrawer(typeof(ValueButtonsAttribute))]
    public class ValueButtonsDrawer : CollectionDisplayDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var valueButtonsAttribute = attribute as ValueButtonsAttribute;

            HelpBox errorBox = new();
            MemberInfo collectionInfo = ReflectionUtils.GetValidMemberInfo(valueButtonsAttribute.CollectionName, property);

            List<string> propertyValues = ConvertCollectionValuesToStrings(valueButtonsAttribute.CollectionName, property, collectionInfo, errorBox);
            List<string> displayValues = GetDisplayValues(collectionInfo, valueButtonsAttribute, property, propertyValues);

            if (!IsCollectionValid(displayValues))
                return new HelpBox("The provided collection is empty", HelpBoxMessageType.Error);

            int buttonsValueIndex = propertyValues.IndexOf(GetPropertyValueAsString(property));

            ValueButtonGroup valueButtons = DrawButtons(buttonsValueIndex, displayValues, valueButtonsAttribute, (value) =>
            {
                if (valueButtonsAttribute.DisplayNames != null || IsCollectionDictionary(collectionInfo, property, out _))
                {
                    if (value >= 0 && value < propertyValues.Count)
                        SetPropertyValueFromString(propertyValues[value], property);
                }
                else
                {
                    if (value >= 0 && value < propertyValues.Count)
                        SetPropertyValueFromString(propertyValues[value], property);
                }
            });

            valueButtons.TrackPropertyValue(property, (trackedProperty) =>
            {
                string propertyStringValue = GetPropertyValueAsString(trackedProperty);

                if (propertyValues.Contains(propertyStringValue))
                {
                    int propertyValueIndex = propertyValues.IndexOf(propertyStringValue);
                    valueButtons.SetValueWithoutNotify(propertyValueIndex);
                }
                else
                {
                    Debug.LogWarning($"The value <b>{propertyStringValue}</b> set to the <b>{trackedProperty.name}</b> variable is not a value available in the button selection", trackedProperty.serializedObject.targetObject);
                }
            });

            AddPropertyContextMenu(valueButtons, property);
            DisplayErrorBox(valueButtons, errorBox);

            return valueButtons;
        }

        private ValueButtonGroup DrawButtons(int buttonsValue, List<string> valueLabels, ValueButtonsAttribute selectionButtonsAttribute, Action<int> onValueChanged)
        {
            ValueButtonGroup buttonGroup = new(selectionButtonsAttribute.ShowLabel ? preferredLabel : string.Empty, valueLabels, selectionButtonsAttribute.ButtonsHeight, onValueChanged);
            buttonGroup.SetValueWithoutNotify(buttonsValue == -1 ? 0 : buttonsValue);
            return buttonGroup;
        }

        private sealed class ValueButtonGroup : VisualElement
        {
            private readonly List<Button> buttons = new();
            private readonly Action<int> onValueChanged;
            private readonly Color activeButtonColor = EditorExtension.GLOBAL_COLOR / 2f;

            private int selectedIndex = -1;

            public ValueButtonGroup(string labelText, List<string> valueLabels, float buttonsHeight, Action<int> onValueChanged)
            {
                this.onValueChanged = onValueChanged;

                AddToClassList(BaseField<bool>.alignedFieldUssClassName);
                style.flexDirection = FlexDirection.Row;
                style.alignItems = Align.Center;

                if (!string.IsNullOrEmpty(labelText))
                {
                    Label label = new(labelText);
                    label.AddToClassList(BaseField<bool>.labelUssClassName);
                    Add(label);
                }

                VisualElement buttonsContainer = new()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexWrap = Wrap.Wrap,
                        flexGrow = 1f
                    }
                };

                Add(buttonsContainer);

                for (int i = 0; i < valueLabels.Count; i++)
                {
                    int buttonIndex = i;
                    Button button = new(() => SetSelectedIndex(buttonIndex, true))
                    {
                        text = valueLabels[i],
                        style =
                        {
                            height = buttonsHeight
                        }
                    };

                    button.style.flexGrow = 1f;

                    buttons.Add(button);
                    buttonsContainer.Add(button);
                }
            }

            public void SetValueWithoutNotify(int index) => SetSelectedIndex(index, false);

            private void SetSelectedIndex(int index, bool notify)
            {
                if (buttons.Count == 0)
                    return;

                selectedIndex = Mathf.Clamp(index, 0, buttons.Count - 1);

                for (int i = 0; i < buttons.Count; i++)
                {
                    bool isActive = i == selectedIndex;

                    buttons[i].style.backgroundColor = isActive ? new StyleColor(activeButtonColor) : new StyleColor(StyleKeyword.Null);
                    buttons[i].style.unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal;
                }

                if (notify)
                    onValueChanged?.Invoke(selectedIndex);
            }
        }
    }
}

﻿using System;
using UnityCommons;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class EnumSelectionDropdown<T> : VisualElement, INotifyValueChanged<T> where T : Enum {
        private readonly Button button;
        private T rawValue;

        internal EnumSelectionDropdown(T value, SelectionTree tree, string label = null) {
            AddToClassList("enum-selection-dropdown");
            button = new Button();
            button.AddToClassList("enum-dropdown-button");
            VisualElement arrow = new();
            arrow.AddToClassList("arrow-down");
            button.Add(arrow);

            if (label != null) {
                Label labelElement = new(label);
                Add(labelElement);
                AddToClassList("with-label");
            }

            Add(button);

            button.clicked += () => {
                Vector2 pos = GUIUtility.GUIToScreenPoint(worldBound.position);
                SelectionWindow.ShowWindow(pos, worldBound.height, tree, selection => {
                    this.value = (T)selection;
                });
            };

            SetValueWithoutNotify(value, 1);
        }

        public void SetValueWithoutNotify(T newValue, int scheduleNesting = 0) {
            rawValue = newValue;
            string buttonText = RandomUtilities.DisplayNameEnum(rawValue);
            button.text = buttonText;

            scheduleNesting = scheduleNesting.Clamped(0, 2);

            if (scheduleNesting == 0) {
                schedule.Execute(() => {
                    Vector2 size = button.MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                    button.style.minWidth = size.x + 24f;
                });
            } else if (scheduleNesting == 1) {
                schedule.Execute(() => {
                    schedule.Execute(() => {
                        Vector2 size = button.MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                        button.style.minWidth = size.x + 24f;
                    });
                });
            } else {
                schedule.Execute(() => {
                    schedule.Execute(() => {
                        schedule.Execute(() => {
                            Vector2 size = button.MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                            button.style.minWidth = size.x + 24f;
                        });
                    });
                });
            }

        }

        void INotifyValueChanged<T>.SetValueWithoutNotify(T newValue) {
            SetValueWithoutNotify(newValue);
        }

        public T value {
            get => rawValue;
            set {
                using ChangeEvent<T> pooled = ChangeEvent<T>.GetPooled(rawValue, value);
                pooled.target = this;
                SetValueWithoutNotify(value);
                SendEvent(pooled);
            }
        }
    }
}
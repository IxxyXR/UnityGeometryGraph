﻿using System;
using System.Globalization;
using GeometryGraph.Editor.Utils;
using GeometryGraph.Runtime;
using UnityCommons;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class ClampedFloatField : TextValueField<float> {
        private float? max;
        private float? min;

        public ClampedFloatField() : this(null) {
        }

        public ClampedFloatField(string label, float? min = null, float? max = null, int maxLength = -1)
            : base(label, maxLength, new Input()) {
            this.min = min;
            this.max = max;

            AddToClassList(FloatField.ussClassName);
            AddToClassList("clamped-field");
            labelElement.AddToClassList(FloatField.labelUssClassName);
            FloatInput.AddToClassList(FloatField.inputUssClassName);
            AddLabelDragger<float>();

            RegisterCallback<BlurEvent>(_ => ClampWithoutNotify());
        }

        public float? Min {
            get => min;
            set {
                min = value;
                if (value == null) return;
                if (this.value < min) this.value = (int)value;
            }
        }

        public float? Max {
            get => max;
            set {
                max = value;
                if (value == null) return;
                if (this.value > max) this.value = (int)value;
            }
        }

        private Input FloatInput => (Input)textInputBase;

        private void ClampWithoutNotify() {
            float val = value;
            if (min != null) val = val.MinClamped((float)min);
            if (max != null) val = val.MaxClamped((float)max);
            SetValueWithoutNotify(val);
        }

        private void Clamp() {
            float val = value;
            if (min != null) val = val.MinClamped((float)min);
            if (max != null) val = val.MaxClamped((float)max);
            if (Math.Abs(val - value) > Constants.FLOAT_TOLERANCE) value = val;
        }

        protected override string ValueToString(float v) {
            return v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        protected override float StringToValue(string str) {
            if (double.TryParse(str, out double num)) return MathUtils.ClampToFloat(num);

            return default;
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue) {
            FloatInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }

        public new class UxmlFactory : UxmlFactory<ClampedFloatField, UxmlTraits> {
        }

        public new class UxmlTraits : TextValueFieldTraits<float, UxmlFloatAttributeDescription> {
        }

        private class Input : TextValueInput {
            internal Input() {
                formatString = "g7";
            }

            private ClampedFloatField ParentField => (ClampedFloatField)parent;

            protected override string allowedCharacters => "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue) {
                double dragSensitivity = NumericFieldDraggerUtility.CalculateFloatDragSensitivity(startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                double num = MathUtils.RoundBasedOnMinimumDifference(StringToValue(text) + NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * dragSensitivity,
                                                                     dragSensitivity);

                if (ParentField.min != null) num = num.MinClamped((double)ParentField.min);
                if (ParentField.max != null) num = num.MaxClamped((double)ParentField.max);

                if (ParentField.isDelayed) text = ValueToString(MathUtils.ClampToFloat(num));
                else ParentField.value = MathUtils.ClampToFloat(num);
            }

            protected override string ValueToString(float v) {
                return v.ToString(formatString);
            }

            protected override float StringToValue(string str) {
                if (double.TryParse(str, out double num)) return MathUtils.ClampToFloat(num);

                return default;
            }
        }
    }
}
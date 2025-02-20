﻿using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    public partial class PlanePrimitiveNode {
        [In] public float Width { get; private set; } = 1.0f;
        [In] public float Height { get; private set; } = 1.0f;

        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Subdivisions { get; private set; }

        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Plane(new float2(Width, Height), Subdivisions);
        }
    }
}
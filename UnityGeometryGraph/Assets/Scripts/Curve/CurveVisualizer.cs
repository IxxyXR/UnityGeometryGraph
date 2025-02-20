﻿using System.Linq;
using GeometryGraph.Runtime.Curve;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class CurveVisualizer : MonoBehaviour {
        public bool ShowSpline = true;
        [ShowIf(nameof(ShowSpline))] public float SplineWidth = 2.0f;
        [ShowIf(nameof(ShowSpline))] public Color SplineColor = Color.white;
        [Space] public bool ShowPoints = true;
        [ShowIf(nameof(ShowPoints))] public float PointSize = 0.01f;
        [ShowIf(nameof(ShowPoints))] public Color PointColor = Color.white;

        [Space] public bool ShowDirectionVectors = true;
        [ShowIf(nameof(ShowDirectionVectors))] public float DirectionVectorLength = 0.1f;
        [ShowIf(nameof(ShowDirectionVectors))] public float DirectionVectorWidth = 2.0f;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionTangentColor = Color.blue;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionNormalColor = Color.red;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionBinormalColor = Color.green;

        public bool Enabled;

        [ShowInInspector] private CurveData curveData;

        public void Load(CurveData curveData) {
            DebugUtility.Log("Loaded curve into visualizer");
            this.curveData = curveData;
        }

        private void OnDrawGizmos() {
            if (!Enabled || curveData == null) return;

            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            if (ShowPoints || ShowSpline) {
                Vector3[] points = curveData.Position.Select(float3 => (Vector3)float3).ToArray();
                Handles.color = SplineColor;
                if (ShowSpline) {
                    Handles.DrawAAPolyLine(SplineWidth, points);
                    if (curveData.IsClosed) Handles.DrawAAPolyLine(SplineWidth, points[0], points[^1]);
                }

                if (ShowPoints) {
                    Gizmos.color = PointColor;
                    foreach (Vector3 p in points) {
                        Gizmos.DrawSphere(p, PointSize);
                    }
                }
            }

            if (ShowDirectionVectors) {
                for (int i = 0; i < curveData.Position.Count; i++) {
                    float3 p = curveData.Position[i];
                    float3 t = curveData.Tangent[i];
                    float3 n = curveData.Normal[i];
                    float3 b = curveData.Binormal[i];

                    Handles.color = DirectionTangentColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + t * DirectionVectorLength);
                    Handles.color = DirectionNormalColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + n * DirectionVectorLength);
                    Handles.color = DirectionBinormalColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + b * DirectionVectorLength);
                }
            }
        }
    }
}

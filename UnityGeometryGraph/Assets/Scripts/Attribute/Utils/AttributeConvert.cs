﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace Attribute {
    // Domain conversion
    internal static partial class AttributeConvert {
        // Average functions
        private static object Average(AttributeType type, params object[] values) {
            return type switch {
                AttributeType.Boolean => Average((bool[])(IEnumerable)values),
                AttributeType.Integer => Average((int[])(IEnumerable)values),
                AttributeType.Float => Average((float[])(IEnumerable)values),
                AttributeType.ClampedFloat => Average((float[])(IEnumerable)values).Clamped01(),
                AttributeType.Vector2 => Average((float2[])(IEnumerable)values),
                AttributeType.Vector3 => Average((float3[])(IEnumerable)values),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        // Note: Maybe there is a better way to 'average' boolean values but idk
        private static bool Average(params bool[] values) => values.Count(b => b) < 0.5f * values.Length;
        private static int Average(params int[] values) => (int) values.Average();
        private static float Average(params float[] values) => values.Average();
        private static float2 Average(params float2[] values) => values.Aggregate((a, b) => a + b) / values.Length;
        private static float3 Average(params float3[] values) => values.Aggregate((a, b) => a + b) / values.Length;
    }

    // Misc
    internal static partial class AttributeConvert {
        internal static bool TryGetType(object value, out AttributeType type) {
            var valueType = value.GetType();
            type = typeConversionDictionary.ContainsKey(valueType) ? typeConversionDictionary[valueType] : AttributeType.Invalid;
            return type != AttributeType.Invalid;
        }
    }
    
    // Type conversion
    internal static partial class AttributeConvert {
        // oof sorry 
        internal static T ConvertType<T>(object value, AttributeType from, AttributeType to) {
            return from switch {
                AttributeType.Boolean => to switch {
                    AttributeType.Boolean => (T)value,
                    AttributeType.Integer => (T)ConvertType_BoolInt((bool)value),
                    AttributeType.Float => (T)ConvertType_BoolFloat((bool)value),
                    AttributeType.ClampedFloat => (T)ConvertType_BoolClampedFloat((bool)value),
                    AttributeType.Vector2 => (T)ConvertType_BoolVec2((bool)value),
                    AttributeType.Vector3 => (T)ConvertType_BoolVec3((bool)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Integer => to switch {
                    AttributeType.Boolean => (T)ConvertType_IntBool((int)value),
                    AttributeType.Integer => (T)value,
                    AttributeType.Float => (T)ConvertType_IntFloat((int)value),
                    AttributeType.ClampedFloat => (T)ConvertType_IntClampedFloat((int)value),
                    AttributeType.Vector2 => (T)ConvertType_IntVec2((int)value),
                    AttributeType.Vector3 => (T)ConvertType_IntVec3((int)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Float => to switch {
                    AttributeType.Boolean => (T)ConvertType_FloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_FloatInt((float)value),
                    AttributeType.Float => (T)value,
                    AttributeType.ClampedFloat => (T)ConvertType_FloatClampedFloat((float)value),
                    AttributeType.Vector2 => (T)ConvertType_FloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_FloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.ClampedFloat => to switch {
                    AttributeType.Boolean => (T)ConvertType_ClampedFloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_ClampedFloatInt((float)value),
                    AttributeType.Float => (T)ConvertType_ClampedFloatFloat((float)value),
                    AttributeType.ClampedFloat => (T)value,
                    AttributeType.Vector2 => (T)ConvertType_ClampedFloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_ClampedFloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector2 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec2Bool((float2)value),
                    AttributeType.Integer => (T)ConvertType_Vec2Int((float2)value),
                    AttributeType.Float => (T)ConvertType_Vec2Float((float2)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec2ClampedFloat((float2)value),
                    AttributeType.Vector2 => (T)value,
                    AttributeType.Vector3 => (T)ConvertType_Vec2Vec3((float2)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector3 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec3Bool((float3)value),
                    AttributeType.Integer => (T)ConvertType_Vec3Int((float3)value),
                    AttributeType.Float => (T)ConvertType_Vec3Float((float3)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec3ClampedFloat((float3)value),
                    AttributeType.Vector2 => (T)ConvertType_Vec3Vec2((float3)value),
                    AttributeType.Vector3 => (T)value,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
            };
        }

        // Boolean
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolInt(bool a) => a ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolClampedFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec2(bool a) => a ? math_util.one2 : float2.zero;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec3(bool a) => a ? math_util.one3 : float3.zero;

        // Integer
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntBool(int a) => a != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntFloat(int a) => a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntClampedFloat(int a) => a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec2(int a) => math_util.one2 * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec3(int a) => math_util.one3 * a;

        // Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatInt(float a) => (int)a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatClampedFloat(float a) => a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec2(float a) => math_util.one2 * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec3(float a) => math_util.one3 * a;

        // Clamped Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatInt(float a) => Mathf.Approximately(a, 1.0f) ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatFloat(float a) => a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec2(float a) => math_util.one2 * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec3(float a) => math_util.one3 * a;

        // float2
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Bool(float2 a) => a.x != 0.0f && a.y != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Int(float2 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Float(float2 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2ClampedFloat(float2 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Vec3(float2 a) => a;

        // float3
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Bool(float3 a) => a.x != 0.0f && a.y != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Int(float3 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Float(float3 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3ClampedFloat(float3 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Vec2(float3 a) => a;


        private static readonly Dictionary<Type, AttributeType> typeConversionDictionary = new Dictionary<Type, AttributeType> {
            { typeof(bool), AttributeType.Boolean },
            { typeof(int), AttributeType.Integer },
            { typeof(float), AttributeType.Float },
            { typeof(float2), AttributeType.Vector2 },
            { typeof(float3), AttributeType.Vector3 },
        };
    }
}
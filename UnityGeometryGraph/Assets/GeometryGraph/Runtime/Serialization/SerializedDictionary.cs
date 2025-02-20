﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Serialization {
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField] private List<TKey> keys = new();
        [SerializeField] private List<TValue> values = new();

        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> item in this)
            {
                keys.Add(item.Key);
                values.Add(item.Value);
            }
        }

        public void OnAfterDeserialize() {
            Clear();
            for (int i = 0; i < keys.Count && i < values.Count; i++) {
                this[keys[i]] = values[i];
            }
        }
    }
}
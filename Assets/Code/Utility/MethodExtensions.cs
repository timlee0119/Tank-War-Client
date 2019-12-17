﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utility {
    public static class MethodExtensions {
        public static string RemoveQuotes(this string Value) {
            return Value.Replace("\"", "");
        }
        public static string RemoveQuotes(this JSONObject Value) {
            return Value.ToString().Replace("\"", "");
        }

        public static int i(this JSONObject Value) {
            return (int)Value.f;
        }

        public static float TwoDecimals(this float Value) {
            return Mathf.Round(Value * 1000.0f) / 1000.0f;
        }
    }
}
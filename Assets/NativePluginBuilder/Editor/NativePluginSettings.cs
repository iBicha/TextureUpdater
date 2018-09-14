﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace iBicha
{
    public class NativePluginSettings
    {
        public static List<NativePlugin> plugins = new List<NativePlugin>();

        public static void Load()
        {
            plugins.Clear();
            plugins.AddRange(FindAssetsByType<NativePlugin>());
        }

        public static void Save()
        {
            foreach (NativePlugin plugin in plugins)
            {
                if (plugin != null && EditorUtility.IsPersistent(plugin))
                {
                    EditorUtility.SetDirty(plugin);
                }
            }
            AssetDatabase.SaveAssets();
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }


    }



}

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;

namespace Utils
{
    public static class AssetDataUtils
    {
#if UNITY_EDITOR
        /// <summary>
        /// Find asset guid by file path in EXISTING ASSETS.
        /// !!! EDITOR ONLY !!!
        /// </summary>
        /// <param name="path">Resource file path</param>
        /// <returns>GUID string. Empty string if not found.</returns>
        public static string GetAssetGuidByFilePath_Editor(string path)
        {
            return AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets);
        }

        /// <summary>
        /// Gets the asset file path by asset's GUID.
        /// </summary>
        /// <param name="guid">GUID of asset</param>
        /// <returns>Asset file path. Empty string if not found</returns>
        public static string GetAssetFilePathByGuid_Editor(string guid)
        {
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        /// <summary>
        /// This function is specifically for non-addressables.
        /// </summary>
        /// <typeparam name="T">Object type to check for, e.g Image</typeparam>
        /// <param name="guid">GUID of asset</param>
        /// <returns>Object</returns>
        public static Object LoadAssetByGuid_Editor<T>(string guid)
        {
            return AssetDatabase.LoadAssetAtPath(GetAssetFilePathByGuid_Editor(guid), typeof(T));
        }

        /// <summary>
        /// Gets asset path given a object
        /// </summary>
        /// <param name="uObject">Unity object</param>
        /// <returns>string containing asset path, or empty if not found</returns>
        public static string GetAssetPath_Editor(Object uObject)
        {
            return AssetDatabase.GetAssetPath(uObject);
        }

        /// <summary>
        /// Gets asset guid given a object.
        /// </summary>
        /// <param name="uObject">Unity object</param>
        /// <returns>GUID string if found, empty if not</returns>
        public static string GetAssetGuid_Editor(Object uObject)
        {
            string path = GetAssetPath_Editor(uObject);

            if (path.Length > 0)
            {
                return GetAssetGuidByFilePath_Editor(path);
            }

            return "";
        }

        /// <summary>
        /// Returns a new reference to an AssetReference by taking in a file path.
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>AssetReference addressable object of given file path, null if none found</returns>
        public static AssetReference GetAssetReferenceByFilePath_Editor(string path)
        {
            string guid = GetAssetGuidByFilePath_Editor(path);

            if (guid != null && guid.Length > 0)
            {
                return new AssetReference(guid);
            }

            return null;
        }

        public static bool IsObjectAddressable_Editor(Object obj)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
            return entry != null;
        }

        public static bool IsAssetAddressable_Editor(string guid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (!settings)
            {
                AddressableAssetEntry entry = settings.FindAssetEntry(guid);

                if (entry != null)
                {
                    return entry != null;
                }
            }

            return false;
        }
#endif

        /// <summary>
        /// Checks if object is given type T
        /// </summary>
        /// <typeparam name="T">Type to check for/expected</typeparam>
        /// <param name="objToCheck">Object to check</param>
        /// <returns>TRUE if object is type T, FALSE if not or object is null</returns>
        public static bool IsObjectType<T>(Object objToCheck)
        {
            if (objToCheck == null)
            {
                return false;
            }
            else if (objToCheck is T)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Searches if given resource i.e file path exists.
        /// This only works if addressables have been initialized!
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="key">File path, key to search for</param>
        /// <returns>TRUE if successfully found, FALSE if not</returns>
        public static bool AddressableResourceExists<T>(string key)
        {
            foreach (var l in Addressables.ResourceLocators)
            {
                Debug.Log(l.ToString());

                IList<IResourceLocation> locs;
                if (l.Locate(key, typeof(T), out locs))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Loads resource at file path if it exists
        /// </summary>
        /// <typeparam name="T">Expected type of resource</typeparam>
        /// <param name="filepath">File path</param>
        /// <returns>Loaded resource in generic type. Cast as needed.</returns>
        public static object LoadResource<T>(string filepath)
        {
            return Resources.Load(filepath, typeof(T));
        }
    }
}
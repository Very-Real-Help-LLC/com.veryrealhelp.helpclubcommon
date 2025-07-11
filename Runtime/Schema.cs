﻿using System;
using System.Linq;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Schema
{
    [Serializable]
    public class BundleSetDefinition
    {
        public string uriRoot;
        public string uriRoot2021;
        public string devUriRoot;
        public string androidPath;
        public string osxPath;
        public string windowsPath;
        public string iosPath;
        public string[] bundlePaths;

        public string Path
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        return osxPath;
                    case RuntimePlatform.Android:
                        return androidPath;
                    case RuntimePlatform.IPhonePlayer:
                        return iosPath;
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                    default:
                        return windowsPath;
                }
            }
        }

        protected string Root => string.IsNullOrWhiteSpace(uriRoot2021) ? uriRoot : uriRoot2021;  // apply new or fallback to old
        public string Uri => $"{Root}/{Path}";
        public string DevUri => string.IsNullOrWhiteSpace(devUriRoot) ? Uri : $"{devUriRoot}/{Path}";
        public string AssetPath => $"{Application.streamingAssetsPath}/{Path}";
        public string ManifestPath => $"{Path}/{Path.Split('/').Last()}";
        public string ManifestAssetPath => $"{Application.streamingAssetsPath}/{ManifestPath}";
        public string ManifestUri => $"{Root}/{ManifestPath}";
        public string DevManifestUri => string.IsNullOrWhiteSpace(devUriRoot) ? ManifestUri : $"{devUriRoot}/{ManifestPath}";
    }

    [Serializable]
    public class BundleSetDefinitionCollection
    {
        public BundleSetDefinition[] bundles;
    }

    [Serializable]
    public class WorldDefinition
    {
        public string id;
        public string name;
        public string version;
        public string infoBundle;
        public string infoAsset;
        public BundleSetDefinition bundles;
        public int defaultUserLimit;
        public string thumbnailPath;
    }

    [Serializable]
    public class WorldDefinitionCollection
    {
        public WorldDefinition[] worlds;
    }

    [Serializable]
    public class ProjectManifest
    {
        public bool isAWorldProject;
        public WorldDefinition world;
        public BundleSetDefinition bundles;
    }
}

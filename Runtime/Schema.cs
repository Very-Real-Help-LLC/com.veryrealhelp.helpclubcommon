using System;
using System.Linq;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Schema
{
    [Serializable]
    public class BundleSetDefinition
    {
        public string uriRoot;
        public string androidPath;
        public string osxPath;
        public string windowsPath;
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
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                    default:
                        return windowsPath;
                }
            }
        }

        public string Uri => $"{uriRoot}/{Path}";
        public string AssetPath => $"{Application.streamingAssetsPath}/{Path}";
        public string ManifestPath => $"{Path}/{Path.Split('/').Last()}";
        public string ManifestAssetPath => $"{Application.streamingAssetsPath}/{ManifestPath}";
        public string ManifestUri => $"{uriRoot}/{ManifestPath}";
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

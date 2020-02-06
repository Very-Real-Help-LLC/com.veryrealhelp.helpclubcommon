These are common pieces of code for use in the Help Club app by Very Real Help.

# Usage

Add a package dependency on this github repository to the `<Project>/Packages/manifest.json` file.

You can do this by adding the following line to the `manifest.json` file in the `dependencies` section:

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.5"```


### example manifest.json
_Yours may vary_
```
{
  "dependencies": {
    "com.unity.assetbundlebrowser": "1.7.0",
    "com.unity.collab-proxy": "1.2.15",
    "com.unity.package-manager-ui": "2.0.8",
    "com.unity.purchasing": "2.0.3",
    "com.unity.textmeshpro": "1.4.1",
    "com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.5"
  }
}
```

# Updating your package dependency to a new version

After opening the project in Unity, the manifest file will be updated to pin the specific version of the dependency.

To update to a newer version you must remove the lock in the manifest file then update the version number to the desired one.

## example going from 0.0.4 to 0.0.5

### remove lock

```
  "lock": {
    "com.veryrealhelp.helpclubcommon": {
      "hash": "b83e5a25f76618be22407f8130f40d449609425a",
      "revision": "0.0.4"
    }
  }
```

becomes 
```
  "lock": {}
```

### update version number

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.4"```

becomes

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.5"```

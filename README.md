These are common pieces of code for use in the Help Club app by Very Real Help.

# Usage

Add a package dependency on this github repository to the `<Project>/Packages/manifest.json` file.

You can do this by adding the following line to the `manifest.json` file in the `dependencies` section:

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.5"```

_See the [releases page](https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon/releases) to find the most current version number._

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

To update to a newer version, simply update the manifest file to the desired version number.

## example going from 0.0.4 to 0.0.5

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.4"```

becomes

```"com.veryrealhelp.helpclubcommon": "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git#0.0.5"```

# Contributing

## Releasing a New Version

To release a new version, involves two steps:

1. First modify the `package.json` file's `version` property to reflect the new version number.
2. Create a release (tag) in github with the name of the version _(i.e. the release for version 0.0.7 would be named `0.0.7`)

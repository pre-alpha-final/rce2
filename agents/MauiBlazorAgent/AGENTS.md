# MauiBlazorAgent

## Publishing

**To publish this app, run the publish script — do not invent your own `dotnet publish` command:**

```powershell
.\publish.ps1
```

(from the `agents/MauiBlazorAgent/` directory; pass `-Configuration Debug` to override the default `Release`.)

The script lives at [publish.ps1](publish.ps1). It wraps the required flags for an
unpackaged, self-contained Windows build (`WindowsPackageType=None`,
`RuntimeIdentifierOverride=win10-x64`, `WindowsAppSDKSelfContained=true`). These flags
are load-bearing — a plain `dotnet publish` will not produce a working unpackaged app.

Output lands in `MauiBlazorAgent\bin\<Configuration>\net10.0-windows10.0.19041.0\win-x64\publish\`.

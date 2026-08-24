param(
    [string]$RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} else {
    (Resolve-Path $RepoRoot).Path
}

$configPath = Join-Path $resolvedRepoRoot "config\fa_radar2.version.json"
if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) {
    throw "Missing Radar 2 version config: $configPath"
}
$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$sourcePath = Join-Path $resolvedRepoRoot ([string]$config.sourceRelativePath).Replace('/', '\')
$presetPath = Join-Path $resolvedRepoRoot ([string]$config.presetRelativePath).Replace('/', '\')
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Missing Radar 2 source: $sourcePath"
}
if (-not (Test-Path -LiteralPath $presetPath -PathType Leaf)) {
    throw "Missing Radar 2 preset: $presetPath"
}

$source = Get-Content -LiteralPath $sourcePath -Raw
$preset = Get-Content -LiteralPath $presetPath -Raw

function Assert-Contains {
    param([string]$Text, [string]$Needle, [string]$Label)
    if ($Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -lt 0) {
        throw "Radar 2 contract missing $Label ('$Needle')."
    }
}

function Assert-Absent {
    param([string]$Text, [string]$Needle, [string]$Label)
    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Radar 2 contract prohibits $Label ('$Needle')."
    }
}

if ([string]$config.version -ne "2.0.0") {
    throw "Radar 2 config version must be 2.0.0."
}
if ([string]$config.publicType -ne "FrameAngelRadar2") {
    throw "Radar 2 public type must be FrameAngelRadar2."
}
if ([string]$config.pluginFileName -ne "fa_radar.2.0.0.dll") {
    throw "Radar 2 plugin filename must be unique and versioned."
}

Assert-Contains $source 'public class FrameAngelRadar2 : MVRScript' 'compiled MVRScript load shape'
Assert-Contains $source 'private const string Version = "2.0.0";' 'source version'
@('Scene', 'Room', 'Left Controller', 'Right Controller', 'Left Wrist', 'Right Wrist') | ForEach-Object {
    Assert-Contains $source ('"' + $_ + '"') ("mode " + $_)
}
Assert-Contains $source 'new JSONStorableBool("Radar Enabled", true)' 'minimal enabled control'
Assert-Contains $source '"Mode",' 'minimal mode control'
Assert-Contains $source 'CreateToggle(radarEnabledField' 'enabled UI'
Assert-Contains $source 'CreatePopup(modeField' 'mode UI'
Assert-Contains $source 'CreateTextField(statusField' 'read-only status UI'

@(
    'Scene Position X', 'Scene Position Y', 'Scene Position Z',
    'Scene Rotation X', 'Scene Rotation Y', 'Scene Rotation Z', 'Scene Rotation W',
    'Scene Scale'
) | ForEach-Object {
    Assert-Contains $source ('"' + $_ + '"') ("hidden scene storable " + $_)
}

Assert-Contains $source 'FAARTrackedHandArmColliders' 'accepted tracked-hand root'
Assert-Contains $source 'faar.tracked-hand-state.v7' 'accepted tracked-hand schema'
Assert-Contains $source 'Segment_0' 'left accepted palm segment'
Assert-Contains $source 'Segment_27' 'right accepted palm segment'
Assert-Contains $source 'MaintainTrackedHandConnection' 'bounded reconnect topology'
Assert-Contains $source 'nextPalmAcquireAt' 'bounded reconnect state'
Assert-Contains $source 'RegisterHandStateReceiver' 'receiver registration'
Assert-Contains $source 'UnregisterHandStateReceiver' 'receiver teardown'
Assert-Contains $source 'ResolveLivePalm' 'wrist tracked-palm consumer'

Assert-Contains $source 'GrabState.SceneSingle' 'single-hand Scene state'
Assert-Contains $source 'GrabState.SceneDual' 'dual-hand Scene state'
Assert-Contains $source 'StartSceneDualGrab' 'dual-grab transition'
Assert-Contains $source 'CaptureScenePose' 'wrist-to-Scene capture transition'
Assert-Contains $source 'modeField.valNoCallback = ModeScene' 'atomic wrist-to-Scene mode handoff'
Assert-Contains $source 'ApplyControllerMode' 'controller-only attachment path'
Assert-Contains $source 'ApplyWristMode' 'wrist-only attachment path'
Assert-Contains $source 'ApplyRootPose' 'Room/Scene pose state topology'

@(
    'System.IO',
    'System.Reflection',
    'MVR.FileManagementSecure',
    'FileManagerSecure',
    'Valve.VR',
    'SteamVR',
    'BindingFlags',
    'MethodInfo',
    'PropertyInfo',
    'FieldInfo',
    'Activator.CreateInstance'
) | ForEach-Object {
    Assert-Absent $source $_ $_
}

@(
    'Desktop Placement',
    'Throw Pin',
    'Throw Surface',
    'HUD Offset',
    'Wrist Offset',
    'Global Prefs',
    'Debug',
    'Radar Range Meters',
    'Sphere Alpha',
    'Ring Alpha',
    'Grid Alpha',
    'Emission Strength',
    'Scene Labels',
    'Grab Haptics'
) | ForEach-Object {
    Assert-Absent $source $_ ("legacy UI/control " + $_)
}

Assert-Contains $preset 'Custom/Plugins/fa_radar.2.0.0.dll' 'preset DLL path'
Assert-Contains $preset 'plugin#0_FrameAngelRadar2' 'preset public type storable'
Assert-Contains $preset '"Mode" : "Scene"' 'preset default Scene mode'

Write-Host "FA Radar 2 contract verification passed."

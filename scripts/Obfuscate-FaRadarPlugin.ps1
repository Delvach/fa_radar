param(
    [string]$RepoRoot = "",
    [string]$PluginKey = "fa_radar",
    [Parameter(Mandatory = $true)]
    [string]$InputAssemblyPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputAssemblyPath,
    [string]$ConfigPath = "config\obfuscation.defaults.json",
    [string[]]$ReferenceSearchPath = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FaRadarPath {
    param(
        [string]$PathValue,
        [string]$BasePath,
        [string]$Label,
        [bool]$MustExist = $true
    )

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        throw "$Label cannot be empty."
    }

    $candidate = if ([System.IO.Path]::IsPathRooted($PathValue)) {
        $PathValue
    } else {
        Join-Path $BasePath $PathValue
    }

    if ($MustExist) {
        if (-not (Test-Path -LiteralPath $candidate)) {
            throw "$Label not found: $candidate"
        }
        return (Resolve-Path -LiteralPath $candidate).Path
    }

    return [System.IO.Path]::GetFullPath($candidate)
}

function Get-FaRadarJsonProperty {
    param(
        [object]$ObjectValue,
        [string]$Name,
        [object]$DefaultValue = $null
    )

    if ($null -eq $ObjectValue) {
        return $DefaultValue
    }

    $property = $ObjectValue.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $DefaultValue
    }

    return $property.Value
}

function Convert-FaRadarBool {
    param(
        [object]$Value,
        [bool]$DefaultValue
    )

    if ($null -eq $Value) {
        return $DefaultValue
    }
    if ($Value -is [bool]) {
        return [bool]$Value
    }

    $text = ([string]$Value).Trim()
    if ([string]::Equals($text, "true", [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($text, "1", [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($text, "yes", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }
    if ([string]::Equals($text, "false", [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($text, "0", [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($text, "no", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }

    return [System.Convert]::ToBoolean($Value)
}

function Convert-FaRadarStringArray {
    param([object]$Value)

    if ($null -eq $Value) {
        return @()
    }

    $result = New-Object System.Collections.Generic.List[string]
    foreach ($item in @($Value)) {
        $text = ([string]$item).Trim()
        if (-not [string]::IsNullOrWhiteSpace($text)) {
            $result.Add($text)
        }
    }

    return $result.ToArray()
}

function Convert-FaRadarSkipMethodArray {
    param([object]$Value)

    $result = New-Object System.Collections.ArrayList
    if ($null -eq $Value) {
        return @($result)
    }

    foreach ($item in @($Value)) {
        $typeName = ([string](Get-FaRadarJsonProperty -ObjectValue $item -Name "type" -DefaultValue "")).Trim()
        $methodName = ([string](Get-FaRadarJsonProperty -ObjectValue $item -Name "name" -DefaultValue "")).Trim()
        if ([string]::IsNullOrWhiteSpace($typeName) -or [string]::IsNullOrWhiteSpace($methodName)) {
            continue
        }

        [void]$result.Add([pscustomobject]@{
            type = $typeName
            name = $methodName
        })
    }

    return @($result)
}

function Add-FaRadarUniqueString {
    param(
        [System.Collections.Generic.List[string]]$List,
        [System.Collections.Generic.HashSet[string]]$Seen,
        [string[]]$Values
    )

    foreach ($value in @($Values)) {
        if ([string]::IsNullOrWhiteSpace($value)) {
            continue
        }
        if ($Seen.Add($value)) {
            $List.Add($value)
        }
    }
}

function Add-FaRadarUniqueSkipMethod {
    param(
        [System.Collections.IList]$List,
        [System.Collections.Generic.HashSet[string]]$Seen,
        [object[]]$Values
    )

    foreach ($value in @($Values)) {
        if ($null -eq $value) {
            continue
        }

        $typeName = ([string](Get-FaRadarJsonProperty -ObjectValue $value -Name "type" -DefaultValue "")).Trim()
        $methodName = ([string](Get-FaRadarJsonProperty -ObjectValue $value -Name "name" -DefaultValue "")).Trim()
        if ([string]::IsNullOrWhiteSpace($typeName) -or [string]::IsNullOrWhiteSpace($methodName)) {
            continue
        }

        $key = "{0}::{1}" -f $typeName, $methodName
        if ($Seen.Add($key)) {
            [void]$List.Add([pscustomobject]@{
                type = $typeName
                name = $methodName
            })
        }
    }
}

function Escape-FaRadarXml {
    param([string]$Value)

    if ($null -eq $Value) {
        return ""
    }

    return [System.Security.SecurityElement]::Escape($Value)
}

function Ensure-FaRadarDirectory {
    param([string]$PathValue)

    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function Ensure-FaRadarObfuscarTool {
    param(
        [string]$ToolPath,
        [string]$PackageName,
        [string]$PackageVersion
    )

    Ensure-FaRadarDirectory -PathValue $ToolPath
    $toolExe = Join-Path $ToolPath "obfuscar.console.exe"
    if (Test-Path -LiteralPath $toolExe -PathType Leaf) {
        return $toolExe
    }

    $installArgs = @("tool", "install", "--tool-path", $ToolPath, $PackageName)
    if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
        $installArgs += "--version"
        $installArgs += $PackageVersion
    }

    Write-Host "Installing obfuscation tool '$PackageName' ($PackageVersion) to $ToolPath"
    $installOutput = & dotnet @installArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install obfuscation tool '$PackageName' with exit code $LASTEXITCODE."
    }
    foreach ($line in @($installOutput)) {
        if (-not [string]::IsNullOrWhiteSpace([string]$line)) {
            Write-Host $line
        }
    }

    if (-not (Test-Path -LiteralPath $toolExe -PathType Leaf)) {
        throw "Obfuscation tool executable not found after install: $toolExe"
    }

    return $toolExe
}

$repoRootResolved = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} else {
    Resolve-FaRadarPath -PathValue $RepoRoot -BasePath (Get-Location).Path -Label "Repo root"
}

$inputAssemblyResolved = Resolve-FaRadarPath -PathValue $InputAssemblyPath -BasePath $repoRootResolved -Label "Input assembly"
$outputAssemblyResolved = Resolve-FaRadarPath -PathValue $OutputAssemblyPath -BasePath $repoRootResolved -Label "Output assembly" -MustExist $false
$configResolved = Resolve-FaRadarPath -PathValue $ConfigPath -BasePath $repoRootResolved -Label "Obfuscation config"

$config = Get-Content -LiteralPath $configResolved -Raw | ConvertFrom-Json
$toolConfig = Get-FaRadarJsonProperty -ObjectValue $config -Name "tool"
$defaultsConfig = Get-FaRadarJsonProperty -ObjectValue $config -Name "defaults"
$profilesConfig = Get-FaRadarJsonProperty -ObjectValue $config -Name "profiles"
$pluginsConfig = Get-FaRadarJsonProperty -ObjectValue $config -Name "plugins"
$pluginConfig = Get-FaRadarJsonProperty -ObjectValue $pluginsConfig -Name $PluginKey

$packageName = [string](Get-FaRadarJsonProperty -ObjectValue $toolConfig -Name "package" -DefaultValue "Obfuscar.GlobalTool")
$packageVersion = [string](Get-FaRadarJsonProperty -ObjectValue $toolConfig -Name "version" -DefaultValue "")
$toolPathValue = [string](Get-FaRadarJsonProperty -ObjectValue $toolConfig -Name "toolPath" -DefaultValue "tools\obfuscar")
$toolPathResolved = Resolve-FaRadarPath -PathValue $toolPathValue -BasePath $repoRootResolved -Label "Obfuscation tool path" -MustExist $false

$enabled = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $defaultsConfig -Name "enabled" -DefaultValue $true) -DefaultValue $true
$enabled = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $pluginConfig -Name "enabled" -DefaultValue $enabled) -DefaultValue $enabled
$profileName = [string](Get-FaRadarJsonProperty -ObjectValue $defaultsConfig -Name "profile" -DefaultValue "vam_compat")
$profileName = [string](Get-FaRadarJsonProperty -ObjectValue $pluginConfig -Name "profile" -DefaultValue $profileName)
if ([string]::IsNullOrWhiteSpace($profileName)) {
    $profileName = "vam_compat"
}

$profileConfig = Get-FaRadarJsonProperty -ObjectValue $profilesConfig -Name $profileName
if ($null -eq $profileConfig) {
    throw "Obfuscation profile '$profileName' is not defined in $configResolved."
}

$keepTypes = New-Object System.Collections.Generic.List[string]
$seenKeepTypes = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
Add-FaRadarUniqueString -List $keepTypes -Seen $seenKeepTypes -Values (Convert-FaRadarStringArray -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "keepTypes"))
Add-FaRadarUniqueString -List $keepTypes -Seen $seenKeepTypes -Values (Convert-FaRadarStringArray -Value (Get-FaRadarJsonProperty -ObjectValue $pluginConfig -Name "keepTypes"))

$skipMethods = New-Object System.Collections.ArrayList
$seenSkipMethods = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
Add-FaRadarUniqueSkipMethod -List $skipMethods -Seen $seenSkipMethods -Values (Convert-FaRadarSkipMethodArray -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "skipMethods"))
Add-FaRadarUniqueSkipMethod -List $skipMethods -Seen $seenSkipMethods -Values (Convert-FaRadarSkipMethodArray -Value (Get-FaRadarJsonProperty -ObjectValue $pluginConfig -Name "skipMethods"))

$outputDirectory = Split-Path -Parent $outputAssemblyResolved
Ensure-FaRadarDirectory -PathValue $outputDirectory
$reportPath = $outputAssemblyResolved + ".obf-report.json"

if (-not $enabled) {
    Copy-Item -LiteralPath $inputAssemblyResolved -Destination $outputAssemblyResolved -Force
    [pscustomobject]@{
        generatedAtUtc = [DateTime]::UtcNow.ToString("o")
        enabled = $false
        pluginKey = $PluginKey
        profile = $profileName
        inputAssemblyPath = $inputAssemblyResolved
        outputAssemblyPath = $outputAssemblyResolved
        outputDiffersFromInput = $false
        keepTypes = @($keepTypes)
        skipMethods = @($skipMethods)
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8
    Write-Host "Obfuscation disabled; copied $outputAssemblyResolved"
    return
}

$toolExe = Ensure-FaRadarObfuscarTool -ToolPath $toolPathResolved -PackageName $packageName -PackageVersion $packageVersion
$dotnetHost = (Get-Command dotnet -ErrorAction Stop).Source
$dotnetRootForTool = Split-Path -Parent $dotnetHost
$workRoot = Join-Path $env:TEMP ("fa-radar-obfuscator-" + [System.Guid]::NewGuid().ToString("N"))
$workInDir = Join-Path $workRoot "in"
$workOutDir = Join-Path $workRoot "out"
Ensure-FaRadarDirectory -PathValue $workInDir
Ensure-FaRadarDirectory -PathValue $workOutDir

$inputFileName = [System.IO.Path]::GetFileName($inputAssemblyResolved)
$stagedInputAssembly = Join-Path $workInDir $inputFileName
Copy-Item -LiteralPath $inputAssemblyResolved -Destination $stagedInputAssembly -Force

$referenceSeen = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
foreach ($rawReferencePath in @($ReferenceSearchPath)) {
    if ([string]::IsNullOrWhiteSpace($rawReferencePath)) {
        continue
    }

    $candidate = Resolve-FaRadarPath -PathValue $rawReferencePath -BasePath $repoRootResolved -Label "Reference path" -MustExist $false
    if (-not (Test-Path -LiteralPath $candidate)) {
        continue
    }

    $item = Get-Item -LiteralPath $candidate
    $referenceFiles = if ($item.PSIsContainer) {
        @(Get-ChildItem -LiteralPath $candidate -File -Filter *.dll -ErrorAction SilentlyContinue)
    } else {
        @($item)
    }

    foreach ($referenceFile in $referenceFiles) {
        if (-not [string]::Equals($referenceFile.Extension, ".dll", [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }
        if (-not $referenceSeen.Add($referenceFile.FullName)) {
            continue
        }
        if ([string]::Equals($referenceFile.Name, $inputFileName, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        Copy-Item -LiteralPath $referenceFile.FullName -Destination (Join-Path $workInDir $referenceFile.Name) -Force
    }
}

$keepPublicApi = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "keepPublicApi" -DefaultValue $true) -DefaultValue $true
$hidePrivateApi = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "hidePrivateApi" -DefaultValue $false) -DefaultValue $false
$renameFields = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "renameFields" -DefaultValue $false) -DefaultValue $false
$renameProperties = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "renameProperties" -DefaultValue $false) -DefaultValue $false
$renameEvents = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "renameEvents" -DefaultValue $false) -DefaultValue $false
$hideStrings = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "hideStrings" -DefaultValue $false) -DefaultValue $false
$reuseNames = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "reuseNames" -DefaultValue $false) -DefaultValue $false
$useUnicodeNames = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "useUnicodeNames" -DefaultValue $false) -DefaultValue $false
$suppressIldasm = Convert-FaRadarBool -Value (Get-FaRadarJsonProperty -ObjectValue $profileConfig -Name "suppressIldasm" -DefaultValue $true) -DefaultValue $true

$obfuscarProjectPath = Join-Path $workRoot "obfuscar.xml"
$xml = New-Object System.Collections.Generic.List[string]
$xml.Add('<?xml version="1.0" encoding="utf-8"?>')
$xml.Add("<Obfuscator>")
$xml.Add('  <Var name="InPath" value="' + (Escape-FaRadarXml $workInDir) + '" />')
$xml.Add('  <Var name="OutPath" value="' + (Escape-FaRadarXml $workOutDir) + '" />')
$xml.Add('  <Var name="KeepPublicApi" value="' + $keepPublicApi.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="HidePrivateApi" value="' + $hidePrivateApi.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="RenameFields" value="' + $renameFields.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="RenameProperties" value="' + $renameProperties.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="RenameEvents" value="' + $renameEvents.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="HideStrings" value="' + $hideStrings.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="ReuseNames" value="' + $reuseNames.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="UseUnicodeNames" value="' + $useUnicodeNames.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Var name="SuppressIldasm" value="' + $suppressIldasm.ToString().ToLowerInvariant() + '" />')
$xml.Add('  <Module file="' + (Escape-FaRadarXml $stagedInputAssembly) + '">')
foreach ($keepType in @($keepTypes)) {
    $xml.Add('    <SkipType name="' + (Escape-FaRadarXml $keepType) + '" />')
}
foreach ($skipMethod in @($skipMethods)) {
    $xml.Add('    <SkipMethod type="' + (Escape-FaRadarXml ([string]$skipMethod.type)) + '" name="' + (Escape-FaRadarXml ([string]$skipMethod.name)) + '" />')
}
$xml.Add("  </Module>")
$xml.Add("</Obfuscator>")
Set-Content -LiteralPath $obfuscarProjectPath -Value $xml.ToArray() -Encoding ASCII

$previousDotnetRoot = $env:DOTNET_ROOT
try {
    $env:DOTNET_ROOT = $dotnetRootForTool
    & $toolExe --verbosity:m $obfuscarProjectPath
    if ($LASTEXITCODE -ne 0) {
        throw "Obfuscar failed for '$PluginKey' with exit code $LASTEXITCODE."
    }
}
finally {
    $env:DOTNET_ROOT = $previousDotnetRoot
}

$obfuscatedOutput = Join-Path $workOutDir $inputFileName
if (-not (Test-Path -LiteralPath $obfuscatedOutput -PathType Leaf)) {
    $outDlls = @(Get-ChildItem -LiteralPath $workOutDir -File -Filter *.dll -ErrorAction SilentlyContinue)
    if ($outDlls.Count -le 0) {
        throw "Obfuscation completed but no DLL was produced in $workOutDir."
    }
    $obfuscatedOutput = $outDlls[0].FullName
}

Copy-Item -LiteralPath $obfuscatedOutput -Destination $outputAssemblyResolved -Force
$inputHash = (Get-FileHash -LiteralPath $inputAssemblyResolved -Algorithm SHA256).Hash
$outputHash = (Get-FileHash -LiteralPath $outputAssemblyResolved -Algorithm SHA256).Hash
[pscustomobject]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    enabled = $true
    pluginKey = $PluginKey
    profile = $profileName
    toolPackage = $packageName
    toolVersion = $packageVersion
    inputAssemblyPath = $inputAssemblyResolved
    outputAssemblyPath = $outputAssemblyResolved
    inputSha256 = $inputHash
    outputSha256 = $outputHash
    outputDiffersFromInput = -not [string]::Equals($inputHash, $outputHash, [System.StringComparison]::OrdinalIgnoreCase)
    keepTypes = @($keepTypes)
    skipMethods = @($skipMethods)
    keepPublicApi = $keepPublicApi
    hidePrivateApi = $hidePrivateApi
    renameFields = $renameFields
    renameProperties = $renameProperties
    renameEvents = $renameEvents
    hideStrings = $hideStrings
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

try {
    Remove-Item -LiteralPath $workRoot -Recurse -Force
}
catch {
    Write-Host "Obfuscation temp cleanup warning: $workRoot"
}

Write-Host "Obfuscated assembly written to $outputAssemblyResolved"

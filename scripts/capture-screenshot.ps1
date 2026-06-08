# Captures WeatherApp main window screenshot for README/docs.
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\docs\screenshots\main-window.png")
)

$ErrorActionPreference = "Stop"
$exe = Join-Path $PSScriptRoot "..\WeatherApp\bin\Release\net8.0-windows\WeatherApp.exe"
if (-not (Test-Path $exe)) {
    throw "Build the app first: dotnet build WeatherApp.sln -c Release"
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;

public struct WinRect {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public static class Win32 {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out WinRect rect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@

$outDir = Split-Path $OutputPath -Parent
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$proc = Start-Process -FilePath $exe -PassThru
try {
    $handle = [IntPtr]::Zero
    for ($i = 0; $i -lt 40; $i++) {
        $proc.Refresh()
        if ($proc.MainWindowHandle -ne [IntPtr]::Zero) {
            $handle = $proc.MainWindowHandle
            break
        }
        Start-Sleep -Milliseconds 500
    }

    if ($handle -eq [IntPtr]::Zero) {
        throw "WeatherApp window did not appear."
    }

    [void][Win32]::SetForegroundWindow($handle)
    Start-Sleep -Seconds 3

    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        "Get IMGW Data"
    )
    $imgwButton = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    if ($null -ne $imgwButton) {
        $invokePattern = $imgwButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($invokePattern) {
            $invokePattern.Invoke()
            Start-Sleep -Seconds 4
        }
    }

    $rect = New-Object WinRect
    [void][Win32]::GetWindowRect($handle, [ref]$rect)
    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    $bitmap = New-Object System.Drawing.Bitmap $width, $height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
    Write-Host "Saved screenshot to $OutputPath"
}
finally {
    if (-not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}

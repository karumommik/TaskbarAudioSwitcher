Add-Type -AssemblyName System.Drawing

$assetsDir = "C:\Users\Karu\TaskbarAudioSwitcher\Repo\Assets"
$icoPath = Join-Path $assetsDir "app.ico"

# Try to load the icon. Since app.ico could contain multiple sizes, we extract a high-res bitmap
$icon = [System.Drawing.Icon]::ExtractAssociatedIcon($icoPath)
$bitmap = $icon.ToBitmap()

# But ExtractAssociatedIcon might only get 32x32. Let's load it as a new Icon and get a larger size if possible.
# Actually, just loading new System.Drawing.Icon($icoPath, 256, 256) works to get the largest.
try {
    $iconLg = New-Object System.Drawing.Icon($icoPath, 256, 256)
    $bitmap = $iconLg.ToBitmap()
} catch {
    Write-Host "Could not load 256x256 icon, falling back to default"
}

function Create-Image {
    param([string]$name, [int]$w, [int]$h)
    $outPath = Join-Path $assetsDir $name
    $img = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($img)
    $g.Clear([System.Drawing.Color]::Transparent)
    
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    
    # Calculate aspect ratio preserving bounds for centering
    $iconSize = [math]::Min($w, $h) - 10
    if ($iconSize -lt 10) { $iconSize = [math]::Min($w, $h) }
    
    $x = ($w - $iconSize) / 2
    $y = ($h - $iconSize) / 2
    
    $rect = New-Object System.Drawing.Rectangle($x, $y, $iconSize, $iconSize)
    $g.DrawImage($bitmap, $rect)
    
    $g.Dispose()
    $img.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $img.Dispose()
    Write-Host "Created $name ($w x $h)"
}

Create-Image "Square44x44Logo.png" 44 44
Create-Image "StoreLogo.png" 50 50
Create-Image "Square150x150Logo.png" 150 150
Create-Image "Wide310x150Logo.png" 310 150
Create-Image "SplashScreen.png" 620 300

$bitmap.Dispose()
if ($iconLg) { $iconLg.Dispose() }
$icon.Dispose()

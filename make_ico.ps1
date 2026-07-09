Add-Type -AssemblyName System.Drawing

$srcFile = "C:\AI\Assets\audio_switcher_logo.jpg"
$destFile = "C:\Users\Karu\TaskbarAudioSwitcher\Repo\Assets\app.ico"

# Load original image
$orig = [System.Drawing.Image]::FromFile($srcFile)

# Resize to 256x256
$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($orig, 0, 0, 256, 256)
$g.Dispose()
$orig.Dispose()

# Save as PNG to memory stream
$ms = New-Object System.IO.MemoryStream
$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $ms.ToArray()
$ms.Dispose()
$bmp.Dispose()

# Create ICO file
$fs = New-Object System.IO.FileStream($destFile, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter($fs)

# ICO header
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Type=ICO
$bw.Write([uint16]1) # NumImages=1

# Image directory
$bw.Write([byte]0) # Width (0 = 256)
$bw.Write([byte]0) # Height (0 = 256)
$bw.Write([byte]0) # ColorCount
$bw.Write([byte]0) # Reserved
$bw.Write([uint16]1) # Planes
$bw.Write([uint16]32) # BitCount
$bw.Write([uint32]$pngBytes.Length) # Image size
$bw.Write([uint32]22) # Image offset

# Image data
$bw.Write($pngBytes)

$bw.Close()
$fs.Close()

Write-Host "Created app.ico successfully."

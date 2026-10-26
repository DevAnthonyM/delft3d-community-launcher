# Draws the launcher's original icon (blue tile with three white waves) and writes a multi-size .ico.
param([string]$Out = "$PSScriptRoot\app.ico")
Add-Type -AssemblyName System.Drawing

function New-WaveBitmap([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $r = [Math]::Max(2, [int]($s * 0.18))
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $r * 2, $r * 2, 180, 90); $path.AddArc($s - $r * 2 - 1, 0, $r * 2, $r * 2, 270, 90)
    $path.AddArc($s - $r * 2 - 1, $s - $r * 2 - 1, $r * 2, $r * 2, 0, 90); $path.AddArc(0, $s - $r * 2 - 1, $r * 2, $r * 2, 90, 90)
    $path.CloseFigure()
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.Point 0, 0), (New-Object System.Drawing.Point 0, $s), [System.Drawing.Color]::FromArgb(0, 140, 220), [System.Drawing.Color]::FromArgb(0, 70, 140))
    $g.FillPath($brush, $path)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([Math]::Max(1.5, $s * 0.075))
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    foreach ($row in 0.33, 0.53, 0.73) {
        $pts = for ($i = 0; $i -le 24; $i++) {
            $x = $s * (0.16 + 0.68 * $i / 24)
            $y = $s * $row + [Math]::Sin($i / 24 * 2 * [Math]::PI * 1.5) * $s * 0.06
            New-Object System.Drawing.PointF $x, $y
        }
        $g.DrawCurve($pen, [System.Drawing.PointF[]]$pts)
    }
    $g.Dispose()
    return $bmp
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$pngs = foreach ($s in $sizes) { $b = New-WaveBitmap $s; $ms = New-Object System.IO.MemoryStream; $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); $b.Dispose(); ,$ms.ToArray() }

$fs = [System.IO.File]::Create($Out); $w = New-Object System.IO.BinaryWriter $fs
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]; $d = if ($s -ge 256) { 0 } else { $s }
    $w.Write([byte]$d); $w.Write([byte]$d); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([UInt16]1); $w.Write([UInt16]32); $w.Write([UInt32]$pngs[$i].Length); $w.Write([UInt32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $w.Write($p) }
$w.Close()
"icon written: $Out"

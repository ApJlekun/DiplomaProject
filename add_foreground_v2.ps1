$files = Get-ChildItem -Path "BarInventoryApp\Pages" -Filter *.xaml
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    if ($content -match '<Page' -and $content -notmatch 'Foreground="{DynamicResource TextBrush}"') {
        $content = $content -replace 'Background="{DynamicResource BackgroundBrush}"', 'Background="{DynamicResource BackgroundBrush}" Foreground="{DynamicResource TextBrush}"'
    }
    
    if ($content -match '<Window' -and $content -notmatch 'Foreground="{DynamicResource TextBrush}"' -and $content -notmatch 'Style="{StaticResource ModernWindow}"') {
        $content = $content -replace 'Background="{DynamicResource BackgroundBrush}"', 'Background="{DynamicResource BackgroundBrush}" Foreground="{DynamicResource TextBrush}"'
    }

    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
}
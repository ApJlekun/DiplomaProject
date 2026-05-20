$files = Get-ChildItem -Path "BarInventoryApp\Pages" -Filter *.xaml
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # Add Foreground to Page tag if not present
    if ($content -match '<Page' -and $content -notmatch 'Foreground="{DynamicResource TextBrush}"') {
        $content = $content -replace '(<Page[^>]*Background="{DynamicResource BackgroundBrush}")', '$1 Foreground="{DynamicResource TextBrush}"'
    }
    
    # Add Foreground to Window tag if not present
    if ($content -match '<Window' -and $content -notmatch 'Foreground="{DynamicResource TextBrush}"' -and $content -notmatch 'Style="{StaticResource ModernWindow}"') {
        $content = $content -replace '(<Window[^>]*Background="{DynamicResource BackgroundBrush}")', '$1 Foreground="{DynamicResource TextBrush}"'
    }

    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
}
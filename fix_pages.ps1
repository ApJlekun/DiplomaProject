$files = Get-ChildItem -Path "BarInventoryApp\Pages" -Filter *.xaml
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '"\{StaticResource ([a-zA-Z0-9]+Brush)\}"', '"{DynamicResource $1}"')
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '"\{StaticResource ([a-zA-Z0-9]+Color)\}"', '"{DynamicResource $1}"')
    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
}
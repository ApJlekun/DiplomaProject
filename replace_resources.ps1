$files = Get-ChildItem -Path "BarInventoryApp" -Recurse -Filter *.xaml
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    # Replace "{StaticResource SomeBrush}" with "{DynamicResource SomeBrush}"
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '"\{StaticResource ([a-zA-Z0-9]+Brush)\}"', '"{DynamicResource $1}"')
    # Replace "{StaticResource SomeColor}" with "{DynamicResource SomeColor}"
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '"\{StaticResource ([a-zA-Z0-9]+Color)\}"', '"{DynamicResource $1}"')
    Set-Content -Path $file.FullName -Value $content -NoNewline
}
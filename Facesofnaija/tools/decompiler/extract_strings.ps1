param([string]$file)
$bytes = [System.IO.File]::ReadAllBytes($file)
$s = New-Object System.Text.StringBuilder
foreach ($b in $bytes) {
    if ($b -ge 32 -and $b -le 126) { [void]$s.Append([char]$b) } else { [void]$s.Append(' ') }
}
$tokens = $s.ToString() -split '\s{2,}'
$patterns = @('CreatePostCommentsAsync','CreatePostComment','create_post_comment','app_api.php','create_comment','createComment','CreateComment','CreateCommentAsync')
foreach ($p in $patterns) {
    $matches = $tokens | Where-Object { $_ -match $p }
    if ($matches) { Write-Output "--- Pattern: $p ---"; $matches | Select-Object -Unique | ForEach-Object { Write-Output $_ } }
}

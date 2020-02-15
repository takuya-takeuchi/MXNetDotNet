
$url = "https://pjreddie.com/media/files/mnist_train.csv"
$filename = "mnist_train.csv"

$directory = "data"
if ((Test-Path $directory) -eq $False) {
   New-Item $directory -ItemType Directory
}

$directory = Join-Path "data" "mnist_data"
if ((Test-Path $directory) -eq $False) {
   New-Item $directory -ItemType Directory
}

$directory = (Get-Item $directory).FullName
$file = Join-Path $directory $filename

Write-Host "Download from ${url} to ${file}" -ForegroundColor Green

Invoke-WebRequest -Uri $url -OutFile $file -UserAgent "Mozilla/5.0 (Windows NT; Windows NT 6.2; ja-JP) WindowsPowerShell/4.0"
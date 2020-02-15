
$urls =
@{
   "http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz" = "train-images-idx3-ubyte.gz"
   "http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz" = "train-labels-idx1-ubyte.gz"
   "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz" = "t10k-images-idx3-ubyte.gz"
   "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz" = "t10k-labels-idx1-ubyte.gz"
}

$directory = "data"
if ((Test-Path $directory) -eq $False) {
   New-Item $directory -ItemType Directory
}

$directory = Join-Path "data" "mnist_data"
if ((Test-Path $directory) -eq $False) {
   New-Item $directory -ItemType Directory
}

$directory = (Get-Item $directory).FullName

foreach ($url in $urls.keys)
{
   $file = $urls[$url]
   $file_wo_ext = [System.IO.Path]::GetFileNameWithoutExtension($file);
   $file = Join-Path $directory $file
   $file_wo_ext = Join-Path $directory $file_wo_ext

   Write-Host "Download from ${url} to ${file}" -ForegroundColor Green

   Invoke-WebRequest -Uri $url -OutFile $file -UserAgent "Mozilla/5.0 (Windows NT; Windows NT 6.2; ja-JP) WindowsPowerShell/4.0"
   #Expand-Archive -Path $file -DestinationPath $directory

   $input = New-Object System.IO.FileStream ($file), ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read);
   $output = New-Object System.IO.FileStream ($file_wo_ext), ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
   $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)
   try
   {
      $buffer = New-Object byte[](1024);
      while ($true)
      {
         $read = $gzipStream.Read($buffer, 0, 1024)
         if ($read -le 0)
         {
            break;
         }

         $output.Write($buffer, 0, $read)
      }
   }
   finally
   {
      $gzipStream.Close();
      $output.Close();
      $input.Close();
   }
}
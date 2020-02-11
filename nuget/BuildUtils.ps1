class Config
{

   $ConfigurationArray =
   @(
      "Debug",
      "Release"
   )

   $TargetArray =
   @(
      "cpu",
      "cuda",
      "mkl",
      "arm"
   )

   $PlatformArray =
   @(
      "desktop",
      "android",
      "ios",
      "uwp"
   )

   $ArchitectureArray =
   @(
      32,
      64
   )

   $CudaVersionArray =
   @(
      90,
      91,
      92,
      100,
      101
   )

   $CudaVersionHash =
   @{
      90 = "CUDA_PATH_V9_0";
      91 = "CUDA_PATH_V9_1";
      92 = "CUDA_PATH_V9_2";
      100 = "CUDA_PATH_V10_0";
      101 = "CUDA_PATH_V10_1"
   }

   $VisualStudio = "Visual Studio 15 2017"
   
   static $BuildLibraryWindowsHash = 
   @{
      "libmxnet"     = "libmxnet.dll";
   }
   
   static $BuildLibraryLinuxHash = 
   @{
      "libmxnet"     = "libmxnet.so";
   }
   
   static $BuildLibraryOSXHash = 
   @{
      "libmxnet"     = "libmxnet.dylib";
   }
   
   static $BuildLibraryIOSHash = 
   @{
      "libmxnet"     = "libmxnet.a";
   }

   [string]   $_Root
   [string]   $_Configuration
   [int]      $_Architecture
   [string]   $_Target
   [string]   $_Platform
   [string]   $_MklDirectory
   [int]      $_CudaVersion
   [string]   $_AndroidABI
   [string]   $_AndroidNativeAPILevel

   #***************************************
   # Arguments
   #  %1: Root directory of MXNetDotNet
   #  %2: Build Configuration (Release/Debug)
   #  %3: Target (cpu/cuda/mkl/arm)
   #  %4: Architecture (32/64)
   #  %5: Platform (desktop/android/ios/uwp)
   #  %6: Optional Argument
   #    if Target is cuda, CUDA version if Target is cuda [90/91/92/100/101]
   #    if Target is mkl and Windows, IntelMKL directory path
   #***************************************
   Config(  [string]$Root,
            [string]$Configuration,
            [string]$Target,
            [int]   $Architecture,
            [string]$Platform,
            [string]$Option
         )
   {
      if ($this.ConfigurationArray.Contains($Configuration) -eq $False)
      {
         $candidate = $this.ConfigurationArray -join "/"
         Write-Host "Error: Specify build configuration [${candidate}]" -ForegroundColor Red
         exit -1
      }

      if ($this.TargetArray.Contains($Target) -eq $False)
      {
         $candidate = $this.TargetArray -join "/"
         Write-Host "Error: Specify Target [${candidate}]" -ForegroundColor Red
         exit -1
      }

      if ($this.ArchitectureArray.Contains($Architecture) -eq $False)
      {
         $candidate = $this.ArchitectureArray -join "/"
         Write-Host "Error: Specify Architecture [${candidate}]" -ForegroundColor Red
         exit -1
      }

      if ($this.PlatformArray.Contains($Platform) -eq $False)
      {
         $candidate = $this.PlatformArray -join "/"
         Write-Host "Error: Specify Architecture [${candidate}]" -ForegroundColor Red
         exit -1
      }

      switch ($Target)
      {
         "cuda"
         {
            $this._CudaVersion = [int]$Option
            if ($this.CudaVersionArray.Contains($this._CudaVersion) -ne $True)
            {
               $candidate = $this.CudaVersionArray -join "/"
               Write-Host "Error: Specify CUDA version [${candidate}]" -ForegroundColor Red
               exit -1
            }
         }
         "mkl"
         {
            $this._MklDirectory = $Option
         }
      }

      switch ($Platform)
      {
         "android"
         {
            $decoded = [Config]::Base64Decode($Option)
            $setting = ConvertFrom-Json $decoded
            $this._AndroidABI            = $setting.ANDROID_ABI
            $this._AndroidNativeAPILevel = $setting.ANDROID_NATIVE_API_LEVEL
         }
      }

      $this._Root = $Root
      $this._Configuration = $Configuration
      $this._Architecture = $Architecture
      $this._Target = $Target
      $this._Platform = $Platform
   }

   static [string] Base64Encode([string]$text)
   {
      $byte = ([System.Text.Encoding]::Default).GetBytes($text)
      return [Convert]::ToBase64String($byte)
   }

   static [string] Base64Decode([string]$base64)
   {
      $byte = [System.Convert]::FromBase64String($base64)
      return [System.Text.Encoding]::Default.GetString($byte)
   }

   static [hashtable] GetBinaryLibraryWindowsHash()
   {
      return [Config]::BuildLibraryWindowsHash
   }

   static [hashtable] GetBinaryLibraryOSXHash()
   {
      return [Config]::BuildLibraryOSXHash
   }

   static [hashtable] GetBinaryLibraryLinuxHash()
   {
      return [Config]::BuildLibraryLinuxHash
   }

   static [hashtable] GetBinaryLibraryIOSHash()
   {
      return [Config]::BuildLibraryIOSHash
   }

   [string] GetRootDir()
   {
      return $this._Root
   }

   [string] GetLibMxNetRootDir()
   {
      return   Join-Path $this.GetRootDir() src |
               Join-Path -ChildPath "libmxnet"
   }

   [string] GetMXNetRootDir()
   {
      return   Join-Path $this.GetRootDir() src |
               Join-Path -ChildPath "incubator-mxnet"
   }

   [string] GetOpenCVRootDir()
   {
      return   Join-Path $this.GetRootDir() src |
               Join-Path -ChildPath opencv
   }

   [string] GetNugetDir()
   {
      return   Join-Path $this.GetRootDir() nuget
   }

   [int] GetArchitecture()
   {
      return $this._Architecture
   }

   [string] GetConfigurationName()
   {
      return $this._Configuration
   }

   [string] GetAndroidABI()
   {
      return $this._AndroidABI
   }

   [string] GetAndroidNativeAPILevel()
   {
      return $this._AndroidNativeAPILevel
   }

   [string] GetArtifactDirectoryName()
   {
      $target = $this._Target
      $platform = $this._Platform
      $name = ""

      switch ($platform)
      {
         "desktop"
         {
            if ($target -eq "cuda")
            {
               $cudaVersion = $this._CudaVersion
               $name = "${target}-${cudaVersion}"
            }
            else
            {
               $name = $target
            }
         }
      }

      return $name
   }

   [string] GetOSName()
   {
      $os = ""

      if ($global:IsWindows)
      {
         $os = "win"
      }
      elseif ($global:IsMacOS)
      {
         $os = "osx"
      }
      elseif ($global:IsLinux)
      {
         $os = "linux"
      }
      else
      {
         Write-Host "Error: This plaform is not support" -ForegroundColor Red
         exit -1
      }

      return $os
   }

   [string] GetIntelMklDirectory()
   {
      return [string]$this._MklDirectory
   }

   [string] GetArchitectureName()
   {
      $arch = ""
      $target = $this._Target
      $architecture = $this._Architecture

      if ($target -eq "arm")
      {
         if ($architecture -eq 32)
         {
            $arch = "arm"
         }
         elseif ($architecture -eq 64)
         {
            $arch = "arm64"
         }
      }
      else
      {
         if ($architecture -eq 32)
         {
            $arch = "x86"
         }
         elseif ($architecture -eq 64)
         {
            $arch = "x64"
         }
      }

      return $arch
   }

   [string] GetTarget()
   {
      return $this._Target
   }

   [string] GetPlatform()
   {
      return $this._Platform
   }

   [string] GetBuildDirectoryName([string]$os="")
   {
      if (![string]::IsNullOrEmpty($os))
      {
         $osname = $os
      }
      elseif (![string]::IsNullOrEmpty($env:TARGETRID))
      {
         $osname = $env:TARGETRID
      }
      else
      {
         $osname = $this.GetOSName()
      }
      
      $target = $this._Target
      $platform = $this._Platform
      $architecture = $this.GetArchitectureName()

      if ($target -eq "cuda")
      {
         $version = $this._CudaVersion
         return "build_${osname}_${platform}_cuda-${version}_${architecture}"
      }
      else
      {
         return "build_${osname}_${platform}_${target}_${architecture}"
      }
   }

   [string] GetVisualStudio()
   {
      return $this.VisualStudio
   }

   [string] GetVisualStudioArchitecture()
   {
      $architecture = $this._Architecture
      $target = $this._Target
      
      if ($target -eq "arm")
      {
         if ($architecture -eq 32)
         {
            return "ARM"
         }
         elseif ($architecture -eq 64)
         {
            return "ARM64"
         }
      }
      else
      {
         if ($architecture -eq 32)
         {
            return "Win32"
         }
         elseif ($architecture -eq 64)
         {
            return "x64"
         }
      }

      Write-Host "${architecture} and ${target} do not support" -ForegroundColor Red
      exit -1
   }

   [string] GetCUDAPath()
   {
      # CUDA_PATH_V10_0=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.0
      # CUDA_PATH_V10_1=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.1
      # CUDA_PATH_V9_0=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v9.0
      # CUDA_PATH_V9_1=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v9.1
      # CUDA_PATH_V9_2=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v9.2
      $version = $this.CudaVersionHash[$this._CudaVersion]      
      return [environment]::GetEnvironmentVariable($version, 'Machine')
   }

}

class ThirdPartyBuilder
{

   [Config]   $_Config

   ThirdPartyBuilder( [Config]$Config )
   {
      $this._Config = $Config
   }

   [string] BuildOpenCV()
   {
      $ret = ""
      $current = Get-Location

      try
      {
         Write-Host "Start Build OpenCV" -ForegroundColor Green

         $opencvDir = $this._Config.GetOpenCVRootDir()
         $opencvTarget = Join-Path $current opencv
         New-Item $opencvTarget -Force -ItemType Directory
         Set-Location $opencvTarget
         $current2 = Get-Location
         $installDir = Join-Path $current2 "install"
         $ret = $installDir

         if ($global:IsWindows)
         {
            Write-Host "   cmake -G "NMake Makefiles" -D CMAKE_BUILD_TYPE=Release `
         -D BUILD_SHARED_LIBS=OFF `
         -D BUILD_WITH_STATIC_CRT=OFF `
         -D CMAKE_INSTALL_PREFIX="$installDir" `
         -D BUILD_opencv_world=OFF `
         -D BUILD_opencv_java=OFF `
         -D BUILD_opencv_python=OFF `
         -D BUILD_opencv_python2=OFF `
         -D BUILD_opencv_python3=OFF `
         -D BUILD_PERF_TESTS=OFF `
         -D BUILD_TESTS=OFF `
         -D BUILD_DOCS=OFF `
         -D BUILD_opencv_core=ON `
         -D BUILD_opencv_highgui=ON `
         -D BUILD_opencv_imgcodecs=ON `
         -D BUILD_opencv_imgproc=ON `
         -D BUILD_opencv_calib3d=OFF `
         -D BUILD_opencv_features2d=OFF `
         -D BUILD_opencv_flann=OFF `
         -D BUILD_opencv_java_bindings_generator=OFF `
         -D BUILD_opencv_ml=OFF `
         -D BUILD_opencv_objdetect=OFF `
         -D BUILD_opencv_photo=OFF `
         -D BUILD_opencv_python_bindings_generator=OFF `
         -D BUILD_opencv_shape=OFF `
         -D BUILD_opencv_stitching=OFF `
         -D BUILD_opencv_superres=OFF `
         -D BUILD_opencv_video=OFF `
         -D BUILD_opencv_videoio=OFF `
         -D BUILD_opencv_videostab=OFF `
         -D WITH_CUDA=OFF `
         -D BUILD_PROTOBUF=OFF `
         -D WITH_PROTOBUF=OFF `
         -D WITH_IPP=OFF `
         -D WITH_FFMPEG=OFF `
         $opencvDir" -ForegroundColor Yellow
            cmake -G "NMake Makefiles" -D CMAKE_BUILD_TYPE=Release `
                                       -D BUILD_SHARED_LIBS=OFF `
                                       -D BUILD_WITH_STATIC_CRT=OFF `
                                       -D CMAKE_INSTALL_PREFIX="$installDir" `
                                       -D BUILD_opencv_world=OFF `
                                       -D BUILD_opencv_java=OFF `
                                       -D BUILD_opencv_python=OFF `
                                       -D BUILD_opencv_python2=OFF `
                                       -D BUILD_opencv_python3=OFF `
                                       -D BUILD_PERF_TESTS=OFF `
                                       -D BUILD_TESTS=OFF `
                                       -D BUILD_DOCS=OFF `
                                       -D BUILD_opencv_core=ON `
                                       -D BUILD_opencv_highgui=ON `
                                       -D BUILD_opencv_imgcodecs=ON `
                                       -D BUILD_opencv_imgproc=ON `
                                       -D BUILD_opencv_calib3d=OFF `
                                       -D BUILD_opencv_features2d=OFF `
                                       -D BUILD_opencv_flann=OFF `
                                       -D BUILD_opencv_java_bindings_generator=OFF `
                                       -D BUILD_opencv_ml=OFF `
                                       -D BUILD_opencv_objdetect=OFF `
                                       -D BUILD_opencv_photo=OFF `
                                       -D BUILD_opencv_python_bindings_generator=OFF `
                                       -D BUILD_opencv_shape=OFF `
                                       -D BUILD_opencv_stitching=OFF `
                                       -D BUILD_opencv_superres=OFF `
                                       -D BUILD_opencv_video=OFF `
                                       -D BUILD_opencv_videoio=OFF `
                                       -D BUILD_opencv_videostab=OFF `
                                       -D WITH_CUDA=OFF `
                                       -D BUILD_PROTOBUF=OFF `
                                       -D WITH_PROTOBUF=OFF `
                                       -D WITH_IPP=OFF `
                                       -D WITH_FFMPEG=OFF `
                                       $opencvDir
            Write-Host "   nmake" -ForegroundColor Yellow
            nmake
            Write-Host "   nmake install" -ForegroundColor Yellow
            nmake install
         }
         else
         {
            Write-Host "   cmake -D CMAKE_BUILD_TYPE=Release `
         -D BUILD_SHARED_LIBS=OFF `
         -D BUILD_WITH_STATIC_CRT=OFF `
         -D CMAKE_INSTALL_PREFIX="$installDir" `
         -D BUILD_opencv_world=OFF `
         -D BUILD_opencv_java=OFF `
         -D BUILD_opencv_python=OFF `
         -D BUILD_opencv_python2=OFF `
         -D BUILD_opencv_python3=OFF `
         -D BUILD_PERF_TESTS=OFF `
         -D BUILD_TESTS=OFF `
         -D BUILD_DOCS=OFF `
         -D BUILD_opencv_core=ON `
         -D BUILD_opencv_highgui=ON `
         -D BUILD_opencv_imgcodecs=ON `
         -D BUILD_opencv_imgproc=ON `
         -D BUILD_opencv_calib3d=OFF `
         -D BUILD_opencv_features2d=OFF `
         -D BUILD_opencv_flann=OFF `
         -D BUILD_opencv_java_bindings_generator=OFF `
         -D BUILD_opencv_ml=OFF `
         -D BUILD_opencv_objdetect=OFF `
         -D BUILD_opencv_photo=OFF `
         -D BUILD_opencv_python_bindings_generator=OFF `
         -D BUILD_opencv_shape=OFF `
         -D BUILD_opencv_stitching=OFF `
         -D BUILD_opencv_superres=OFF `
         -D BUILD_opencv_video=OFF `
         -D BUILD_opencv_videoio=OFF `
         -D BUILD_opencv_videostab=OFF `
         -D BUILD_PNG=ON `
         -D BUILD_JPEG=ON `
         -D WITH_CUDA=OFF `
         -D WITH_GTK=ON `
         -D WITH_GTK_2_X=ON `
         -D BUILD_PROTOBUF=OFF `
         -D WITH_PROTOBUF=OFF `
         -D WITH_IPP=OFF `
         -D WITH_FFMPEG=OFF `
         $opencvDir" -ForegroundColor Yellow
            cmake -D CMAKE_BUILD_TYPE=Release `
                  -D BUILD_SHARED_LIBS=OFF `
                  -D BUILD_WITH_STATIC_CRT=OFF `
                  -D CMAKE_INSTALL_PREFIX="$installDir" `
                  -D BUILD_opencv_world=OFF `
                  -D BUILD_opencv_java=OFF `
                  -D BUILD_opencv_python=OFF `
                  -D BUILD_opencv_python2=OFF `
                  -D BUILD_opencv_python3=OFF `
                  -D BUILD_PERF_TESTS=OFF `
                  -D BUILD_TESTS=OFF `
                  -D BUILD_DOCS=OFF `
                  -D BUILD_opencv_core=ON `
                  -D BUILD_opencv_highgui=ON `
                  -D BUILD_opencv_imgcodecs=ON `
                  -D BUILD_opencv_imgproc=ON `
                  -D BUILD_opencv_calib3d=OFF `
                  -D BUILD_opencv_features2d=OFF `
                  -D BUILD_opencv_flann=OFF `
                  -D BUILD_opencv_java_bindings_generator=OFF `
                  -D BUILD_opencv_ml=OFF `
                  -D BUILD_opencv_objdetect=OFF `
                  -D BUILD_opencv_photo=OFF `
                  -D BUILD_opencv_python_bindings_generator=OFF `
                  -D BUILD_opencv_shape=OFF `
                  -D BUILD_opencv_stitching=OFF `
                  -D BUILD_opencv_superres=OFF `
                  -D BUILD_opencv_video=OFF `
                  -D BUILD_opencv_videoio=OFF `
                  -D BUILD_opencv_videostab=OFF `
                  -D BUILD_PNG=ON `
                  -D BUILD_JPEG=ON `
                  -D WITH_CUDA=OFF `
                  -D WITH_GTK=ON `
                  -D WITH_GTK_2_X=ON `
                  -D BUILD_PROTOBUF=OFF `
                  -D WITH_PROTOBUF=OFF `
                  -D WITH_IPP=OFF `
                  -D WITH_FFMPEG=OFF `
                  $opencvDir
            Write-Host "   make" -ForegroundColor Yellow
            make -j4
            Write-Host "   make install" -ForegroundColor Yellow
            make install
         }
      }
      finally
      {
         Set-Location $current
         Write-Host "End Build OpenCV" -ForegroundColor Green
      }

      return $ret
   }

}

function ConfigCPU([Config]$Config)
{
   # get root directory of incubator-mxnet
   $rootDir = $Config.GetMXNetRootDir()

   $Builder = [ThirdPartyBuilder]::new($Config)
   
   # Build opencv
   $installOpenCVDir = $Builder.BuildOpenCV()

   $current = Get-Location
   $build_dir = Join-Path $current "incubator-mxnet"
   if ((Test-Path $build_dir) -eq $False)
   {
      New-Item $build_dir -ItemType Directory
   }
   Set-Location $build_dir

   if ($IsWindows)
   {
      $env:OpenCV_DIR = $installOpenCVDir

      cmake -G $Config.GetVisualStudio() -A $Config.GetVisualStudioArchitecture() -T host=x64 `
            -D USE_CPP_PACKAGE=1 `
            -D USE_CUDA:BOOL=0 `
            -D USE_CUDNN:BOOL=0 `
            -D OpenCV_DIR=$installOpenCVDir `
            $rootDir
   }
   else
   {
      $env:OpenCV_DIR = $installOpenCVDir

      cmake -D USE_CPP_PACKAGE=1 `
            -D USE_CUDA:BOOL=0 `
            -D USE_CUDNN:BOOL=0 `
            -D OpenCV_DIR=$installOpenCVDir `
            $rootDir
   }
}

function ConfigCUDA([Config]$Config)
{
   # get root directory of incubator-mxnet
   $rootDir = $Config.GetMXNetRootDir()

   $Builder = [ThirdPartyBuilder]::new($Config)
   
   # Build opencv
   $installOpenCVDir = $Builder.BuildOpenCV()

   $current = Get-Location
   $build_dir = Join-Path $current "incubator-mxnet"
   if ((Test-Path $build_dir) -eq $False)
   {
      New-Item $build_dir -ItemType Directory
   }
   Set-Location $build_dir

   if ($IsWindows)
   {
      $cudaPath = $Config.GetCUDAPath()
      if (!(Test-Path $cudaPath))
      {
         Write-Host "Error: '${cudaPath}' does not found" -ForegroundColor Red
         exit -1
      }

      $env:OpenCV_DIR = $installOpenCVDir
      $env:CUDA_PATH="${cudaPath}"
      $env:PATH="$env:CUDA_PATH\bin;$env:CUDA_PATH\libnvvp;$ENV:PATH"
      Write-Host "Info: CUDA_PATH: ${env:CUDA_PATH}" -ForegroundColor Green

      cmake -G $Config.GetVisualStudio() -A $Config.GetVisualStudioArchitecture() -T host=x64 `
            -D USE_CPP_PACKAGE=1 `
            -D USE_CUDA:BOOL=1 `
            -D USE_CUDNN:BOOL=1 `
            -D OpenCV_DIR=$installOpenCVDir `
            $rootDir
   }
   else
   {
      $env:OpenCV_DIR = $installOpenCVDir

      cmake -D USE_CPP_PACKAGE=1 `
            -D USE_CUDA:BOOL=1 `
            -D USE_CUDNN:BOOL=1 `
            -D OpenCV_DIR=$installOpenCVDir `
            $rootDir
   }
}

function Build([Config]$Config)
{
   $Current = Get-Location

   $Output = $Config.GetBuildDirectoryName("")
   if ((Test-Path $Output) -eq $False)
   {
      New-Item $Output -ItemType Directory
   }

   Write-Host "Info: Output: ${Output}" -ForegroundColor Green
   Set-Location -Path $Output

   $Target = $Config.GetTarget()
   $Platform = $Config.GetPlatform()

   switch ($Platform)
   {
      "desktop"
      {
         switch ($Target)
         {
            "cpu"
            {
               ConfigCPU $Config
            }
            "mkl"
            {
               ConfigMKL $Config
            }
            "cuda"
            {
               ConfigCUDA $Config
            }
            "arm"
            {
               ConfigARM $Config
            }
         }
      }
   }

   cmake --build . --config $Config.GetConfigurationName()

   # Move to Root directory
   Set-Location -Path $Current
}

function CopyToArtifact()
{
   Param([string]$srcDir, [string]$build, [string]$libraryName, [string]$dstDir, [string]$rid, [string]$configuration="")

   if ($configuration)
   {
      $binary = Join-Path ${srcDir} ${build}  | `
               Join-Path -ChildPath ${configuration} | `
               Join-Path -ChildPath ${libraryName}
   }
   else
   {
      $binary = Join-Path ${srcDir} ${build}  | `
               Join-Path -ChildPath "incubator-mxnet" | ` 
               Join-Path -ChildPath ${libraryName}
   }

   $output = Join-Path $dstDir runtimes | `
            Join-Path -ChildPath ${rid} | `
            Join-Path -ChildPath native | `
            Join-Path -ChildPath $libraryName

   Write-Host "Copy ${libraryName} to ${output}" -ForegroundColor Green
   Copy-Item ${binary} ${output}
}
@echo off
@call SetEnv.bat

set CURDIR=%cd%
mkdir "%ARTIFACTS%"

cd incubator-mxnet

git submodule update --init --recursive

set BUILDDIR=build_gpu
mkdir %BUILDDIR%
cd %BUILDDIR%
cmake -G "Visual Studio 14 2015 Win64" ^
      -D USE_CUDA:BOOL=1 ^
      -D USE_CUDNN:BOOL=1 ^
      -D USE_CPP_PACKAGE:BOOL=0 ^
..

cmake --build . --config %1

rem **************************************
rem Collect artifacts
rem **************************************
xcopy "%1\libmxnet.dll" "%CURDIR%\%ARTIFACTS%" /y

cd %CURDIR%
xcopy "%OpenBLAS_HOME%\bin\libopenblas.dll" %ARTIFACTS% /y
xcopy "%CUDNN_HOME%\bin\cudnn64_%CUDNN_VERSION%.dll" %ARTIFACTS% /y
xcopy "%CUDA_HOME%\v%CUDA_VERSION%\bin\cublas64_%CUDA_DLL_VERSION%.dll" %ARTIFACTS% /y
xcopy "%CUDA_HOME%\v%CUDA_VERSION%\bin\cufft64_%CUDA_DLL_VERSION%.dll" %ARTIFACTS% /y
xcopy "%CUDA_HOME%\v%CUDA_VERSION%\bin\curand64_%CUDA_DLL_VERSION%.dll" %ARTIFACTS% /y
xcopy "%CUDA_HOME%\v%CUDA_VERSION%\bin\cusolver64_%CUDA_DLL_VERSION%.dll" %ARTIFACTS% /y
xcopy "%CUDA_HOME%\v%CUDA_VERSION%\bin\nvrtc64_%CUDA_DLL_VERSION%.dll" %ARTIFACTS% /y
xcopy "%OpenCV_DIR%\%OpenCV_LIB%" %ARTIFACTS% /y

rem **************************************
rem Download mingw64
rem **************************************
python -m wget %MINGW_URL%%MINGW_ZIP%
python copy_mingw64.py %MINGW_ZIP%
move mingw64_dll\*.dll %ARTIFACTS%
del %MINGW_ZIP%
rmdir mingw64_dll
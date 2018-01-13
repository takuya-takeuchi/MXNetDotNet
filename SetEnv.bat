@echo off
rem *************************************
rem You MUST follow the fhe rules
rem 1. Do NOT use '"' to handle space in path
rem *************************************
@set CURDIR=%cd%


rem *************************************
rem Edit only this sections
rem *************************************
@set CUDA_VERSION=7.5
@set CUDA_DLL_VERSION=75
@set CUDNN_VERSION=5
@set CUDA_HOME=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA
@set CUDNN_HOME=D:\Works\Lib2\NVIDIA\cuDNN\7.5\5.1\Win7
@set OpenBLAS_HOME=D:\Works\Lib2\OpenBLAS\0.2.19
@set OpenCV_DIR=D:\Works\Lib2\OpenCV\3.2.0.prebuild\build
@set OpenCV_LIB=x64\vc14\bin\opencv_world320.dll


@set MXNET_ROOT=%CURDIR%\incubator-mxnet
@set ARTIFACTS=artifacts
@set MINGW_URL=https://sourceforge.net/projects/openblas/files/v0.2.15/
@set MINGW_ZIP=mingw64_dll.zip
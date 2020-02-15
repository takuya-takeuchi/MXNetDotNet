@echo off 
@setlocal enabledelayedexpansion 
 
@set CURRENT=%cd% 
 
@set TARGET=Release 
@set SRC_DIR=..\..\..\..\..\src 
@set LIBMXNET_DIR=%SRC_DIR%\libmxnet\build_win_desktop_cuda-101_x64\incubator-mxnet\%TARGET% 
@set OPENCV_DIR=%SRC_DIR%\libmxnet\build_win_desktop_cuda-101_x64\opencv\bin 
 
@set target_1=AlexNet 
@set target_2=GoogLeNet 
@set target_3=ImageClassification 
@set target_4=InceptionBN 
@set target_5=LeNet 
@set target_6=LeNetWithMXDataIter 
@set target_7=MLP 
@set target_8=MLPCPU 
@set target_9=MLPGPU 
@set target_10=TestScore 
 
@set i=1 
:FOREACH 
@set it=!target_%i%! 
if defined it ( 
 	echo %it% 
	@set DIRECTORY=%CURRENT%\%it%\bin\%TARGET%\netcoreapp2.0 
 
	@mkdir "%CURRENT%\%it%\bin\%TARGET%\netcoreapp2.0" 
	cd "%CURRENT%\%it%\bin\%TARGET%\netcoreapp2.0" 
	 
	@del libmxnet.dll 
	@del libmxnet.pdb 
	@del cudnn64_7.dll 
	@del libgcc_s_seh-1.dll 
	@del libgfortran-3.dll 
	@del libquadmath-0.dll 
	@del libopenblas.dll 
	@del opencv_world349.dll 
	@del opencv_world349.pdb 
	@del opencv_world349d.dll 
	@del opencv_world349d.pdb 
	@del cudnn64_7.dll 
 
	@mklink libmxnet.dll "%LIBMXNET_DIR%\libmxnet.dll" 
	@mklink libmxnet.pdb "%LIBMXNET_DIR%\libmxnet.pdb" 
	@mklink libgcc_s_seh-1.dll "%SRC_DIR%\mingw64\mingw64_dll\libgcc_s_seh-1.dll 
	@mklink libgfortran-3.dll "%SRC_DIR%\mingw64\mingw64_dll\libgfortran-3.dll 
	@mklink libquadmath-0.dll "%SRC_DIR%\mingw64\mingw64_dll\libquadmath-0.dll 
	@mklink libopenblas.dll "%SRC_DIR%\openblas\bin\libopenblas.dll" 
	@mklink opencv_world349.dll "%OPENCV_DIR%\opencv_world349.dll" 
	@mklink opencv_world349.pdb "%OPENCV_DIR%\opencv_world349.pdb" 
	@mklink opencv_world349d.dll "%OPENCV_DIR%\opencv_world349d.dll" 
	@mklink opencv_world349d.pdb "%OPENCV_DIR%\opencv_world349d.pdb" 
	@mklink cudnn64_7.dll "C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.1\bin\cudnn64_7.dll" 
 
	@cd %CURRENT% 
  set /a i+=1 
  goto :FOREACH 
)
@echo off
@set CURDIR=%cd%
@python prepare.py

@rmdir /s /q train

@cd bin\Release\netcoreapp2.0
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\train ..\..\..\data\train/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\val ..\..\..\data\test/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 224 --quality 90 --num-thread 16 ..\..\..\train ..\..\..\data\train/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 224 --quality 90 --num-thread 16 ..\..\..\val ..\..\..\data\test/

@rem move to data folders
@move ..\..\..\train.idx ..\..\..\data\train.idx
@move ..\..\..\train.lst ..\..\..\data\train.lst
@move ..\..\..\train.rec ..\..\..\data\train.rec 
@move ..\..\..\val.idx ..\..\..\data\val.idx
@move ..\..\..\val.lst ..\..\..\data\val.lst
@move ..\..\..\val.rec ..\..\..\data\val.rec

@cd %CURDIR%
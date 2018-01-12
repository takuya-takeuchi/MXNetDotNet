python prepare.py

bin\Release\netcoreapp2.0
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\train ..\..\..\data\train/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\val ..\..\..\data\test/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 256 --quality 90 --num-thread 16 ..\..\..\train ..\..\..\data\train/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 256 --quality 90 --num-thread 16 ..\..\..\val ..\..\..\data\test/

# move to data folders
move ..\..\..\train.idx ..\..\..\data\train.idx
move ..\..\..\train.lst ..\..\..\data\train.lst
move ..\..\..\train.rec ..\..\..\data\train.rec 
move ..\..\..\val.idx ..\..\..\data\val.idx
move ..\..\..\val.lst ..\..\..\data\val.lst
move ..\..\..\val.rec ..\..\..\data\val.rec
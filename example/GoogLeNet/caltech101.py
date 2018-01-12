import os
import sys
import random
import shutil

num_train = 30 # num for images files/per category for using train
output_dir = 'caltech_101_train'
if not os.path.exists(output_dir):
  os.mkdir(output_dir)

directory = '101_ObjectCategories'
for root, dirs, files in os.walk(directory):
  for dir in dirs: # list category
    out_cat = os.path.join(output_dir, dir)
    if not os.path.exists(out_cat):
      os.mkdir(out_cat)

    # pickup files
    cat = os.path.join(directory, dir)
    for root2, dirs2, files2 in os.walk(cat):
      random.shuffle(files2)
      for im in files2[0:num_train]:
        src = os.path.join(cat, im)
        dst = os.path.join(out_cat, im)
        shutil.move(src, dst)
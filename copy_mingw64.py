import zipfile
import sys
 
args = sys.argv

# extract zips
zip = zipfile.ZipFile(args[1], 'r')
zip.extractall()
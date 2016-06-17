#coding:s-jis
import sys
from romkan import *

# かなのみのファイルを読み込んで、かなとローマ字表記のcsvに変換する

if (len(sys.argv) != 2):
  print "usage: "+ sys.argv[0]+" [target_txt_file]"
  sys.exit()

filename = sys.argv[1]
write_file_name = filename + ".dic"
file = open(filename)
data = file.readlines()
file.close()

newdict = []
for line in data:
  row = (line.strip().split('　'))[0]
  alph = to_roma(row.decode('cp932')).encode('cp932')
  if alph.find("'")>0:
    print alph
    continue
  newdict.append((row, alph.upper()))


fout = open(write_file_name, 'w')

for line in newdict:
  writing = line[0]+','+line[1]+'\n'
  fout.write(writing)
  
fout.close()
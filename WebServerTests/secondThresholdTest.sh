#!/bin/bash

#size should be 4096
s=""
sweg="TEST:$i\r\n" #test headers

for ((i=0; i<102400; i++));
do s=$s$sweg;
done

printf "\n"
printf "GET / HTTP/1.1\r\n$s\r\n"| nc localhost 4220
printf "\n\n"


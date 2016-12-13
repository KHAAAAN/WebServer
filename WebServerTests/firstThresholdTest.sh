#!/bin/bash

#size should be 4096
s=""
sweg="0"

for ((i=0; i<2032; i++));  #putting it from 2032 to 2033, will have it so server wont respond anymore
do s=$s$sweg;
done;

printf "\n"
printf "GET /$s HTTP/1.1\r\n\r\n"| nc localhost 4220 
printf "\n\n"

#NOTE: len of string before CRLFCRLF without $s is 14



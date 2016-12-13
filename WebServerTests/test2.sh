#!/bin/sh

#size should be 4096
s=""
for i in `seq 1 1024`;
do
	sweg="sweg"
	s=$s$sweg
done

printf "\n"
printf "GET / HTTP/1.1\r\nSweg:2\r\nContent-Length:4096\r\n\r\n$s"| nc localhost 4220
printf "\n\n"


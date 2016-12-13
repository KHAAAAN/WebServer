#!/bin/sh

printf "\n"
printf "GET / HTTP/1.1\r\nSweg:2\r\nContent-Length:4096\r\n\r\n$s"| nc https://debianvm.eecs.wsu.edu/api/questions 4220
printf "\n\n"



#!/bin/sh

printf "\n"
printf "GET /files/4ByteFile.txt HTTP/1.1\r\nRange: bytes=1-\r\n\r\n"| nc localhost 4220
printf "\n\n"


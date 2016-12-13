#!/bin/sh

printf "\n"
printf "GET /files/4ByteFile.txt HTTP/1.1\r\nRange: bytes=0-2030\r\n\r\n$s"| nc localhost 4220
printf "\n\n"


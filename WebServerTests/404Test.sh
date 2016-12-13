#!/bin/sh

printf "\n"
printf "GET a HTTP/1.1\r\n\r\n" | nc localhost 4220
printf "\n\n"


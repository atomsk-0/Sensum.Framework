#!/bin/bash
set -e
g++ -c enet.cpp -o enet.o -O3
ar rcs libenet.a enet.o
rm enet.o
echo "Library created successfully."

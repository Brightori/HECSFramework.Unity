#!/bin/bash

cd "$(dirname "$0")"

current_dir=$(pwd)
scripts_path=$(echo "$current_dir" | awk -F '/Assets' '{print $1 "/Assets"}')

echo "Scripts path: $scripts_path"

codesign --force --deep -s - ./RoslynHECS

./RoslynHECS no_commands no_resolvers path:"$scripts_path"


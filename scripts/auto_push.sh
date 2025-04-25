#!/bin/bash

# Caminho até o repo
REPO_DIR="$1"
COMMIT_MSG="$2"

# Entra no diretorio do repo
cd "$REPO_DIR" || exit 1
git add .
git commit -m "$COMMIT_MSG"

git push
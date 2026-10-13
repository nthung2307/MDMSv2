#!/usr/bin/env bash

while getopts "e:v:h" opt
do
  # shellcheck disable=SC2220
  case "$opt" in
    e) env="$OPTARG" ;;
    v) version="$OPTARG" ;;
  esac
done

# Export to system env for docker compose 

case "$env" in
  development)
    export env=development
    export COMPOSE_PROJECT_NAME="amr-dev-frontend-services" 
    export command='yarn preview'
    ;;
  production)
    export env=prod
    export COMPOSE_PROJECT_NAME="amr-prod-frontend-services"
    export command='yarn preview'
    source ~/.zshrc
    ;;
  uat)
    export env=uat
    export COMPOSE_PROJECT_NAME="amr-uat-frontend-services"
    export command='yarn preview'
    source ~/.zshrc
    ;;
esac
export version=$env-v$version
echo "Building version [$version] service on {$env} environment"

docker-compose -f docker-compose.$env.yml config
docker-compose -f docker-compose.$env.yml build --no-cache --progress=plain
docker-compose -f docker-compose.$env.yml up -d

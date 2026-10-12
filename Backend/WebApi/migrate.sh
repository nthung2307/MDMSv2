#!/usr/bin/env bash
if [ $1 = '--help' ]; then
  echo -e '\t----------------------------------------------------';
  echo -e '\t./scripts/migrate.sh <Environment> <Migration Name> ';
  echo -e '\tE.g.';
  echo -e '\t./scripts/migrate.sh Development InitDatabase';
  exit;
fi
export ASPNETCORE_ENVIRONMENT=$1
dotnet ef migrations add "$2" --project WebApi.csproj --context AppReadWriteDbContext --output-dir Migrations
$SHELL
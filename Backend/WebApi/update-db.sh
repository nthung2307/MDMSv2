#!/usr/bin/env bash
dotnet ef database update --project WebApi.csproj --context AppReadWriteDbContext
$SHELL
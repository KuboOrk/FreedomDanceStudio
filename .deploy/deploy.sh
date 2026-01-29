#!/bin/bash
cd /home/github/docker/freedom/_deploy
echo "gldt-PWdsHfjLTigjWAcqCWVo" | docker login registry.gitlab.com -u pull.images --password-stdin

docker compose -f docker-compose.yml down
docker rmi registry.gitlab.com/kuboork-server/docker/freedomdancestudio:latest

docker compose -f docker-compose.yml up -d
docker logout registry.gitlab.com

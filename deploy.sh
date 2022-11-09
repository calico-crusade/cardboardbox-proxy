#!/bin/sh

docker pull ghcr.io/calico-crusade/cardboardbox-proxy/api:latest

docker-compose -f docker-compose.yml up -d
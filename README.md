# Tilework

## About

Tilework is a fully integrated reverse proxying and load balancing platform. It is designed with the main objective of being simple and fast to configure rather than being in the way.

## Features
- Deployment of HTTP/TCP/UDP load balancers with multiple backends
- HTTP rules based routing, including hostname, URL path, query string
- Certificate issuing via popular services, lifecycle management, auto-renewal
- Realtime and historical service statistics
- Docker based service deployment - no disruption of the host environment


## Install
1. Install [docker engine](https://docs.docker.com/engine/install/) or [docker desktop](https://docs.docker.com/get-started/get-docker/). Be sure that docker compose is also installed.




2. Create a docker-compose.yml file as follows:

```yaml
services:
  tileworkui:
    image: tilework/tilework:latest
    ports:
      - 5180:5180
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
    volumes:
      - tilework_data:/var/lib/tilework
      - /var/run/docker.sock:/var/run/docker.sock

volumes:
  tilework_data:
    external: false
```

3. Start the service up
```
# If using the docker-compose command
docker-compose up -d

# If using docker-compose-plugin
docker compose up -d
```

4. Navigate your browser to http://\<host>:5180

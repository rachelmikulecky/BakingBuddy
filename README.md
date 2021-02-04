# BakingBuddy

## What is BakingBuddy?

BakingBuddy is a self-hosted personal recipe web application. It was created with Docker, C#, and the .NET Core framework. 

Features:

* Create, edit, and view personal recipes
* Store recipes in plain markdown files
* Log your meals (add notes and an image)
* Keep a list of the next meals you'll make
* Add ingredient-specific volume to mass conversions
* View your volume-measured recipes as mass
* Recipe scaling

Inspired by [OpenEats](https://github.com/open-eats/OpenEats), BakingBuddy was created as a way to keep track of all of the recipes my family uses regularly.

## Usage

### Quick Start

```shell
sudo mkdir /bakingbuddy
sudo mkdir /bakingbuddy/recipes
sudo docker run -d \
  -p 23516:23516 \
  -v /bakingbuddy:/webapp/volume_data \
  --name bakingbuddy \
  rachelmikulecky/bakingbuddy:testing
```

For those new to Docker, here is an explanation of the options:

* `-d` - Run as a daemon ("detached").
* `-p` - Expose ports.
* `-v` - Mount `/bakingbuddy/volume_data` on the local file system to `/webapp/volume_data` in the container.
* `--restart` - Restart the server if it crashes and at system start
* `--name` - Name the container "bakingbuddy" (otherwise it has a funny random name).

Check the logs to see what happened:

```shell
docker logs bakingbuddy
```

Open a bash shell in the container:

```shell
docker exec -it bakingbuddy bash
```

Stop the container:

```shell
docker kill bakingbuddy
```

Remove the container:

```shell
docker rm bakingbuddy
```
Verify that the recipes are being saved to your computer by making a recipe and stopping and starting the container. 

```shell
docker kill bakingbuddy
docker rm bakingbuddy
sudo docker run -d \
  -p 23516:23516 \
  -v /bakingbuddy:/webapp/volume_data \
  --name bakingbuddy \
  rachelmikulecky/bakingbuddy:testing
```

### Docker Compose

[Docker Compose](https://docs.docker.com/compose/install/) is an easy way to run Docker containers.

First make a docker-compose.yml file.

```shell
version: '2'
services:
  bakingbuddy:
    image: rachelmikulecky/bakingbuddy:testing
    ports:
     - "23516:23516"
    volumes:
     - /bakingbuddy:/webapp/volume_data
```

Now cd to the directory with docker-compose.yml and run:

```shell
sudo mkdir /bakingbuddy
sudo mkdir /bakingbuddy/recipes
sudo docker-compose up -d
```

### Local Development

Clone this repository and after making the desired changes run the following

```shell
dotnet publish -c release -o webapp
docker-compose build
docker-compose up
```

Once the image has been created, push your image to Docker Hub via [these instructions](https://ropenscilabs.github.io/r-docker-tutorial/04-Dockerhub.html)


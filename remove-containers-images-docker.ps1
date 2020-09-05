#stop all containers:
#docker kill $(docker ps -q)
docker stop $(docker ps -a -q)

#remove all containers
docker rm $(docker ps -a -q)

#remove all docker images
docker rmi $(docker images -q)

#remove all docker volumes
docker volume ls -qf dangling=true | xargs -r docker volume rm
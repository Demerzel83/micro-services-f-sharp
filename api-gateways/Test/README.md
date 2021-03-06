# basket_service

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

## Build and test the application

### Windows

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

### Linux/macOS

Run the `build.sh` script in order to restore, build and test (if you've selected to include tests) the application:

```
$ ./build.sh
```

## Run the application

After a successful build you can start the web application by executing the following command in your terminal:

```
dotnet run src/basket_service
```

After the application has started visit [http://localhost:5000](http://localhost:5000) in your preferred browser.

Steps to build the docker image and run the docker container
docker build -t basketservice .
docker run -p 5000:80 -p 5001:80 -v ${HOME}/.aspnet/https:/https/ basketservice

The api gateway is calling other services by GRPC
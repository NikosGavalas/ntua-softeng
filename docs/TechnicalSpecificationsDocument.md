# Technical Specifications

## Architecture and Design

The development stack has been arranged as follows:

- Database: **MySql**
	- The relational model, generally suits well any application that does not work with any Artificial Intelligence technologies, or any form of data that is otherwise sparsely defined and/or distributed. Therefore, it became apparent that the application's back-end storage would be within the SQL family. Within that set of languages, MySql was pretty straight-forward choice as it is free, flexible, easy to set up or use and widely supported in any programming language by numerous libraries and functions.
- Web-Server: **C#**
	- Although the choice here was mostly preferential, it is a strong one, as the .NET Framework is very versatile, robust and agile in development. This language is further supported by Microsoft's own Visual Studio, which makes any workflow effortlessly streamlined and optimized.
	- For any interaction between the web server and the application, the former will provide a JSON API for data retrieval, as well as any operation the user might intend to perform through the application, for example booking an event or changing their password.
- Front-End: **HTML, CSS, Javascript**
	- As this is a web application, the default W3C set of languages has been chosen to both design and render the application. Any alternatives would involve the use of specialized front-end rendering frameworks, which were deemed to be beyond the scope of the project. Describing the application natively in the web languages also ensures abstraction, making the front-end of the project not have any major dependency on any other parts of stack.


### Frameworks / Libraries

#### Back-End

- [Newtonsoft.Json](https://www.newtonsoft.com/json)
	- The choice of this library has become pretty obvious when it comes to web applications that communicate over APIs. In our case, the communication between the web page and the web server is done via asynchronous HTTP requests and all the messages are in JSON format, thus making this library almost necessary.
- [HaathDB](https://git.gmantaos.com/Haath/HaathDB)
	- This library extends - and depends on - the official [MySql.Data](https://www.nuget.org/packages/MySql.Data/). Offering significant simplification and abstraction over basic database functions, through serialization, this library - in most cases - eliminates the need for most additional development when integrating C# with MySql.
- [Watermark](https://bitbucket.org/teamdroptabel/watermark)
	- The other part of this project, a library that will be used to watermark images provided by event organizers. Its function will be to protect them from copyright theft, which can happen on a platform such as this one.
	- The releases of this project are hosted on a [private NuGet server](https://nuget.gmantaos.com/?specialType=singlePackage&id=ProgTech.Watermak).
- [Chance.NET](https://github.com/gmantaos/Chance.NET)
	- This library will not see any use during the deployment of the application. It's a randomness generation which will be used during development, by generating large amounts of fake data for testing purposes.

#### Front-End

- [jQuery](https://jquery.com/)
	- This leading library in DOM parsing and manipulation, will be utilized for just about every function of the website. Using its AJAX capabilities for asynchronous http requests, as well as its high-level methods for document event handling, each and every page will be designed to be fast and responsive.
- [Bootstrap 3](https://getbootstrap.com/)
	- Bootstrap will be used mostly for designing the responsive layout of the pages but also for providing certain functionalities, like pop-up windows.


<!-- Visualizations/Modeling, flow charts, data flow charts, decision trees...


### UML diagrams

UX visualizations, control flow

### Wireframes

describing the UI

### OOP Models

also Interfacing between components etc

### Design Trade-offs

for example we assume that users have cookies available (to keep the sessions alive)  -->

## System Requirements

### Environment

The web server can run in either one of the following environments with their respective dependencies

- Microsoft Windows
	- .NET Framework 4.6 or higher
- Any unix distribution
	- Mono Framework 5.4 or higher
- Any unix distribution
	- Docker

The web server can be built in either one of the following environments with their respective dendencies

- Microsoft Windows
	- NuGet
	- MSbuild
- Any unix distribution
	- NuGet
	- xBuild
- Any unix distribution
	- Docker

The application also requires a stable connection to a `MySql server`.

### Hardware requirements

The application itself has a small memory footprint but is linearly scalable. The application has to store the high-resolution images that the organizers will upload and thus has a significant requirement for storage. 

Furthermore, since the application is going to handle a currency-like system as well as paid event booking, security coherency and fault tolerance are also big concerns.


<!-- ## Implementation

### Acceptance Tests

tests descriptions -->

## Build & Deployment

The project's deployment will revolve around [Docker](https://www.docker.com/). A linux bash script will be used to automate the process, which will do the following:

1. Pull the source code from the repository.
2. Build the project using a Makefile which in summary does the following
	1. Use `nuget restore`, which will download all the dependencies of the solution.
	2. Use `xbuild`, which is a clone of the official `MSBuild`, to build all projects in the solution in the correct order using the *Release* configuration.
3. Build the docker image from the `Dockerfile` which in summary does the following
	1. Use an image that has `mono-complete` installed.
	2. Copies the build folder `Pleisure/bin/Release/` to `/opt/Pleisure` inside the container
	3. Runs the web server as the image's entrypoint using the command `mono /opt/Pleisure/Pleisure.exe`

Additionally, any options that need to be passed to the application, such as the credentials for the MySql server for example, will be passed when running the container in the form of environment variables, like the following example

```bash
docker run \
	-e MYSQL_HOST=172.17.0.1 \
	-e MYSQL_USER=progtech \
	...
```

### Monitoring

As the application will run in a Docker container, there are plenty of ways to monitor its status and performance

The following command will follow the stdout of the web server running inside the container

```bash
docker logs -f pleisure
```

The following command will provide diagnostic information on the running container

```bash
docker stats pleisure

CONTAINER           CPU %               MEM USAGE / LIMIT     MEM %               NET I/O             BLOCK I/O           PIDS
pleisure           0.00%               95.36MiB / 923.4MiB   10.33%              10MB / 11.5MB       58.5MB / 1.62MB     0
```








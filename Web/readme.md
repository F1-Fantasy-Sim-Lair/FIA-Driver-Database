# Web

## Starting the application

### CommandLine
To start the application from the command line, use the following command: `dotnet run`. The database file will be created automatically in the project directory. The application will start on `http://localhost:5026`. Alternatively, use the following command to start the application with https support: `dotnet run --launch-profile https`

### Docker
To create the container image, use the following command: `docker build -t lair/fiadriverdb-server:latest .`

To run the container image, use the following command: `docker run --name fiadriverdb -p 8080:8080 lair/fiadriverdb-server:latest` The database file will be created automatically in `/db` within the container. The application will start on `http://localhost:8080`

To run the container image with a persistent database, add a volume mapped to the `/db` directory: `docker run --name fiadriverdb -p 8080:8080  -v fiadriverdb_db:/db lair/fiadriverdb-server:latest`

To persist the database on your local machine, mount a directory to the `/db` directory: `docker run --name fiadriverdb -p 8080:8080  -v <path>:/db lair/fiadriverdb-server:latest`
# DeepStabP - WebUI

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 6.0 or higher
* [Node 16](https://nodejs.org/en/download/)
* [Python 3.9](https://www.python.org/downloads/release/python-3916/) (used for ml api)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server, client and the python fastapi components in watch mode use the following command:

```bash
./build.cmd
```

Then open `http://localhost:8080` in your browser for the web ui and `http://localhost:8000/docs` for the python api OpenApi docs.


## Docker

### Environment

- **DEEPSTABP_URL**: Sets the url for the api predictor backend.

  Default: *"http://localhost:8000"*

### Publish

1. Before publishing make sure to commit all changes and update the version with

   `./build.cmd releasenotes semver:xxx`, where xxx is the required version [major|minor|patch].
2. If the backend api was changed update the static version string in `src/api/app/main.py` **predictor_version**.
3. Use `./build.cmd dockerbundle [--uionly]` to create new docker images
4. Use `./build.cmd dockertest [--uionly]` to run both images for testing.

   This might take some time as the api image will downloag ~7GB model.
   Use the `--uionly` option to only bundle/test the ui and not the api.
5. Use `./build.cmd dockerpublish`, after logging into the csbdocker account (docker login) to push both images to dockerhub
6. Use `./build.cmd dockertestproduction` to pull and test both images from remote as docker compose stack.

   Ui will be accessible on `localhost:5000` and api on `localhost:8000`

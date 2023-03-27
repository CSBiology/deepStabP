# deepStabP.Api

# Development

Info for future development and maintenance

## Install Dependencies

- `python:3.9`
- `pip install --no-cache-dir --upgrade --user -r requirements.txt` or in root `/build.cmd InstallApi`

## Run Locally

- `uvicorn app.main:app --reload`. Also run by `./build.cmd` together with the full stack.

Default port is `localhost:8000`

## Publish

`Dockerfile.Api` in ~/build is used to create a image from this api service.
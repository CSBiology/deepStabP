from typing import Union
from fastapi import FastAPI
from pydantic import BaseModel
from .routers import version1

tags_metadata = [
    {
        "name": "latest",
        "description": "Latest version of all accessible APIs.",
    },
    {
        "name": "v1",
        "description": "All v1 APIs.",
    },
]

predictor_version = "1.0.1"

app = FastAPI(
    openapi_tags=tags_metadata
)

# if reload on every request try this https://github.com/tiangolo/uvicorn-gunicorn-fastapi-docker/blob/master/docker-images/python3.9.dockerfile

app.include_router(version1.router)

@app.get("/")
def read_root():
    return {"Hello": "World"}

@app.get("/predictor_version")
def read_root():
    return {"Version": predictor_version}





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

## Supported formats

deepSTAPp expects input in either the FASTA format or as pure amino acid sequence. 
The FASTA format consists of two building blocks. The first is a description which explains the following sequence. This description starts with ">" and is written in a single line. The amino acid sequence follows in the next line and can span multiple lines. An example for this format is:
```
>sp|A0A178WF56|CSTM3_ARATH Protein CYSTEINE-RICH TRANSMEMBRANE MODULE 3 OS=Arabidopsis thaliana OX=3702 GN=CYSTM3 PE=1 SV=1
MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
MCCCCVLDCVF
```
deepSTABp was developed with the assumption that the description follows the standard used by the Universal Protein Resource ([Uniprot](https://www.uniprot.org/)) and only returns the Uniprot ID as description in the output table. This can be circumvented by removing the "|" in the description. In this case the complete description gets returned.

The only other supported format are pure amino acid sequences. An example for this format is:
```
MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
MCCCCVLDCVF
```
This format can only be used for a single amino acid sequence. Multiple amino acid sequences must be in the following format:
```
>MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
MCCCCVLDCVF
```

## Maximum length of a amino acid sequence

This repository was built for the deepSTABp webservice. To ensure that the webservice does not get stuck on a single request a maximum length for a single amino acid sequence is enforced. This length is 10 000 amino acids. We advise you to run the docker local if you need to parse such a long protein. 





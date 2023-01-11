from typing import Union
from fastapi import FastAPI
from pydantic import BaseModel
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import tqdm
import gc
import app.HelloWorld as test
import app.predictor as pred 

app = FastAPI()

# if reload on every request try this https://github.com/tiangolo/uvicorn-gunicorn-fastapi-docker/blob/master/docker-images/python3.9.dockerfile

class Item(BaseModel):
    name: str
    price: float
    is_offer: Union[bool, None] = None

# mirrored in dotnet Shared/DeepStabP.Types.fs
class PredictorInfo(BaseModel):
    growth_temp: int

@app.get("/")
def read_root():
    return {"Hello": "World"}

variable= '''>sp|A0A178VEK7|DUO1_ARATH Transcription factor DUO1 OS=Arabidopsis thaliana OX=3702 GN=DUO1 PE=1 SV=1
MRKMEAKKEEIKKGPWKAEEDEVLINHVKRYGPRDWSSIRSKGLLQRTGKSCRLRWVNKL
RPNLKNGCKFSADEERTVIELQSEFGNKWARIATYLPGRTDNDVKNFWSSRQKRLARILH
NSSDASSSSFNPKSSSSHRLKGKNVKPIRQSSQGFGLVEEEVTVSSSCSQMVPYSSDQVG
DEVLRLPDLGVKLEHQPFAFGTDLVLAEYSDSQNDANQQAISPFSPESRELLARLDDPFY
YDILGPADSSEPLFALPQPFFEPSPVPRRCRHVSKDEEADVFLDDFPADMFDQVDPIPSP
>sp|A0A178WF56|CSTM3_ARATH Protein CYSTEINE-RICH TRANSMEMBRANE MODULE 3 OS=Arabidopsis thaliana OX=3702 GN=CYSTM3 PE=1 SV=1
MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
MCCCCVLDCVF
'''

tokenizer = T5Tokenizer.from_pretrained("Rostlab/prot_t5_xl_uniref50", do_lower_case=False )
model = T5EncoderModel.from_pretrained("Rostlab/prot_t5_xl_uniref50")
gc.collect()
prediction_net = pred.LSMTNeuralNet.load_from_checkpoint ('trained_model/sampled_32_batchsize/checkpoints/epoch=46-step=42535.ckpt')

@app.post("/predict")
def predict(info: PredictorInfo):
    new_df = pred.create_dataframe (variable, 'lysate', info.growth_temp)
    prediction = pred.determine_tm (new_df, model, prediction_net, pred.new_features, tokenizer)
    return {"Prediction": prediction}




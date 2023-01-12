from fastapi import APIRouter
from typing import Union
from fastapi import FastAPI
from pydantic import BaseModel
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import *
import gc
from ..predictor import *

# mirrored in dotnet Shared/DeepStabP.Types.fs
class PredictorInfo(BaseModel):
    growth_temp: int

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

router = APIRouter(
    prefix="/api/v1",
    tags=["v1"]
)

tokenizer = T5Tokenizer.from_pretrained("Rostlab/prot_t5_xl_uniref50", do_lower_case=False )
model = T5EncoderModel.from_pretrained("Rostlab/prot_t5_xl_uniref50")
gc.collect()
prediction_net = LSMTNeuralNet.load_from_checkpoint ('trained_model/sampled_32_batchsize/checkpoints/epoch=46-step=42535.ckpt')

@router.post("/predict", tags=["latest"])
def predict(info: PredictorInfo):
    new_df = create_dataframe (variable, 'lysate', info.growth_temp)
    prediction = determine_tm (new_df, model, prediction_net, new_features, tokenizer)
    return {"Prediction": prediction}
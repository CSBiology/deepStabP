from typing import Union
import predcit_with_human_ptc_cpu_only
from fastapi import FastAPI
from pydantic import BaseModel
import predcit_with_human_ptc_cpu_only as pred
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import tqdm
import gc

app = FastAPI()

class Item(BaseModel):
    name: str
    price: float
    is_offer: Union[bool, None] = None

@app.get("/")
def read_root():
    return {"Hello": "World"}


@app.get("/items/{item_id}")
def read_item(item_id: int, q: Union[str, None] = None):
    return {"item_id": item_id, "q": q}

@app.put("/items/{item_id}")
def update_item(item_id: int, item: Item):
    return {"item_name": item.name, "item_id": item_id}

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
prediction_net = pred.LSMTNeuralNet.load_from_checkpoint ('../trained_model/sampled_32_batchsize/checkpoints/epoch=46-step=42535.ckpt')

new_df = pred.create_dataframe (variable, 'lysate', 22)
prediction = pred.determine_tm (new_df, model, prediction_net, pred.new_features, tokenizer)


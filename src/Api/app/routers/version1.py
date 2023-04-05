from fastapi import APIRouter
from pydantic import BaseModel
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import *
import gc
from ..predictor import *

# mirrored in dotnet Shared/DeepStabP.Types.fs
class FastaRecord(BaseModel):
    header      : str
    sequence    : str

# mirrored in dotnet Shared/DeepStabP.Types.fs
class PredictorInfo(BaseModel):
    growth_temp : int
    mt_mode     : str
    fasta       : list[FastaRecord]

router = APIRouter(
    prefix="/api/v1",
    tags=["v1"]
)

tokenizer = T5Tokenizer.from_pretrained("Rostlab/prot_t5_xl_uniref50", do_lower_case=False )
model = T5EncoderModel.from_pretrained("Rostlab/prot_t5_xl_uniref50")
gc.collect()
prediction_net = LSMTNeuralNet.load_from_checkpoint ("trained_model/b25_sampled_10k_tuned_2_d01/checkpoints/epoch=1-step=2316.ckpt")

@router.post("/predict", tags=["latest"])
def predict(info: PredictorInfo):
    new_df = create_dataframe (info.fasta, info.mt_mode, info.growth_temp)
    prediction = determine_tm (new_df, model, prediction_net, new_features, tokenizer)
    return {"Prediction": prediction}
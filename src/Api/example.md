# Dependencies

```py
# https://github.com/CSBiology/deepStabP/blob/main/src/Api/requirements.txt
from fastapi import APIRouter
from pydantic import BaseModel
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import *
import gc
from ..predictor import * # https://github.com/CSBiology/deepStabP/blob/main/src/Api/app/predictor.py
```

# Setup

```py
# mirrored in dotnet Shared/DeepStabP.Types.fs
class FastaRecord(BaseModel):
    header      : str
    sequence    : str

# mirrored in dotnet Shared/DeepStabP.Types.fs
class PredictorInfo(BaseModel):
    growth_temp : int
    mt_mode     : str # "Lysate" or "Cell"
    fasta       : list[FastaRecord]

tokenizer = T5Tokenizer.from_pretrained("Rostlab/prot_t5_xl_uniref50", do_lower_case=False )
model = T5EncoderModel.from_pretrained("Rostlab/prot_t5_xl_uniref50")
gc.collect()
# https://github.com/CSBiology/deepStabP/tree/main/src/Api/trained_model/b25_sampled_10k_tuned_2_d01
prediction_net = deepSTAPpMLP.load_from_checkpoint ("trained_model/b25_sampled_10k_tuned_2_d01/checkpoints/epoch=1-step=2316.ckpt")
```

# Run

Here you can find the fasta example strings used in the deepstabp frontend UI: https://github.com/CSBiology/deepStabP/blob/main/src/Client/View/Components/Examples.fs

```py
# The processing can be found here: https://github.com/CSBiology/deepStabP/blob/main/src/Server/FastaReader.fs
# Replace 'O', 'U', 'J', 'Z', 'B' with "X" (None in this example)
let SingleFastaMinimal = """>ExampleName
MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAAMCCCCVLDCVF"""

sequence = "MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAAMCCCCVLDCVF"

seq_transform = " ".join(sequence)

fasta_record_1 = FastaRecord(header="ExampleName", sequence="MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAAMCCCCVLDCVF")

predictor_info = PredictorInfo(
    growth_temp=37,
    mt_mode="Lysate",
    fasta=[fasta_record_1]
)

prediction = determine_tm (predictor_info.fasta, predictor_info.mt_mode, predictor_info.growth_temp, model, prediction_net, new_features, tokenizer)

```
import pandas as pd
from Bio import SeqIO
import numpy as np
import torch as torch
import torch.nn as nn
import torch.nn.functional as F
import pytorch_lightning as pl
from transformers import  T5EncoderModel, T5Tokenizer
from tqdm.auto import tqdm
import gc
from IPython.display import display
import time
from io import StringIO

#function to convert the fasta sequence into the embeddingns with a lenght of 1024 (the returned embedding is the mean of all amino acid embeddings of the sequence)
def new_features (features, model, tokenizer):
    device = torch.device('cpu')
    model = model.eval()
    ids = tokenizer.batch_encode_plus(features, add_special_tokens=True, padding=True)
    input_ids = torch.tensor(ids['input_ids']).to(device)
    attention_mask = torch.tensor(ids['attention_mask']).to(device)
    with torch.no_grad():
        embedding = model(input_ids=input_ids,attention_mask=attention_mask)
    embedding = embedding.last_hidden_state.cpu().numpy()
    new_feature = []
    for seq_num in range(len(embedding)):
        seq_len = (attention_mask[seq_num]==1).sum()
        seq_emd = embedding[seq_num][:seq_len-1]
        new_feature.append(seq_emd)
    new_feature = np.array (new_feature)
    out = torch.tensor (new_feature)
    out = out.mean(1)
    out = out.reshape(-1)
    return out
        
class LSMTNeuralNet (pl.LightningModule):
    #initialisation of parameters and layers
    def __init__(self, dropout, learning_rate, batch_size):
        super().__init__()
        #parameters for the folliwing functions
        self.learning_rate = learning_rate
        self.batch_size = batch_size
        self.dropout =dropout
        
        #layer of the neureal network
        self.zero_layer = nn.Linear (1044, 4098)
        self.zero_dropout = nn.Dropout1d (dropout)
        self.first_layer = nn.Linear (4098, 512)
        self.first_dropout = nn.Dropout1d (dropout)
        self.second_layer = nn.Linear (512, 256)
        self.second_dropout = nn.Dropout1d (dropout)
        
        self.third_layer = nn.Linear (256, 128)
        self.third_dropout = nn.Dropout1d (dropout)
        
        
        self.seventh_layer = nn.Linear (128, 1)
        
        self.species_layer_one = nn.Linear (1, 20)
        self.species_layer_two = nn.Linear (20, 1)
        self.species_dropout = nn.Dropout1d(dropout)
        self.output = nn.Linear (2,1)
        self.batch_norm0 = nn.LayerNorm (4098)
        self.batch_norm1 = nn.LayerNorm (512)
        self.batch_norm2 = nn.LayerNorm (256)
        self.batch_norm3 = nn.LayerNorm (128)

        self.lysate = nn.Linear (1, 10)
        self.lysate_dropout = nn.Dropout1d(dropout)
        self.cell = nn.Linear (1, 10)
        self.cell_dropout = nn.Dropout1d(dropout)
        

    #forward pass through the neural network    
    def forward (self, x, species_feature,lysate, cell):
        x = x.reshape (1,-1 ).float()
        species_feature = torch.tensor (species_feature, dtype=torch.float32)
        lysate = torch.tensor (lysate, dtype=torch.float32)
        cell = torch.tensor (cell, dtype=torch.float32)
        lysate = lysate.reshape (-1, 1)
        lysate = self.lysate_dropout(F.selu(self.lysate (lysate)))
        cell = cell.reshape (-1, 1)
        cell = self.cell_dropout(F.selu(self.cell (cell)))
        x = torch.cat ([lysate, cell,x], dim=1)
        x = self.zero_dropout (self.batch_norm0 (F.selu(self.zero_layer(x))))
        x = self.first_dropout(self.batch_norm1(F.selu(self.first_layer (x))))
        x = self.second_dropout(self.batch_norm2(F.selu(self.second_layer (x))))
        x = self.third_dropout(self.batch_norm3(F.selu(self.third_layer (x))))
        tm_est = self.seventh_layer (x)
        
        species_feature = species_feature.reshape (-1, 1)
        species_feature = self.species_dropout(F.selu(self.species_layer_one (species_feature)))
        species_feature = self.species_layer_two (species_feature)
        
        combined = torch.cat ([tm_est, species_feature], dim=1)
        
        tm = self.output (combined)
        return tm
#function to convert the fasta file, the lysate/cell property and the growth temperature into a single dataframe
def create_dataframe (fasta, lycell, growth):
    growth_temp = (growth-30.44167)/(97.4167-30.44167)
    if lycell == 'Lysate':
        lysate = 1
        cell = 0
    else:
        lysate = 0
        cell = 1
    with StringIO(fasta) as fastq_io:
        genome = SeqIO.parse (fastq_io, 'fasta')
        for x,fasta in enumerate (genome):
            name, sequence = fasta.id, str(fasta.seq)
            name = name.split('|')[1]
            sequence = sequence.replace ("O",'X').replace ("U",'X').replace( "J",'X').replace ("Z",'X').replace ("B",'X')
            sequence = list (sequence)
            sequence = ' '.join (sequence)
            sequence = [sequence]
            if x == 0:
                df_dict = {'Protein':name, 'feature':[sequence]}
                dataframe = pd.DataFrame (df_dict)
            else:
                df_dict = {'Protein':name, 'feature':[sequence]}
                new_df2 = pd.DataFrame (df_dict)
                dataframe = pd.concat ((dataframe, new_df2))
    dataframe['growth_feature'] = growth_temp
    dataframe['lysate'] = lysate
    dataframe['cell'] = cell
    return dataframe

#function to predict the melting temperature of the dataframe
def determine_tm (dataframe, transformer, tm_predicter, new_features, tokenizer):
    output = []
    protein = []
    for index, row in dataframe.iterrows():
        embedding = new_features(row['feature'], transformer, tokenizer)
        tm_predicter = tm_predicter.eval()
        tm_prediction = tm_predicter (embedding, row['growth_feature'],row['lysate'], row['cell'])
        output.append (tm_prediction.cpu().detach().numpy().tolist()[0][0])
        protein.append (row['Protein'])
    output_df = pd.DataFrame ({'Protein':protein, 'Tm':output})
    output_df['Tm'] = output_df['Tm']*(97.4166905791789-30.441673997070385)+30.441673997070385
    output_df = output_df.to_records(index=False)
    return output_df


    
    
    
import json
import pandas as pd
from Bio import SeqIO
import numpy as np
import torch as torch
import torch.nn as nn
import torch.nn.functional as F
import pytorch_lightning as pl
from io import StringIO
import sys
import re

#function to convert the fasta sequence into the embeddingns with a lenght of 1024 (the returned embedding is the mean of all amino acid embeddings of the sequence)
def new_features (fasta_record, model, tokenizer):
    device = torch.device('cpu')
    model = model.to(device)
    for x,fasta in enumerate(fasta_record):
        sequence = [fasta.sequence]
        ids = tokenizer.batch_encode_plus(sequence, add_special_tokens=True, padding=True)
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
        out = torch.from_numpy (new_feature)
        out = out.reshape(1, -1, 1024)
        out = out.mean(1)
        out = out.reshape(1,-1)
        if x ==0:
            new_out = out.clone()
        else:
            new_out = torch.cat ((new_out, out),axis=0)
    return new_out

class deepSTAPpMLP (pl.LightningModule):
    #initialisation of parameters and layers
    def __init__(self, dropout, learning_rate, batch_size):
        super().__init__()
        #parameters for the folliwing functions
        self.learning_rate = learning_rate
        self.batch_size = batch_size
        self.dropout =dropout
        #layer of the neureal network
        self.zero_layer = nn.Linear (1064, 4098)
        self.zero_dropout = nn.Dropout1d (dropout)
        self.first_layer = nn.Linear (4098, 512)
        self.first_dropout = nn.Dropout1d (dropout)
        self.second_layer = nn.Linear (512, 256)
        self.second_dropout = nn.Dropout1d (dropout)
        self.third_layer = nn.Linear (256, 128)
        self.third_dropout = nn.Dropout1d (dropout)
        self.seventh_layer = nn.Linear (128, 1)
        self.species_layer_one = nn.Linear (1, 20)
        self.species_layer_two = nn.Linear (20, 20)
        self.species_dropout = nn.Dropout1d(dropout)
        self.batch_norm0 = nn.LayerNorm (4098)
        self.batch_norm1 = nn.LayerNorm (512)
        self.batch_norm2 = nn.LayerNorm (256)
        self.batch_norm3 = nn.LayerNorm (128)
        self.lysate = nn.Linear (1, 20)
        self.lysate2 = nn.Linear (20, 10)
        self.lysate_dropout = nn.Dropout1d(dropout)
        self.cell = nn.Linear (1, 20)
        self.cell2 = nn.Linear (20, 10)
        self.cell_dropout = nn.Dropout1d(dropout)
    #forward pass through the neural network 
    def forward (self, x, species_feature,lysate, cell):
        x = x.float()
        species_feature = species_feature.float().reshape (-1, 1)
        lysate = lysate.float().reshape (-1, 1)
        cell = cell.float().reshape (-1, 1)
        lysate = self.lysate_dropout(F.selu(self.lysate (lysate)))
        lysate = self.lysate_dropout(F.selu(self.lysate2 (lysate)))
        cell = self.cell_dropout(F.selu(self.cell (cell)))
        cell = self.cell_dropout(F.selu(self.cell2 (cell)))
        species_feature = self.species_dropout(F.selu(self.species_layer_one (species_feature)))
        species_feature = self.species_dropout(F.selu(self.species_layer_two (species_feature)))
        x = torch.cat ([lysate, cell,x,species_feature], dim=1)
        x = self.zero_dropout (self.batch_norm0 (F.selu(self.zero_layer(x))))
        x = self.first_dropout(self.batch_norm1(F.selu(self.first_layer (x))))
        x = self.second_dropout(self.batch_norm2(F.selu(self.second_layer (x))))
        x = self.third_dropout(self.batch_norm3(F.selu(self.third_layer (x))))
        tm = self.seventh_layer (x)
        return tm

def determine_tm (fasta, lysate, species, transformer, tm_predicter, new_features, tokenizer):
    length = len(fasta)
    transformer = transformer.eval()
    tm_predicter = tm_predicter.eval()
    embedding = new_features(fasta, transformer, tokenizer)
    species = (species-30.44167)/(97.4167-30.44167)
    species_list = torch.from_numpy(np.array([species]*length))
    if lysate == 'Lysate':
        lysate = torch.from_numpy(np.array([1]*length))
        cell = torch.from_numpy(np.array([0]*length))
    else:
        lysate = torch.from_numpy(np.array([0]*length))
        cell = torch.from_numpy(np.array([1]*length))
    tm_prediction = tm_predicter (embedding, species_list, lysate, cell)
    tm_prediction = tm_prediction.flatten()
    tm_prediction = tm_prediction.tolist()
    output_df = pd.DataFrame (list(zip((fasta_record.header for fasta_record in fasta),tm_prediction)))
    columns = ['Protein','Tm']
    output_df.columns = columns
    output_df['Protein'] = output_df['Protein'].str.replace(r"[A-Za-z][A-Za-z]\|",'')
    output_df['Protein'] = output_df['Protein'].str.replace(r"\|.*",'')
    output_df['Protein'] = output_df['Protein'].fillna('No valid name', inplace=False) 
    output_df['Tm'] = output_df['Tm']*(97.4166905791789-30.441673997070385)+30.441673997070385
    return output_df


    
    
    
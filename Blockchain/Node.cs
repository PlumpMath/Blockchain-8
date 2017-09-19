﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain
{
	// Nodes keep a temporary ledger of the latest block before it is added to the blockchain
	// Nodes receive the latest blocks and save a copy in the chain folder
	// Nodes can be queried for transaction information, including temporary ledger

	class Node
	{
		public Block currentBlock;
		public List<Block> chain = new List<Block>();
		public Ledger newLedger = new Ledger();
		public List<Accounts> accounts = new List<Accounts>();
		public int blockHeight;

		public Node()
		{
			createGenesisBlock();
			currentBlock = createGenesisBlock();
			chain.Add(currentBlock);
		}

		private Block createGenesisBlock()
		{
			// The Following lines show how the Genesis block was created originally:
			//Ledger genesisData = new Ledger();
			//genesisData.addTransaction(new Transaction("SteedyBucks", "Steedy", 1000000.0f));
			//Block genesis = new Block();
			//genesis.newBlock(0, DateTime.Now, genesisData, "0");
			//return genesis;

			// Reads the hard coded genesis block from JSON file
			return Serialize.ReadBlock("0000000000");
		}

		public Tuple<string,string,string> queryBlockInfo(int index)
		{
			string data = chain.ElementAt(index).GetData().getString();
			string timestamp = chain.ElementAt(index).getTimestamp().ToUniversalTime().ToString();
			string previousHash = chain.ElementAt(index).getPreviousHash();
			return new Tuple<string, string, string>(data, timestamp, previousHash);
		}

		public static double getBalance(string pubkey)
		{
			double balance = 0.0;
			string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\");
			int count = 0;

			foreach (string file in files)
			{
				string index = count.ToString("0000000000");
				Block b = Serialize.ReadBlock(index);
				Ledger l = b.GetData();
				balance += l.getBalance(pubkey);
				count++;
			}

			return balance;
		}

		public double getUnconfirmedBalance(string pubkey)
		{
			return currentBlock.data.getBalance(pubkey);
		}

		public Tuple<bool, string> SendTransaction(string sender, string recipient, double amount, string txid)
		{
			Transaction t = new Transaction(sender, recipient, amount, txid);
			Tuple<bool, string> result = verifyTransaction(t);

			if (result.Item1)
			{				
				newLedger.addTransaction(t);
				// TODO: Transmit to network
				return result;
			}
			else
			{
				return result;
			}
		}

		public static Tuple<bool, string> verifyTransaction(Transaction t)
		{
            // check signature via asymmetric signature verification
            if(Keys.VerifyData((t.sender + t.recipient + t.amount.ToString()), t.txid, t.sender))
			{
				if (getBalance(t.sender) >= t.amount)
				{
					return new Tuple<bool,string>(true, "Transaction ID " + t.txid + " successful verified.");
				}
				else
				{
					return new Tuple<bool, string>(false, "Transaction failed. Insufficient balance.");
				}
			}
			else
			{
				return new Tuple<bool, string>(false, "Transaction failed. Invalid TXID.");
			}
            

            
		}

		public void verifyTransactions(Transaction t)
		{
			if (getBalance(t.sender) >= t.amount)
			{
				// Transaction OK
			}
			else
			{
				// Delete transaction
				currentBlock.data.removeTransaction(t);
			}
		}	

	}
}

using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace ReadVault;

internal class TacoVaultCheck
{
    static async Task Main(string[] args)
    {
        //set the staking contract address
        const string tacoStakingContract = "0xd1b51678f0De7F9EEC1d6e68F0234875F6eBCbBB";
        
        //max token id for taco tribe, but better to set the current max since we have to check each one
        //if you really wanted you could read if from the TacoTribe contract, but easy enough just to hard code it before you run 
        int maxTacoId = 2136; //8226; 

        //a map that will contain all the addresses and what tokens they are staking
        var addressToTokenListMap = new Dictionary<string, List<int>>();

        //generate a private key
        //you don't really need a key to use read functions but the web3 object needs a key so just make a new one
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();

        //use the official RPC (or change to a private one)
        var rpc = "https://polygon-rpc.com";
        //make an account object from the private key and polygon chain
        var account = new Account(privateKey, 137);
        //make a web 3 instance from the account and the rpc
        var web3Poly = new Web3(account, rpc);

        //create a contract object from the vault abi and address (we only need the vault function)
        var contract = web3Poly.Eth.GetContract(ABI.vault, tacoStakingContract);
        //get the function from the contract
        var readVault = contract.GetFunction("vault");
        
        try
        {
            //loop through every token id
            for (var i = 0; i <= maxTacoId; i++)
            {
                Console.WriteLine($"Check Token {i}");
                
                //query the contract and let it decode with default params
                //(we could make a class if we really wanted, but its overkill for this)
                //use some error handling because it sometimes fails to read - why?
                List<ParameterOutput>? vaultResponse;
                try
                {
                    vaultResponse = await readVault.CallDecodingToDefaultAsync(i);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to Check Token {i}: {e.Message}");
                    i--; //keep the same token id to try again
                    continue;
                }
                
                if (vaultResponse.Count < 2) //the response was not full, bad read - try again?
                {
                    i--; //keep the same token id to try again
                    continue;
                }

                //the address is the last object
                var address = vaultResponse[2].Result.ToString();

                switch (address)
                {
                    case null: //shouldn't be null at this point but make sure
                        i--;  //keep the same token id to try again
                        continue;
                    
                    //if the address is zeros no one staked it yet just check the next token
                    case "0x0000000000000000000000000000000000000000":
                        continue;
                }

                if (!addressToTokenListMap.ContainsKey(address)) //if the address doesn't exist in the map yet
                {
                    //add it to our mapping
                    Console.WriteLine($"New holder address: {address} token: {i}");
                    addressToTokenListMap.Add(address, new List<int>() {i});
                }
                else //its already existing in out mapping
                {
                    //add the token to the list for this address
                    Console.WriteLine($"New token for address: {address} token: {i}");
                    addressToTokenListMap[address].Add(i);
                }
            }

            //export the reports from our mapping
            //serialize the dictionary
            var idsByAddresses = JsonConvert.SerializeObject(addressToTokenListMap, Formatting.Indented);
            //write the file
            Console.WriteLine("Write file tokenIdsByAddress.txt!");
            await File.WriteAllTextAsync(@"tokenIdsByAddress.txt", idsByAddresses);

            //maybe its better just to have address and count
            //remap the dictionary to just address and count
            var holderCounts = addressToTokenListMap
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            //serialize the dictionary
            var countsByAddresses = JsonConvert.SerializeObject(holderCounts, Formatting.Indented);
            //write the file
            Console.WriteLine("Write file tokenCountsByAddress.txt!");
            await File.WriteAllTextAsync(@"tokenCountsByAddress.txt", countsByAddresses);

            Console.WriteLine("Complete!");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


    }

}
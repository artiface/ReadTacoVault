namespace ReadVault
{
    internal class ABI
    {
        //we only ned the ABI for the vault function since its all we are using
        internal const string vault = @"
        [{
            inputs: [
                {
                    internalType: 'uint256',
                    name: '',
                    type: 'uint256'
                }
            ],
            name: 'vault',
            outputs: [
                {
                    internalType: 'uint24',
                    name: 'tokenId',
                    type: 'uint24'
                },
                {
                    internalType: 'uint48',
                    name: 'timestamp',
                    type: 'uint48'
                },
                {
                    internalType: 'address',
                    name: 'owner',
                    type: 'address'
                }
            ],
            stateMutability: 'view',
            type: 'function'
        }]";

    }
}

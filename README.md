# Coinbase Recurring Buy Function

An Azure Function that automates recurring cryptocurrency purchases on Coinbase using their v3 API. The function supports both scheduled execution and manual triggering via HTTP endpoint.

## Features

- Automated cryptocurrency purchases using Coinbase v3 API
- Configurable allocations stored in Azure Blob Storage
- Timer-based execution with configurable schedule
- HTTP endpoint for manual execution
- JWT authentication with EC private keys
- Detailed logging and error handling

## Configuration

### Allocations File (allocations.json)

The allocations file defines which cryptocurrencies to buy and how much USDC to spend on each. Upload this file to your Azure Blob Storage container.

Fields:
- `symbol`: The cryptocurrency trading symbol (e.g., BTC, ETH, SOL)
- `usdcAmount`: Amount of USDC to spend on each purchase
- `isActive`: Whether this allocation should be executed (true/false)

### Application Settings

Configure the following in your `local.settings.json` or Azure Function App settings:

## Setup Instructions

1. Create Azure Storage Account
2. Create a container named `autobuy-config`
3. Upload `allocations.json` to the container
4. Configure application settings
5. Deploy the function

## Local Development

1. Clone the repository
2. Create `local.settings.json` with your configuration
3. Run the Azure Storage Emulator
4. Upload allocations.json to local storage
5. Start the function:

```bash:README.md
func start
```

## Endpoints

### Timer Trigger (CoinbaseRecurringBuyTimer)
- Executes automatically based on CRON expression in settings
- Default schedule: Every hour

### HTTP Trigger (CoinbaseRecurringBuyHttp)
- URL: `/api/CoinbaseRecurringBuyHttp`
- Methods: GET, POST
- Authorization: Function key required
- Returns: JSON array of operation results

## Managing Allocations

You can manage your cryptocurrency purchases by:
1. Modifying the allocations.json file
2. Uploading the updated file to blob storage
3. The function will use the new allocations on its next execution

To disable a purchase:
- Set `isActive` to `false` for that cryptocurrency
- The function will skip inactive allocations

To add a new cryptocurrency:
- Add a new entry to the allocations array
- Set `symbol`, `usdcAmount`, and `isActive`

## Error Handling

The function handles various scenarios:
- Invalid API credentials
- Insufficient funds
- Network issues
- Invalid allocations configuration
- Storage access issues

All errors are logged and can be monitored through Azure Function logs.

## Security Considerations

- Store API credentials securely in Azure Key Vault
- Use Managed Identity for Azure services access
- Enable HTTPS-only access
- Configure appropriate function access levels
- Monitor function execution logs

## License

MIT License
# Funcify: Secure Image Processing with Azure Functions

Funcify is a powerful Azure Functions-based solution that processes images securely and efficiently using Serverless Functions, Azure Blob Storage and Queue Storage. This project demonstrates the creation, deployment, and automation of two serverless functions to upload images, and enqueue tasks for further image processing.

## Overview
This project covers the following:
- **Building an Azure Function** for image processing.
- **Storing images securely** in Azure Blob Storage.
- **Enqueuing tasks** for further processing in Azure Queue Storage.
- **Utilizing Managed Identity** for secure access to resources.
- **Automating deployment** with GitHub Actions.

## Resources
### Services and Tools
- Azure Blob Storage
- Azure Queue Storage
- Azure Function App
- Managed Identity
- GitHub Actions
- Visual Studio Code

### Azure CLI Commands
#### Set Up Azure Resources
```bash
# Create a resource group
az group create --name FuncifyResourceGroup --location "Uk West"

# Create a storage account
az storage account create --name funcifystorage --resource-group FuncifyResourceGroup --location "Uk West" --sku Standard_LRS

# Create a queue
az storage queue create --name image-processing-queue --account-name funcifystorage

# Create a function app
az functionapp create --name FuncifyApp --resource-group FuncifyResourceGroup --storage-account funcifystorage --runtime dotnet --consumption-plan-location "Uk West"
```

#### Assign Managed Identity and RBAC
```bash
# Assign Managed Identity to the Function App
az functionapp identity assign --name FuncifyApp --resource-group FuncifyResourceGroup

# Get the Managed Identity principal ID
principal_id=$(az functionapp identity show --name FuncifyApp --resource-group FuncifyResourceGroup --query principalId --output tsv)

# Assign Storage Blob Data Contributor role
az role assignment create --assignee $principal_id --role "Storage Blob Data Contributor" --scope $(az storage account show --name funcifystorage --query id --output tsv)

# Assign Storage Queue Data Contributor role
az role assignment create --assignee $principal_id --role "Storage Queue Data Contributor" --scope $(az storage account show --name funcifystorage --query id --output tsv)
```

#### Test Locally and Deploy
- [Install Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=macos%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)
```bash
# Start the function locally
func start

# Deploy the function to Azure
func azure functionapp publish FuncifyApp
```

## Features
1. **Blob Storage Integration**: Images are uploaded to a secure container.
2. **Queue Storage Integration**: Tasks are enqueued for processing using the image's metadata.
3. **Secure Access with Managed Identity**: Securely access Blob and Queue Storage without secrets.
4. **Automated Deployment**: CI/CD pipeline implemented with GitHub Actions.

## Project Structure
```
Funcify/
├── Actions/
│   ├── CreateProduct.cs
│   ├── UploadImage.cs
│   ├── UpdateProduct.cs
│   └── EnqueueTask.cs
├── Contracts/
│   └── Services/
│       ├── IBlobService.cs
│       ├── ICosmosDBService.cs
│       └── IQueueService.cs
├── Services/
│   ├── BlobService.cs
│   ├── CosmosDBService.cs
│   └── QueueService.cs
├── Program.cs
├── host.json
├── .github/
│   └── workflows/
│       └── deploy.yml
└── README.md
```

## Videos
- [Azure Function Walkthrough: Secure Image Processing](https://youtu.be/L8nUEITFhWk)
- [Build, Deploy, and Automate Deployment of Your First Serverless Function](https://youtu.be/zzfSpmhi6tE)

## How It Works
### Step 1: Process Form Data and JSON
The function processes incoming HTTP requests in either JSON or multipart form-data format, validating the image file.

### Step 2: Upload to Blob Storage
The unprocessed image is uploaded to the `unprocessed-images` container.

### Step 3: Create a Product
A product object is created with details such as the image URL.

### Step 4: Enqueue a Task
A task containing the product ID and unprocessed image URL is enqueued for further processing which will be covered later.

### Step 5: Automation and Deployment
Deployment is automated using GitHub Actions, ensuring efficient CI/CD.

## Documentation and Blog
- [Setting Up Managed Identity for Azure Functions](https://freemancodz.hashnode.dev/storing-secrets-in-azure-key-vault-and-accessing-with-managed-identity)

## Connect with Me
- **Instagram & Twitter**: [@Freemancodz](https://twitter.com/Freemancodz)
- **LinkedIn**: [Freeman Madudili](https://www.linkedin.com/in/freeman-madudili-9864101a2/)

## Tags
`Azure Functions`, `Blob Storage`, `Queue Storage`, `Managed Identity`, `Serverless`, `Cloud Computing`, `CI/CD`, `GitHub Actions`

# Funcify: Secure Image Processing with Azure Functions

Funcify is an Azure Functions-based serverless application for securely processing images using Blob Storage, Queue Storage, and Managed Identity. This project demonstrates how to build, deploy, and automate serverless functions for image processing and task management, with secure access to Azure resources.

## Overview
Funcify enables:
- **Secure storage** of images in Azure Blob Storage.
- **Task queuing** for asynchronous image processing using Azure Queue Storage.
- **Image resizing and processing** using Azure Functions.
- **Role-Based Access Control (RBAC)** with Managed Identity.
- **Automated deployment** using GitHub Actions.

---

## Features
### 1. **Blob Storage Integration**
   - Unprocessed images are stored in the `unprocessed-images` folder.
   - Processed images are stored in the `processed-images` folder.

### 2. **Queue Storage Integration**
   - Tasks for image processing are enqueued with metadata including the product ID and image URL.

### 3. **Managed Identity**
   - Provides secure access to Blob Storage and Queue Storage without exposing secrets.

### 4. **Automated CI/CD**
   - Deployments are automated with GitHub Actions for seamless updates.

### 5. **Image Processing**
   - Resizes and processes uploaded images, ensuring scalability and security.

---

## Project Structure
```
freeman-md-funcify/
├── README.md
├── Funcify.sln
├── Funcify/
│   ├── ProductProcessingFunction.cs
│   ├── ResizeImageFunction.cs
│   ├── Actions/
│   │   ├── CreateProduct.cs
│   │   ├── EnqueueTask.cs
│   │   ├── ImageResize.cs
│   │   ├── UpdateProduct.cs
│   │   └── UploadImage.cs
│   ├── Contracts/
│   │   └── Services/
│   │       ├── IBlobService.cs
│   │       ├── ICosmosDBService.cs
│   │       └── IQueueService.cs
│   ├── Extensions/
│   │   └── AzureServicesExtension.cs
│   ├── Models/
│   │   ├── Product.cs
│   │   ├── ProductImageProcessingMessage.cs
│   │   └── Task.cs
│   └── Services/
│       ├── BlobService.cs
│       ├── CosmosDBService.cs
│       └── QueueService.cs
├── Funcify.Tests/
│   ├── Actions/
│   │   ├── CreateProductTests.cs
│   │   ├── EnqueueTaskTests.cs
│   │   ├── ImageResizeTests.cs
│   │   ├── UpdateProductTests.cs
│   │   └── UploadImageTests.cs
│   ├── Builders/
│   │   └── ProductBuilder.cs
│   ├── Functions/
│   │   └── ProductProcessingFunctionTests.cs
│   └── Services/
│       ├── BlobServiceTests.cs
│       ├── CosmosDBServiceTests.cs
│       └── QueueServiceTests.cs
└── .github/
    └── workflows/
        └── deploy.yml
```

---

## How It Works
### Step 1: Process Form Data or JSON
- The **ProductProcessingFunction** processes incoming requests (JSON or form-data) and validates image files.

### Step 2: Upload to Blob Storage
- Images are uploaded to the `unprocessed-images` folder in Blob Storage.

### Step 3: Enqueue Image Processing Task
- A task containing the product ID and unprocessed image URL is added to the **Queue Storage**.

### Step 4: Process Queue and Resize Image
- The **ResizeImageFunction** listens for tasks in the queue, downloads the unprocessed image, resizes it, and uploads it to the `processed-images` folder.

### Step 5: Automate Deployment
- CI/CD pipeline in GitHub Actions automates deployments for efficient updates.

---

## Setting Up Resources
### Required Azure Services
1. **Azure Blob Storage**
2. **Azure Queue Storage**
3. **Azure Function App**
4. **Managed Identity** for secure access

### Azure CLI Commands
#### Create Azure Resources
```bash
# Create a resource group
az group create --name FuncifyResourceGroup --location "UK West"

# Create a storage account
az storage account create --name funcifystorage --resource-group FuncifyResourceGroup --location "UK West" --sku Standard_LRS

# Create a queue
az storage queue create --name image-processing-queue --account-name funcifystorage

# Create a function app
az functionapp create --name FuncifyApp --resource-group FuncifyResourceGroup --storage-account funcifystorage --runtime dotnet --consumption-plan-location "UK West"
```

#### Assign Managed Identity
```bash
# Assign Managed Identity to the Function App
az functionapp identity assign --name FuncifyApp --resource-group FuncifyResourceGroup

# Assign roles for Blob and Queue Storage
principal_id=$(az functionapp identity show --name FuncifyApp --resource-group FuncifyResourceGroup --query principalId --output tsv)
az role assignment create --assignee $principal_id --role "Storage Blob Data Contributor" --scope $(az storage account show --name funcifystorage --query id --output tsv)
az role assignment create --assignee $principal_id --role "Storage Queue Data Contributor" --scope $(az storage account show --name funcifystorage --query id --output tsv)
```

#### Deploy Function
```bash
# Test locally
func start

# Deploy to Azure
func azure functionapp publish FuncifyApp
```

---

## Videos
1. **[Azure Function Walkthrough: Secure Image Processing](https://youtu.be/L8nUEITFhWk)**
2. **[Build, Deploy, and Automate Deployment of Your First Serverless Function](https://youtu.be/zzfSpmhi6tE)**

---

## Documentation
- **[Setting Up Managed Identity for Azure Functions](https://freemancodz.hashnode.dev/storing-secrets-in-azure-key-vault-and-accessing-with-managed-identity)**

---

## Connect with Me
- **Instagram & Twitter**: [@Freemancodz](https://twitter.com/Freemancodz)
- **LinkedIn**: [Freeman Madudili](https://www.linkedin.com/in/freeman-madudili-9864101a2/)
- **GitHub**: [Project Repository](https://github.com/Freeman-md/Funcify)

---

## Tags
`Azure Functions`, `Blob Storage`, `Queue Storage`, `Managed Identity`, `Serverless`, `Cloud Computing`, `CI/CD`, `GitHub Actions`

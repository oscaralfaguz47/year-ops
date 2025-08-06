#### Ripple By Oceans

###### \## Architecture Overview



The solution is composed of 5 independent but integrated projects:



###### \### 1. \*\*AzureFunctionsApp\*\*

Handles background email processing using Azure Functions.

\- SMTP/email logic

\- Integrated with Azure Key Vault

\- Triggered by background queues or logic



###### \### 2. \*\*OceansApp.DataAccess\*\*

Responsible for data layer abstraction and database operations.

\- Entity Framework Core (DbContext)

\- Repositories \& Unit of Work

\- Migrations \& Initializer

\- SQL Server



###### \### 3. \*\*OceansApp.Models\*\*

Holds all domain models shared across the solution.

\- ViewModels

\- DTOs

\- Domain entities



###### \### 4. \*\*OceansApp.Utility\*\*

Contains shared logic and helpers.

\- Constant definitions

\- Lazy loading and shared methods

\- Email notification templates

\- Authorization policies



###### \### 5. \*\*OceansAppWeb\*\*

The main web project (frontend and backend).

\- Razor Pages \& Layouts

\- JavaScript (Vanilla)

\- Controllers, Views, Components

\- Full UI \& UX logic

\- User authentication and role-based access



---



###### \## Technologies Used



\- ASP.NET Core 8.0.0

\- Razor Pages

\- JavaScript (Vanilla)

\- SQL Server

\- Entity Framework Core

\- Azure Functions

\- Azure Key Vault

\- GitHub Actions (CI/CD)



###### **## Environment Variables**



**These variables are required to run the application and Azure Functions securely.**



**| Variable Name                  | Description |**

**|--------------------------------|-------------|**

**| `AzureWebJobsStorage`          | Azure Storage connection string for Function App triggers. |**

**| `BonuslyApiKey`                | API key for integrating with Bonusly (if used). |**

**| `DefaultConnection`            | Main SQL Server connection string. |**

**| `FilesStorageAccountENV`       | Azure Storage account name for file handling. |**

**| `FileStorageAccountKeyENV`     | Azure Storage account key. |**

**| `TwoFactorAppNameENV`          | Application name displayed in authenticator apps for 2FA. |**






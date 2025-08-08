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
**| `MasterUserEmailENV`           | The email for the Master user created by default (choose your email). |**
**| `MasterUserPassENV`            | The password for the Master user created by default (The password must contain at least 8 characters, a lowercase letter, an uppercase letter, a number, and a special symbol). |**


## Pull Request & Commit Guidelines

This repository follows a structured Git workflow to ensure code quality, control, and safe collaboration.

### Branch Permissions

| Branch | Who can commit directly? | Description |
|--------|--------------------------|-------------|
| `main` | **Administrators only**  | Protected branch. Only approved pull requests can be merged by admins. |
| `demo` | All collaborators        | Open branch for testing and staging. |

---

### How to contribute as a collaborator

1. Create a new feature branch from `demo`.
2. Commit your changes locally with clear messages.
3. Push your branch to the remote repository.
4. Open a Pull Request.
5. Wait for an administrator to review and approve your PR.
6. The administrator will approve your PR and merge it to main.



# Legal Document Management System

This project is a web application designed to streamline the management and approval workflow of legal documents. It features a .NET Core backend API and a React frontend.

## Features

- User registration and authentication (JWT-based).
- Role-based access control (Admin, Uploader, LegalApprover, ManagerApprover, FinalApprover).
- Secure document uploading.
- Multi-step document approval workflows.
- Ability to view document status and history.
- Approve or reject documents at each stage of the workflow.

## Tech Stack

### Backend

- **.NET 8.0 (C#)**: Framework for building the API.
- **ASP.NET Core Identity**: For user authentication and authorization.
- **Entity Framework Core**: For data access and interaction with the database.
- **JWT (JSON Web Tokens)**: For secure token-based authentication.
- **Database**: (Likely SQL Server by default with .NET templates. Update if you are using PostgreSQL, SQLite, etc. Configuration is in `appsettings.json`)

### Frontend

- **React (JavaScript/JSX)**: Library for building the user interface.
- **Vite**: Build tool and development server.
- **Tailwind CSS (v3)**: Utility-first CSS framework for styling. (Ensuring stable v3 for robust styling)
- **Axios**: For making HTTP requests to the backend API.
- **React Router DOM**: For client-side routing.

## API Endpoints

The backend exposes the following RESTful API endpoints:

### Account Management (`/api/Account`)

- `POST /register`
  - **Description**: Registers a new user. New users are assigned the "Uploader" role by default.
  - **Request Body**: `{ "email": "user@example.com", "password": "Password123!" }`
  - **Response**: Success or error message.
- `POST /login`
  - **Description**: Logs in an existing user.
  - **Request Body**: `{ "email": "user@example.com", "password": "Password123!" }`
  - **Response**: JWT token and user information on success, or error message.

### Document Management (`/api/documents`)

_All endpoints require authentication._

- `POST /upload`
  - **Description**: Uploads a new document. Requires "Uploader" or "Admin" role.
  - **Request Body**: `FormData` containing the file and optional description.
  - **Response**: Details of the uploaded document or error message.
- `GET /`
  - **Description**: Retrieves a list of all documents.
  - **Response**: Array of document information objects.
- `GET /{id}`
  - **Description**: Retrieves details of a specific document by its ID.
  - **Response**: Document information object or 404 if not found.
- `POST /{id}/approve`
  - **Description**: Approves a document at its current approval step. Requires appropriate approver role for the current step (e.g., "LegalApprover" for legal step).
  - **Response**: Success or error message.
- `POST /{id}/reject`
  - **Description**: Rejects a document at its current approval step. Requires appropriate approver role for the current step.
  - **Request Body (optional)**: `{ "comments": "Reason for rejection" }`
  - **Response**: Success or error message.

## Getting Started

### Prerequisites

- **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js**: Latest LTS version (e.g., v22.x or v20.x) recommended. [Download here](https://nodejs.org/)
  - npm is included with Node.js.
- **Git**: For cloning the repository. [Download here](https://git-scm.com/downloads)
- A relational database compatible with Entity Framework Core (e.g., SQL Server, PostgreSQL, SQLite).
  - SQL Server Express LocalDB is often used for development with .NET on Windows.

### Backend Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/ajlorans/document-management.git
    cd document-management/src/backend
    ```
2.  **Configure Database Connection:**
    - Open `appsettings.Development.json` (and `appsettings.json` for production builds).
    - Update the `ConnectionStrings` section with your database connection details. The default often targets SQL Server LocalDB:
      ```json
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LegalDocManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
      ```
    - You can also configure the Admin User credentials here under `AppSettings`:
      ```json
      "AppSettings": {
        "AdminUserEmail": "admin@yourdomain.com",
        "AdminUserPassword": "YourSecurePassword!"
      }
      ```
3.  **Apply Migrations & Seed Data:**
    This will create the database schema and seed initial roles and the admin user.
    ```bash
    dotnet ef database update
    ```
    _(Note: If you haven't installed `dotnet-ef` tools yet, run `dotnet tool install --global dotnet-ef` first)._
    The application will also attempt to seed data on startup if roles/admin are missing.
4.  **Run the Backend:**
    ```bash
    dotnet run
    ```
    The API will typically be available at `https://localhost:7260` or a similar port (check console output).

### Frontend Setup

1.  **Navigate to the client directory:**
    ```bash
    # Assuming you are in the root of the repository
    cd src/frontend/client
    ```
2.  **Install Dependencies:**
    ```bash
    npm install
    ```
3.  **Configure API Base URL:**
    - The backend API URL is configured in `src/frontend/client/src/services/api.js`. Look for the `API_BASE_URL` constant.
    - Example: `const API_BASE_URL = "http://localhost:5147/api";` (Update port and protocol as per your backend).
    - _(Alternatively, for more flexible configuration, you can use a `.env` file by uncommenting the `VITE_API_BASE_URL` line in `api.js` and creating a `.env` file in `src/frontend/client/` with `VITE_API_BASE_URL=http://your-backend-url/api`)_
4.  **Run the Frontend:**
    ```bash
    npm run dev
    ```
    The React application will typically be available at `http://localhost:5173`.

## Usage

1.  Ensure both backend and frontend applications are running.
2.  Open your browser and navigate to the frontend URL (e.g., `http://localhost:5173`).
3.  **UI Development**: The user interface is actively being enhanced for a more visually appealing experience using Tailwind CSS.
4.  **Admin Access**: Log in with the admin credentials configured in `appsettings.Development.json` (default: `admin@example.com` / `AdminPa$$w0rd` if not changed).
5.  **User Registration**: New users can register through the UI. They are automatically assigned the "Uploader" role.
6.  **Document Upload**: Logged-in users with the "Uploader" or "Admin" role can upload documents via the dashboard.
7.  **Approval Workflow**:
    - Uploaded documents enter a workflow based on predefined `ApprovalSteps` (Legal, Manager, Final).
    - The system expects roles: "LegalApprover", "ManagerApprover", "FinalApprover". Users need to be manually assigned these roles by an Admin through the database or a future user management interface to participate in these specific approval stages.
    - Users with the appropriate role for the document's current step can approve or reject it.

## Workflow Overview

1.  **Document Upload**: An "Uploader" or "Admin" submits a document.
2.  **Initial Status**: The document enters the workflow with an initial status based on the first approval step defined (e.g., "AwaitingLegal" if LegalApprover is the first step).
3.  **Sequential Approval**:
    - The document moves through predefined `ApprovalSteps` (e.g., Legal, Manager, Final as defined in `SeedData.cs` and potentially configurable in the DB).
    - Users with the `RequiredRole` for the current active step (e.g., "LegalApprover", "ManagerApprover", "FinalApprover") can **approve** it.
    - If approved, the document moves to the next step or its status becomes "Approved" if it's the final step.
    - If **rejected**, the document status becomes "Rejected," and the workflow for that document typically ends there unless a resubmission process is part of further development.

## Contributing

We welcome contributions! Please follow these guidelines:

1.  **Fork the repository.**
2.  **Create a new branch** for your feature or bug fix: `git checkout -b feature/your-feature-name` or `bugfix/your-bug-fix`.
3.  **Make your changes.** Adhere to the existing code style.
    - For .NET, follow common C# coding conventions.
    - For React/JS, follow common JavaScript/React best practices. Consider using the included ESLint configuration.
4.  **Test your changes thoroughly.**
5.  **Commit your changes** with a clear and descriptive commit message: `git commit -m "feat: Implement X feature"` or `fix: Resolve Y bug`.
6.  **Push to your forked repository:** `git push origin feature/your-feature-name`.
7.  **Open a Pull Request** to the `main` branch of the original repository.
    - Provide a clear title and description for your PR.
    - Link any relevant issues.

## License

This project is currently unlicensed.

Consider adding an open-source license like the [MIT License](https://opensource.org/licenses/MIT). To do so, create a `LICENSE.md` file in the root of your project and paste the MIT license text into it, replacing `[year]` and `[fullname]` with your details.

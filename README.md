# Legal Document Management System

This project is a web application designed to streamline the management and approval workflow of legal documents. It features a .NET Core backend API and a React frontend.

## Features

- User registration and authentication (JWT-based).
- Role-based access control (e.g., Uploader, Admin, various Approver roles).
- Secure document uploading.
- Multi-step document approval workflows.
- Ability to view document status and history (implied, to be fully developed).
- Approve or reject documents at each stage of the workflow.

## Tech Stack

### Backend

- **.NET Core (C#)**: Framework for building the API.
- **ASP.NET Core Identity**: For user authentication and authorization.
- **Entity Framework Core**: For data access and interaction with the database.
- **JWT (JSON Web Tokens)**: For secure token-based authentication.
- **Database**: (Likely SQL Server, a common choice with .NET. Specify your database here if different, e.g., PostgreSQL, SQLite).

### Frontend

- **React (JavaScript/JSX)**: Library for building the user interface.
- **Vite**: Build tool and development server.
- **Tailwind CSS**: Utility-first CSS framework for styling.
- **Axios**: For making HTTP requests to the backend API.
- **React Router DOM**: For client-side routing.

## API Endpoints

The backend exposes the following RESTful API endpoints:

### Account Management (`/api/Account`)

- `POST /register`
  - **Description**: Registers a new user.
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
  - **Description**: Approves a document at its current approval step. Requires appropriate approver role for the current step.
  - **Response**: Success or error message.
- `POST /{id}/reject`
  - **Description**: Rejects a document at its current approval step. Requires appropriate approver role for the current step.
  - **Request Body (optional)**: `{ "comments": "Reason for rejection" }`
  - **Response**: Success or error message.

## Getting Started

### Prerequisites

- .NET SDK (specify version, e.g., .NET 8.0 SDK)
- Node.js and npm (specify versions, e.g., Node.js >= 18.x, npm >= 9.x)
- A relational database compatible with Entity Framework Core (e.g., SQL Server, PostgreSQL, SQLite).
- (TODO: Add any other specific tools or SDKs required)

### Backend Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/ajlorans/document-management.git
    cd document-management/src/backend
    ```
2.  **Configure Database Connection:**
    - Open `appsettings.Development.json` (and `appsettings.json` for production).
    - Update the `ConnectionStrings` section with your database connection details. Example for SQL Server:
      ```json
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=LegalDocManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
      ```
3.  **Apply Migrations:**
    This will create the database schema based on the Entity Framework Core models.
    ```bash
    dotnet ef database update
    ```
    _(Note: If you haven't installed `dotnet-ef` tools yet, run `dotnet tool install --global dotnet-ef` first)._
4.  **Run the Backend:**
    ```bash
    dotnet run
    ```
    The API will typically be available at `https://localhost:5001` or `http://localhost:5000`.

### Frontend Setup

1.  **Navigate to the client directory:**
    ```bash
    cd ../frontend/client
    ```
    _(Assuming you are in the `src/backend` directory from the previous step)._
2.  **Install Dependencies:**
    ```bash
    npm install
    ```
3.  **Configure API Base URL (if needed):**
    - The frontend uses `axios` to make API calls. By default, it might assume the API is on the same host/port or a relative path.
    - Update API call base URLs in `src/frontend/client/src/services/api.js` or create an environment variable (e.g., `VITE_API_BASE_URL` in a `.env` file) if your backend is running on a different URL.
      Example `.env` file in `src/frontend/client/`:
      ```
      VITE_API_BASE_URL=https://localhost:5001/api
      ```
4.  **Run the Frontend:**
    ```bash
    npm run dev
    ```
    The React application will typically be available at `http://localhost:5173`.

## Usage

1.  Open your browser and navigate to the frontend URL (e.g., `http://localhost:5173`).
2.  **Register** a new user account.
3.  **Login** with your credentials.
4.  If your user has the "Uploader" role (the default for new registrations in the current seed data), you can navigate to the upload section and submit a document.
5.  Users with designated approver roles can then view pending documents and approve or reject them.
    _(TODO: Detail the exact roles and how they are assigned or seeded if not covered by default registration.)_

## Workflow Overview

1.  **Document Upload**: An "Uploader" submits a document.
2.  **Initial Status**: The document enters the workflow with an initial status (e.g., "AwaitingLegal").
3.  **Sequential Approval**:
    - The document moves through predefined `ApprovalSteps` (e.g., Legal, Manager, Final).
    - Users with the `RequiredRole` for the current active step can **approve** it.
    - If approved, the document moves to the next step or becomes "Approved" if it's the final step.
    - If **rejected**, the document status becomes "Rejected," and the workflow for that document typically ends unless a resubmission process is implemented.

## Contributing

(TODO: Add instructions on how to contribute to the project. Include guidelines for code style, branching, pull requests, etc.)

## License

(TODO: Specify the license for your project, e.g., MIT License. If you don't have one, consider adding one like MIT: `LICENSE.md` with the MIT license text.)

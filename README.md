# User Management System

## Overview

The User Management System is a web-based application built using C# .NET MVC for the frontend and Node.js for the backend. It allows administrators to manage users by performing CRUD (Create, Read, Update, Delete) operations. The system supports filtering and searching users.

## Features

- User creation, editing, deletion, and viewing.
- Filtering and searching functionality.

### Installation

# Install dependencies for the frontend (C# .NET MVC):

```sh
cd UserManagementApp
dotnet restore
```

# Install dependencies for the backend (Node.js):

```sh
cd ../user-management-api
npm install
```

### Running the Project

#### Start the Backend

1. Navigate to the backend directory:
   ```sh
   cd user-management-api
   ```
2. Start the Node.js server:
   ```sh
   npm start
   ```

#### Start the Frontend

1. Navigate to the frontend directory:
   ```sh
   cd ../UserManagementApp
   ```
2. Run the application:
   ```sh
   dotnet run
   ```

## To Be Done (TBD)

- Find a secure way to store the client ID.
- Optimize token retrieval to avoid calling `GetAuthToken` every time.

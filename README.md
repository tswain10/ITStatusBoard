# IT Status Board

A simple ASP.NET Core MVC application for monitoring the status of APIs and websites.  
It allows you to configure endpoints, view their online/offline status, and see details such as server IP and error messages.

## Features

- **Dashboard:**  
  View the status of all configured APIs/websites, including:
  - Name
  - Status (Online/Offline)
  - Last checked time
  - Server IP address
  - Error messages (if any)
  - Clickable links to each API/website

- **Configuration Page:**  
  - Add new APIs/websites to monitor.
  - Remove existing entries.
  - All configuration is stored in a JSON file (`ApiConfigs.json`) for easy editing outside the app.

- **Persistent Configuration:**  
  - Endpoints are stored in `ApiConfigs.json` (default location: `bin\Debug\net6.0\ApiConfigs.json`).
  - You can edit this file directly or use the web interface.

- **Bootstrap UI:**  
  - Responsive and clean interface using Bootstrap.

## Setup

1. **Clone the repository.**
2. **Restore NuGet packages** and build the project.
3. **Run the application** using `dotnet run` or from Visual Studio.
4. **Navigate to the app** (default: `http://localhost:5000/`).
5. **Configure endpoints** via the "Configure Checks" menu.

## Configuration File

- The monitored endpoints are stored in `ApiConfigs.json`.
- Example:
  ```json
  [
    {
      "Name": "Example API",
      "Url": "https://example.com/api"
    }
  ]
  ```
- You can edit this file directly or use the web UI.

## Files to Remove Before Publishing

- **`bin/` and `obj/` folders:**  
  These contain build artifacts and should not be committed.

- **`ApiConfigs.json` (if it contains sensitive or private endpoints):**  
  Remove or replace with a sample file (e.g., `ApiConfigs.sample.json`) before making the repo public.

- **`appsettings.Development.json` and any other environment-specific config files**  
  if they contain secrets or sensitive data.


# ClusterOS

ClusterOS is a WINUI/.NET 8.0 application designed to provide a comprehensive suite of tools for managing and tracking various tasks, journals, and applications. It leverages WinUI for the user interface and integrates with several services to offer a seamless experience.

## Features

- **Task Management**: Create, edit, and manage tasks efficiently.
- **Journal Management**: Maintain and organize daily journal entries.
- **Application Tracking**: Track application usage and focus.
- **SignalR Integration**: Real-time communication using SignalR, for tracking browser activities. (With the Firefox extension: Repo:https://github.com/ATClus/tab_focus_sync  Extension:https://addons.mozilla.org/en-US/firefox/addon/tab-focus-sync/)
- **Markdown Parsing**: Convert Markdown formatted text to HTML, for using with the integrated CMS.
- **MSIX Packaging**: Single-project MSIX packaging for easy deployment.

## Project Structure

- **ClusterOS**: Main application project.
- **Hub**: Backend services and APIs.
- **MarkdownLib**: Library for parsing Markdown to HTML.
- **TrayServiceLib**: Services for system tray interactions.
- **WinTracker**: Application tracking services.

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8.0 SDK
- Windows 10 or later

### Building the Project

1. Clone the repository:
    git clone https://github.com/yourusername/ClusterOS.git
cd ClusterOS

2. Open the solution in Visual Studio 2022.

3. Restore the NuGet packages:
    dotnet restore

4. Build the solution:
    dotnet build


### Running the Application

1. Set `ClusterOS` as the startup project.
2. Press `F5` to run the application.


## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [Microsoft](https://www.microsoft.com) for the .NET platform and tools.
- [Serilog](https://serilog.net/) for logging.

## Contact

For any inquiries or support, please contact [atclus@clusterat.com](mailto:atclus@clusterat.com).


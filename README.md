# SitecoreCommander
SitecoreCommander is a tool designed for managing Sitecore instances, typically utilized by developers and administrators working with the Sitecore Content Management System (CMS). It helps in performing various administrative tasks by Using the API's

For Sitecore XM Cloud and Sitecore 10.3+ 


## Overview:

SitecoreCommander and Visual Studio, providing a powerful scripting and automation framework for managing Sitecore environments. It allows developers and administrators to execute complex scripts and manage long-running tasks efficiently within Sitecore.

## Note:
While SitecoreCommander offers a lot of possibilities, please be aware that this is the first version of the tool. For many functionalities, there isn't pre-existing code available, and you may need to add or customize the code yourself to meet your specific needs.

## Key Features:

### Use Visual Studio:

- Utilize the full power of Visual Studio for programming in C# and executing Sitecore tasks.
Benefit from Visual Studio’s advanced debugging features to troubleshoot and refine scripts.
Complex Script Management:

- Handle more intricate scripts that require extensive logic or need to run over extended periods.
Automate repetitive tasks, saving time and reducing the potential for human error.
Enhanced Security:

- For security reasons, Sitecore PowerShell Elevation is recommended only for local environments.
SitecoreCommander mitigates these security concerns by only using various Sitecore APIs.
Efficient Task Execution:

- Run long-running tasks without the hassle associated with traditional PowerShell scripting within Sitecore.
Optimize task management, ensuring that resource-intensive operations are handled smoothly.
API Interaction:

- Tools like Postman and Firecamp.dev are excellent for making single API calls to Sitecore, useful for testing and debugging.
SitecoreCommander is ideal for scenarios requiring multiple API calls and complex scripting, offering a more robust solution for automation.
Use Cases:

### Automating Content Management:
- Automate the creation, update, and deletion of content items in bulk, making it easier to manage large volumes of content.

### Deployment and Configuration:
- Script deployment processes and configuration changes to ensure consistency across multiple Sitecore instances.

### Maintenance Tasks:
- Schedule and run maintenance scripts, such as cleaning up old versions or reindexing, to keep the Sitecore environment healthy.

### Data Import/Export:
- Automate the import and export of data between Sitecore and other systems, streamlining integration processes.

## SETUP
- use the Sitecore CLI to create an user.json
- edit the Config.cs and supply the values for your Sitecore 
- edit the Program.cs and create the code you need.

### Recent Updates:
19 July initial version with Remove Language/Version from a single item or a Tree of items
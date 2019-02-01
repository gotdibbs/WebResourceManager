# Web Resource Manager
A utility to improve your Dynamics 365 Web Resource workflow.

<img alt="ScreenShot" src="https://raw.githubusercontent.com/gotdibbs/WebResourceManager/master/screenshot.JPG" style="border: 1px solid #444;" />

# Key Features
- Upload and publish multiple files at a time
- Monitor the local file system for changes and automatically select the files for upload
- Monitor the remote system for changes to prevent conflicts with multiple devs and warn before upload
- View a Web Resource in Dynamics
- Quickly open a file in VS Code
- Quickly open a folder in VS Code (particularly useful for custom UI projects)
- Open a folder in Windows Explorer
- Integration with Imposter for Fiddler
- Diff local files vs. their remote counterparts
- Automatic "namespacing" for files (ex. c:\source\project\webresources\form\account.js becomes new_/form/account.js)
- View a list of all web resources in the target solution and their current status, mixed with all local files to see what's where at a glance
- Filtered the previously mentioned views further to just see remote, local, modified, etc.

# Optional "Requirements"
1. [Visual Studio Code](https://code.visualstudio.com/): Recommended editor/diff tool
2. [Fiddler](http://www.telerik.com/fiddler) 4.5+: For debugging without uploading over, and over, and over again
3. [Imposter for Fiddler](https://github.com/gotdibbs/Imposter.Fiddler): Similar to Fiddler's AutoResponder, but with more power

# Installation
Please see the [RELEASES](../../releases)

# Problems? Suggestions?
Pull requests speak louder than issues, but I'll take both.

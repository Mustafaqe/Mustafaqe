# Simple .NET Todo List

This repository contains a minimal console-based todo list built with the .NET SDK. The app keeps your tasks in a local `todos.json` file so you can add, complete, and remove items across runs.

## Features

- Add new tasks with a description
- View all tasks with a clear completed/pending indicator and a progress dashboard
- Toggle the completion state of an existing task
- Remove tasks that you no longer need
- Automatic persistence to `todos.json`

## Getting Started

1. Install the [.NET SDK 9.0 or later](https://dotnet.microsoft.com/en-us/download).
2. Run the application from the repository root:

   ```bash
   dotnet run --project src/TodoListApp
   ```

3. Follow the interactive menu to manage your tasks.

The app now shows a progress dashboard at the top of the screen with total, completed, and pending tasks plus a visual progress bar. Status updates from your last action appear beneath the dashboard so you never miss a confirmation message.

Your todos are stored in `todos.json` in the working directory so they will be there the next time you launch the app.

## Shipping a self-contained Linux build

If you need to deploy the tool to a Linux machine that does not have the .NET runtime installed, publish a self-contained binary:

```bash
dotnet publish src/TodoListApp \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true
```

The output binary will be located in `src/TodoListApp/bin/Release/net9.0/linux-x64/publish/`. Copy that directory to the Linux system and run the `TodoListApp` executable directly—no additional runtime installation is required.

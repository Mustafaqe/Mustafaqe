# Simple .NET Todo List

This repository contains a minimal console-based todo list built with the .NET SDK. The app keeps your tasks in a local `todos.json` file so you can add, complete, and remove items across runs.

## Features

- Add new tasks with a description
- View all tasks with a clear completed/pending indicator
- Toggle the completion state of an existing task
- Remove tasks that you no longer need
- Automatic persistence to `todos.json`

## Getting Started

1. Install the [.NET SDK 6.0 or later](https://dotnet.microsoft.com/en-us/download).
2. Run the application from the repository root:

   ```bash
   dotnet run --project src/TodoListApp
   ```

3. Follow the interactive menu to manage your tasks.

Your todos are stored in `todos.json` in the working directory so they will be there the next time you launch the app.

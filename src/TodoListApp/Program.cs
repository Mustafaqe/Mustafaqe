using System.Text.Json;

namespace TodoListApp;

internal class TodoItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public void MarkComplete()
    {
        IsComplete = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkIncomplete()
    {
        IsComplete = false;
        CompletedAt = null;
    }
}

internal class TodoList
{
    private const string StorageFileName = "todos.json";
    private readonly List<TodoItem> _items = new();

    public TodoList()
    {
        Load();
    }

    public IReadOnlyList<TodoItem> Items => _items;

    public void Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A task must have a description.", nameof(title));
        }

        _items.Add(new TodoItem
        {
            Title = title.Trim()
        });

        Save();
    }

    public bool ToggleCompletion(int index)
    {
        if (!TryGetItem(index, out var item))
        {
            return false;
        }

        if (item.IsComplete)
        {
            item.MarkIncomplete();
        }
        else
        {
            item.MarkComplete();
        }

        Save();
        return true;
    }

    public bool Remove(int index)
    {
        if (!TryGetItem(index, out var item))
        {
            return false;
        }

        _items.Remove(item);
        Save();
        return true;
    }

    private bool TryGetItem(int index, out TodoItem item)
    {
        var adjustedIndex = index - 1;
        if (adjustedIndex < 0 || adjustedIndex >= _items.Count)
        {
            item = default!;
            return false;
        }

        item = _items[adjustedIndex];
        return true;
    }

    private void Load()
    {
        if (!File.Exists(StorageFileName))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(StorageFileName);
            var items = JsonSerializer.Deserialize<List<TodoItem>>(stream);
            if (items is not null)
            {
                _items.Clear();
                _items.AddRange(items);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load saved todos: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(StorageFileName));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(StorageFileName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save todos: {ex.Message}");
        }
    }
}

public static class Program
{
    private static readonly TodoList Todos = new();

    public static void Main()
    {
        Console.WriteLine("Simple .NET Todo List\n");

        var exitRequested = false;
        while (!exitRequested)
        {
            DisplayMenu();
            Console.Write("Select an option: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    DisplayTasks();
                    break;
                case "2":
                    AddTask();
                    break;
                case "3":
                    ToggleTaskCompletion();
                    break;
                case "4":
                    RemoveTask();
                    break;
                case "5":
                    exitRequested = true;
                    break;
                default:
                    Console.WriteLine("Unknown option. Please choose between 1 and 5.\n");
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("===========================");
        Console.WriteLine("1. View tasks");
        Console.WriteLine("2. Add a task");
        Console.WriteLine("3. Toggle task completion");
        Console.WriteLine("4. Remove a task");
        Console.WriteLine("5. Exit");
        Console.WriteLine("===========================");
    }

    private static void DisplayTasks()
    {
        if (Todos.Items.Count == 0)
        {
            Console.WriteLine("No tasks found. Add your first task!\n");
            return;
        }

        Console.WriteLine();
        for (var i = 0; i < Todos.Items.Count; i++)
        {
            var todo = Todos.Items[i];
            var status = todo.IsComplete ? "[x]" : "[ ]";
            Console.WriteLine($"{i + 1}. {status} {todo.Title}");
        }

        Console.WriteLine();
    }

    private static void AddTask()
    {
        Console.Write("Enter the task description: ");
        var title = Console.ReadLine();

        try
        {
            Todos.Add(title ?? string.Empty);
            Console.WriteLine("Task added successfully!\n");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Could not add task: {ex.Message}\n");
        }
    }

    private static void ToggleTaskCompletion()
    {
        if (!TryReadTaskIndex(out var index))
        {
            return;
        }

        if (Todos.ToggleCompletion(index))
        {
            Console.WriteLine("Updated task completion status.\n");
        }
        else
        {
            Console.WriteLine("Task not found.\n");
        }
    }

    private static void RemoveTask()
    {
        if (!TryReadTaskIndex(out var index))
        {
            return;
        }

        if (Todos.Remove(index))
        {
            Console.WriteLine("Task removed.\n");
        }
        else
        {
            Console.WriteLine("Task not found.\n");
        }
    }

    private static bool TryReadTaskIndex(out int index)
    {
        if (Todos.Items.Count == 0)
        {
            Console.WriteLine("No tasks to select.\n");
            index = -1;
            return false;
        }

        Console.Write("Enter the task number: ");
        var input = Console.ReadLine();

        if (!int.TryParse(input, out index))
        {
            Console.WriteLine("Please enter a valid number.\n");
            return false;
        }

        return true;
    }
}

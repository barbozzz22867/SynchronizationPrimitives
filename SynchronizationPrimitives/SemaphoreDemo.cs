namespace SynchronizationPrimitives;

public class SemaphoreDemo
{
    // TODO 1: лимит снижен с 3 до 2.
    private readonly SemaphoreSlim _semaphore = new(2, 2);

    private int _activeTasks;
    private int _maxActiveTasks;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== SEMAPHORE DEMO ===");

        Task[] tasks = Enumerable.Range(1, 12)
            .Select(id => ProcessAsync(id, cancellationToken))
            .ToArray();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Обработка отменена пользователем.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine($"Максимум одновременных задач: {_maxActiveTasks}");
        Console.WriteLine($"Свободных разрешений: {_semaphore.CurrentCount}");

        Console.WriteLine();
        Console.WriteLine("Проверка повторного использования:");

        await ProcessAsync(99, CancellationToken.None);

        Console.WriteLine("Проверка завершена.");
    }

    private async Task ProcessAsync(int id, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Задача {id}: ожидает разрешения");

        // TODO 5: тайм-аут 2 с + отмена.
        bool acquired;
        try
        {
            acquired = await _semaphore.WaitAsync(
                TimeSpan.FromSeconds(2),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Задача {id}: ожидание отменено");
            return;
        }

        if (!acquired)
        {
            // Разрешение не получено — Release() вызывать НЕЛЬЗЯ.
            Console.WriteLine($"Задача {id}: тайм-аут ожидания разрешения");
            return;
        }

        try
        {
            int active = Interlocked.Increment(ref _activeTasks);
            UpdateMaximum(active);
            Console.WriteLine($"Задача {id}: начала работу. Активно: {active}");

            // TODO 4: token передаётся в Delay.
            await Task.Delay(Random.Shared.Next(500, 1500), cancellationToken);

            // TODO 2: искусственная ошибка id == 5 удалена.

            Console.WriteLine($"Задача {id}: завершена");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Задача {id}: отменена во время работы");
        }
        finally
        {
            // TODO 3: Release() обязан быть здесь, а не в конце try.
            Interlocked.Decrement(ref _activeTasks);
            _semaphore.Release();
            Console.WriteLine($"Задача {id}: освободила разрешение");
        }
    }

    private void UpdateMaximum(int current)
    {
        int previous;
        do
        {
            previous = Volatile.Read(ref _maxActiveTasks);
            if (current <= previous)
                return;
        }
        while (Interlocked.CompareExchange(
                   ref _maxActiveTasks,
                   current,
                   previous) != previous);
    }
}
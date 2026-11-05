using System.Collections.Concurrent;

namespace SynchronizationPrimitives;

public class ProducerConsumerDemo : IDisposable
{
    private readonly ConcurrentQueue<int> _queue = new();
    private readonly AutoResetEvent _signal = new(false);
    private int _processedCount;

    public async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=== PRODUCER-CONSUMER DEMO ===");

        // Потребитель работает в отдельном потоке: AutoResetEvent — синхронный.
        Task consumer = Task.Run(Consume);

        // TODO 1 + TODO 2: четыре производителя по 10 элементов.
        Task[] producers =
        {
            Task.Run(() => Produce(1, 10)),
            Task.Run(() => Produce(2, 10)),
            Task.Run(() => Produce(3, 10)),
            Task.Run(() => Produce(4, 10)),
        };

        await Task.WhenAll(producers);

        // Маркер завершения + сигнал.
        _queue.Enqueue(-1);
        _signal.Set();

        await consumer;

        Console.WriteLine();
        Console.WriteLine($"Обработано элементов: {_processedCount}");
    }

    private void Produce(int producerId, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            int value = producerId * 100 + i;
            _queue.Enqueue(value);

            Console.WriteLine($"Производитель {producerId}: создал {value}");

            // Будим потребителя.
            _signal.Set();

            Thread.Sleep(Random.Shared.Next(100, 400));
        }

        Console.WriteLine($"Производитель {producerId}: завершил работу");
    }

    private void Consume()
    {
        while (true)
        {
            // Ждём сигнал о появлении данных.
            _signal.WaitOne();

            // Забираем ВСЕ доступные элементы: несколько Set() могли слипнуться.
            while (_queue.TryDequeue(out int value))
            {
                if (value == -1)
                {
                    Console.WriteLine("Потребитель завершает работу.");
                    return;
                }

                // TODO 4: высокоприоритетные значения.
                if (value > 250)
                    Console.WriteLine($"Высокоприоритетная задача: {value}");
                else
                    Console.WriteLine($"Потребитель обработал: {value}");

                Interlocked.Increment(ref _processedCount);

                // TODO 5: обработка замедлена — очередь накапливается.
                Thread.Sleep(1000);
            }
        }
    }

    public void Dispose() => _signal.Dispose();
}
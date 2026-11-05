using System.Collections.Concurrent;

namespace SynchronizationPrimitives;

// TODO 1: Изменить количество производителей
// TODO 2: Изменить количество данных
// TODO 3: Убрать сигнализацию
// TODO 4: Изменить поведение потребителя
// TODO 5: Добавить задержку обработки
public class ProducerConsumerDemo : IDisposable
{
    private readonly ConcurrentQueue<int> _queue = new();

    private readonly AutoResetEvent _signal = new(false);

    private int _processedCount;

    public async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=== PRODUCER-CONSUMER DEMO ===");

        // Запускаем потребителя.
        Task consumer = Task.Run(Consume);

        // Запускаем производителей.
        // TODO 1 + TODO 2: четыре производителя по 10 элементов.
        Task[] producers =
        {
            Task.Run(() => Produce(1, 10)),
            Task.Run(() => Produce(2, 10)),
            Task.Run(() => Produce(3, 10)),
            Task.Run(() => Produce(4, 10))
        };

        // Ждём завершения производителей.
        await Task.WhenAll(producers);

        // Специальное значение означает завершение работы.
        _queue.Enqueue(-1);

        _signal.Set();

        // Ждём завершения потребителя.
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

            Console.WriteLine(
                $"Производитель {producerId}: создал {value}");

            // Сообщаем потребителю о появлении данных через _signal.
            _signal.Set();

            Thread.Sleep(Random.Shared.Next(100, 400));
        }

        Console.WriteLine(
            $"Производитель {producerId}: завершил работу");
    }

    private void Consume()
    {
        while (true)
        {
            // Ждём сигнала о появлении данных.
            _signal.WaitOne();

            // Забираем все доступные элементы.
            while (_queue.TryDequeue(out int value))
            {
                // -1 — специальный сигнал завершения.
                if (value == -1)
                {
                    Console.WriteLine("Потребитель завершает работу.");
                    return;
                }

                // TODO 4: высокоприоритетная задача.
                if (value > 250)
                {
                    Console.WriteLine(
                        $"Высокоприоритетная задача: {value}");
                }
                else
                {
                    Console.WriteLine(
                        $"Потребитель обработал: {value}");
                }

                Interlocked.Increment(ref _processedCount);

                // TODO 5: задержка 1000 мс.
                Thread.Sleep(1000);
            }
        }
    }

    public void Dispose()
    {
        _signal.Dispose();
    }
}
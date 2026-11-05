namespace SynchronizationPrimitives;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("\n === Примитивы синхронизации === \n");

        Console.WriteLine("1. SemaphoreSlim");
        Console.WriteLine("2. Producer-Consumer");
        Console.WriteLine("3. Mutex");

        Console.Write("\n Выберите эксперимент: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                {
                    // Отмена по Enter (для проверки TODO 4).
                    using var cts = new CancellationTokenSource();
                    _ = Task.Run(() =>
                    {
                        Console.WriteLine("Нажмите Enter для отмены...");
                        Console.ReadLine();
                        cts.Cancel();
                    });

                    var demo = new SemaphoreDemo();
                    await demo.RunAsync(cts.Token);
                    break;
                }

            case "2":
                {
                    using var demo = new ProducerConsumerDemo();
                    await demo.RunAsync();
                    break;
                }

            case "3":
                {
                    MutexDemo.Run();
                    break;
                }

            default:
                {
                    Console.WriteLine("Неизвестная команда.");
                    break;
                }
        }
    }
}
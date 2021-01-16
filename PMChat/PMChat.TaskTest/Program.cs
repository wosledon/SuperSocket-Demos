using System;
using System.Threading;
using System.Threading.Tasks;

namespace PMChat.TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Task1();
        }

        static async void Task1()
        {
            int i = 0;
            while (true)
            {
                Console.WriteLine("1");
                if (++i == 5)
                {
                    Task2();
                }
                Task.Delay(1000).Wait();
            }
        }

        static async void Task2()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("2");
                await Task.Delay(1000);
            }
        }
        static async Task Task3()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("3");
                await Task.Delay(1000);
            }
        }
    }
}

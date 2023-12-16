namespace Concurrency.SectionTwo
{
    /// <summary>
    /// Bank accounts are shared resources that can be accessed by multiple threads. These are great examples of
    /// resources that need to be protected from concurrent access.
    /// 
    /// This class is not thread safe because the balance can be modified by multiple threads at the same time. 
    /// </summary>
    public class UnsafeBankAccount : IBankAccount
    {
        public int Balance { get; set; }

        public void Deposit(int amount)
        {
            Balance += amount;
        }

        public void Withdraw(int amount)
        {
            Balance -= amount;
        }
    }

    public class SafeBankAccount : IBankAccount
    {
        public object padlock = new object();
        public int Balance { get; set; }

        public void Deposit(int amount)
        {
            // this is a critical section
            lock (padlock)
            {
                Balance += amount;
            }
        }

        public void Withdraw(int amount)
        {
            // this is a critical section
            lock (padlock)
            {
                Balance -= amount;
            }
        }
    }

    public interface IBankAccount
    {
        int Balance { get; set; }
        void Deposit(int amount);
        void Withdraw(int amount);
    }

    public class Exchange
    {
        public static void Run(IBankAccount ba)
        {
            if (ba == null)
            {
                throw new System.ArgumentNullException(nameof(ba));
            }

            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    for (int j = 0; j < 1000; j++)
                    {
                        ba.Deposit(100);
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() => {
                    for (int j = 0; j < 1000; j++)
                    {
                        ba.Withdraw(100);
                    }
                }));
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine($"Final balance is {ba.Balance}");
            }
        }
    }
}
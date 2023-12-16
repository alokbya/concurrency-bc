namespace Concurrency.SectionTwo
{
    /// <summary>
    /// Bank accounts are shared resources that can be accessed by multiple threads. These are great examples of
    /// resources that need to be protected from concurrent access.
    /// 
    /// This class is not thread safe because the balance can be modified by multiple threads at the same time. 
    /// </summary>
    internal class UnsafeBankAccount : IBankAccount
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

    internal class SafeBankAccount : IBankAccount
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

    /// <summary>
    /// The Interlocked class provides atomic operations for variables that are shared by multiple threads. It changes things atomically for us.
    /// </summary>
    internal class InterlockedBankAccount : IBankAccount
    {
        private int _balance;
        public int Balance { get => _balance; set => _balance = value; }

        public void Deposit(int amount)
        {
            Interlocked.Add(ref _balance, amount);
        }

        public void Withdraw(int amount)
        {
            Interlocked.Add(ref _balance, -amount); // subtracting is the same as adding a negative number
        }
    }

    internal interface IBankAccount
    {
        int Balance { get; set; }
        void Deposit(int amount);
        void Withdraw(int amount);
    }

    internal class Exchange
    {
        internal static void Transact(IBankAccount ba)
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
            }
            Console.WriteLine($"Final balance is {ba.Balance}");
        }

        internal static void Run()
        {
            var unsafeBank = new UnsafeBankAccount();
            Console.WriteLine("Unsafe bank account");
            Transact(unsafeBank);

            Console.WriteLine();

            var safeBank = new SafeBankAccount();
            Console.WriteLine("Safe bank account");
            Transact(safeBank);

            Console.WriteLine();

            var interlockedBank = new InterlockedBankAccount();
            Console.WriteLine("Interlocked bank account");
            Transact(interlockedBank);
        }
    }
}
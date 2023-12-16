namespace Concurrency.SectionTwo
{
    /// <summary>
    /// Bank accounts are shared resources that can be accessed by multiple threads. These are great examples of
    /// resources that need to be protected from concurrent access.
    /// </summary>
    public class BankAccount
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

    public class Exchange
    {
        /// <summary>
        /// This method is not thread safe. It is possible for the balance to be incorrect if multiple threads.
        /// Because the Deposit and Withdraw methods are not atomic, it is possible for the balance to be incorrect. 
        /// Balance should be 0, but it is not (most of the time). 
        /// </summary>
        public static void NotThreadSafe()
        {
            var tasks = new List<Task>();
            var ba = new BankAccount();

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
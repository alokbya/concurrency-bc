namespace Concurrency.SectionOne
{
    public static class BasicTasks
    {
        // Methods
        public static void DifferentTokenSources()
        {
            // this is a regular token source, token, and registration
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(() =>
            {
                Console.WriteLine("Cancellation has been requested");
            });

            // creating tokens for different types of cancellation
            var planned = new CancellationTokenSource();
            var preventative = new CancellationTokenSource();
            var emergency = new CancellationTokenSource();

            // link the different tokens
            var paranoid = CancellationTokenSource.CreateLinkedTokenSource(planned.Token, preventative.Token, emergency.Token);

            // register a callback for each token to specify which token was used to cancel the operation
            planned.Token.Register(() => Console.WriteLine("Planned cancellation requested"));
            preventative.Token.Register(() => Console.WriteLine("Preventative cancellation requested"));
            emergency.Token.Register(() => Console.WriteLine("Emergency cancellation requested"));
            paranoid.Token.Register(() => Console.WriteLine("Paranoid cancellation requested"));

            var task = new Task(() =>
            {
                // run this indefinitely so we can see cancellation requests
                while (!token.IsCancellationRequested)
                {
                    // token.ThrowIfCancellationRequested();
                    Console.Write("*");
                    Thread.Sleep(1000);
                }
            }, paranoid.Token); // pass in the paranoid tokens
            task.Start();


            Console.Read();
            // cancel the token (any of them will do)
            planned.Cancel();
        }

        public static void WaitingForTimeToPass()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var t = new Task(() => {
                Console.WriteLine("You have 5 seconds to disarm this bomb by pressing a key");
                bool cancelled = token.WaitHandle.WaitOne(5000);
                Console.WriteLine(cancelled ? "Bomb disarmed." : "BOOM!!!!");
            }, token);
            t.Start();
            
            Console.Read();
            cts.Cancel();

            Console.WriteLine("Main program done.");
        }

    }
}
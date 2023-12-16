// See https://aka.ms/new-console-template for more information


// Concurrency.SectionOne.BasicTasks.DifferentTokenSources();
// Concurrency.SectionOne.BasicTasks.WaitingForTimeToPass();

using Concurrency.SectionTwo;
var unsafeBank = new UnsafeBankAccount();
Console.WriteLine("Unsafe bank account");
Exchange.Run(unsafeBank);

Console.WriteLine();

var safeBank = new SafeBankAccount();
Console.WriteLine("Safe bank account");
Exchange.Run(safeBank);

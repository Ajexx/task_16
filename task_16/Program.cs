public class BankAccount
{
    public int Balance = 1000;
    public object locker = new object();
    public int GetBalance()
    {
        return Balance;
    }    
    public int DepositWithoutLock(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }
        Balance += amount;
        return Balance;
    }
    public int DepositWithLock(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }
        lock (locker)
        {
            Balance += amount;
        }
        return Balance;
    }
    public int DepositInterlocked(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }
        Interlocked.Add(ref Balance, amount);
        return Balance;
    }
}

public class SeedBankAccount
{
    public void SeedSync1()
    {
        List<Thread> threads = new List<Thread>();
        BankAccount account = new BankAccount();

        for (int i = 0; i < 10; i++)
        {
            Thread thread = new Thread(() =>
            {
                for (int j = 0; j < 10000; j++)
                {
                    account.DepositWithLock(1);
                }
            });
            threads.Add(thread);
        }
        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"Final Balance (WithLock): {account.GetBalance()}");
    }
    public void SeedSync2()
    {
        List<Thread> threads = new List<Thread>();
        BankAccount account = new BankAccount();

        for (int i = 0; i < 10; i++)
        {
            Thread thread = new Thread(() =>
            {
                for (int j = 0; j < 10000; j++)
                {
                    account.DepositWithoutLock(1);
                }
            });
            threads.Add(thread);
        }
        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"Final Balance (WithoutLock): {account.GetBalance()}");
    }
    public void SeedSync3()
    {
        List<Thread> threads = new List<Thread>();
        BankAccount account = new BankAccount();
        for (int i = 0; i < 10; i++)
        {
            Thread thread = new Thread(() =>
            {
                for (int j = 0; j < 10000; j++)
                {
                    account.DepositInterlocked(1);
                }
            });
            threads.Add(thread);
        }
        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"Final Balance (Interlocked): {account.GetBalance()}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        SeedBankAccount seedBankAccount = new SeedBankAccount();
        seedBankAccount.SeedSync1();
        seedBankAccount.SeedSync2();
        seedBankAccount.SeedSync3();
    }
}

//1. Какой баланс должен получиться?
// Баланс должен быть равен 1000 (начальный баланс) + 10 потоков * 10000 депозитов * 1 (сумма депозита) = 1000 + 100000 = 101000.

//2. Почему фактический баланс может оказаться меньше ожидаемого?
// Баланс может оказаться меньше ожидаемого из-за race condition. Когда несколько потоков одновременно выполняют операцию Deposit,
// они могут считывать и изменять значение Balance одновременно, что приводит к потере некоторых обновлений.

//3. Почему операция Balance += amount не является атомарной?
// Операция Balance += amount остоит из трех отдельных шагов: чтение значения Balance, увеличение его на amount, и запись нового значения.
// Эти шаги не выполняются как единое атомарное действие, поэтому могут быть прерваны другими потоками.
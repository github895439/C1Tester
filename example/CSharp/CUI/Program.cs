using System;

namespace CSharp
{
    class Program
    {
        static void f1(int number)
        {
            for (int index = 0; index <= number; index++)
            {
                if (index % 2 == 0)
                {
                    Console.WriteLine(index.ToString() + " is even.");
                }
                else
                {
                    Console.WriteLine(index.ToString() + " is odd.");
                }
                
                switch (index)
                {
                    case 1:
                    {
                        Console.WriteLine(index.ToString() + " is one.");
                        break;
                    }
                    case 2:
                    {
                        Console.WriteLine(index.ToString() + " is two.");
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }

        static int Main(string[] args)
        {
            int n = Convert.ToInt32(args[1]);

            if ((n < 0) || (n > 5))
            {
                Console.WriteLine("argument must be 0-5.");
                return 1;
            }

            f1(n);
            return 0;
        }
    }
}

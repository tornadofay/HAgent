using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAgent.Example
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args != null && Array.Exists(args, x => string.Equals(x, "--verify-intervention", StringComparison.OrdinalIgnoreCase)))
            {
                RunVerification().GetAwaiter().GetResult();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static async Task RunVerification()
        {
            var result = await InterventionVerification.RunAsync().ConfigureAwait(false);
            Console.WriteLine(result);
        }
    }
}

using System.Linq;
using System.Windows.Input;

namespace MyTimer
{
    public static class KeyEventArgsExtensions
    {
        public static bool IsNumber(this KeyEventArgs @this)
        {
            return (@this.Key >= Key.D0 && @this.Key <= Key.D9) || (@this.Key >= Key.NumPad0 && @this.Key <= Key.NumPad9);
        }

        public static char ToCharDigit(this KeyEventArgs @this)
        {
            return @this.Key.ToString().Last();
        }
    }
}

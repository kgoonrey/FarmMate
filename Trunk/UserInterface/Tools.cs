using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UserInterface
{
    public class Tools
    {
        public static void RunProgram(string exe, bool waitForExit)
        {
            Process process = Process.Start(exe);
            int id = process.Id;
            Process tempProc = Process.GetProcessById(id);
            if(waitForExit)
                tempProc.WaitForExit();
        }

        public static void MoveToNextUIElement(KeyEventArgs e)
        {
            FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
            TraversalRequest request = new TraversalRequest(focusDirection);
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
            if (elementWithFocus != null)
            {
                if (elementWithFocus.MoveFocus(request)) e.Handled = true;
            }
        }

        public static void MoveToNextUIElement(Control currentControl)
        {
            TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
            request.Wrapped = true;
            currentControl.MoveFocus(request);
        }

        public static bool IsKeyAChar(Key key)
        {
            return key >= Key.A && key <= Key.Z;
        }

        public static bool IsKeyADigit(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }
    }
}

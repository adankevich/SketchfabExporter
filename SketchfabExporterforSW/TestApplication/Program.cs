using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            SketchfabPublisher.ParametersForm dlg = new SketchfabPublisher.ParametersForm();
            dlg.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            dlg.ShowDialog();
        }
    }
}

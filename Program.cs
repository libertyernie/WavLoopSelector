using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavLoopSelector {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var s = Audio.WAV.FromFile(@"C:\Windows\Media\Ring08.wav");
            using (LoopSelectorForm dialog = new LoopSelectorForm(s)) {
                dialog.ShowDialog();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WavLoopSelector.Audio;

namespace WavLoopSelector {
    static class Program {
        private const string USAGE = @"WAV Loop Selector
https://github.com/libertyernie/WavLoopSelector

This program is provided as-is without any warranty, implied
or otherwise. By using this program, the end user agrees to
take full responsibility regarding its proper and lawful use.
The authors/hosts/distributors cannot be held responsible for
any damage resulting in the use of this program, nor can they
be held accountable for the manner in which it is used.

Normal usage:
  WavLoopSelector.exe [in-file] [out-file]
Read-only usage:
  WavLoopSelector.exe [in-file]";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length != 1 && args.Length != 2) {
                MessageBox.Show(USAGE);
                return;
            }

            string infile = args[0];
            string outfile = args.Length == 2
                ? args[1]
                : null;
            if (!File.Exists(infile)) {
                MessageBox.Show("Could not find file " + infile, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] input = File.ReadAllBytes(infile);

            IAudioStream stream;
            string message = null;
            try {
                stream = new PCMStream(input, ignoreUnknownChunks: false);
            } catch (PCMStream.UnknownChunkException e) {
                message = e.Message;
                stream = new PCMStream(input, ignoreUnknownChunks: true);
            }
            using (LoopSelectorForm dialog = new LoopSelectorForm(stream)) {
                if (message != null)
                    dialog.Shown += (o, e) => MessageBox.Show(dialog, message);

                if (outfile == null)
                    dialog.ShowOkayCancelButtons = false;

                Application.Run(dialog);
                if (dialog.DialogResult != DialogResult.OK)
                    return;
            }

            byte[] output = WAV.ToByteArray(stream, appendSmplChunk: true);
            File.WriteAllBytes(outfile, output);

            stream.Dispose();
        }
    }
}

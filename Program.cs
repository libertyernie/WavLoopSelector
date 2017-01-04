using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WavLoopSelector.Audio;

namespace WavLoopSelector {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IAudioStream stream;
            string message = null;
            try {
                stream = WAV.FromFile("in.wav", ignoreUnknownChunks: false);
            } catch (PCMStream.UnknownChunkException e) {
                message = e.Message;
                stream = WAV.FromFile("in.wav", ignoreUnknownChunks: true);
            }
            using (LoopSelectorForm dialog = new LoopSelectorForm(stream)) {
                if (message != null)
                    dialog.Shown += (o, e) => MessageBox.Show(dialog, message);
                Application.Run(dialog);
                if (dialog.DialogResult != DialogResult.OK)
                    return;
            }

            byte[] output = WAV.ToByteArray(stream, appendSmplChunk: true);
            File.WriteAllBytes("out.wav", output);
        }
    }
}

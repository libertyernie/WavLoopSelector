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
  WavLoopSelector.exe [in-file]

Both in-file and out-file can be '-' for stdin/stdout.";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length != 1 && args.Length != 2) {
                MessageBox.Show(USAGE);
                return 64;
            }

            string infile = args[0];
            string outfile = args.Length == 2
                ? args[1]
                : null;

            byte[] input;
            if (infile == "-") {
                using (var stdin = Console.OpenStandardInput()) {
                    try {
                        input = ReadRIFFFromStream(stdin);
                    } catch (InvalidDataException e) {
                        Console.Error.WriteLine(e.Message);
                        return 65;
                    }
                }
            } else {
                if (!File.Exists(infile)) {
                    MessageBox.Show("Could not find file " + infile, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 66;
                }

                input = File.ReadAllBytes(infile);
            }

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
                    return 1;
            }

            byte[] output = WAV.ToByteArray(stream, appendSmplChunk: true);
            if (outfile == "-") {
                Console.OpenStandardOutput().Write(output, 0, output.Length);
            } else {
                File.WriteAllBytes(outfile, output);
            }

            stream.Dispose();
            return 0;
        }

        private static byte[] ReadRIFFFromStream(Stream stdin) {
            byte[] header = new byte[8];
            if (stdin.Read(header, 0, 8) != 8) {
                throw new InvalidDataException("Could not read standard input (too short)");
            }

            string tag = "";
            for (int i = 0; i < 4; i++) {
                tag += (char)header[i];
            }
            if (tag != "RIFF") {
                throw new InvalidDataException("Could not read standard input (not a RIFF container)");
            }

            int size = 0;
            size |= header[4] << 0;
            size |= header[5] << 8;
            size |= header[6] << 16;
            size |= header[7] << 24;
            if (size == 0 || size == -1) {
                throw new InvalidDataException("Could not read standard input (length missing at 0x4)");
            }

            byte[] input = new byte[size + 8];
            Array.Copy(header, input, 8);

            int offset = 8;
            byte[] buffer = new byte[4096];
            do {
                Console.Error.Write($"\r{offset}");
                int read = stdin.Read(buffer, 0, Math.Min(size - offset, buffer.Length));
                Array.Copy(buffer, 0, input, offset, read);
                offset += read;
            } while (offset < size);
            Console.Error.WriteLine(" bytes read");

            return input;
        }
    }
}

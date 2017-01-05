using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WavLoopSelector {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            label1.Text = Program.USAGE;
        }

        private void btnOpen_Click(object sender, EventArgs e) {
            using (OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "16-bit PCM WAV files|*.wav";
                if (dialog.ShowDialog(this) != DialogResult.OK) {
                    return;
                }
                Program.Main(this, dialog.FileName, null);
            }
        }
    }
}

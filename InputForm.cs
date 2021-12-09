using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ArchiveManager {
    public partial class InputForm : DevExpress.XtraEditors.XtraForm {

        public bool ok = false;
        public string input_str = "";

        public InputForm() { InitializeComponent(); }

        public InputForm(string form_caption, string label_caption, string default_str) {
            InitializeComponent(); this.Text = form_caption; captionLbl.Text = label_caption; inputBox.Text = default_str;
        }

        private void okBtn_Click(object sender, EventArgs e) {
            if(inputBox.Text == "") {
                MessageBox.Show("Lütfen Zorunlu Alanları Boş Geçmeyiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                inputBox.SelectAll(); inputBox.Focus(); return;
            }

            input_str = inputBox.Text; ok = true; this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Enter) okBtn_Click(null, null);
        }
    }
}
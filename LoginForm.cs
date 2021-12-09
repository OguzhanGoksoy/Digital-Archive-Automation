using System;
using System.Windows.Forms;

using static ArchiveManager.archive_db;

namespace ArchiveManager {
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm {

        public bool login = false;

        public LoginForm() {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e) {
            login_info.usr_lst = db_list_user_records(); nameBox.SelectAll(); nameBox.Focus();
        }

        private void nameBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Enter) okBtn_Click(null, null);
        }

        private void okBtn_Click(object sender, EventArgs e) {
            if(nameBox.Text == "") { msg.warn("Lütfen Kullanıcı Adınızı Giriniz!"); nameBox.SelectAll(); nameBox.Focus(); return; }

            if(pswBox.Text == "") { msg.warn("Lütfen Şifrenizi Giriniz!"); pswBox.SelectAll(); pswBox.Focus(); return; }

            login_info.act_user = login_info.usr_lst.Find(i => i.user_name == nameBox.Text && i.psw == pswBox.Text && !i.del);

            if(login_info.act_user == null) {
                msg.warn("Lütfen Kullanıcı Adınızı ve Şifrenizi Kontrol Ediniz!"); nameBox.SelectAll(); nameBox.Focus(); return;
            }

            login = true; this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
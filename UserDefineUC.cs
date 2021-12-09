using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

using static ArchiveManager.archive_t;
using static ArchiveManager.archive_db;
using static ArchiveManager.login_info;

namespace ArchiveManager {
    public partial class UserDefineUC : DevExpress.XtraEditors.XtraUserControl {

        enum mode_t {
            none,
            new_record,
            edit_record
        }

        user_def_t _udt = null;
        mode_t _mode = mode_t.none;

        public UserDefineUC() { InitializeComponent(); }

        void update_editors() {
            editPanel.Enabled = _mode != mode_t.none;

            saveBtn.Enabled = cancelBtn.Enabled = _mode != mode_t.none;
            newBtn.Enabled = editBtn.Enabled = delBtn.Enabled = _mode == mode_t.none;
        }

        void load_records() {
            update_editors();

            List<user_def_t> lst = usr_lst.FindAll(i => !i.del);

            db.grid_load(lst, gc1, "!id", "Adı Soyadı", "Kullanıcı Adı", "!psw", "Oluşturma Tarihi", "!cre_user_id", "!del", "!del_time", "!del_user_id",
                "!auth_lst");

            if(gw1.Columns["name"].Summary.Count == 0) gw1.Columns["name"].Summary.Add(DevExpress.Data.SummaryItemType.Count);
        }

        private void UserDefineUC_Load(object sender, EventArgs e) {
            load_records();
        }

        private void gw1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            if(e.FocusedRowHandle < 0) return;

            int user_id = int.Parse(gw1.GetFocusedRowCellValue("id").ToString());

            _udt = usr_lst.Find(i => i.id == user_id && !i.del);

            if(_udt == null) return;

            nameBox.Text = _udt.name;
            userNameBox.Text = _udt.user_name;
            pswBox1.Text = pswBox2.Text = _udt.psw;

            creDirCheck.Checked = !_udt.auth_lst.Find(i => i.name == "cre_dir").del;
            delDirCheck.Checked = !_udt.auth_lst.Find(i => i.name == "del_dir").del;
            renameDirCheck.Checked = !_udt.auth_lst.Find(i => i.name == "rename_dir").del;
            moveDirCheck.Checked = !_udt.auth_lst.Find(i => i.name == "move_dir").del;
            addFileCheck.Checked = !_udt.auth_lst.Find(i => i.name == "add_file").del;
            extractFileCheck.Checked = !_udt.auth_lst.Find(i => i.name == "extract_file").del;
            delFileCheck.Checked = !_udt.auth_lst.Find(i => i.name == "del_file").del;
            renameFileCheck.Checked = !_udt.auth_lst.Find(i => i.name == "rename_file").del;
            moveFileCheck.Checked = !_udt.auth_lst.Find(i => i.name == "move_file").del;
        }

        private void gw1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if(e.Column == null || e.Value == null) return;

            if(e.Column.FieldName.EndsWith("time")) e.DisplayText = date_to_str(e.Value);
        }

        private void newBtn_Click(object sender, EventArgs e) {
            if(_udt == null) return;
            _mode = mode_t.new_record; update_editors();
        }

        private void editBtn_Click(object sender, EventArgs e) {
            if(_udt == null) return;
            _mode = mode_t.edit_record; update_editors();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            user_def_t udt = null;

            if(nameBox.Text == "") { msg.warn("Lütfen Zorunlu Alanları Kontrol Ediniz!"); nameBox.SelectAll(); nameBox.Focus(); return; }
            if(userNameBox.Text == "") { msg.warn("Lütfen Zorunlu Alanları Kontrol Ediniz!"); userNameBox.SelectAll(); userNameBox.Focus(); return; }
            if(pswBox1.Text == "") { msg.warn("Lütfen Zorunlu Alanları Kontrol Ediniz!"); pswBox1.SelectAll(); pswBox1.Focus(); return; }
            if(pswBox1.Text != pswBox2.Text) { msg.warn("Lütfen Şifrenizi Kontrol Ediniz!"); pswBox1.SelectAll(); pswBox1.Focus(); return; }

            switch(_mode) {
                case mode_t.new_record:
                    if(usr_lst.Find(i => i.user_name == userNameBox.Text && !i.del) != null) {
                        msg.warn("Bu İsimde Bir Kullanıcı Zaten Tanımlıdır!"); userNameBox.SelectAll(); userNameBox.Focus(); return;
                    }

                    udt = new user_def_t() {
                        name = nameBox.Text,
                        user_name = userNameBox.Text,
                        psw = pswBox1.Text,
                        cre_time = DateTime.Now,
                        cre_user_id = act_user.id
                    };

                    if(!db_create_user_record(udt)) { msg.err("Bir Hata Oluştu!"); return; }

                    udt.auth_lst.Add(new user_auth_def_t() { name = "cre_dir", user_id = udt.id, del = !creDirCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "del_dir", user_id = udt.id, del = !delDirCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "rename_dir", user_id = udt.id, del = !renameDirCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "move_dir", user_id = udt.id, del = !moveDirCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "add_file", user_id = udt.id, del = !addFileCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "extract_file", user_id = udt.id, del = !extractFileCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "del_file", user_id = udt.id, del = !delFileCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "rename_file", user_id = udt.id, del = !renameFileCheck.Checked });
                    udt.auth_lst.Add(new user_auth_def_t() { name = "move_file", user_id = udt.id, del = !moveFileCheck.Checked });

                    for(int i = 0; i < udt.auth_lst.Count; i++)
                        if(!db_create_user_auth_record(udt.auth_lst[i])) { msg.err("Bir Hata Oluştu!"); return; }

                    usr_lst.Add(udt); gc1.RefreshDataSource(); gw1.BestFitColumns();

                    msg.inf("Kullanıcı Başarıyla Eklendi!", "Kullanıcı Ekle");
                    break;

                case mode_t.edit_record:
                    if(usr_lst.Find(i => i.user_name == userNameBox.Text && i.id != _udt.id && !i.del) != null) {
                        msg.warn("Bu İsimde Bir Kullanıcı Zaten Tanımlıdır!"); userNameBox.SelectAll(); userNameBox.Focus(); return;
                    }

                    udt = new user_def_t() {
                        id = _udt.id,
                        name = nameBox.Text,
                        user_name = userNameBox.Text,
                        psw = pswBox1.Text,
                        cre_user_id = act_user.id
                    };

                    if(!db_set_user_record(udt)) { msg.err("Bir Hata Oluştu!"); return; }

                    _udt.auth_lst.Find(i => i.name == "cre_dir").del = !creDirCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "del_dir").del = !delDirCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "rename_dir").del = !renameDirCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "move_dir").del = !moveDirCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "add_file").del = !addFileCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "extract_file").del = !extractFileCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "del_file").del = !delFileCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "rename_file").del = !renameFileCheck.Checked;
                    _udt.auth_lst.Find(i => i.name == "move_file").del = !moveFileCheck.Checked;

                    for(int i = 0; i < _udt.auth_lst.Count; i++)
                        if(!db_set_user_auth_record(_udt.auth_lst[i])) { msg.err("Bir Hata Oluştu!"); return; }

                    _udt.name = nameBox.Text;
                    _udt.user_name = userNameBox.Text;
                    _udt.psw = pswBox1.Text;

                    gc1.RefreshDataSource(); gw1.BestFitColumns();

                    msg.inf("Kullanıcı Başarıyla Güncellendi!", "Kullanıcı Güncellendi");
                    break;
            }
            _mode = mode_t.none; update_editors();
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            _mode = mode_t.none; update_editors();
        }

        private void delBtn_Click(object sender, EventArgs e) {
            if(_udt == null) return;

            DialogResult dr = msg.yn("Seçilen Kullanıcı Silinsin Mi?", "Sil");

            if(dr == DialogResult.No) return;

            _udt.del_user_id = act_user.id; _udt.del_time = DateTime.Now;

            if(!db_delete_user_record(_udt)) { msg.err("Bir Hata Oluştu!"); return; }

            _udt = null;

            load_records();

            msg.inf("Kullanıcı Başarıyla Silindi!", "Kullanıcı Sil");
        }
    }
}

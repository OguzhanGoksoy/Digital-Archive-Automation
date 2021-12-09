using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using static ArchiveManager.archive_t;
using static ArchiveManager.archive_db;
using static ArchiveManager.archive_fs;

namespace ArchiveManager {
    public partial class ArchiveForm : DevExpress.XtraEditors.XtraForm {
        byte _result_code = 0;

        dir_def_t _act_dir = null;
        List<dir_def_t> _dir_lst = null;

        public ArchiveForm() {
            InitializeComponent();

            List<file_def_t> lst = new List<file_def_t>();
            db.grid_load(lst, gc1, "!id", "!dir_id", "Dosya Adı", "Dosya Türü", "Dosya Boyutu", "Oluşturma Tarihi", "Değiştirme Tarihi", "!cre_user_id",
                "Silindi", "!del_time", "!del_user_id");
        }

        void set_buttons(bool en) {
            expandBtn.Enabled = creDirBtn.Enabled = delDirBtn.Enabled = renameDirBtn.Enabled = moveDirBtn.Enabled = en;
            addFileBtn.Enabled = extractFileBtn.Enabled = delFileBtn.Enabled = renameFileBtn.Enabled = moveFileBtn.Enabled = en;
        }

        #region Archive Functions

        /// <summary>
        /// Aktif Çalışma Dizini
        /// </summary>
        /// <param name="ddt">Aktif Klasör</param>
        /// <returns></returns>
        string get_dir_full_path(dir_def_t ddt) {
            string path = "";

            if(ddt.par_dir != null) path += get_dir_full_path(ddt.par_dir);

            path += @"\" + ddt.name;

            return path;
        }

        /// <summary>
        /// Node Klonla
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        TreeListNode clone_node(TreeListNode node) {
            TreeListNode copy_node = (TreeListNode)node.Clone();

            foreach(TreeListNode sub_node in node.Nodes) {
                copy_node.Nodes.Add(clone_node(sub_node));
            }

            return copy_node;
        }

        /// <summary>
        /// Node Bul
        /// </summary>
        /// <param name="node_lst"></param>
        /// <param name="dir_id"></param>
        /// <returns></returns>
        TreeListNode find_node(TreeListNodes node_lst, int dir_id) {
            foreach(TreeListNode node in node_lst) {
                if(int.Parse(node.GetValue("id").ToString()) == dir_id) return node;

                TreeListNode node_inner = find_node(node.Nodes, dir_id);
                if(node_inner != null) return node_inner;
            }
            return null;
        }

        /// <summary>
        /// Klasör Bul
        /// </summary>
        /// <param name="dir_lst">Klasör Listesi</param>
        /// <param name="dir_id">Klasör ID</param>
        /// <returns></returns>
        dir_def_t find_dir(List<dir_def_t> dir_lst, int dir_id) {
            foreach(dir_def_t ddt in dir_lst) {
                if(ddt.id == dir_id) return ddt;

                dir_def_t ddt_inner = find_dir(ddt.dir_lst, dir_id);
                if(ddt_inner != null) return ddt_inner;
            }
            return null;
        }

        /// <summary>
        /// Alt Klasör Hiyerarşisini Yükle
        /// </summary>
        /// <param name="par_node">Ana TreeNode</param>
        /// <param name="dir_lst">Klasör Listesi</param>
        void load_sub_directory_tree(TreeListNode par_node, List<dir_def_t> dir_lst) {
            foreach(dir_def_t ddt in dir_lst)
                load_sub_directory_tree(par_node.Nodes.Add(ddt.id, ddt.dir_id, ddt.name, ddt.cre_time, ddt.wri_time, ddt.del), ddt.dir_lst);
        }

        /// <summary>
        /// Klasör Hiyerarşisini Yükle
        /// </summary>
        void load_directory_tree(TreeList tl) {
            tl.Nodes.Clear();

            //Root Klasör Listesini Al
            _dir_lst = db_list_dir_records(null).FindAll(i => !i.del);

            foreach(dir_def_t ddt in _dir_lst)
                load_sub_directory_tree(tl.Nodes.Add(ddt.id, ddt.dir_id, ddt.name, ddt.cre_time, ddt.wri_time, ddt.del), ddt.dir_lst);

            tl.BestFitColumns();
        }

        #endregion

        private void HomePage_Load(object sender, EventArgs e) {
            string archive_part_dir = Path.Combine(Environment.CurrentDirectory, "archive_files");

            if(!fs_init(archive_part_dir)) { msg.err("Arşiv Başlatma Hatası!"); return; }

            //Arşiv Klasörlerini Listele
            load_directory_tree(dirTree);
        }

        private void HomePage_SizeChanged(object sender, EventArgs e) {
            dirPanel.Size = new Size(this.Width / 2, dirPanel.Height);
        }

        private void dirTree_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e) {
            if(e.Node == null) return;

            int dir_id = int.Parse(e.Node.GetValue("id").ToString());

            _act_dir = find_dir(_dir_lst, dir_id);

            gw1.ViewCaption = "'" + get_dir_full_path(_act_dir) + "' Klasör İçeriği";

            List<file_def_t> file_lst = _act_dir != null ? _act_dir.file_lst : new List<file_def_t>();

            db.grid_load(file_lst, gc1, "!id", "!dir_id", "Dosya Adı", "Dosya Türü", "Dosya Boyutu", "Oluşturma Tarihi", "Değiştirme Tarihi", "!cre_user_id",
                "!del", "!del_time", "!del_user_id");

            if(gw1.Columns["name"].Summary.Count == 0) gw1.Columns["name"].Summary.Add(DevExpress.Data.SummaryItemType.Count);
            if(gw1.Columns["size"].Summary.Count == 0) gw1.Columns["size"].Summary.Add(DevExpress.Data.SummaryItemType.Sum);
        }

        private void gw1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if(e.Column == null || e.Value == null) return;

            if(e.Column.FieldName == "size") e.DisplayText = len_to_str(long.Parse(e.Value.ToString()));
            if(e.Column.FieldName.EndsWith("time")) e.DisplayText = date_to_str(e.Value);
        }

        #region Klasör İşlem Butonları
        private void expandBtn_Click(object sender, EventArgs e) {
            if(expandBtn.Tag == null) { dirTree.ExpandAll(); expandBtn.Tag = 1; }
            else { dirTree.CollapseAll(); expandBtn.Tag = null; }
        }

        private void creDirBtn_Click(object sender, EventArgs e) {
            if(!login_info.has_auth(login_info.auth_t.cre_dir)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            InputForm inf = new InputForm("Klasör Ekle", "Yeni Klasör İsmi", _act_dir == null ? "root" : "Yeni Klasör");
            inf.ShowDialog(this);
            if(inf.ok) {
                dir_def_t ddt = null;

                //Aynı isimde farklı bir klasör bulunma durumu
                ddt = (_act_dir != null ? _act_dir.dir_lst : _dir_lst).Find(i => i.name == inf.input_str);

                if(ddt != null) { msg.warn("Bu İsimde Bir Klasör Zaten Mevcuttur!", "Klasör Ekle"); return; }

                ddt = new dir_def_t() {
                    id = 0,
                    dir_id = _act_dir == null ? 0 : _act_dir.id,
                    name = inf.input_str,
                    cre_time = DateTime.Now,
                    wri_time = DateTime.Now,
                    cre_user_id = login_info.act_user.id,
                    par_dir = _act_dir
                };

                if(!db_create_dir_record(ddt)) { msg.err("Klasör Oluşturma Hatası!", "Klasör Ekle"); return; }

                TreeListNode tln = null;

                if(_act_dir == null) {
                    _dir_lst.Add(ddt); tln = dirTree.Nodes.Add(ddt.id, ddt.dir_id, ddt.name, ddt.cre_time, ddt.wri_time, ddt.del);
                }
                else {
                    _act_dir.dir_lst.Add(ddt); tln = dirTree.FocusedNode.Nodes.Add(ddt.id, ddt.dir_id, ddt.name, ddt.cre_time, ddt.wri_time, ddt.del);
                }

                dirTree.FocusedNode = tln;

                proc_log_def_t pld = new proc_log_def_t() {
                    proc_type = "Klasör Ekle",
                    dir_id = ddt.id,
                    file_id = 0,
                    source_dir_id = 0,
                    dest_dir_id = _act_dir.id,
                    old_name = "",
                    proc_date = DateTime.Now,
                    user_id = login_info.act_user.id
                };

                if(!db_create_proc_log_record(pld)) { msg.err("Klasör Oluşturma Hatası!", "Klasör Ekle"); return; }

                msg.inf("Klasör Başarıyla Oluşturuldu!", "Klasör Ekle");
            }

            inf.Dispose();
        }

        private void delDirBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null) return;

            if(_act_dir.par_dir == null) { msg.warn("'root' Klasörü Silenemez!"); return; }

            if(!login_info.has_auth(login_info.auth_t.del_dir)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            if(msg.yn("Seçilen Klasör Silinsin Mi?", "Klasör Sil") == DialogResult.No) return;

            _act_dir.del_time = DateTime.Now; _act_dir.del_user_id = login_info.act_user.id;

            //Veritabanı Klasör Kaydını Sil
            if(!db_delete_dir_record(_act_dir)) { msg.err("Klasör Silme Hatası!", "Klasör Sil"); return; }

            //Aktif Klasörü Sil
            (_act_dir.par_dir != null ? _act_dir.par_dir.dir_lst : _dir_lst).Remove(_act_dir);

            //Aktif Node'u sil
            TreeListNode act_node = dirTree.FocusedNode;
            (act_node.ParentNode != null ? act_node.ParentNode.Nodes : dirTree.Nodes).Remove(act_node);

            proc_log_def_t pld = new proc_log_def_t() {
                proc_type = "Klasör Sil",
                dir_id = _act_dir.id,
                file_id = 0,
                source_dir_id = _act_dir.dir_id,
                dest_dir_id = 0,
                old_name = "",
                proc_date = DateTime.Now,
                user_id = login_info.act_user.id
            };

            if(!db_create_proc_log_record(pld)) { msg.err("Klasör Silme Hatası!", "Klasör Sil"); return; }

            _act_dir = null;

            msg.inf("Klasör Başarıyla Silindi!", "Klasör Sil");
        }

        private void renameDirBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null) return;

            if(_act_dir.name == "root" && _act_dir.par_dir == null) { msg.warn("'root' Klasörü Yeniden Adlandırılamaz!"); return; }

            if(!login_info.has_auth(login_info.auth_t.rename_dir)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            InputForm inf = new InputForm("Klasör Yeniden Adlandır", "Yeni Klasör İsmi", _act_dir.name);
            inf.ShowDialog(this);
            if(inf.ok) {
                dir_def_t ddt = null;

                //Aynı isimde farklı bir klasör bulunma durumu
                ddt = (_act_dir.par_dir != null ? _act_dir.par_dir.dir_lst : _dir_lst).Find(i => i.id != _act_dir.id && i.name == inf.input_str);

                if(ddt != null) { msg.warn("Bu İsimde Bir Klasör Zaten Mevcuttur!", "Klasör Yeniden Adlandır"); return; }

                string old_dir_name = _act_dir.name; _act_dir.name = inf.input_str; _act_dir.wri_time = DateTime.Now;

                if(!db_set_dir_record(_act_dir)) { msg.err("Klasör Yeniden Adlandırma Hatası!", "Klasör Yeniden Adlandır"); return; }

                dirTree.FocusedNode.SetValue("name", _act_dir.name); dirTree.FocusedNode.SetValue("wri_time", DateTime.Now);

                proc_log_def_t pld = new proc_log_def_t() {
                    proc_type = "Klasör Yeniden Adlandır",
                    dir_id = _act_dir.id,
                    file_id = 0,
                    source_dir_id = 0,
                    dest_dir_id = 0,
                    old_name = old_dir_name,
                    proc_date = DateTime.Now,
                    user_id = login_info.act_user.id
                };

                if(!db_create_proc_log_record(pld)) { msg.err("Klasör Yeniden Adlandırma Hatası!", "Klasör Yeniden Adlandır"); return; }

                msg.inf("Klasör İsmi Başarıyla Değiştirildi!", "Klasör Yeniden Adlandır");
            }

            inf.Dispose();
        }

        private void moveDirBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null) return;

            if(!login_info.has_auth(login_info.auth_t.move_dir)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            if(_act_dir.par_dir == null) { msg.warn("'root' Klasörü Taşınamaz!"); return; }

            DirectorySelectForm dsf = new DirectorySelectForm() { dir_lst = _dir_lst };
            dsf.ShowDialog(this);
            if(dsf.ok) {
                //Klasörün kendi içine taşınma durumu
                if(_act_dir.id == dsf.dest_dir.id) { msg.warn("Klasör Kendi İçine Taşınamaz!"); return; }

                //Klasörün kendi alt klasörlerinden birine taşınma durumu
                if(find_dir(_act_dir.dir_lst, dsf.dest_dir.id) != null) { msg.warn("Klasör Kendi Alt Klasörlerinden Birine Taşınamaz!"); return; }

                //Klasörün bulunduğu konuma tekrar taşınması durumu
                if(_act_dir.dir_id == dsf.dest_dir.id) { msg.warn("Hedef Klasör Kaynak Klasörden Farklı Olmalıdır!"); return; }

                //Hedef klasörde aynı isimde bir klasör bulunması durumu
                if(dsf.dest_dir.dir_lst.Find(i => i.name == _act_dir.name) != null) {
                    msg.warn("Hedef Klasörde Zaten Bu İsimde Bir Klasör Mevcuttur!"); return;
                }

                proc_log_def_t pld = new proc_log_def_t() {
                    proc_type = "Klasör Taşı",
                    dir_id = _act_dir.id,
                    file_id = 0,
                    source_dir_id = _act_dir.dir_id,
                    dest_dir_id = dsf.dest_dir.id,
                    old_name = "",
                    proc_date = DateTime.Now,
                    user_id = login_info.act_user.id
                };

                if(!db_create_proc_log_record(pld)) { msg.err("Klasör Taşıma Hatası!", "Klasör Taşı"); return; }

                //Aktif Klasörü Sil
                _act_dir.par_dir.dir_lst.Remove(_act_dir); dsf.dest_dir.dir_lst.Add(_act_dir);

                _act_dir.par_dir = dsf.dest_dir; _act_dir.dir_id = dsf.dest_dir.id; _act_dir.wri_time = DateTime.Now;

                //Veritabanı klasör kaydını güncelle
                if(!db_set_dir_record(_act_dir)) { msg.err("Klasör Taşıma Hatası!", "Klasör Taşı"); return; }

                //Aktif Node'u sil
                TreeListNode act_node = dirTree.FocusedNode, new_node = clone_node(dirTree.FocusedNode);
                (act_node.ParentNode != null ? act_node.ParentNode.Nodes : dirTree.Nodes).Remove(act_node);

                TreeListNode dest_node = find_node(dirTree.Nodes, dsf.dest_dir.id);

                dest_node.Nodes.Add(new_node);

                msg.inf("Klasör Başarıyla Taşındı!", "Klasör Taşı");
            }
            dsf.Dispose();
        }
        #endregion

        #region Dosya İşlem Butonları
        private async void addFileBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null) return;

            if(!login_info.has_auth(login_info.auth_t.add_file)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            set_buttons(false);

            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK) {
                FileInfo fi = new FileInfo(ofd.FileName);

                if(_act_dir.file_lst.Find(i => i.name == fi.Name) == null) {//Dosya bu klasörde var mı?
                    add_file_result_t afrt = await fs_add_file_async(_act_dir, ofd.FileName);

                    switch(afrt.result_code) {
                        case 0: msg.inf("Dosya Arşive Başarıyla Eklendi", "Dosya Ekle"); break;
                        case 1: msg.err("Part Oluşturma Hatası!", "Dosya Ekle"); break;
                        case 2: msg.err("Segment Güncelleme Hatası!", "Dosya Ekle"); break;
                        case 3: msg.err("Segment Oluşturma Hatası!", "Dosya Ekle"); break;
                    }

                    if(afrt.result_code == 0) {
                        proc_log_def_t pld = new proc_log_def_t() {
                            proc_type = "Dosya Ekle",
                            dir_id = 0,
                            file_id = afrt.fdt.id,
                            source_dir_id = 0,
                            dest_dir_id = _act_dir.id,
                            old_name = "",
                            proc_date = DateTime.Now,
                            user_id = login_info.act_user.id
                        };

                        if(!db_create_proc_log_record(pld)) { msg.err("Dosya Ekleme Hatası!", "Dosya Ekle"); return; }

                        _act_dir.file_lst.Add(afrt.fdt); gc1.RefreshDataSource(); gw1.BestFitColumns();
                    }
                }
                else msg.warn("Bu Dosya Zaten Mevcut!", "Dosya Ekle");
            }
            ofd.Dispose();

            set_buttons(true);
        }

        private async void extractFileBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null || gw1.FocusedRowHandle < 0) return;

            if(!login_info.has_auth(login_info.auth_t.extract_file)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            set_buttons(false);

            int file_id = int.Parse(gw1.GetFocusedRowCellValue("id").ToString());

            file_def_t fdt = _act_dir.file_lst.Find(i => i.id == file_id);

            if(fdt != null) {
                SaveFileDialog sfd = new SaveFileDialog() { FileName = fdt.name };
                if(sfd.ShowDialog() == DialogResult.OK) {
                    _result_code = await fs_extract_file_async(fdt, sfd.FileName);

                    switch(_result_code) {
                        case 0: msg.inf("Dosya Arşivden Başarıyla Çıkartıldı", "Dosya Çıkart"); break;
                        case 1: msg.err("Part Listeleme Hatası!", "Dosya Çıkart"); break;
                    }

                    proc_log_def_t pld = new proc_log_def_t() {
                        proc_type = "Dosya Çıkart",
                        dir_id = 0,
                        file_id = fdt.id,
                        source_dir_id = _act_dir.id,
                        dest_dir_id = 0,
                        old_name = "",
                        proc_date = DateTime.Now,
                        user_id = login_info.act_user.id
                    };

                    if(!db_create_proc_log_record(pld)) { msg.err("Dosya Çıkartma Hatası!", "Dosya Çıkart"); return; }
                }
                sfd.Dispose();
            }

            set_buttons(true);
        }

        private async void delFileBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null || gw1.FocusedRowHandle < 0) return;

            if(!login_info.has_auth(login_info.auth_t.del_file)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            if(msg.yn("Seçilen Dosya Silindin Mi?", "Dosya Sil") == DialogResult.No) return;

            set_buttons(false);

            int file_id = int.Parse(gw1.GetFocusedRowCellValue("id").ToString());

            file_def_t fdt = _act_dir.file_lst.Find(i => i.id == file_id);

            if(fdt != null) {
                _result_code = await fs_delete_file_async(fdt);

                switch(_result_code) {
                    case 0: msg.inf("Dosya Başarıyla Silindi", "Dosya Sil"); break;
                    case 1: msg.err("Segment Listeleme Hatası!", "Dosya Sil"); break;
                    case 2: msg.err("Segment Silme Hatası!", "Dosya Sil"); break;
                    case 3: msg.err("Dosya Silme Hatası!", "Dosya Sil"); break;
                    case 4: msg.err("Defrag: Segment Listeleme Hatası!", "Dosya Sil"); break;
                    case 5: msg.err("Defrag: Segment Silme Hatası!", "Dosya Sil"); break;
                    case 6: msg.err("Defrag: Segment Güncelleme Hatası!", "Dosya Sil"); break;
                }

                if(_result_code == 0) {
                    proc_log_def_t pld = new proc_log_def_t() {
                        proc_type = "Dosya Sil",
                        dir_id = 0,
                        file_id = fdt.id,
                        source_dir_id = _act_dir.id,
                        dest_dir_id = 0,
                        old_name = "",
                        proc_date = DateTime.Now,
                        user_id = login_info.act_user.id
                    };

                    if(!db_create_proc_log_record(pld)) { msg.err("Dosya Silme Hatası!", "Dosya Sil"); return; }

                    _act_dir.file_lst.Remove(fdt); gc1.RefreshDataSource(); gw1.BestFitColumns();
                }
            }

            set_buttons(true);
        }

        private void renameFileBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null || gw1.FocusedRowHandle < 0) return;

            if(!login_info.has_auth(login_info.auth_t.rename_file)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            int file_id = int.Parse(gw1.GetFocusedRowCellValue("id").ToString());

            file_def_t fdt = _act_dir.file_lst.Find(i => i.id == file_id);

            if(fdt != null) {
                InputForm inf = new InputForm("Dosya Yeniden Adlandır", "Yeni Dosya İsmi", fdt.name);
                inf.ShowDialog(this);
                if(inf.ok) {
                    file_def_t fdt_check = _act_dir.file_lst.Find(i => i.id != fdt.id && i.name == inf.input_str);

                    if(fdt_check != null) { msg.warn("Bu İsimde Bir Dosya Zaten Mevcuttur!", "Dosya Yeniden Adlandır"); return; }

                    string old_file_name = fdt.name; fdt.name = inf.input_str; fdt.wri_time = DateTime.Now;

                    if(!db_set_file_record(fdt)) { msg.err("Dosya Yeniden Adlandırma Hatası!", "Dosya Yeniden Adlandır"); return; }

                    proc_log_def_t pld = new proc_log_def_t() {
                        proc_type = "Dosya Yeniden Adlandır",
                        dir_id = 0,
                        file_id = fdt.id,
                        source_dir_id = 0,
                        dest_dir_id = 0,
                        old_name = old_file_name,
                        proc_date = DateTime.Now,
                        user_id = login_info.act_user.id
                    };

                    if(!db_create_proc_log_record(pld)) { msg.err("Dosya Yeniden Adlandırma Hatası!", "Dosya Yeniden Adlandır"); return; }

                    gc1.RefreshDataSource(); gw1.BestFitColumns();

                    msg.inf("Dosya İsmi Başarıyla Değiştirildi!", "Dosya Yeniden Adlandır");
                }

                inf.Dispose();
            }
        }

        private void moveFileBtn_Click(object sender, EventArgs e) {
            if(_act_dir == null || gw1.FocusedRowHandle < 0) return;

            if(!login_info.has_auth(login_info.auth_t.move_file)) { msg.warn("Bu İşlemi Yapma Yetkisine Sahip Değilsiniz!"); return; }

            int file_id = int.Parse(gw1.GetFocusedRowCellValue("id").ToString());

            file_def_t fdt = _act_dir.file_lst.Find(i => i.id == file_id);

            if(fdt != null) {
                DirectorySelectForm dsf = new DirectorySelectForm() { dir_lst = _dir_lst };
                dsf.ShowDialog(this);
                if(dsf.ok) {
                    //Hedef klasörde aynı isimde bir dosya bulunması durumu
                    file_def_t fdt_check = dsf.dest_dir.file_lst.Find(i => i.name == fdt.name);
                    if(fdt_check != null) { msg.warn("Hedef Klasörde Zaten Aynı İsimde Bir Dosya Mevcuttur!"); return; }

                    fdt.dir_id = dsf.dest_dir.id; fdt.wri_time = DateTime.Now;

                    //Veritabanı dosya kaydını güncelle
                    if(!db_set_file_record(fdt)) { msg.err("Dosya Taşıma Hatası!", "Dosya Taşı"); return; }

                    //Dosyayı Taşı
                    _act_dir.file_lst.Remove(fdt); dsf.dest_dir.file_lst.Add(fdt);

                    dirTree_FocusedNodeChanged(null, new FocusedNodeChangedEventArgs(null, dirTree.FocusedNode));

                    proc_log_def_t pld = new proc_log_def_t() {
                        proc_type = "Dosya Taşı",
                        dir_id = 0,
                        file_id = fdt.id,
                        source_dir_id = _act_dir.id,
                        dest_dir_id = dsf.dest_dir.id,
                        old_name = "",
                        proc_date = DateTime.Now,
                        user_id = login_info.act_user.id
                    };

                    if(!db_create_proc_log_record(pld)) { msg.err("Dosya Taşıma Hatası!", "Dosya Taşı"); return; }

                    msg.inf("Dosya Başarıyla Taşındı!", "Dosya Taşı");
                }

                dsf.Dispose();
            }
        }
        #endregion
    }
}
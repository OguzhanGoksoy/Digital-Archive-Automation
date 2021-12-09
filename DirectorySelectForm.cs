using System;
using System.Collections.Generic;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList;

using static ArchiveManager.archive_t;
using static ArchiveManager.archive_db;

namespace ArchiveManager {
    public partial class DirectorySelectForm : DevExpress.XtraEditors.XtraForm {

        public bool ok = false;
        public dir_def_t dest_dir = null;
        public List<dir_def_t> dir_lst = null;

        public DirectorySelectForm() {
            InitializeComponent();
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

            foreach(dir_def_t ddt in dir_lst)
                load_sub_directory_tree(tl.Nodes.Add(ddt.id, ddt.dir_id, ddt.name, ddt.cre_time, ddt.wri_time, ddt.del), ddt.dir_lst);

            tl.BestFitColumns();
        }

        #endregion

        private void DirectorySelectForm_Load(object sender, EventArgs e) {
            //Arşiv Klasörlerini Listele
            load_directory_tree(dirTree);
        }

        private void dirTree_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e) {
            if(e.Node == null) return;

            int dir_id = int.Parse(e.Node.GetValue("id").ToString());

            dest_dir = find_dir(dir_lst, dir_id);

            pathBox.Text = get_dir_full_path(dest_dir);
        }

        private void expandBtn_Click(object sender, EventArgs e) {
            if(expandBtn.Tag == null) { dirTree.ExpandAll(); expandBtn.Tag = 1; }
            else { dirTree.CollapseAll(); expandBtn.Tag = null; }
        }

        private void okBtn_Click(object sender, EventArgs e) {
            if(dest_dir == null) return;

            ok = true;

            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
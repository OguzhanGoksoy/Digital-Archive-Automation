using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ArchiveManager {
    public partial class ArchiveProcReportUC : DevExpress.XtraEditors.XtraUserControl {

        string _sql = "";

        public ArchiveProcReportUC() {
            InitializeComponent();
        }

        private void ArchiveProcReportUC_Load(object sender, EventArgs e) {
            DataTable dt = new DataTable();

            _sql = "select pl.proc_type, dd.name as dir_name, fd.name as file_name, sdd.name as source_dir_name, ddd.name as dest_dir_name, " +
                "pl.old_name, pl.proc_date, us.name from proc_log pl left join dir_def dd on dd.id=pl.dir_id left join file_def fd on " +
                "fd.id=pl.file_id left join dir_def sdd on sdd.id=pl.source_dir_id left join dir_def ddd on ddd.id=pl.dest_dir_id left join " +
                "user_def us on us.id=pl.user_id";

            db.grid_load(dt, gc1, _sql, "İşlem Tipi", "Klasör Adı", "Dosya Adı", "Kaynak Klasör Adı", "Hedef Klasör Adı", "Eski Ad", "İşlem Tarihi",
                "Kullanıcı");
        }

        private void printBtn_Click(object sender, EventArgs e) {
            gc1.ShowPrintPreview();
        }

        private void exportBtn_Click(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog() { Title = "Excel'e Aktar", Filter = "(.xlsx)|Excel Kitaplığı", FilterIndex = 1 };
            if(sfd.ShowDialog() == DialogResult.OK) {
                gc1.ExportToXlsx(sfd.FileName + ".xlsx");
            }
            sfd.Dispose();
        }
    }
}

using System;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraTab;

namespace ArchiveManager {
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm {

        public MainForm() {
            InitializeComponent();
        }

        private void userDefItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            XtraTabPage xtp = xtraTabControl1.TabPages.ToList().Find(i => i.Text == e.Item.Caption);

            if(xtp != null) { xtraTabControl1.SelectedTabPage = xtp; return; }

            xtp = new XtraTabPage(); xtp.Text = e.Item.Caption;
            UserDefineUC ud = new UserDefineUC(); ud.Dock = DockStyle.Fill;
            xtp.Controls.Add(ud);
            xtraTabControl1.TabPages.Add(xtp); xtraTabControl1.SelectedTabPage = xtp;
        }

        private void fileManItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            XtraTabPage xtp = xtraTabControl1.TabPages.ToList().Find(i => i.Text == e.Item.Caption);

            if(xtp != null) { xtraTabControl1.SelectedTabPage = xtp; return; }

            xtp = new XtraTabPage(); xtp.Text = e.Item.Caption;
            ArchiveForm ap = new ArchiveForm(); ap.TopLevel = false; ap.Parent = xtp; ap.Dock = DockStyle.Fill;
            xtp.Controls.Add(ap);
            xtraTabControl1.TabPages.Add(xtp); xtraTabControl1.SelectedTabPage = xtp; ap.Show();
        }

        private void procRepItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            XtraTabPage xtp = xtraTabControl1.TabPages.ToList().Find(i => i.Text == e.Item.Caption);

            if(xtp != null) { xtraTabControl1.SelectedTabPage = xtp; return; }

            xtp = new XtraTabPage(); xtp.Text = e.Item.Caption;
            ArchiveProcReportUC ap = new ArchiveProcReportUC(); ap.Dock = DockStyle.Fill;
            xtp.Controls.Add(ap);
            xtraTabControl1.TabPages.Add(xtp); xtraTabControl1.SelectedTabPage = xtp;
        }

        private void statusRepItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {

        }

        private void aboutItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {

        }

        private void xtraTabControl1_CloseButtonClick(object sender, EventArgs e) {
            xtraTabControl1.TabPages.Remove(xtraTabControl1.SelectedTabPage);
        }
    }
}
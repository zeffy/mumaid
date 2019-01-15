using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace mumaid
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            NativeMethods.SetWindowTheme(tabPage1_listView1, "explorer", null);
            NativeMethods.SetWindowTheme(tabPage2_listView1, "explorer", null);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "uTorrent\\resume.dat");
            if ( File.Exists(path) )
                textBox1.Text = path;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            this.UseWaitCursor = true;
            Application.DoEvents();

            tabPage1_listView1.Items.Clear();

            var parser = new BencodeParser();
            var resume = parser.Parse<BDictionary>(textBox1.Text);
            var directoryInfo = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "uTorrent"));
            var torrents = directoryInfo.EnumerateFiles("*.torrent");

            var files = new Dictionary<string, FileInfo>();
            foreach ( var dir in textBox2.Text.Split(';') ) {
                foreach ( var fileinfo in new DirectoryInfo(dir).EnumerateFiles("*", SearchOption.AllDirectories) )
                    files.Add(fileinfo.FullName, fileinfo);
            }

            tabPage2_listView1.BeginUpdate();
            var listViewItemCollection = new ListView.ListViewItemCollection(tabPage1_listView1);
            foreach ( var fileinfo in torrents ) {
                if ( resume.TryGetValue(fileinfo.Name, out IBObject o) ) {
                    var entry = (BDictionary)o;

                    var torrent = parser.Parse<Torrent>(fileinfo.FullName);

                    switch ( torrent.FileMode ) {
                    case TorrentFileMode.Single:
                        files.Remove(entry["path"].ToString());
                        break;
                    case TorrentFileMode.Multi:
                        foreach ( var file in torrent.Files )
                            files.Remove(Path.Combine(entry["path"].ToString(), file.FullPath));
                        break;
                    }
                } else {
                    var item = new ListViewItem(new[] {
                        fileinfo.Name,
                        fileinfo.LastWriteTime.ToString("g"),
                        fileinfo.Extension,
                        NativeMethods.StrFormatByteSize(fileinfo.Length)
                    });
                    item.Tag = fileinfo;
                    tabPage2_listView1.Items.Add(item);
                }
            }
            tabPage2_listView1.EndUpdate();

            tabPage1_listView1.BeginUpdate();
            foreach ( var kvp in files ) {
                var fileinfo = kvp.Value;
                if ( fileinfo.Extension == ".!ut" )
                    continue;

                var item = new ListViewItem(new[] {
                    fileinfo.FullName,
                    fileinfo.LastWriteTime.ToString("g"),
                    fileinfo.Extension,
                    NativeMethods.StrFormatByteSize(fileinfo.Length)
                });
                item.Tag = fileinfo;
                tabPage1_listView1.Items.Add(item);
            }
            tabPage1_listView1.EndUpdate();

            this.UseWaitCursor = false;
            this.Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = File.Exists(textBox1.Text);
        }

        private void tabPage1_listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            tabPage1_button1.Enabled = tabPage1_listView1.SelectedItems.Count > 0;
        }

        private void tabPage1_button1_Click(object sender, EventArgs e)
        {
            tabPage1_listView1.BeginUpdate();
            while ( tabPage1_listView1.SelectedItems.Count > 0 ) {
                var item = tabPage1_listView1.SelectedItems[0];

                var fileinfo = (FileInfo)item.Tag;
                fileinfo.Delete();
                item.Remove();
            }
            tabPage1_listView1.EndUpdate();
        }

        private void tabPage2_listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            tabPage2_button1.Enabled = tabPage2_listView1.SelectedItems.Count > 0;
        }

        private void tabPage2_button1_Click(object sender, EventArgs e)
        {
            tabPage2_listView1.BeginUpdate();
            while ( tabPage2_listView1.SelectedItems.Count > 0 ) {
                var item = tabPage2_listView1.SelectedItems[0];

                var fileinfo = (FileInfo)item.Tag;
                fileinfo.Delete();
                item.Remove();
            }
            tabPage2_listView1.EndUpdate();
        }
    }
}

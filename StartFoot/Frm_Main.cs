using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Newtonsoft.Json;

namespace StartFoot
{
    public partial class Frm_Main : XtraForm
    {
        public string jsonPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
        public static BindingList<SoftwareBag> softwares { get; set; } = new BindingList<SoftwareBag>();
        public Frm_Main()
        {
            InitializeComponent();
            string json = File.ReadAllText(jsonPath);
            softwares = JsonConvert.DeserializeObject<BindingList<SoftwareBag>>(json) ?? new BindingList<SoftwareBag>();
            gridControl1.DataSource = softwares;

            gridControl1.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var view = gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
                    if (view != null)
                    {
                        var menu = new ContextMenuStrip();
                        var deleteItem = new ToolStripMenuItem("删除行");
                        deleteItem.Click += (sender2, e2) =>
                        {
                            int rowHandle = view.FocusedRowHandle;
                            if (rowHandle >= 0)
                            {
                                var list = gridControl1.DataSource as BindingList<SoftwareBag>;
                                var item = view.GetRow(rowHandle) as SoftwareBag;
                                if (item != null)
                                    list.Remove(item);
                            }
                        };
                        menu.Items.Add(deleteItem);
                        menu.Show(gridControl1, e.Location);
                    }
                }
            };
        }

        private void Start()
        {
            List<StartSf> finRes = new List<StartSf>();
            softwares.Select(x =>
            {
                StartSf temp = new StartSf();
                temp.Path = x.FilePath;
                temp.Name = x.Name;
                if (!string.IsNullOrWhiteSpace(x.Url))
                {
                    temp.Args = x.Url.Split(',');
                }
                finRes.Add(temp);
                return finRes;
            }).ToList();


            if (finRes?.Count > 0)
            {

                try
                {
                    foreach (var req in finRes)
                    {
                        if (string.IsNullOrWhiteSpace(req.Path))
                        {
                            //var r=$"文件不存在，{req.Path}";
                            continue;
                        }

                        bool isRunning = Process.GetProcessesByName(req.Name.Split('.').FirstOrDefault()).Any();
                        if (isRunning)
                        {
                            continue;
                        }

                        if (req?.Args?.Count() > 0)
                        {
                            req.Args.Select(x =>
                            {
                                return Process.Start(new ProcessStartInfo
                                {
                                    FileName = req.Path,
                                    Arguments = x,
                                    UseShellExecute = false
                                });
                            }).ToList();
                        }
                        else
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = req.Path,
                                Arguments = null,
                                UseShellExecute = true
                            });

                        }
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            SoftwareBag softwareBag = new SoftwareBag();
            softwareBag.FilePath = textEdit1.Text;
            softwareBag.Name = Path.GetFileName(textEdit1.Text);
            softwares.Add(softwareBag);
            gridControl1.Refresh();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(jsonPath))
            {
                File.Create(jsonPath);
            }
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(softwares, Formatting.Indented));
            XtraMessageBox.Show("保存成功");
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void textEdit1_DoubleClick(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择程序";
                openFileDialog.Filter = "可执行文件|*.exe";   // 只显示 .exe 文件
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog.Multiselect = false; // 是否允许多选

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    textEdit1.Text = filePath;
                }
            }
        }
    }

    public class SoftwareBag
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Url { get; set; }

    }


    public class StartSf
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Args { get; set; }
    }
}

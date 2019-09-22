using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Windows.Forms;
using System.Linq;

namespace ReportViewerTest3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int subReportCount;
        int mainPageCount;
        List<MainpageItem> dummySource;

        //List<IEnumerable<Item>> splitSource;
        List<IEnumerable<Customer>> splitSource;


        private void Form1_Load(object sender, EventArgs e)
        {
            reportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);

            var customerList = new List<Customer>();
            ////////////////////////////////////////////////////////////////////////////////
            //データベースからデータを取得する
            ////////////////////////////////////////////////////////////////////////////////
            using (var db = new Model1())
            {
                try
                {
                    //データベースよりデータを100件取得する
                    var query = db.Customer.Take(100).ToList();
                    customerList = query;

                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessage = "";
                    ex.EntityValidationErrors.SelectMany(error => error.ValidationErrors).ToList()
                        .ForEach(model => errorMessage = errorMessage + model.PropertyName + " - " + model.ErrorMessage);
                    MessageBox.Show(errorMessage);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////
            //レポートにデータを設定する
            ////////////////////////////////////////////////////////////////////////////////

            const int RowCountInPage = 50;
            splitSource = new List<IEnumerable<Customer>>();

            int seq = 0;
            foreach (var items in customerList.GroupBy(_ => seq++ / RowCountInPage))
            {//カテゴリをさらに、1ページに表示できる行数に分割
                splitSource.Add(items.ToArray());
                subReportCount++;
            }
            mainPageCount = (subReportCount + 1) / 2;
            dummySource = new List<MainpageItem>();
            for (int i = 0; i < mainPageCount; i++)
            {//メインのレポートで繰り返しを行わせるためにダミーのデータソースを作る
                dummySource.Add(new MainpageItem() { Index = i });
            }
            //this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("MainPageDataSet", dummySource));
            this.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            this.reportViewer1.ZoomPercent = 25;
            this.reportViewer1.RefreshReport();
            //const int RowCountInPage = 5;
            //splitSource = new List<IEnumerable<Item>>();

            //var originalSource = Item.CreateTestItems();

            //int seq = 0;
            //foreach (var items in originalSource.GroupBy(_ => seq++ / RowCountInPage))
            //{//カテゴリをさらに、1ページに表示できる行数に分割
            //    splitSource.Add(items.ToArray());
            //    subReportCount++;
            //}
            //mainPageCount = (subReportCount + 1) / 2;
            //dummySource = new List<MainpageItem>();
            //for (int i = 0; i < mainPageCount; i++)
            //{//メインのレポートで繰り返しを行わせるためにダミーのデータソースを作る
            //    dummySource.Add(new MainpageItem() { Index = i });
            //}
            //this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("MainPageDataSet", dummySource));
            //this.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            //this.reportViewer1.ZoomPercent = 25;
            //this.reportViewer1.LocalReport.Refresh();
            //this.reportViewer1.RefreshReport();

        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            //サブレポートにサブレポートの番号がわたるように設定してあるので、
            int pageIndex = int.Parse(e.Parameters["SubPageIndex"].Values[0]);
            if (splitSource.Count > pageIndex)
            {
                //サブレポート用のデータを渡す
                ReportDataSource subReportSource = new ReportDataSource("PageItems", splitSource[pageIndex]);
                e.DataSources.Add(subReportSource);
            }
            else
            {
                //片側分のデータしかない場合
                ReportDataSource subReportSource = new ReportDataSource("PageItems", new Customer[0]);
                e.DataSources.Add(subReportSource);
            }
        }
    }

    class Item
    {
        public int Category { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }

        public Item(int cat, int id, string name)
        {
            this.Category = cat; this.ID = id; this.Name = name;
        }

        public static IEnumerable<Item> CreateTestItems()
        {
            List<Item> list = new List<Item>();
            for (int i = 1; i <= 7; i++) { list.Add(new Item(1, i, "AAAA")); }
            for (int i = 1; i <= 3; i++) { list.Add(new Item(2, i, "AAAA")); }
            for (int i = 6; i <= 7; i++) { list.Add(new Item(3, i, "AAAA")); }
            return list;
        }
    }
}

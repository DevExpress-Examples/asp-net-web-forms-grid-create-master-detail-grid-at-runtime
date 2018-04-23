using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Web;
using System.Data.SqlClient;

namespace MasterDetailGrids {
    public partial class _Default : System.Web.UI.Page {
        ASPxGridView masterGrid;
        protected void Page_Init(object sender, EventArgs e) {
            Session["CategoryID"] = 4;
            CreateMasterGrid();
        }

        protected void Page_Load(object sender, EventArgs e) {
            masterGrid.DataBind();
        }

        private void CreateMasterGrid() {
            masterGrid = new ASPxGridView();
            masterGrid.ID = "masterGrid";
            masterGrid.AutoGenerateColumns = false;
            form1.Controls.Add(masterGrid);

            CreateMasterColumns(masterGrid);
            masterGrid.SettingsDetail.ShowDetailRow = true;
            masterGrid.KeyFieldName = "CategoryID";
            masterGrid.DataSource = GetMasterDataSource();

            masterGrid.Templates.DetailRow = new DetailGridTemplate();
            masterGrid.RowUpdating += new DevExpress.Web.Data.ASPxDataUpdatingEventHandler(masterGrid_RowUpdating);
        }

        private DataTable GetMasterDataSource() {
            DataTable table = new DataTable("Categories");
            string query = "SELECT [CategoryID], [CategoryName], [Description] FROM [Categories]";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            return table;
        }
        void masterGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e) {
            string query = "UPDATE [Categories] SET [CategoryName] = @CategoryName, [Description] = @Description WHERE [CategoryID] = @CategoryID";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar).Value = e.NewValues["CategoryName"];
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = e.NewValues["Description"];
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = e.NewValues["CategoryID"];
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            e.Cancel = true;
            masterGrid.CancelEdit();
            masterGrid.DataSource = GetMasterDataSource();
            masterGrid.DataBind();
        }

        private void CreateMasterColumns(ASPxGridView masterGrid) {
            GridViewCommandColumn colCmd = new GridViewCommandColumn();
            colCmd.ShowEditButton = true;
            masterGrid.Columns.Add(colCmd);
            masterGrid.Columns.Add(new GridViewDataColumn("CategoryID"));
            masterGrid.Columns.Add(new GridViewDataColumn("CategoryName"));
            masterGrid.Columns.Add(new GridViewDataColumn("Description"));
        }
    }

    internal static class DataHelper {
        public static SqlConnection GetConnection() {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["NorthwindConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            return conn;
        }
    }
    public class DetailGridTemplate : ITemplate {
        Control parent;
        object masterKey;
        ASPxGridView detailGrid;

        public void InstantiateIn(Control container) {
            parent = container;
            masterKey = ((GridViewDetailRowTemplateContainer)parent).KeyValue;
            CreateDetailGrid();
        }

        private void CreateDetailGrid() {
            detailGrid = new ASPxGridView();
            detailGrid.ID = "detailGrid";
            detailGrid.AutoGenerateColumns = false;
            parent.Controls.Add(detailGrid);

            CreateDetailColumns(detailGrid);
            detailGrid.KeyFieldName = "ProductID";
            detailGrid.DataSource = GetDetailDataSource();
            detailGrid.DataBind();
            detailGrid.RowInserting += new DevExpress.Web.Data.ASPxDataInsertingEventHandler(detailGrid_RowInserting);
            detailGrid.RowUpdating += new DevExpress.Web.Data.ASPxDataUpdatingEventHandler(detailGrid_RowUpdating);
            detailGrid.RowDeleting += new DevExpress.Web.Data.ASPxDataDeletingEventHandler(detailGrid_RowDeleting);
        }

        void detailGrid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e) {
            ASPxGridView senderGridView = (ASPxGridView)sender;
            Random randomizer = new Random();
            string query = "INSERT INTO [Products] ([ProductName], [SupplierID], [CategoryID], [UnitPrice], [Discontinued]) VALUES (@ProductName, @SupplierID, @CategoryID, @UnitPrice, @Discontinued)";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar).Value = e.NewValues["ProductName"];
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = randomizer.Next(1, 29);
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = senderGridView.GetMasterRowKeyValue();
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = e.NewValues["UnitPrice"];
            cmd.Parameters.Add("@Discontinued", SqlDbType.Bit).Value = e.NewValues["Discontinued"];
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            e.Cancel = true;
            senderGridView.CancelEdit();
            senderGridView.DataSource = GetDetailDataSource();
        }

        void detailGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e) {
            ASPxGridView senderGridView = (ASPxGridView)sender;
            string query = "UPDATE [Products] SET [ProductName] = @ProductName, [UnitPrice] = @UnitPrice, [Discontinued] = @Discontinued WHERE [ProductID] = @ProductID";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar).Value = e.NewValues["ProductName"];
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = e.NewValues["UnitPrice"];
            cmd.Parameters.Add("@Discontinued", SqlDbType.Bit).Value = e.NewValues["Discontinued"];
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = e.Keys[0];
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            e.Cancel = true;
            senderGridView.CancelEdit();
            senderGridView.DataSource = GetDetailDataSource();
        }

        void detailGrid_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e) {
            ASPxGridView senderGridView = (ASPxGridView)sender;
            string query = "DELETE FROM [Products] WHERE [ProductID] = @ProductID";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = e.Keys[0];
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            e.Cancel = true;
            senderGridView.CancelEdit();
            senderGridView.DataSource = GetDetailDataSource();
        }

        private DataTable GetDetailDataSource() {
            DataTable table = new DataTable("Products");
            string query = "SELECT [ProductID], [ProductName], [CategoryID], [UnitPrice], [Discontinued] FROM [Products] WHERE ([CategoryID] = @CategoryID)";
            SqlCommand cmd = new SqlCommand(query, DataHelper.GetConnection());
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = masterKey;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            return table;
        }

        private void CreateDetailColumns(ASPxGridView detailGrid) {
            GridViewCommandColumn colCmd = new GridViewCommandColumn();
            colCmd.ShowEditButton = true;
            colCmd.ShowNewButton = true;
            colCmd.ShowDeleteButton = true;
            detailGrid.Columns.Add(colCmd);

            GridViewDataColumn colProductID = new GridViewDataColumn();
            colProductID.FieldName = "ProductID";
            colProductID.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False;
            detailGrid.Columns.Add(colProductID);
            
            detailGrid.Columns.Add(new GridViewDataColumn("ProductName"));

            GridViewDataTextColumn colCategoryID = new GridViewDataTextColumn();
            colCategoryID.FieldName = "CategoryID";
            colCategoryID.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False;
            detailGrid.Columns.Add(colCategoryID);
            
            detailGrid.Columns.Add(new GridViewDataColumn("UnitPrice"));

            GridViewDataCheckColumn colDiscontinued = new GridViewDataCheckColumn();
            colDiscontinued.FieldName = "Discontinued";
            detailGrid.Columns.Add(colDiscontinued);
        }
    }
}
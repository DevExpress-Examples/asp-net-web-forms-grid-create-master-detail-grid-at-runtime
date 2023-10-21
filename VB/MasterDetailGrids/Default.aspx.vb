Imports System
Imports System.Data
Imports System.Configuration
Imports System.Collections
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports DevExpress.Web
Imports System.Data.SqlClient

Namespace MasterDetailGrids
	Partial Public Class _Default
		Inherits System.Web.UI.Page

		Private masterGrid As ASPxGridView
		Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs)
			Session("CategoryID") = 4
			CreateMasterGrid()
		End Sub

		Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
			masterGrid.DataBind()
		End Sub

		Private Sub CreateMasterGrid()
			masterGrid = New ASPxGridView()
			masterGrid.ID = "masterGrid"
			masterGrid.AutoGenerateColumns = False
			form1.Controls.Add(masterGrid)

			CreateMasterColumns(masterGrid)
			masterGrid.SettingsDetail.ShowDetailRow = True
			masterGrid.KeyFieldName = "CategoryID"
			masterGrid.DataSource = GetMasterDataSource()

			masterGrid.Templates.DetailRow = New DetailGridTemplate()
			AddHandler masterGrid.RowUpdating, AddressOf masterGrid_RowUpdating
		End Sub

		Private Function GetMasterDataSource() As DataTable
			Dim table As New DataTable("Categories")
			Dim query As String = "SELECT [CategoryID], [CategoryName], [Description] FROM [Categories]"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			Dim da As New SqlDataAdapter(cmd)
			da.Fill(table)
			Return table
		End Function
		Private Sub masterGrid_RowUpdating(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataUpdatingEventArgs)
			Dim query As String = "UPDATE [Categories] SET [CategoryName] = @CategoryName, [Description] = @Description WHERE [CategoryID] = @CategoryID"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar).Value = e.NewValues("CategoryName")
			cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = e.NewValues("Description")
			cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = e.NewValues("CategoryID")
			cmd.Connection.Open()
			cmd.ExecuteNonQuery()
			e.Cancel = True
			masterGrid.CancelEdit()
			masterGrid.DataSource = GetMasterDataSource()
			masterGrid.DataBind()
		End Sub

		Private Sub CreateMasterColumns(ByVal masterGrid As ASPxGridView)
			Dim colCmd As New GridViewCommandColumn()
			colCmd.ShowEditButton = True
			masterGrid.Columns.Add(colCmd)
			masterGrid.Columns.Add(New GridViewDataColumn("CategoryID"))
			masterGrid.Columns.Add(New GridViewDataColumn("CategoryName"))
			masterGrid.Columns.Add(New GridViewDataColumn("Description"))
		End Sub
	End Class

	Friend Module DataHelper
		Public Function GetConnection() As SqlConnection
			Dim connStr As String = System.Configuration.ConfigurationManager.ConnectionStrings("NorthwindConnectionString").ConnectionString
			Dim conn As New SqlConnection(connStr)
			Return conn
		End Function
	End Module
	Public Class DetailGridTemplate
		Implements ITemplate

		Private parent As Control
		Private masterKey As Object
		Private detailGrid As ASPxGridView

		Public Sub InstantiateIn(ByVal container As Control) Implements ITemplate.InstantiateIn
			parent = container
			masterKey = CType(parent, GridViewDetailRowTemplateContainer).KeyValue
			CreateDetailGrid()
		End Sub

		Private Sub CreateDetailGrid()
			detailGrid = New ASPxGridView()
			detailGrid.ID = "detailGrid"
			detailGrid.AutoGenerateColumns = False
			parent.Controls.Add(detailGrid)

			CreateDetailColumns(detailGrid)
			detailGrid.KeyFieldName = "ProductID"
			detailGrid.DataSource = GetDetailDataSource()
			detailGrid.DataBind()
			AddHandler detailGrid.RowInserting, AddressOf detailGrid_RowInserting
			AddHandler detailGrid.RowUpdating, AddressOf detailGrid_RowUpdating
			AddHandler detailGrid.RowDeleting, AddressOf detailGrid_RowDeleting
		End Sub

		Private Sub detailGrid_RowInserting(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataInsertingEventArgs)
			Dim senderGridView As ASPxGridView = DirectCast(sender, ASPxGridView)
			Dim randomizer As New Random()
			Dim query As String = "INSERT INTO [Products] ([ProductName], [SupplierID], [CategoryID], [UnitPrice], [Discontinued]) VALUES (@ProductName, @SupplierID, @CategoryID, @UnitPrice, @Discontinued)"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar).Value = e.NewValues("ProductName")
			cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = randomizer.Next(1, 29)
			cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = senderGridView.GetMasterRowKeyValue()
			cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = e.NewValues("UnitPrice")
			cmd.Parameters.Add("@Discontinued", SqlDbType.Bit).Value = e.NewValues("Discontinued")
			cmd.Connection.Open()
			cmd.ExecuteNonQuery()

			e.Cancel = True
			senderGridView.CancelEdit()
			senderGridView.DataSource = GetDetailDataSource()
		End Sub

		Private Sub detailGrid_RowUpdating(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataUpdatingEventArgs)
			Dim senderGridView As ASPxGridView = DirectCast(sender, ASPxGridView)
			Dim query As String = "UPDATE [Products] SET [ProductName] = @ProductName, [UnitPrice] = @UnitPrice, [Discontinued] = @Discontinued WHERE [ProductID] = @ProductID"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar).Value = e.NewValues("ProductName")
			cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = e.NewValues("UnitPrice")
			cmd.Parameters.Add("@Discontinued", SqlDbType.Bit).Value = e.NewValues("Discontinued")
			cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = e.Keys(0)
			cmd.Connection.Open()
			cmd.ExecuteNonQuery()

			e.Cancel = True
			senderGridView.CancelEdit()
			senderGridView.DataSource = GetDetailDataSource()
		End Sub

		Private Sub detailGrid_RowDeleting(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataDeletingEventArgs)
			Dim senderGridView As ASPxGridView = DirectCast(sender, ASPxGridView)
			Dim query As String = "DELETE FROM [Products] WHERE [ProductID] = @ProductID"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = e.Keys(0)
			cmd.Connection.Open()
			cmd.ExecuteNonQuery()

			e.Cancel = True
			senderGridView.CancelEdit()
			senderGridView.DataSource = GetDetailDataSource()
		End Sub

		Private Function GetDetailDataSource() As DataTable
			Dim table As New DataTable("Products")
			Dim query As String = "SELECT [ProductID], [ProductName], [CategoryID], [UnitPrice], [Discontinued] FROM [Products] WHERE ([CategoryID] = @CategoryID)"
			Dim cmd As New SqlCommand(query, DataHelper.GetConnection())
			cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = masterKey
			Dim da As New SqlDataAdapter(cmd)
			da.Fill(table)
			Return table
		End Function

		Private Sub CreateDetailColumns(ByVal detailGrid As ASPxGridView)
			Dim colCmd As New GridViewCommandColumn()
			colCmd.ShowEditButton = True
			colCmd.ShowNewButton = True
			colCmd.ShowDeleteButton = True
			detailGrid.Columns.Add(colCmd)

			Dim colProductID As New GridViewDataColumn()
			colProductID.FieldName = "ProductID"
			colProductID.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False
			detailGrid.Columns.Add(colProductID)

			detailGrid.Columns.Add(New GridViewDataColumn("ProductName"))

			Dim colCategoryID As New GridViewDataTextColumn()
			colCategoryID.FieldName = "CategoryID"
			colCategoryID.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False
			detailGrid.Columns.Add(colCategoryID)

			detailGrid.Columns.Add(New GridViewDataColumn("UnitPrice"))

			Dim colDiscontinued As New GridViewDataCheckColumn()
			colDiscontinued.FieldName = "Discontinued"
			detailGrid.Columns.Add(colDiscontinued)
		End Sub
	End Class
End Namespace
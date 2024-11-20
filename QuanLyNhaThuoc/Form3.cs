using MySql.Data.MySqlClient;
using DGVPrinterHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
    public partial class Form3 : Form
    {
        private MySqlConnection connection;
        public Form3(MySqlConnection dbConnection)
        {
            InitializeComponent();
            connection = dbConnection;
            LoadCategories();
            txtProductName.Visible = false;
            txtPurchasePrice.Visible = false;
            cboCategory.Items.Clear();
            dtpExpiryDate.Visible = false;

        }
        private void LoadCategories()
        {
            // Câu truy vấn chỉ lấy cột name
            string query = "SELECT name FROM products";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
            DataTable dataTable = new DataTable();

            try
            {
                // Nạp dữ liệu từ cơ sở dữ liệu vào DataTable
                adapter.Fill(dataTable);

                // Xóa các mục cũ trong ListBox trước khi thêm dữ liệu mới
                lstBoxMedicine.Items.Clear();

                // Duyệt qua từng hàng của DataTable và thêm giá trị cột CategoryName vào ListBox
                foreach (DataRow row in dataTable.Rows)
                {
                    lstBoxMedicine.Items.Add(row["name"].ToString());
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có vấn đề xảy ra
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void lstBoxMedicine_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem có mục nào trong ListBox đã được chọn không
            if (lstBoxMedicine.SelectedIndex != -1)
            {
                // Hiển thị các TextBox khi người dùng chọn mục
                txtProductName.Visible = true;
                cboCategory.Visible = true;
                txtPurchasePrice.Visible = true;
                dtpExpiryDate.Visible = true;

                // Lấy tên sản phẩm được chọn trong ListBox
                string selectedname = lstBoxMedicine.SelectedItem.ToString();

                // Gọi hàm để tải thông tin chi tiết về sản phẩm dựa trên tên
                LoadProductDetails(selectedname);
            }
        }


        private void LoadProductDetails(string name)
        {
            // Câu truy vấn SQL để lấy thông tin chi tiết của sản phẩm dựa trên tên sản phẩm
            string query = "SELECT id, name, categoryID, price, expiry_date FROM products WHERE name = @name";

            try
            {
                // Mở kết nối nếu nó chưa mở
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Tạo command và thêm tham số
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);  // Dùng tham số @name cho câu truy vấn

                // Thực thi truy vấn và lấy dữ liệu
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Điền thông tin vào các TextBox từ dữ liệu truy vấn
                    txtId.Text = reader["id"].ToString();
                    txtProductName.Text = reader["name"].ToString();
                    cboCategory.Text = reader["categoryID"].ToString();  // categoryID có thể là một mã danh mục
                    txtPurchasePrice.Text = reader["price"].ToString();
                    dtpExpiryDate.Value = Convert.ToDateTime(reader["expiry_date"]);
                }
                else
                {
                    // Xử lý nếu không tìm thấy dữ liệu cho sản phẩm
                    MessageBox.Show("Không tìm thấy thông tin sản phẩm.");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có vấn đề xảy ra
                MessageBox.Show("Lỗi khi tải thông tin sản phẩm: " + ex.Message);
            }
            finally
            {
                // Đảm bảo đóng kết nối sau khi truy vấn xong
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void txtSoLuong_TextChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem txtPurchasePrice và txtSoLuong có giá trị hợp lệ không
            if (decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice) &&
                int.TryParse(txtSoLuong.Text, out int soLuong) && soLuong > 0)
            {
                // Tính giá tiền (TotalPrice)
                decimal totalPrice = purchasePrice * soLuong;

                // Hiển thị giá trị vào ô txtGia
                txtGia.Text = totalPrice.ToString("N2"); // Hiển thị với 2 chữ số thập phân
            }
            else
            {
                // Nếu không hợp lệ, đặt giá trị totalPrice về 0
                txtGia.Text = "0";
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Lấy từ khóa tìm kiếm từ txtSearch
            string searchKeyword = txtSearch.Text.Trim();

            // Nếu từ khóa rỗng, tải lại toàn bộ danh sách sản phẩm
            if (string.IsNullOrEmpty(searchKeyword))
            {
                LoadCategories();
                return;
            }

            // Tạo câu truy vấn tìm kiếm
            string query = "SELECT name FROM products WHERE name LIKE @keyword";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@keyword", "%" + searchKeyword + "%");
            DataTable dataTable = new DataTable();

            try
            {
                // Nạp dữ liệu từ cơ sở dữ liệu vào DataTable
                adapter.Fill(dataTable);

                // Xóa danh sách cũ trong ListBox trước khi thêm dữ liệu mới
                lstBoxMedicine.Items.Clear();

                // Duyệt qua từng hàng trong DataTable và thêm vào ListBox
                foreach (DataRow row in dataTable.Rows)
                {
                    lstBoxMedicine.Items.Add(row["name"].ToString());
                }

                // Nếu không tìm thấy kết quả, hiển thị thông báo
                if (dataTable.Rows.Count == 0)
                {
                    lstBoxMedicine.Items.Add("Không tìm thấy sản phẩm.");
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có vấn đề xảy ra
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message);
            }
        }


        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ các TextBox, ComboBox và DateTimePicker
                string id = txtId.Text.Trim();
                string productName = txtProductName.Text.Trim();
                string category = cboCategory.Text.Trim();
                string purchasePrice = txtPurchasePrice.Text.Trim();
                DateTime expiryDate = dtpExpiryDate.Value;
                string quantity = txtSoLuong.Text.Trim();
                string totalPrice = txtGia.Text.Trim();

                // Kiểm tra dữ liệu hợp lệ
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(productName) ||
                    string.IsNullOrEmpty(category) || string.IsNullOrEmpty(purchasePrice) ||
                    string.IsNullOrEmpty(quantity) || string.IsNullOrEmpty(totalPrice))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
        
                // Thêm dữ liệu vào DataGridView
                dataGridView1.Rows.Add(id, productName, category, purchasePrice, quantity, totalPrice, expiryDate.ToShortDateString());
            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem người dùng đã chọn hàng trong DataGridView hay chưa
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    // Xác nhận trước khi xóa
                    DialogResult confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này không?",
                                                                 "Xác nhận xóa",
                                                                 MessageBoxButtons.YesNo,
                                                                 MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        // Duyệt qua các hàng được chọn và xóa từng hàng
                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                        {
                            // Xóa hàng khỏi DataGridView
                            dataGridView1.Rows.Remove(row);
                        }

                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một sản phẩm để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
             
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnInHD_Click(object sender, EventArgs e)
        {
            DGVPrinter print = new DGVPrinter();
            print.Title = "Hóa Đơn Thuốc";
            print.SubTitle = string.Format("Ngày: {0}", DateTime.Now.Date);
            print.SubTitleFormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip;
            print.PageNumbers = true;
            print.PageNumberInHeader = true;
            print.PorportionalColumns = true;
            print.HeaderCellAlignment = StringAlignment.Near;
            print.Footer = "Tổn Số Tiền Phải Trả : " + txtGia.Text;
            print.FooterSpacing = 15;
            print.PrintDataGridView(dataGridView1);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

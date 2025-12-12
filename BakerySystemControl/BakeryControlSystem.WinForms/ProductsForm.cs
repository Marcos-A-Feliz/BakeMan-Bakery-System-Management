using BakeryControlSystem.Data.RepositoriesInterfaces;
using System;
using System.Windows.Forms;

namespace BakeryControlSystem.Forms
{
    public partial class ProductsForm : Form
    {
        private readonly IProductRepository _repo;

        public ProductsForm(IProductRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            LoadProducts();
        }

        private void LoadProducts()
        {
            var products = _repo.GetAll();
            dataGridViewProducts.DataSource = products;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var createForm = new CreateProductForm(_repo);
            createForm.FormClosed += (s, args) => LoadProducts();
            createForm.ShowDialog();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.");
                return;
            }

            var selectedRow = dataGridViewProducts.SelectedRows[0];
            var productId = (int)selectedRow.Cells["Id"].Value;

            var result = MessageBox.Show($"Are you sure you want to delete product #{productId}?",
                "Confirm Delete", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _repo.Delete(productId);
                LoadProducts();
                MessageBox.Show("Product deleted successfully.");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }
    }
}
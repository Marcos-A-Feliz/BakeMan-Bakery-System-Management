// CreateProductForm.cs
using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using System;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BakeryControlSystem.Forms
{
    public partial class CreateProductForm : Form
    {
        private readonly IProductRepository _repo;

        public CreateProductForm(IProductRepository repo)
        {
            InitializeComponent();
            _repo = repo;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Invalid price format.");
                return;
            }

            var product = new Product
            {
                Name = txtName.Text,
                Description = txtDescription.Text,
                Category = txtCategory.Text,
                SalePrice = price,
                CreationDate = DateTime.Now,
                IsActive = true
            };

            _repo.Add(product);
            MessageBox.Show("Product created successfully!");
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
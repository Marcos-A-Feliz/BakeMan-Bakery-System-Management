using BakeryControlSystem.Data.DB;
using BakeryControlSystem.Data.Repositories;
using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Forms;
using System;
using System.Windows.Forms;

namespace BakeryControlSystem
{
    public partial class MainForm : Form
    {
        private readonly BakeryDbContext _dbContext;
        private readonly IProductRepository _productRepo;

        public MainForm()
        {
            InitializeComponent();

            // Inicializar contexto de base de datos
            _dbContext = new BakeryDbContext();
            _productRepo = new ProductRepository(_dbContext);

            // Asegurar que la base de datos existe
            _dbContext.Database.EnsureCreated();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            OpenProductsForm();
        }

        private void OpenProductsForm()
        {
            var productsForm = new ProductsForm(_productRepo);
            productsForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Liberar recursos
            _dbContext?.Dispose();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad de Pedidos en desarrollo...", "Información",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad de Clientes en desarrollo...", "Información",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
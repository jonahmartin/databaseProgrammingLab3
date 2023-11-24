using Microsoft.EntityFrameworkCore;
using System.Configuration;
using MartinLab3.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace MartinLab3
{
    public partial class frmKitchenSupply : Form
    {

        public frmKitchenSupply()
        {
            InitializeComponent();

        }

        //class level context
        public KitchenContext? context;



        //form load, populate data grid views
        private void frmKitchenSupply_Load(object sender, EventArgs e)
        {
            context = new KitchenContext();

            //loads dgvOrders w/ customer name field added
            RefreshOrders();

            //setting up Products dgv
            context.Products.Load();
            productBindingSource.DataSource = context.Products.Local.ToBindingList();

            //setting up Services dgv
            context.Services.Load();
            serviceBindingSource.DataSource = context.Services.Local.ToBindingList();

            lblSelectedItem.Text = "";


            //make new customer with my details
            Customer jonah = new()
            {
                FirstName = "Jonah",
                LastName = "Martin",
                Email = "jonah@email.com",
                Phone = "999 999 9999",
                Address = "111 Main St.",
                City = "Halifax",
                State = "Nevada",
                ZipCode = "AAABBB"

            };

            //checking if customer by name of jonah exists - real world would need to be PK - customerId
            bool customerExists = context.Customers.Any(c => c.FirstName == jonah.FirstName);

            //checks if customer already exists, if not, add jonah to context
            if (!customerExists)
            {
                try
                {
                    context.Customers.Add(jonah);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
                
            }
        }

        /*Name: RefreshOrders
        Description: refreshes dgvOrders 
        In: none
        Out: none*/
        private void RefreshOrders()
        {
            //setting up Orders dgv using LINQ (additional field using inner join required: customer name)
            var ordersQuery =
                from order in context.Orders
                join customer in context.Customers
                on order.CustomerId equals customer.CustomerId
                select new
                {
                    //all fields in table
                    order.OrderId,
                    order.CustomerId,
                    order.OrderDate,
                    order.TotalAmount,
                    customer = customer.FirstName + " " + customer.LastName
                };

            //data source for orders dgv is now LINQ query list of data
            dgvOrders.DataSource = ordersQuery.ToList();
        }


        //searches through current tab's dgv for fields 
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            //input
            string filterByText = txtSearch.Text.ToUpper();

            //orders
            if (tabOPS.SelectedTab == tabOrders)
            {
                var queriedOrders =
                    from order in context.Orders
                    join customer in context.Customers on
                    order.CustomerId equals customer.CustomerId
                    where order.OrderId.ToString().Contains(filterByText) ||
                    (customer.FirstName + " " + customer.LastName).Contains(filterByText)
                    select new
                    {
                        order.OrderId,
                        order.CustomerId,
                        order.OrderDate,
                        order.TotalAmount,
                        customer = customer.FirstName + " " + customer.LastName
                    };

                //display newly queried table
                dgvOrders.DataSource = queriedOrders.ToList();
            }
            //products
            else if (tabOPS.SelectedTab == tabProducts)
            {
                //querying by search term in product ID or product Name 
                var queriedProducts = context.Products.Where(
                    products => products.ProductId.ToString().Contains(filterByText) ||
                    products.ProductName.Contains(filterByText));

                //display newly queried table
                dgvProducts.DataSource = queriedProducts.ToList();
            }
            //services
            else if (tabOPS.SelectedTab == tabServices)
            {
                var queriedServices = context.Services.Where(
                    services => services.ServiceName.Contains(filterByText) ||
                    services.ServiceId.ToString().Contains(filterByText));

                //displays newly queried table
                dgvServices.DataSource = queriedServices.ToList();
            }

            //if no records in search, blank out order items
            if (dgvOrders.Rows.Count == 0)
            {
                dgvOrderItems.DataSource = null;
            }
        }

        //fills Orders-associated labels with relevant information
        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {

            if (dgvOrders.SelectedRows.Count >= 1)
            {
                //currently selected row
                DataGridViewRow selectedRow = dgvOrders.SelectedRows[0];

                //capturing orderID value & display
                string orderID = selectedRow.Cells[0].Value.ToString();
                lblSelectedOrder.Text = orderID;


                FillOrderItems();

                //clears selection of orderitems table
                dgvOrderItems.ClearSelection();


                //gather and display stats for order items table
                string customerName = dgvOrders.SelectedRows[0].Cells[4].Value.ToString();

                int numberOfItems = dgvOrderItems.Rows.Count;
                decimal subtotal = (decimal)dgvOrders.SelectedRows[0].Cells[3].Value;
                decimal taxRate = 0.15m;
                decimal tax = taxRate * subtotal;
                decimal total = subtotal + tax;

                //display data
                lblCustomer.Text = customerName;
                lblNumItems.Text = numberOfItems.ToString();
                lblSubtotal.Text = subtotal.ToString("C");
                lblTax.Text = tax.ToString("C");
                lblTotal.Text = total.ToString("C");
            }
        }

        //fills Products-associated labels with relevant information
        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 1 && dgvProducts.Rows.Count > 0)
            {
                if (dgvServices.SelectedRows.Count > 0)
                    dgvServices.ClearSelection();

                if (dgvOrderItems.SelectedRows.Count > 0)
                    dgvOrderItems.ClearSelection();

                //currently selected row
                DataGridViewRow selectedRow = dgvProducts.SelectedRows[0];

                //capturing productID, productName & displaying
                string productID = selectedRow.Cells[0].Value.ToString();
                string productName = selectedRow.Cells[1].Value.ToString();
                lblSelectedItem.Text = productID + " " + productName;

            }
        }


        //when services selection change, update selected item label
        private void dgvServices_SelectionChanged(object sender, EventArgs e)
        {

            if (dgvServices.SelectedRows.Count == 1 && dgvServices.Rows.Count > 0)
            {
                if (dgvProducts.SelectedRows.Count > 0)
                    dgvProducts.ClearSelection();

                if (dgvOrderItems.SelectedRows.Count > 0)
                    dgvOrderItems.ClearSelection();

                //currently selected row
                DataGridViewRow selectedRow = dgvServices.SelectedRows[0];

                //capturing productID, productName & displaying
                string serviceID = selectedRow.Cells[0].Value.ToString();
                string serviceName = selectedRow.Cells[1].Value.ToString();
                lblSelectedItem.Text = serviceID + " " + serviceName;
            }


        }

        //creates an order for jonah customer
        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            //find most recently added customer
            var customer = context.Customers.OrderBy(c => c.CustomerId).Last();

            //new order with most recently added customer
            Order order = new Order()
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.Today,
                TotalAmount = 0m,
            };

            try
            {
                //add order to db
                context.Orders.Add(order);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            

            //refresh dgv
            RefreshOrders();
        }

        //deletes a row in dgvOrders if there are no order items associated with it
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count >= 1)
            {
                //capturing current row & associated ID
                DataGridViewRow currentRow = dgvOrders.SelectedRows[0];
                int currentRowId = (int)currentRow.Cells[0].Value;

                //check if order has 
                bool orderHasItems = context.OrderItems.Any(oi => oi.OrderId == currentRowId);
                Order orderToDelete = context.Orders.First(o => o.OrderId == currentRowId);

                if (orderHasItems)
                {
                    MessageBox.Show("Can't delete Order " + currentRowId + " as there are " +
                        dgvOrderItems.Rows.Count + " item(s) attached to it", "Error deleting");
                    return;
                }

                try
                {
                    //deletes item & saves
                    context.Orders.Remove(orderToDelete);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }

                RefreshOrders();
            }
        }
        private void FillOrderItems()
        {
            //search criteria
            string filterById = dgvOrders.SelectedRows[0].Cells[0].Value.ToString();
            //query order items depending on which item is clicked in orders table
            var queriedOrderItems = context.OrderItems.Where(
                orderitems => orderitems.OrderId.ToString().Contains(filterById));

            dgvOrderItems.DataSource = queriedOrderItems.ToList();
        }


        //adds or removes item from order items depending on context
        private void btnAddRemove_Click(object sender, EventArgs e)
        {
            //remove item from order items
            if (dgvOrderItems.SelectedRows.Count == 1)
            {

                //capture line id (PK) of order items
                int lineId = (int)dgvOrderItems.SelectedRows[0].Cells[0].Value;
                OrderItem itemToRemove = context.OrderItems.First(oi => oi.LineId == lineId);

                try
                {
                    context.OrderItems.Remove(itemToRemove);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
                

                FillOrderItems();
            }
            else if (dgvOrderItems.SelectedRows.Count > 1)
            {
                MessageBox.Show("Cannot delete more than one record at a time", "Selection Error");
                dgvOrders.ClearSelection();
            }

            //add products/services to order items
            if (dgvProducts.SelectedRows.Count == 1)
            {
                int productId = (int)dgvProducts.SelectedRows[0].Cells[0].Value;
                Product productToAdd = context.Products.First(p => p.ProductId == productId);
                int orderId = Convert.ToInt32(lblSelectedOrder.Text);

                //new order item with
                OrderItem orderItem = new OrderItem()
                {
                    Product = productToAdd,
                    ProductId = productId,
                    Price = productToAdd.Price,
                    Quantity = 1,
                    OrderId = orderId,
                };

                try
                {
                    //save changes & refresh
                    context.OrderItems.Add(orderItem);
                    context.SaveChanges();
                    FillOrderItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }

            }
            else if (dgvServices.SelectedRows.Count == 1)
            {
                int serviceId = (int)dgvServices.SelectedRows[0].Cells[0].Value;
                Service serviceToAdd = context.Services.First(p => p.ServiceId == serviceId);
                int orderId = Convert.ToInt32(lblSelectedOrder.Text);

                //new order item with
                OrderItem orderItem = new OrderItem()
                {
                    Service = serviceToAdd,
                    ServiceId = serviceId,
                    Price = serviceToAdd.Price,
                    Quantity = 1,
                    OrderId = orderId,
                };

                try
                {
                    //save changes & refresh
                    context.OrderItems.Add(orderItem);
                    context.SaveChanges();
                    FillOrderItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        //deselects other dgvs when dgv order items is clicked
        private void dgvOrderItems_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrderItems.SelectedRows.Count == 1 && dgvOrderItems.Rows.Count > 0)
            {
                if (dgvServices.SelectedRows.Count > 0)
                    dgvServices.ClearSelection();

                if (dgvProducts.SelectedRows.Count > 0)
                    dgvProducts.ClearSelection();
            }
        }
    }
}
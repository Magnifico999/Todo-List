using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace EF_Todo_list
{
    public partial class Form1 : Form
    {
        Task model = new Task();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            clear();
        }
        void clear()
        {
            textBox2.Text = textBox3.Text = comboBox1.Text = dateTimePicker1.Text = "";
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            model.TaskID = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clear();
            populateDataGridView();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(comboBox1.Text) || string.IsNullOrWhiteSpace(dateTimePicker1.Text))
                {
                    MessageBox.Show("Please fill in all required fields");
                }
                else
                {
                    model.Activities = textBox2.Text.Trim();
                    model.Priority = textBox3.Text.Trim();
                    model.Status = comboBox1.Text.Trim();
                    model.ExpiryDate = DateTime.Parse(dateTimePicker1.Text);
                    using (EFDBEntities db = new EFDBEntities())
                    {
                        if (model.TaskID == 0)// Insert
                            db.Tasks.Add(model);
                        else // Update
                            db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    clear();
                    populateDataGridView();
                    MessageBox.Show("Submitted successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        void populateDataGridView()
        {
            dgvTask.AutoGenerateColumns = false;
            using (EFDBEntities db = new EFDBEntities())
            {
                dgvTask.DataSource = db.Tasks.ToList<Task>();
            }
        }

        private void dgvTask_DoubleClick(object sender, EventArgs e)
        {
            if (dgvTask.CurrentRow.Index != -1)
            {
                model.TaskID = Convert.ToInt32(dgvTask.CurrentRow.Cells["TaskID"].Value);
                using (EFDBEntities db = new EFDBEntities())
                {
                    model = db.Tasks.Where(x => x.TaskID == model.TaskID).FirstOrDefault();
                    model.Activities = textBox2.Text.Trim();
                    model.Priority = textBox3.Text;
                    model.Status = comboBox1.Text;
                    model.ExpiryDate = DateTime.Parse(dateTimePicker1.Text);
                }
                btnSave.Text = "Update";
                btnDelete.Enabled = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this Record ?", "EF Todo list", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (EFDBEntities db = new EFDBEntities())
                {
                    var entry = db.Entry(model);
                    if (entry.State == EntityState.Detached)
                        db.Tasks.Attach(model);
                    db.Tasks.Remove(model);
                    db.SaveChanges();
                    populateDataGridView();
                    clear();
                    MessageBox.Show("Deleted successfully");
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (EFDBEntities db = new EFDBEntities())
            {
                string searchQuery = txtSearch.Text.Trim();
                DateTime searchDate;
                bool isDate = DateTime.TryParse(searchQuery, out searchDate);

                var query = from t in db.Tasks
                            where t.Activities.Contains(searchQuery)
                            || t.Priority.Contains(searchQuery)
                            || t.Status.Contains(searchQuery)
                            || (isDate && t.ExpiryDate == searchDate)
                            select t;

                List<Task> results = query.ToList();
                if (results.Count > 0)
                {
                    // Display search results
                    dgvTask.DataSource = results;
                    MessageBox.Show("Search completed successfully.");
                }
                else
                {
                    // No matching records found
                    MessageBox.Show("No matching records found.");
                }
            }
        }

        private void btnDis_Click(object sender, EventArgs e)
        {
            populateDataGridView();
        }
    }
}

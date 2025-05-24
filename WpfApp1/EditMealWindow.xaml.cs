using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace WpfApp1
{
    public partial class EditMealWindow : Window
    {
        public string MealDescription { get; private set; }
        public List<FoodItem> SelectedProducts { get; } = new List<FoodItem>();
        private List<FoodItem> _allProducts;
    
        public string DayAndMeal { get; }

        public EditMealWindow(string dayOfWeek, string mealType, string currentValue)
        {
            InitializeComponent();
            DataContext = this;
            _allProducts = new List<FoodItem>();
            LoadProducts();
    
            DayAndMeal = $"{dayOfWeek}, {mealType}";
            
            if (!string.IsNullOrEmpty(currentValue))
            {
                var lines = currentValue.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var productLines = lines.Where(l => l.StartsWith("- ")).ToList();
        
                if (productLines.Any())
                {
                    var manualDescription = string.Join("\n", lines.TakeWhile(l => !l.StartsWith("- ")));
                    MealTextBox.Text = manualDescription;
                    
                    foreach (var line in productLines)
                    {
                        var productName = line.Substring(2).Split('(')[0].Trim();
                        if (_allProducts.FirstOrDefault(p => p.Name == productName) is FoodItem product)
                        {
                            SelectedProducts.Add(product);
                        }
                    }
                }
                else
                {
                    MealTextBox.Text = currentValue;
                }
            }
    
            LoadProducts();
        }

        private void LoadProducts()
        {
            string filePath = "foods.json";
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _allProducts = JsonSerializer.Deserialize<List<FoodItem>>(json) ?? new List<FoodItem>();
                ProductsGrid.ItemsSource = _allProducts;
            }
            else
            {
                _allProducts = new List<FoodItem>();
                MessageBox.Show("Файл foods.json не знайдено.");
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            SelectedProducts.Clear();
            
            MealTextBox.Clear();
            
            ProductsGrid.Items.Refresh();
            
            MessageBox.Show("Прийом їжі очищено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is FoodItem selectedProduct)
            {
                if (!SelectedProducts.Any(p => p.Name == selectedProduct.Name))
                {
                    SelectedProducts.Add(new FoodItem 
                    {
                        Name = selectedProduct.Name,
                        Calories = selectedProduct.Calories,
                        Protein = selectedProduct.Protein,
                        Fat = selectedProduct.Fat,
                        Carbohydrates = selectedProduct.Carbohydrates
                    });
                    
                    ProductsGrid.Items.Refresh(); 
                    UpdateMealDescription();
                }
            }
        }

        private void UpdateMealDescription()
        {
            var description = new StringBuilder();
            
            if (SelectedProducts.Any())
            {
                description.AppendLine("\nДодані продукти:");
                foreach (var product in SelectedProducts)
                {
                    description.AppendLine($"- {product.Name} ({product.Calories} ккал)");
                }
            }
    
            MealTextBox.Text = description.ToString();
            MealTextBox.CaretIndex = MealTextBox.Text.Length; 
        }
        

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mealDetails = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(MealTextBox.Text))
                    mealDetails.AppendLine(MealTextBox.Text.Trim());

                if (SelectedProducts.Any())
                {
                    var total = new
                    {
                        Calories = SelectedProducts.Sum(p => p.Calories),
                        Protein = SelectedProducts.Sum(p => p.Protein),
                        Fat = SelectedProducts.Sum(p => p.Fat),
                        Carbs = SelectedProducts.Sum(p => p.Carbohydrates)
                    };

                    mealDetails.AppendLine($"\nВсього: {total.Calories} ккал, Білки: {total.Protein:F1}g, Жири: {total.Fat:F1}g, Вуглеводи: {total.Carbs:F1}g");
                }

                MealDescription = mealDetails.ToString();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
            finally
            {
                Close();
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

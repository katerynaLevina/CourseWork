using System;
using System.Windows;

namespace WpfApp1
{
    public partial class AddFoodWindow : Window
    {
        public FoodItem NewItem { get; private set; }
        
        public AddFoodWindow()
        {
            InitializeComponent();
            NewItem = new FoodItem();
            DataContext = NewItem;
        }
        
        public AddFoodWindow(FoodItem existingItem) : this()
        {
            NewItem.Name = existingItem.Name;
            NewItem.Calories = existingItem.Calories;
            NewItem.Protein = existingItem.Protein;
            NewItem.Fat = existingItem.Fat;
            NewItem.Carbohydrates = existingItem.Carbohydrates;
            
            NameBox.Text = NewItem.Name;
            CaloriesBox.Text = NewItem.Calories.ToString();
            ProteinBox.Text = NewItem.Protein.ToString();
            FatBox.Text = NewItem.Fat.ToString();
            CarbsBox.Text = NewItem.Carbohydrates.ToString();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NewItem.Name = NameBox.Text;
                NewItem.Calories = int.Parse(CaloriesBox.Text);
                NewItem.Protein = double.Parse(ProteinBox.Text);
                NewItem.Fat = double.Parse(FatBox.Text);
                NewItem.Carbohydrates = double.Parse(CarbsBox.Text);

                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show("Перевірте правильність введення чисел", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

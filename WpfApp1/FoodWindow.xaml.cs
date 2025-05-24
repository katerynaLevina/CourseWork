using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfApp1
{
    public partial class FoodWindow : Window, INotifyPropertyChanged
    {
        private List<FoodItem> allFoodItems;
        private string _searchText;
        private ICommand _searchCommand;

        private const string FilePath = "foods.json";

        public FoodWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            LoadFoodItems();
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand => _searchCommand ??= new RelayCommand(
            _ => SearchFoodItems(),
            _ => !string.IsNullOrWhiteSpace(SearchText)
        );

        private void LoadFoodItems()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                allFoodItems = JsonSerializer.Deserialize<List<FoodItem>>(json) ?? new List<FoodItem>();
            }
            else
            {
                allFoodItems = new List<FoodItem>
                {
                    new FoodItem { Name = "Яблуко", Calories = 52, Protein = 0.3, Fat = 0.2, Carbohydrates = 14 },
                    new FoodItem { Name = "Банан", Calories = 89, Protein = 1.1, Fat = 0.3, Carbohydrates = 23 },
                    new FoodItem { Name = "Куряче філе", Calories = 165, Protein = 31, Fat = 3.6, Carbohydrates = 0 },
                    new FoodItem { Name = "Лосось", Calories = 208, Protein = 20, Fat = 13, Carbohydrates = 0 },
                    new FoodItem { Name = "Грецький йогурт", Calories = 59, Protein = 10, Fat = 0.4, Carbohydrates = 3.6 },
                    new FoodItem { Name = "Мигдаль", Calories = 579, Protein = 21, Fat = 50, Carbohydrates = 22 }
                };

                SaveFoodItems(); 
            }

            FoodItemsControl.ItemsSource = allFoodItems;
        }

        private void SaveFoodItems()
        {
            var json = JsonSerializer.Serialize(allFoodItems, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        private void SearchFoodItems()
        {
            var text = SearchTextBox.Text?.Trim() ?? "";

            bool TryParse(string? input, out double value)
                => double.TryParse(input?.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);

            TryParse(MinCaloriesBox.Text, out var minCalories);
            TryParse(MaxCaloriesBox.Text, out var maxCalories);
            TryParse(MinProteinBox.Text, out var minProtein);
            TryParse(MaxProteinBox.Text, out var maxProtein);
            TryParse(MinFatBox.Text, out var minFat);
            TryParse(MaxFatBox.Text, out var maxFat);
            TryParse(MinCarbsBox.Text, out var minCarbs);
            TryParse(MaxCarbsBox.Text, out var maxCarbs);

            var filtered = allFoodItems
                .Where(f =>
                    (string.IsNullOrWhiteSpace(text) || f.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (minCalories == 0 || f.Calories >= minCalories) &&
                    (maxCalories == 0 || f.Calories <= maxCalories) &&
                    (minProtein == 0 || f.Protein >= minProtein) &&
                    (maxProtein == 0 || f.Protein <= maxProtein) &&
                    (minFat == 0 || f.Fat >= minFat) &&
                    (maxFat == 0 || f.Fat <= maxFat) &&
                    (minCarbs == 0 || f.Carbohydrates >= minCarbs) &&
                    (maxCarbs == 0 || f.Carbohydrates <= maxCarbs)
                )
                .ToList();

            FoodItemsControl.ItemsSource = filtered;
        }
        
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchFoodItems();
        }
        
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.DataContext is FoodItem food)
            {
                var editWindow = new AddFoodWindow(food);
                if (editWindow.ShowDialog() == true)
                {
                    food.Name = editWindow.NewItem.Name;
                    food.Calories = editWindow.NewItem.Calories;
                    food.Protein = editWindow.NewItem.Protein;
                    food.Fat = editWindow.NewItem.Fat;
                    food.Carbohydrates = editWindow.NewItem.Carbohydrates;
                    
                    FoodItemsControl.ItemsSource = null;
                    FoodItemsControl.ItemsSource = allFoodItems;

                    SaveFoodItems();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.DataContext is FoodItem food)
            {
                var result = MessageBox.Show($"Видалити продукт {food.Name}?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    allFoodItems.Remove(food);
                    FoodItemsControl.ItemsSource = null;
                    FoodItemsControl.ItemsSource = allFoodItems;
                    SaveFoodItems();
                }
            }
        }



        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddFoodWindow();

            if (dialog.ShowDialog() == true)
            {
                var newFood = dialog.NewItem;
                allFoodItems.Add(newFood);

                FoodItemsControl.ItemsSource = null;
                FoodItemsControl.ItemsSource = allFoodItems;

                SaveFoodItems();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FoodItem
    {
        public string Name { get; set; } = string.Empty; 
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value!;
            remove => CommandManager.RequerySuggested -= value!;
        }
    }
}

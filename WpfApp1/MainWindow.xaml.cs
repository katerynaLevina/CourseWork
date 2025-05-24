using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void OpenFoodWindow_Click(object sender, RoutedEventArgs e)
        {
            FoodWindow foodWindow = new FoodWindow();
            foodWindow.ShowDialog();
        }

        private void OpenMealWindow_Click(object sender, RoutedEventArgs e)
        {
            MealWindow mealWindow = new MealWindow();
            mealWindow.ShowDialog();  
        }

    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MealWindow : Window
    {
        private ObservableCollection<MealEntry> mealEntries;
        
        public class DailyCalories
        {
            public int MondayCalories { get; set; }
            public int TuesdayCalories { get; set; }
            public int WednesdayCalories { get; set; }
            public int ThursdayCalories { get; set; }
            public int FridayCalories { get; set; }
            public int SaturdayCalories { get; set; }
            public int SundayCalories { get; set; }
        }
        
        private DailyCalories dailyCalories = new DailyCalories();
        
        public class MealEntry
        {
            public string MealType { get; set; }
            public string MealIcon { get; set; }
            public string Monday { get; set; }
            public string Tuesday { get; set; }
            public string Wednesday { get; set; }
            public string Thursday { get; set; }
            public string Friday { get; set; }
            public string Saturday { get; set; }
            public string Sunday { get; set; }
        }

        public class MealPlanData
        {
            public DateTime Week { get; set; }
            public string Goal { get; set; }
            public List<MealEntry> Meals { get; set; }
        }

        public MealWindow()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            mealEntries = new ObservableCollection<MealEntry>
            {
                new MealEntry { MealType = "Сніданок", MealIcon = "🍳" },
                new MealEntry { MealType = "Обід", MealIcon = "🍲" },
                new MealEntry { MealType = "Перекус", MealIcon = "🥗" },
                new MealEntry { MealType = "Вечеря", MealIcon = "🍽️" }
            };
            MealGrid.ItemsSource = mealEntries;
            UpdateTotalCaloriesText();
        }

        private void AddMeal_Click(object sender, RoutedEventArgs e)
        {
            mealEntries.Add(new MealEntry { MealType = "Додатковий прийом", MealIcon = "🍽️" });
        }

        private void SaveMealsToFile()
        {
            var saveData = new MealPlanData
            {
                Week = WeekPicker.SelectedDate ?? DateTime.Today,
                Goal = GoalTextBox.Text,
                Meals = new List<MealEntry>(mealEntries)
            };

            try
            {
                var serializer = new XmlSerializer(typeof(MealPlanData));
                var filename = $"mealplan_{saveData.Week:yyyyMMdd}.xml";
                using (var writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, saveData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMealsToFile();
            MessageBox.Show("План харчування збережено!");
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = WeekPicker.SelectedDate ?? DateTime.Today;
            var filename = $"mealplan_{selectedDate:yyyyMMdd}.xml";

            if (File.Exists(filename))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(MealPlanData));
                    using (var reader = new StreamReader(filename))
                    {
                        var loadedData = (MealPlanData)serializer.Deserialize(reader);
                        WeekPicker.SelectedDate = loadedData.Week;
                        GoalTextBox.Text = loadedData.Goal;
                        mealEntries = new ObservableCollection<MealEntry>(loadedData.Meals);
                        MealGrid.ItemsSource = mealEntries;
                        CalculateCalories();
                        UpdateTotalCaloriesText();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("План харчування для обраного тижня не знайдено.");
            }
        }

        private void MealGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cellInfo = MealGrid.CurrentCell;
            
            var mealEntry = cellInfo.Item as MealEntry;
            if (mealEntry == null) return;

            string dayOfWeek = cellInfo.Column.Header.ToString();
            string currentValue = GetMealValue(mealEntry, dayOfWeek);

            var editWindow = new EditMealWindow(dayOfWeek, mealEntry.MealType, currentValue);
            if (editWindow.ShowDialog() == true)
            {
                SetMealValue(mealEntry, dayOfWeek, editWindow.MealDescription);

               
                var view = CollectionViewSource.GetDefaultView(MealGrid.ItemsSource);
                if (view is IEditableCollectionView editableView)
                {
                    if (editableView.IsEditingItem)
                        editableView.CommitEdit();
                    if (editableView.IsAddingNew)
                        editableView.CommitNew();
                }

                MealGrid.Items.Refresh();
                CalculateCalories();
                UpdateTotalCaloriesText();
            }
        }

        private string GetMealValue(MealEntry meal, string day)
        {
            switch (day)
            {
                case "Понеділок": return meal.Monday;
                case "Вівторок": return meal.Tuesday;
                case "Середа": return meal.Wednesday;
                case "Четвер": return meal.Thursday;
                case "П'ятниця": return meal.Friday;
                case "Субота": return meal.Saturday;
                case "Неділя": return meal.Sunday;
                default: return string.Empty;
            }
        }

        private void SetMealValue(MealEntry mealEntry, string dayOfWeek, string value)
        {
            switch (dayOfWeek)
            {
                case "Понеділок":
                    mealEntry.Monday = value;
                    break;
                case "Вівторок":
                    mealEntry.Tuesday = value;
                    break;
                case "Середа":
                    mealEntry.Wednesday = value;
                    break;
                case "Четвер":
                    mealEntry.Thursday = value;
                    break;
                case "П'ятниця":
                    mealEntry.Friday = value;
                    break;
                case "Субота":
                    mealEntry.Saturday = value;
                    break;
                case "Неділя":
                    mealEntry.Sunday = value;
                    break;
            }
        }

        private void UpdateCalories_Click(object sender, RoutedEventArgs e)
        {
            CalculateCalories();
            UpdateTotalCaloriesText();
        }

        private void CalculateCalories()
        {
            dailyCalories.MondayCalories = CalculateDayCalories("Понеділок");
            dailyCalories.TuesdayCalories = CalculateDayCalories("Вівторок");
            dailyCalories.WednesdayCalories = CalculateDayCalories("Середа");
            dailyCalories.ThursdayCalories = CalculateDayCalories("Четвер");
            dailyCalories.FridayCalories = CalculateDayCalories("П'ятниця");
            dailyCalories.SaturdayCalories = CalculateDayCalories("Субота");
            dailyCalories.SundayCalories = CalculateDayCalories("Неділя");
        }

        private int CalculateDayCalories(string day)
        {
            int total = 0;
            foreach (var mealEntry in mealEntries)
            {
                string meal = GetMealValue(mealEntry, day);
                
                if (!string.IsNullOrEmpty(meal))
                {
                    total += EstimateCaloriesFromText(meal);
                }
            }
            return total;
        }

        private int EstimateCaloriesFromText(string mealText)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"Всього:\s*(\d+)");
            var match = regex.Match(mealText);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int calories))
            {
                return calories;
            }
            return 0;
        }



        private void UpdateTotalCaloriesText()
        {
            TotalCaloriesText.Text = $"Загальна кількість калорій: \n" +
                                    $"Понеділок: {dailyCalories.MondayCalories} | " +
                                    $"Вівторок: {dailyCalories.TuesdayCalories} | " +
                                    $"Середа: {dailyCalories.WednesdayCalories} | " +
                                    $"Четвер: {dailyCalories.ThursdayCalories} | " +
                                    $"П'ятниця: {dailyCalories.FridayCalories} | " +
                                    $"Субота: {dailyCalories.SaturdayCalories} | " +
                                    $"Неділя: {dailyCalories.SundayCalories}";
        }

        private void MealGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.GetIndex() == MealGrid.Items.Count - 1)
            {
                e.Row.Background = new SolidColorBrush(Color.FromRgb(240, 247, 244));
                e.Row.FontWeight = FontWeights.Bold;
            }
        }
    }
}
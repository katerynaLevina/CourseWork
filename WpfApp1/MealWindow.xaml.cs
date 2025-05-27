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
        
        public class NutritionInfo
        {
            public int Calories { get; set; }
            public int Proteins { get; set; }
            public int Fats { get; set; }
            public int Carbs { get; set; }
        }

        public class DailyNutrition
        {
            public NutritionInfo Monday { get; set; } = new();
            public NutritionInfo Tuesday { get; set; } = new();
            public NutritionInfo Wednesday { get; set; } = new();
            public NutritionInfo Thursday { get; set; } = new();
            public NutritionInfo Friday { get; set; } = new();
            public NutritionInfo Saturday { get; set; } = new();
            public NutritionInfo Sunday { get; set; } = new();
        }
        
        private DailyNutrition dailyNutrition = new DailyNutrition();

        
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
                        CalculateNutrition();
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
                CalculateNutrition();
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

        private void CalculateNutrition()
        {
            dailyNutrition.Monday = CalculateDayNutrition("Понеділок");
            dailyNutrition.Tuesday = CalculateDayNutrition("Вівторок");
            dailyNutrition.Wednesday = CalculateDayNutrition("Середа");
            dailyNutrition.Thursday = CalculateDayNutrition("Четвер");
            dailyNutrition.Friday = CalculateDayNutrition("П'ятниця");
            dailyNutrition.Saturday = CalculateDayNutrition("Субота");
            dailyNutrition.Sunday = CalculateDayNutrition("Неділя");
        }


        private NutritionInfo CalculateDayNutrition(string day)
        {
            var info = new NutritionInfo();
            foreach (var mealEntry in mealEntries)
            {
                string meal = GetMealValue(mealEntry, day);
                if (!string.IsNullOrEmpty(meal))
                {
                    var extracted = ExtractNutritionFromText(meal);
                    info.Calories += extracted.Calories;
                    info.Proteins += extracted.Proteins;
                    info.Fats += extracted.Fats;
                    info.Carbs += extracted.Carbs;
                }
            }
            return info;
        }


        // правки обрахунок кбжв статистика
        private NutritionInfo ExtractNutritionFromText(string text)
        {
            var info = new NutritionInfo();

            var regex = new System.Text.RegularExpressions.Regex(
                @"Всього:\s*([\d,]+)\s*ккал.*?Білки:\s*([\d,]+)г.*?Жири:\s*([\d,]+)г.*?Вуглеводи:\s*([\d,]+)г",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            var match = regex.Match(text);

            if (match.Success)
            {
                if (TryParseDecimal(match.Groups[1].Value, out int calories))
                    info.Calories = calories;

                if (TryParseDecimal(match.Groups[2].Value, out int proteins))
                    info.Proteins = proteins;

                if (TryParseDecimal(match.Groups[3].Value, out int fats))
                    info.Fats = fats;

                if (TryParseDecimal(match.Groups[4].Value, out int carbs))
                    info.Carbs = carbs;
            }

            return info;
        }


        private bool TryParseDecimal(string input, out int result)
        {
            result = 0;
            
            var normalized = input.Replace(',', '.');

            if (double.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                result = (int)Math.Round(val);
                return true;
            }
            return false;
        }





        private void UpdateTotalCaloriesText()
        {
            string FormatDay(string name, NutritionInfo n) =>
                $"{name}: {n.Calories} ккал, Б: {n.Proteins}г, Ж: {n.Fats}г, В: {n.Carbs}г";

            TotalCaloriesText.Text = "Загальна кількість за день:\n" +
                                     FormatDay("Понеділок", dailyNutrition.Monday) + "\n" +
                                     FormatDay("Вівторок", dailyNutrition.Tuesday) + "\n" +
                                     FormatDay("Середа", dailyNutrition.Wednesday) + "\n" +
                                     FormatDay("Четвер", dailyNutrition.Thursday) + "\n" +
                                     FormatDay("П'ятниця", dailyNutrition.Friday) + "\n" +
                                     FormatDay("Субота", dailyNutrition.Saturday) + "\n" +
                                     FormatDay("Неділя", dailyNutrition.Sunday);
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
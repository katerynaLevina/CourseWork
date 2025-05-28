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

        // правка 
        public class MealEntry
        {
            public string MealType { get; set; }
            public string MealIcon { get; set; }
            public List<FoodItem> Monday { get; set; } = new();
            public List<FoodItem> Tuesday { get; set; } = new();
            public List<FoodItem> Wednesday { get; set; } = new();
            public List<FoodItem> Thursday { get; set; } = new();
            public List<FoodItem> Friday { get; set; } = new();
            public List<FoodItem> Saturday { get; set; } = new();
            public List<FoodItem> Sunday { get; set; } = new();
            
            private static string GetMealSummary(List<FoodItem> items)
            {
                if (items == null || !items.Any())
                    return "";

                var summary = new System.Text.StringBuilder();
                summary.AppendLine("Додані продукти:");

                foreach (var item in items)
                {
                    summary.AppendLine($"{item.Name}: {item.Calories} ккал, Б: {item.Protein}г, Ж: {item.Fat}г, В: {item.Carbohydrates}г");
                }

                return summary.ToString().TrimEnd();
            }

            
            public string MondaySummary => GetMealSummary(Monday);
            public string TuesdaySummary => GetMealSummary(Tuesday);
            public string WednesdaySummary => GetMealSummary(Wednesday);
            public string ThursdaySummary => GetMealSummary(Thursday);
            public string FridaySummary => GetMealSummary(Friday);
            public string SaturdaySummary => GetMealSummary(Saturday);
            public string SundaySummary => GetMealSummary(Sunday);


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

        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMealsToFile();
            MessageBox.Show("План харчування збережено!");
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        public class AllMealPlans
        {
            public List<MealPlanData> Plans { get; set; } = new List<MealPlanData>();
        }
        
        private const string AllPlansFileName = "all_mealplans.xml";

        // правки аби все зберігалось в одному файлі, підвантажується відповідно до обраного тижня
        private void SaveMealsToFile()
        {
            var selectedDate = WeekPicker.SelectedDate ?? DateTime.Today;
            var weekStart = GetStartOfWeek(selectedDate);

            
            AllMealPlans allPlans = new AllMealPlans();

            if (File.Exists(AllPlansFileName))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(AllMealPlans));
                    using (var reader = new StreamReader(AllPlansFileName))
                    {
                        allPlans = (AllMealPlans)serializer.Deserialize(reader);
                    }
                }
                catch
                {
                    
                    allPlans = new AllMealPlans();
                }
            }

            
            var existingPlan = allPlans.Plans.FirstOrDefault(p => p.Week == weekStart);

            if (existingPlan != null)
            {
                existingPlan.Goal = GoalTextBox.Text;
                existingPlan.Meals = mealEntries.ToList();
            }
            else
            {
                allPlans.Plans.Add(new MealPlanData
                {
                    Week = weekStart,
                    Goal = GoalTextBox.Text,
                    Meals = mealEntries.ToList()
                });
            }
            
            try
            {
                var serializer = new XmlSerializer(typeof(AllMealPlans));
                using (var writer = new StreamWriter(AllPlansFileName))
                {
                    serializer.Serialize(writer, allPlans);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
        }


        private void LoadMealsFromFile()
        {
            var selectedDate = WeekPicker.SelectedDate ?? DateTime.Today;
            var weekStart = GetStartOfWeek(selectedDate);

            if (!File.Exists(AllPlansFileName))
            {
                MessageBox.Show("Файл планів не знайдено.");
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(AllMealPlans));
                using (var reader = new StreamReader(AllPlansFileName))
                {
                    var allPlans = (AllMealPlans)serializer.Deserialize(reader);

                    var plan = allPlans.Plans.FirstOrDefault(p => p.Week == weekStart);

                    if (plan != null)
                    {
                        WeekPicker.SelectedDate = plan.Week;
                        GoalTextBox.Text = plan.Goal;
                        mealEntries = new ObservableCollection<MealEntry>(plan.Meals);
                        MealGrid.ItemsSource = mealEntries;

                        CalculateNutrition();
                        UpdateTotalCaloriesText();
                    }
                    else
                    {
                        MessageBox.Show("План харчування для обраного тижня не знайдено.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMealsFromFile();
        }



        private void MealGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cellInfo = MealGrid.CurrentCell;
            var mealEntry = cellInfo.Item as MealEntry;
            if (mealEntry == null) return;

            string dayOfWeek = cellInfo.Column?.Header?.ToString();
            if (string.IsNullOrEmpty(dayOfWeek)) return;

            var editWindow = new EditMealWindow(dayOfWeek, mealEntry.MealType, null);
            if (editWindow.ShowDialog() == true)
            {
                SetMealItems(mealEntry, dayOfWeek, new List<FoodItem>(editWindow.SelectedProducts));
                
                if (MealGrid.CommitEdit(DataGridEditingUnit.Row, true))
                {
                    MealGrid.Items.Refresh(); 
                }

                CalculateNutrition();
                UpdateTotalCaloriesText();
            }
        }
        
        
        private List<FoodItem> GetMealItems(MealEntry entry, string day) => day switch
        {
            "Понеділок" => entry.Monday,
            "Вівторок" => entry.Tuesday,
            "Середа" => entry.Wednesday,
            "Четвер" => entry.Thursday,
            "П'ятниця" => entry.Friday,
            "Субота" => entry.Saturday,
            "Неділя" => entry.Sunday,
            _ => new List<FoodItem>()
        };


        private void SetMealItems(MealEntry entry, string day, List<FoodItem> items)
        {
            switch (day)
            {
                case "Понеділок": entry.Monday = items; break;
                case "Вівторок": entry.Tuesday = items; break;
                case "Середа": entry.Wednesday = items; break;
                case "Четвер": entry.Thursday = items; break;
                case "П'ятниця": entry.Friday = items; break;
                case "Субота": entry.Saturday = items; break;
                case "Неділя": entry.Sunday = items; break;
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


        // обрахунок кбжв використовуючи об'єкти FoodItem
        private NutritionInfo CalculateDayNutrition(string day)
        {
            var info = new NutritionInfo();
            foreach (var mealEntry in mealEntries)
            {
                var items = GetMealItems(mealEntry, day);
                foreach (var item in items)
                {
                    info.Calories += item.Calories;
                    info.Proteins += (int)Math.Round(item.Protein);
                    info.Fats += (int)Math.Round(item.Fat);
                    info.Carbs += (int)Math.Round(item.Carbohydrates);
                }
            }
            return info;
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
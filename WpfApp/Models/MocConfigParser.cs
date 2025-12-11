using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using WpfApp.Models;

namespace WpfApp.Models
{
    public class MocConfigParser
    {
        public JointLimits ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            string content = File.ReadAllText(filePath);
            return ParseContent(content);
        }

        public JointLimits ParseContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new JointLimits();

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            bool inArmSection = false;
            var lowerBounds = new double[6];
            var upperBounds = new double[6];
            bool[] found = new bool[6]; // отслеживаем, найдены ли все 6 осей

            string currentArmName = null;
            double? lower = null;
            double? upper = null;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // Пропускаем комментарии и пустые строки
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // Начало секции ARM
                if (trimmed.Equals("ARM:", StringComparison.OrdinalIgnoreCase))
                {
                    inArmSection = true;
                    continue;
                }

                // Выход из секции ARM (новая секция)
                if (inArmSection && trimmed.Contains(':') && !trimmed.StartsWith("-"))
                {
                    inArmSection = false;
                    SaveCurrentJoint(ref currentArmName, ref lower, ref upper, lowerBounds, upperBounds, found);
                    continue;
                }

                if (!inArmSection)
                    continue;

                // Новая ось
                if (trimmed.StartsWith("-name \"rob1_"))
                {
                    // Сохраняем предыдущую ось
                    SaveCurrentJoint(ref currentArmName, ref lower, ref upper, lowerBounds, upperBounds, found);

                    // Извлекаем имя оси
                    var nameMatch = Regex.Match(trimmed, @"-name\s+""(rob1_(\d+))""");
                    if (nameMatch.Success)
                    {
                        currentArmName = nameMatch.Groups[1].Value;
                        string indexStr = nameMatch.Groups[2].Value;
                        if (int.TryParse(indexStr, out int jointIndex) && jointIndex >= 1 && jointIndex <= 6)
                        {
                            // Всё ок — имя валидно
                        }
                        else
                        {
                            currentArmName = null; // игнорируем недопустимые индексы
                        }
                    }
                    else
                    {
                        currentArmName = null;
                    }

                    lower = null;
                    upper = null;
                }

                if (currentArmName == null)
                    continue;

                // Извлекаем lower_joint_bound
                var lowerMatch = Regex.Match(trimmed, @"-lower_joint_bound\s+([+-]?\d*\.?\d+)");
                if (lowerMatch.Success && double.TryParse(lowerMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double l))
                {
                    lower = l;
                }

                // Извлекаем upper_joint_bound
                var upperMatch = Regex.Match(trimmed, @"-upper_joint_bound\s+([+-]?\d*\.?\d+)");
                if (upperMatch.Success && double.TryParse(upperMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double u))
                {
                    upper = u;
                }
            }

            // Сохраняем последнюю ось
            SaveCurrentJoint(ref currentArmName, ref lower, ref upper, lowerBounds, upperBounds, found);

            // Проверяем, что все 6 осей найдены
            for (int i = 0; i < 6; i++)
            {
                if (!found[i])
                {
                    // Если ось не найдена — используем безопасные значения (можно заменить на исключение)
                    lowerBounds[i] = -Math.PI;
                    upperBounds[i] = Math.PI;
                }
            }

            return new JointLimits(lowerBounds, upperBounds);
        }

        private void SaveCurrentJoint(
            ref string currentArmName,
            ref double? lower,
            ref double? upper,
            double[] lowerBounds,
            double[] upperBounds,
            bool[] found)
        {
            if (currentArmName != null && lower.HasValue && upper.HasValue)
            {
                // rob1_1 → индекс 0, rob1_2 → индекс 1, ..., rob1_6 → индекс 5
                if (currentArmName.Length >= 6 && currentArmName.StartsWith("rob1_"))
                {
                    string indexPart = currentArmName.Substring(5); // "1", "2", ...
                    if (int.TryParse(indexPart, out int index) && index >= 1 && index <= 6)
                    {
                        int arrayIndex = index - 1;
                        lowerBounds[arrayIndex] = lower.Value;
                        upperBounds[arrayIndex] = upper.Value;
                        found[arrayIndex] = true;
                    }
                }
            }

            currentArmName = null;
            lower = null;
            upper = null;
        }

        // Метод можно оставить, хотя сейчас он не используется в парсере
        public double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }
    }
}
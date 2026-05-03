using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Errors Helper", menuName = "Errors Reader")]
public class LogReader : ScriptableObject
{
    [Header("Настройки файла с ошибками")]
    [Tooltip("XML файл с описанием ошибок")]
    public TextAsset errorsXmlFile;

    /// <summary>
    /// Получает информацию об ошибке по ID
    /// </summary>
    public ErrorInfo GetError(string id)
    {
        if (errorsXmlFile == null)
        {
            Debug.LogError($"Errors Helper: файл с ошибками не выбран! ({name})");
            return null;
        }

        try
        {
            using (StringReader stringReader = new StringReader(errorsXmlFile.text))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Error")
                    {
                        using (XmlReader subtree = reader.ReadSubtree())
                        {
                            var element = XElement.Load(subtree);
                            string errorId = element.Element("ID")?.Value;

                            if (errorId == id)
                            {
                                return new ErrorInfo
                                {
                                    ID = errorId,
                                    Category = element.Element("Category")?.Value,
                                    Type = element.Element("Type")?.Value,
                                    Level = element.Element("Level")?.Value,
                                    UnityMessage = element.Element("UnityMessage")?.Value,
                                    SuggestedFix = element.Element("SuggestedFix")?.Value,
                                    AdditionalInfo = element.Element("AdditionalInfo")?.Value
                                };
                            }
                        }
                    }
                }
            }

            Debug.LogWarning($"Errors Helper: ошибка с ID '{id}' не найдена");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при чтении Errors Helper: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Получает все ошибки определенного уровня
    /// </summary>
    public List<ErrorInfo> GetErrorsByLevel(string level)
    {
        var result = new List<ErrorInfo>();

        if (errorsXmlFile == null)
        {
            Debug.LogError($"Errors Helper: файл с ошибками не выбран! ({name})");
            return result;
        }

        try
        {
            using (StringReader stringReader = new StringReader(errorsXmlFile.text))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Error")
                    {
                        using (XmlReader subtree = reader.ReadSubtree())
                        {
                            var element = XElement.Load(subtree);
                            string errorLevel = element.Element("Level")?.Value;

                            if (string.Equals(errorLevel, level, StringComparison.OrdinalIgnoreCase))
                            {
                                result.Add(new ErrorInfo
                                {
                                    ID = element.Element("ID")?.Value,
                                    Category = element.Element("Category")?.Value,
                                    Type = element.Element("Type")?.Value,
                                    Level = errorLevel,
                                    UnityMessage = element.Element("UnityMessage")?.Value,
                                    SuggestedFix = element.Element("SuggestedFix")?.Value,
                                    AdditionalInfo = element.Element("AdditionalInfo")?.Value
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при чтении Errors Helper: {e.Message}");
        }

        return result;
    }

    /// <summary>
    /// Проверяет существование ошибки
    /// </summary>
    public bool HasError(string id)
    {
        if (errorsXmlFile == null) return false;

        try
        {
            using (StringReader stringReader = new StringReader(errorsXmlFile.text))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Error")
                    {
                        using (XmlReader subtree = reader.ReadSubtree())
                        {
                            var element = XElement.Load(subtree);
                            if (element.Element("ID")?.Value == id)
                                return true;
                        }
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Получает общее количество ошибок
    /// </summary>
    public int TotalErrorsCount
    {
        get
        {
            if (errorsXmlFile == null) return 0;
            int count = 0;

            try
            {
                using (StringReader stringReader = new StringReader(errorsXmlFile.text))
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Error")
                        {
                            count++;
                            reader.Skip(); // Пропускаем содержимое для скорости
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }

            return count;
        }
    }

    /// <summary>
    /// Получает все уникальные уровни ошибок
    /// </summary>
    public List<string> GetAllLevels()
    {
        var levels = new HashSet<string>();

        if (errorsXmlFile == null) return new List<string>();

        try
        {
            using (StringReader stringReader = new StringReader(errorsXmlFile.text))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Error")
                    {
                        using (XmlReader subtree = reader.ReadSubtree())
                        {
                            var element = XElement.Load(subtree);
                            string level = element.Element("Level")?.Value;
                            if (!string.IsNullOrEmpty(level))
                            {
                                levels.Add(level);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при чтении Errors Helper: {e.Message}");
        }

        return new List<string>(levels);
    }

    /// <summary>
    /// Выводит ошибку в консоль
    /// </summary>
    public void LogError(string id)
    {
        var error = GetError(id);
        if (error != null)
        {
            string message = FormatLogMessage(error);
            switch (error.Level?.ToUpper())
            {
                case "ERROR":
                    Debug.LogError(message);
                    break;
                case "WARNING":
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
    }

    private string FormatLogMessage(ErrorInfo error)
    {
        var message = $"[{error.Level}] ID: {error.ID}\n" +
                     $"Категория: {error.Category}\n" +
                     $"Тип: {error.Type}\n" +
                     $"Сообщение: {error.UnityMessage}\n" +
                     $"Решение: {error.SuggestedFix}";

        if (!string.IsNullOrEmpty(error.AdditionalInfo))
        {
            message += $"\nДоп. информация: {error.AdditionalInfo}";
        }

        return message;
    }

    [Serializable]
    public class ErrorInfo
    {
        public string ID { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Level { get; set; }
        public string UnityMessage { get; set; }
        public string SuggestedFix { get; set; }
        public string AdditionalInfo { get; set; }

        public override string ToString()
        {
            return $"[{Level}] {ID}: {UnityMessage}";
        }
    }
}
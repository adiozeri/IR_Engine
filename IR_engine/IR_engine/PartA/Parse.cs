using IR_engine.PartA;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngine.PartA
{
    /// <summary>
    /// get text and parse it into terms 
    /// </summary>
    public class Parse
    {
        // indicate the months to find in the text
        private enum Months { jan = 01, january = 01, feb = 02, february = 02, mar = 03, march = 03, apr = 04, april = 04, may = 05, jun = 06, june = 06, jul = 07, july = 07, aug = 08, august = 08, sep = 09, september = 09, oct = 10, october = 10, nov = 11, november = 11, dec = 12, december = 12 };

        bool ToStem;
        // stemmer to stem the words if neccasry
        private static StemmerInterface stemmer = new Stemmer();
        // set of stop words
        HashSet<string> StopWord;
        // save the delimeters we split the words in the text according
        private char[] delimetersToSplitWords = { '-', ' ', '\n', ';', '?', '!', '~', '=', '&', '+', '*', '|', '-', };
        /// <summary>
        /// parse constructor - save the stop words needed to be removed and the stem flag
        /// </summary>
        /// <param name="stopWordPath">the path of the stop word list</param>
        /// <param name="toStem">flag indicate whether to stem the terms or not</param>
        public Parse(string stopWordPath, bool toStem)
        {
            ToStem = toStem;
            StopWord = new HashSet<string>(File.ReadAllLines(stopWordPath));
            AddSpecialTermToStopWords();
        }
        /// <summary>
        /// add corpus stop words into the hash set of stop words 
        /// </summary>
        private void AddSpecialTermToStopWords()
        {
            StopWord.Add("article");
            StopWord.Add("typebfn");
            StopWord.Add("language");
            StopWord.Add("text");
            StopWord.Add("chj");
            StopWord.Add("cvj");
            StopWord.Add("wdm");
            StopWord.Add("cwl");
            StopWord.Add("ncols");
            StopWord.Add("�.subfe");
            StopWord.Add("�.subfeco");
            StopWord.Add("�.sup");
            StopWord.Add("�.sup2capacity");

        }

        /// <summary>
        /// parse the given text according to parsing rule 
        /// </summary>
        /// <param name="text">the text needed to parse</param>
        /// <param name="maxTerm">update the max term in the text</param>
        /// <returns>the dictionary of the terms and it's frequncy in the text</returns>
        public Dictionary<string, int> ParseText(string text, out KeyValuePair<string, int> maxTerm)
        {
            // update the max term
            int maxTermFreq = 0;
            string maxTermString = "";
            Dictionary<string, int> parsingDictionary = new Dictionary<string, int>();
            text = RemoveSymbolsFromDocument(text);
            string[] terms = text.Split(delimetersToSplitWords, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < terms.Length; i++)
            {
                string new_word = terms[i];
                new_word = RemoveSpecialCahracter(new_word);

                if (IsStopWord(new_word))
                    continue;

                // check for date
                if (i < terms.Length - 3 && ((new_word.All(char.IsDigit) || terms[i + 1].All(char.IsDigit)) &&
                    (Enum.IsDefined(typeof(Months), new_word.ToLower()) || Enum.IsDefined(typeof(Months), terms[i + 1].ToLower()))))
                {
                    int retNum1;
                    if (Int32.TryParse(terms[i + 2], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum1))
                    {
                        new_word = ParseDate(new_word + ' ' + terms[i + 1] + ' ' + terms[i + 2]);
                        i = i + 2;
                    }
                    else
                    {
                        new_word = ParseDate(new_word + ' ' + terms[i + 1]);
                        i++;
                    }
                }
                // parse expression with numbers
                else if (new_word.Any(char.IsDigit))
                {
                    string next_word = "";
                    if (i < terms.Length - 1)
                        next_word = terms[i + 1];
                    int increaseI = 0;
                    new_word = DealWithNumber(terms[i], next_word, out increaseI);
                    if (increaseI == -1)
                        continue;
                    i = i + increaseI;
                }
                else
                {
                    new_word = RemoveSpecialCahracterAfterDealingWithNumbers(new_word);
                    if (IsStopWord(new_word) || new_word.ToLower() == "may")
                        continue;
                    // check for capital letters
                    if (new_word.Any(c => char.IsUpper(c)))
                    {
                        new_word = (new_word).ToLower();
                        string next_word = "";
                        if (i < terms.Length - 2)
                        {
                            next_word = RemoveSpecialCahracter(terms[i + 1]).ToLower();
                            next_word = RemoveSpecialCahracterAfterDealingWithNumbers(next_word);
                        }

                        if (i < terms.Length - 2 && terms[i + 1].Any(c => char.IsUpper(c)) && !IsStopWord(next_word))
                        {
                            string new_expression = new_word + " " + next_word;

                            if (!parsingDictionary.ContainsKey(next_word))
                                parsingDictionary[next_word] = 1;
                            else
                            {
                                parsingDictionary[next_word]++;
                                if (parsingDictionary[next_word] > maxTermFreq)
                                {
                                    maxTermString = next_word;
                                    maxTermFreq = parsingDictionary[next_word];
                                }
                            }

                            if (!parsingDictionary.ContainsKey(new_expression))
                                parsingDictionary[new_expression] = 1;
                            else
                            {
                                parsingDictionary[new_expression]++;
                                if (parsingDictionary[new_expression] > maxTermFreq)
                                {
                                    maxTermString = new_expression;
                                    maxTermFreq = parsingDictionary[new_expression];
                                }
                            }
                            i++;
                        }
                    }
                }

                if (ToStem)
                    new_word = stemmer.stemTerm(new_word);

                if (!parsingDictionary.ContainsKey(new_word))
                    parsingDictionary[new_word] = 1;

                else
                {
                    parsingDictionary[new_word]++;
                    if (parsingDictionary[new_word] > maxTermFreq)
                    {
                        maxTermString = new_word;
                        maxTermFreq = parsingDictionary[new_word];
                    }
                }

            }
            if (maxTermFreq == 0 && parsingDictionary.Count > 0)
            {
                maxTermString = parsingDictionary.First().Key;
                maxTermFreq = parsingDictionary[maxTermString];
            }
            maxTerm = new KeyValuePair<string, int>(maxTermString, maxTermFreq);
            return parsingDictionary;
        }
        /// <summary>
        /// remove special characters from the begining and ending of the given term
        /// </summary>
        /// <param name="new_word">the given term needs to be clean</param>
        /// <returns>the cleaned term</returns>
        private string RemoveSpecialCahracter(string new_word)
        {
            if (new_word.Length > 1)
            {
                //new_word = new_word.TrimEnd('.', '^', '\\', '/', '|', '\"', '$', '-', ' ','\r');
                //new_word = new_word.TrimStart('.', '^', '\\', '/', '|', '\"', '-', ' ','\r');
                new_word = new_word.TrimEnd('.', '^', '\\', '/', '|', '\"', '$', '-', ' ');
                new_word = new_word.TrimStart('.', '^', '\\', '/', '|', '\"', '-', ' ');
            }

            return new_word;
        }
        /// <summary>
        /// remove special characters like % and $ from the given term
        /// </summary>
        /// <param name="new_word">the given term needs to be clean</param>
        /// <returns>the cleaned term</returns>
        private string RemoveSpecialCahracterAfterDealingWithNumbers(string new_word)
        {
            if (new_word.Length > 1)
            {
                new_word = new_word.Trim('%', '$');
            }
            return new_word;
        }
        /// <summary>
        /// remove special characters from the given text
        /// </summary>
        /// <param name="text">the text need to be cleand</param>
        /// <returns>the cleaned document text</returns>
        private string RemoveSymbolsFromDocument(string text)
        {
            StringBuilder textBuilder = new StringBuilder(text);
            textBuilder.Replace(",", String.Empty);
            textBuilder.Replace("'", String.Empty);
            textBuilder.Replace("`", String.Empty);
            textBuilder.Replace("\\", String.Empty);
            textBuilder.Replace("/", String.Empty);
            textBuilder.Replace("(", String.Empty);
            textBuilder.Replace(")", String.Empty);
            textBuilder.Replace("[", String.Empty);
            textBuilder.Replace("]", String.Empty);
            textBuilder.Replace("{", String.Empty);
            textBuilder.Replace("}", String.Empty);
            textBuilder.Replace("_", String.Empty);
            textBuilder.Replace(":", String.Empty);
            textBuilder.Replace("\"", String.Empty);

            return textBuilder.ToString();
        }
        /// <summary>
        /// convert the given term into the format of number needs: 00.00
        /// </summary>
        /// <param name="term">the term need to convert</param>
        /// <returns>the converted term</returns>
        private string NumberConvert(string term)
        {
            int IntResult;
            if (int.TryParse(term, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out IntResult))
            {
                return String.Format("{0}", IntResult);
            }

            double doubleResult;
            if (double.TryParse(term, out doubleResult))
            {
                return String.Format("{0:0.00}", doubleResult);
            }

            string[] split = term.Split(new char[] { ' ', '/' });

            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;

                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        return String.Format("{0:0.00}", (double)a / b);
                    }

                    int c;

                    if (int.TryParse(split[2], out c))
                    {
                        return String.Format("{0:0.00}", a + (double)b / c);
                    }
                }
            }
            return term;

        }
        /// <summary>
        /// convert the given term into the format of percent needs: 5 percent
        /// </summary>
        /// <param name="term">the term need to convert</param>
        /// <returns>the converted term</returns>
        private string Percentage(string term)
        {
            return NumberConvert(term) + " percent";
        }
        #region DateParse
        /// <summary>
        /// convert the given term into the format of date needs: 00/00/000
        /// </summary>
        /// <param name="term">the term need to convert</param>
        /// <returns>the converted term</returns>
        private string ParseDate(string term)
        {
            int format = checkFormat(term);
            switch (format)
            {
                case 1:
                    term = dayMonthYear(term);
                    break;
                case 2:
                    term = dayMonth(term);
                    break;
                case 3:
                    term = MonthYear(term);
                    break;
            }
            return term;
        }
        /// <summary>
        /// check in which format of date the given date is
        /// </summary>
        /// <param name="term">the term needed to check the foramt for </param>
        /// <returns>the number of the format of date</returns>
        private int checkFormat(string term)
        {
            string[] tokens = term.Split(' ');
            if (tokens.Length > 2)
                return 1;
            int number;
            bool result = Int32.TryParse(tokens[1], out number);
            if (result && number > 31)
                return 3;
            return 2;
        }
        /// <summary>
        /// convert the given term into the format of date with day,month and year needs: 00/00/0000
        /// </summary>
        /// <param name="term">the term need to convert</param>
        /// <returns>the converted term</returns>
        private string dayMonthYear(string term)
        {
            bool result;
            int number1, number, number2;
            string year, month;
            StringBuilder textBuilder = new StringBuilder(term);
            textBuilder.Replace(",", String.Empty);

            string s1 = textBuilder.ToString();
            string[] tokens = s1.Split(' ');

            result = Int32.TryParse(tokens[2], out number1);

            if (number1 < 100)
            {
                number1 = number1 + 1900;
                year = number1.ToString();
            }
            else year = number1.ToString();

            Months M;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (!(result = Int32.TryParse(tokens[i], out number)) && !tokens[i].Contains("th"))
                {
                    if (Enum.TryParse(tokens[i].ToLower(), false, out M))
                    {
                        month = ((int)Enum.Parse(typeof(Months), tokens[i].ToLower())).ToString();
                        if (month.Length < 2)
                            month = "0" + month;

                        for (int j = 0; j < tokens.Length; j++)
                        {
                            if (j == i)
                                continue;

                            result = Int32.TryParse(tokens[j], out number2);
                            if (!result)
                            {
                                string[] tmp = tokens[j].Split('t');

                                return tmp[0] + "/" + month + "/" + year;
                            }
                            else
                            {
                                return tokens[j] + "/" + month + "/" + year;
                            }

                        }
                    }
                }
                else
                {

                    if (!(result = Int32.TryParse(tokens[i], out number2)))
                    {

                        string[] tmp = tokens[i].Split('t');
                        if (Enum.TryParse(tokens[i + 1].ToLower(), false, out M))
                        {
                            month = ((int)Enum.Parse(typeof(Months), tokens[i + 1].ToLower())).ToString();
                            if (month.Length < 2)
                                month = "0" + month;
                            tokens[i + 1] = month;
                        }
                        return tmp[0] + "/" + tokens[i + 1] + "/" + year;

                    }
                    if (Enum.TryParse(tokens[i + 1].ToLower(), false, out M))
                    {
                        month = ((int)Enum.Parse(typeof(Months), tokens[i + 1].ToLower())).ToString();
                        if (month.Length < 2)
                            month = "0" + month;
                        tokens[i + 1] = month;
                    }
                    return tokens[i] + "/" + tokens[i + 1] + "/" + year;

                }


            }
            return "not a date!!";
        }
        /// <summary>
        ///  convert the given term into the format of date with month and year needs: 00/0000
        /// </summary>
        /// <param name="term"></param>
        /// <returns>the converted term</returns>
        private string MonthYear(string term)
        {
            string[] tokens = term.Split(' ');
            string month = ((int)Enum.Parse(typeof(Months), tokens[0].ToLower())).ToString();
            if (month.Length < 2)
                month = "0" + month;


            return month + "/" + tokens[1];
        }
        /// <summary>
        /// convert the given term into the format of date with day and month needs: 00/00
        /// </summary>
        /// <param name="term"></param>
        /// <returns>the converted term</returns>
        private string dayMonth(string term)
        {

            string[] tokens = term.Split(' ');
            string month;

            int number, number1; ;
            bool result = Int32.TryParse(tokens[1], out number);
            bool res2 = Int32.TryParse(tokens[0], out number1);
            if (result)
            {
                if (number < 10)
                    tokens[1] = "0" + tokens[1];

                month = ((int)Enum.Parse(typeof(Months), tokens[0].ToLower())).ToString();
                if (month.Length < 2)
                    month = "0" + month;
                return tokens[1] + "/" + month;
            }

            month = ((int)Enum.Parse(typeof(Months), tokens[1].ToLower())).ToString();
            if (month.Length < 2)
                month = "0" + month;
            if (number1 < 10)
                tokens[0] = "0" + tokens[0];
            return tokens[0] + "/" + month;


        }
        #endregion
        /// <summary>
        /// check if word is "stop word", means belong to stop word list,  size less then 3 (excluding numbers) etc.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private bool IsStopWord(string term)
        {
            return ((term.Length < 3 && !term.All(c => char.IsDigit(c))) || !term.Any(c => char.IsLetterOrDigit(c)) ||
                (StopWord.Contains(term.ToLower()) && !(Enum.IsDefined(typeof(Months), term.ToLower()))) || term.Any(c => c == '<' || c == '>'));
        }
        /// <summary>
        /// convert the given terms into one of the numbers rules (including dollars,percent,distance)
        /// </summary>
        /// <param name="new_word">the term sespected of number</param>
        /// <param name="next_word">the next word may belong to the rule</param>
        /// <param name="increaseI">the value needed to change for keeping the order of iteration</param>
        /// <returns>the converted term</returns>
        private string DealWithNumber(string new_word, string next_word, out int increaseI)
        {
            increaseI = 0;
            float checkFloatNumber;

            if (!string.IsNullOrEmpty(next_word) && ((next_word).ToLower().Equals("percentage") || (next_word).ToLower().Equals("percent") || next_word.Equals("%")))
            {
                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = Percentage(new_word);
                }
                increaseI++;
            }
            else if (new_word[new_word.Length - 1] == '%')
            {
                new_word = new_word.Split('%')[0];

                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = Percentage(new_word);
                }
                else
                {
                    increaseI = -1;
                }
            }
            // check dollar
            else if ((new_word.Contains("US$") || new_word.Contains("$")))
            {
                string dollarsWithoutDollarsSymbols = new_word.Split(new string[] { "$$", "$", "US$" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string dollarWithoutEndingSymbols = dollarsWithoutDollarsSymbols.Split(new string[] { "bn", "m", "b" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string numberOfDollars = RemoveSpecialCahracter(dollarWithoutEndingSymbols);
                if (float.TryParse(numberOfDollars, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    numberOfDollars = NumberConvert(numberOfDollars);
                    if (!string.IsNullOrEmpty(next_word) && ((next_word.ToLower().StartsWith("billion") || next_word.ToLower().StartsWith("million") || next_word.ToLower().StartsWith("thousand"))))
                    {
                        new_word = numberOfDollars + " " + next_word.ToLower() + " dollars";
                        increaseI++;
                    }
                    else if (dollarsWithoutDollarsSymbols.Length > 1 && (dollarsWithoutDollarsSymbols[dollarsWithoutDollarsSymbols.Length - 1] == 'b') || (dollarsWithoutDollarsSymbols.Length > 2 && (dollarsWithoutDollarsSymbols.Substring(dollarsWithoutDollarsSymbols.Length - 2) == "bn")))
                        new_word = numberOfDollars + " billion dollars";
                    else if (dollarsWithoutDollarsSymbols.Length > 1 && new_word[new_word.Length - 1] == 'm')
                        new_word = numberOfDollars + " million dollars";
                    else
                        new_word = numberOfDollars + " dollars";
                }
                else
                {
                    increaseI = -1;
                }
            }
            //kilometer check
            else if (((string.IsNullOrEmpty(next_word)) && (next_word.ToLower().Equals("kilometer") || next_word.ToLower().Equals("kilometers") || next_word.ToLower().Equals("km"))) ||
                (new_word.Length > 2 && new_word.Substring(new_word.Length - 2) == "km" && char.IsDigit(new_word[new_word.Length - 3])) || (new_word.Length > 3 && new_word.Substring(new_word.Length - 3) == "kms" && char.IsDigit(new_word[new_word.Length - 4])))
            {
                new_word = new_word.Split(new string[] { "km", "KM" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = NumberConvert(new_word) + " Kilometers";
                    if (!string.IsNullOrEmpty(next_word))
                    {
                        if (next_word.ToLower().Equals("kilometer") || next_word.ToLower().Equals("kilometers") || next_word.Equals("km"))
                        {
                            increaseI++;
                        }
                    }
                }
            }
            else if (((string.IsNullOrEmpty(next_word)) && (next_word.ToLower().Equals("centimeter") || next_word.ToLower().Equals("centimeters") || next_word.ToLower().Equals("cm"))) ||
                (new_word.Length > 2 && new_word.Substring(new_word.Length - 2) == "cm" && char.IsDigit(new_word[new_word.Length - 3])) || (new_word.Length > 3 && new_word.Substring(new_word.Length - 3) == "cms" && char.IsDigit(new_word[new_word.Length - 4])))
            {
                new_word = new_word.Split(new string[] { "cm", "CM", "cms", "CMS" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = NumberConvert(new_word) + " Centimeters";
                    if (!string.IsNullOrEmpty(next_word))
                    {
                        if (next_word.ToLower().Equals("centimeter") || next_word.ToLower().Equals("centimeters") || next_word.ToLower().Equals("cm"))
                        {
                            increaseI++;
                        }
                    }
                }
            }
            else if (((string.IsNullOrEmpty(next_word)) && (next_word.ToLower().Equals("millimeter") || next_word.ToLower().Equals("millimeters") || next_word.ToLower().Equals("km"))) ||
                (new_word.Length > 2 && new_word.Substring(new_word.Length - 2) == "mm" && char.IsDigit(new_word[new_word.Length - 3])) || (new_word.Length > 3 && new_word.Substring(new_word.Length - 3) == "mms" && char.IsDigit(new_word[new_word.Length - 4])))
            {
                new_word = new_word.Split(new string[] { "mm", "MM", "mms", "MMS" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = NumberConvert(new_word) + " Millimeters";
                    if (!string.IsNullOrEmpty(next_word))
                    {
                        if (next_word.ToLower().Equals("millimeter") || next_word.ToLower().Equals("millimeters") || next_word.ToLower().Equals("mm"))
                        {
                            increaseI++;
                        }
                    }
                }
            }
            else if (((string.IsNullOrEmpty(next_word)) && (next_word.ToLower().Equals("m") || next_word.ToLower().Equals("meter") || next_word.ToLower().Equals("meters") || next_word.ToLower().Equals("ms"))) ||
                (new_word.Length > 1 && new_word[new_word.Length - 1] == 'm' && char.IsDigit(new_word[new_word.Length - 2])) || (new_word.Length > 2 && new_word.Substring(new_word.Length - 2) == "ms" && char.IsDigit(new_word[new_word.Length - 3])))
            {
                new_word = new_word.Split(new string[] { "m", "M", "MS", "ms" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
                {
                    new_word = NumberConvert(new_word) + " Meters";
                    if (!string.IsNullOrEmpty(next_word))
                    {
                        if (next_word.ToLower().Equals("meters") || next_word.ToLower().Equals("meter") || next_word.ToLower().Equals("m"))
                        {
                            increaseI++;
                        }
                    }
                }
            }
            // check number
            else if (float.TryParse(new_word, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out checkFloatNumber))
            {
                new_word = NumberConvert(new_word);
            }
            else
            {
                increaseI = -1;
            }

            return new_word.ToLower();
        }
    }
}
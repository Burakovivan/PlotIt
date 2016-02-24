using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace PlottingGraphsSystem
{
    public static class Аnalyzer
    {
        public static Func<double, double> GetFunction(string source)
        {
            RemoveClearSpaces(ref source);
            return (Func<double, double>)Expression.Lambda(Analyze2(source), VariableExpression).Compile();
        }

        public static Expression Analyze2(string source)
        {
            Expression Result = Expression.Default(typeof(double));

            #region Number
            if (isConstatnExpression(source))
            {
                return Expression.Constant(double.Parse(source));
            }
            #endregion
            else
            #region Parametr
            if (isParametrExpression(source))
            {
                if (source[0] == '-')
                    Result = Expression.Multiply(Expression.Constant(-1d), VariableExpression);
                else
                    Result = VariableExpression;
            }
            #endregion
            else
            #region Brackets
            if (isContainsBrackets(source))
            {
                if (source[0] == '-' && (OpeningBrackets).Contains(source[1]) && ClosingBrackets.Contains(source.Last()) & isSameBrackets(source, 2, source.Length - 1))
                {
                    return Expression.Multiply(Analyze2(source.Substring(2, source.Length - 3)), Expression.Constant(-1d));
                }
                //Если скобка в начале
                if ((OpeningBrackets).Contains(source.First()))
                {
                    //Если скобка которая открылась в начале закрывается в конце
                    if (ClosingBrackets.Contains(source.Last()) & isSameBrackets(source, 1, source.Length - 1))
                    {
                        Result = Analyze2(source.Substring(1, source.Length - 2));
                    }
                    else
                    {
                        int firstFree = GetFreeOperationIndex(source);

                        Expression left = Analyze2(source.Substring(0, firstFree));
                        Expression right = Analyze2(source.Substring(firstFree + 1));

                        Result = GetExpressionFromOperator(left, right, GetOperator(source[firstFree]));
                    }
                }
                else
                {
                    int firstFree = GetFreeOperationIndex(source);

                    Expression left = Analyze2(source.Substring(0, firstFree));
                    Expression right = Analyze2(source.Substring(firstFree + 1));

                    Result = GetExpressionFromOperator(left, right, GetOperator(source[firstFree]));
                }
            }
            else
            {
                int firstFree = GetFreeOperationIndex(source);

                Expression left = Analyze2(source.Substring(0, firstFree));
                Expression right = Analyze2(source.Substring(firstFree + 1));

                Result = GetExpressionFromOperator(left, right, GetOperator(source[firstFree]));
            }
            #endregion


            return Result;
        }

        /// <summary>
        /// Возвращает индекс символа операции вне скобок по которому следует производить деление
        /// </summary>
        /// <param name="source">исходная строка</param>
        /// <returns></returns>
        public static int GetFreeOperationIndex(string source)
        {
            string temp = string.Empty;
            int brackets = 0;
            bool dimas = false;

            for (int i = 0; i < source.Length; ++i)
            {
                if (OpeningBrackets.Contains(source[i]))
                {
                    brackets++;

                }
                else
                if (ClosingBrackets.Contains(source[i]))
                {
                    if (brackets > 0)
                    {
                        brackets--;
                        dimas = true;
                    }
                    else return -1;
                }

                if (brackets == 0 && !dimas) temp += source[i]; else { temp += "#"; dimas = false; }
            }

            foreach (char c in CorrectOrderOfOperation)
            {
                if (temp.Contains(c))
                {
                    if (temp.Count(g => g == c) != 1 || temp.IndexOf(c) != 0)
                        return temp.IndexOf(c);
                }
            }
            return -1;

        }

        public static Expression Analyze(string source)
        {

            source = source.Trim();


            Expression AnalysResult = Expression.Default(typeof(double));
            //Если выражение содержит только параметр (аргумент функции)
            if (isParametrExpression(source))
            {
                if (source[0] == '-')
                    AnalysResult = Expression.Multiply(Expression.Constant(-1d), VariableExpression);
                else
                    AnalysResult = VariableExpression;
            }
            else
            //Если выражение содержит только число или число в какой то степени
            if (isConstatnExpression(source))
            {

                if (source.Contains('^'))
                {
                    if (isParametrExpression(source.Split('^')[0]))
                        AnalysResult = Expression.Power(VariableExpression, Expression.Constant(double.Parse(source.Split('^')[1])));
                    else
                        AnalysResult = Expression.Power(Expression.Constant(double.Parse(source.Split('^')[0])), Expression.Constant(double.Parse(source.Split('^')[1])));
                }

                else
                    AnalysResult = Expression.Constant(double.Parse(source));

            }
            else

            //Если содержит СКОБКУ
            if (isContainsBrackets(source))
            {
                //Если последовательность скобок и их количество валидны
                if (isValidOpeningClosingBrackets(source))
                {
                    //Если скобка в самом начале
                    if ((OpeningBrackets).Contains(source.First()))
                    {
                        //Если скобка которая открылась в самом начале закрывается в самом конце
                        if (ClosingBrackets.Contains(source.Last()) & isSameBrackets(source, 1, source.Length - 1))
                        {
                            AnalysResult = Analyze(source.Substring(1, source.Length - 2));
                        }
                        else
                        {
                            //находим место закрытия скобки
                            int indxEnd = GetIndexOfClosingBracket(source, 0);
                            Expression left = Analyze(source.Substring(1, indxEnd - 1));
                            Expression right = Analyze(source.Substring(indxEnd + 2));

                            AnalysResult = GetExpressionFromOperator(left, right, GetOperator(source[indxEnd + 1]));
                        }
                    }
                    //Если скобка не в начале, а где то там дальше
                    else
                    {
                        //исправить!!!!
                        int partitionIndex = GetCorrectIndexOfPartition(source.Remove(source.IndexOfAny(OpeningBrackets.ToCharArray())), source.IndexOfAny(OpeningBrackets.ToCharArray()));

                        Expression left = Analyze(source.Remove(partitionIndex));
                        Expression right = Analyze(source.Substring(partitionIndex + 1));
                        AnalysResult = GetExpressionFromOperator(left, right, GetOperator(source[partitionIndex]));
                    }

                }
                else throw new InvalidBraketsExeption();
            }
            else
            //Если НЕ содержит СКОБКУ
            {
                int partitionIndex = GetCorrectIndexOfPartition(source);

                Expression left = Analyze(source.Remove(partitionIndex));
                Expression right = Analyze(source.Substring(partitionIndex + 1));
                AnalysResult = GetExpressionFromOperator(left, right, GetOperator(source[partitionIndex]));
            }

            return AnalysResult;
        }

        private static bool isParametrExpression(string source)
        {
            return source == "x" || source == "-x";
        }

        /// <summary>
        /// Являются ли скобки парными
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <param name="opening">открывающая скобка</param>
        /// <param name="closing">закрывающая скобка</param>
        /// <returns></returns>
        private static bool isSameBrackets(string source, int opening, int closing)
        {
            if (opening >= 0 && opening < source.Length && closing > 0 && closing < source.Length && closing != opening)
                return isValidOpeningClosingBrackets(source.Substring(opening, closing - opening));
            else
                throw new ArgumentException("Opening or closing brackets index was wrong");
        }
        /// <summary>
        /// Проверяет правильный ли порядок открывающизся и закрывающихся скобок в исходной строке
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <returns></returns>
        private static bool isValidOpeningClosingBrackets(string source)
        {
            int brackets = 0;

            foreach (char c in source)
            {
                if (OpeningBrackets.Contains(c))
                {
                    brackets++;
                }
                else
                if (ClosingBrackets.Contains(c))
                {
                    if (brackets > 0)
                        brackets--;
                    else return false;
                }
            }
            if (brackets == 0)
                return true;
            else return false;
        }

        /// <summary>
        /// Содержит ли скобки
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <returns></returns>
        private static bool isContainsBrackets(string source)
        {
            return source.IndexOfAny((OpeningBrackets + ClosingBrackets).ToCharArray()) != -1;
        }

        /// <summary>
        /// Проверяет является ли исходная строка константным выражением (числом или числом в степени)
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <returns></returns>
        private static bool isConstatnExpression(string source)
        {
            double t;
            return double.TryParse(source, out t);
            //    if(source[0] == '-')
            //    {
            //        source = source.Substring(1);
            //    }

            //    foreach (char c in source)
            //    {
            //        if (!("0123456789,".Contains(c)))
            //            return false;
            //    }
            //    return true;
        }

        /// <summary>
        /// Получает место разделения выражения в котором отсутствуют скобки
        /// </summary>
        /// <param name="source">Исходная сткрока</param>
        /// <returns>Индекс разделения</returns>
        private static int GetCorrectIndexOfPartition(string source, int endIndex = -1)
        {
            if (endIndex == -1)
                endIndex = source.Length - 1;

            foreach (char c in CorrectOrderOfOperation)
            {
                if (source.Contains(c))
                {
                    return source.IndexOf(c);
                }
            }
            return -1;
        }

        /// <summary>
        /// Создает выражение из двух выражений и действия над ними
        /// </summary>
        /// <param name="left">левое выражение</param>
        /// <param name="right">правой выражение</param>
        /// <param name="operation">выполняемое действие</param>
        /// <returns></returns>
        private static Expression GetExpressionFromOperator(Expression left, Expression right, Operator operation)
        {
            switch (operation)
            {
                case Operator.Add: return Expression.Add(left, right);
                case Operator.Subtract: return Expression.Subtract(left, right);
                case Operator.Multiply: return Expression.Multiply(left, right);
                case Operator.Divide: return Expression.Divide(left, right);
                case Operator.Power: return Expression.Power(left, right);
                default: return Expression.Constant(-1);
            }
        }

        /// <summary>
        /// Получает позицию в которой скобка закрывается
        /// </summary>
        /// <param name="source">Исходная сткрока</param>
        /// <param name="openingBracketIndex">Позиция в которой скобка открылась</param>
        /// <returns></returns>
        public static int GetIndexOfClosingBracket(string source, int openingBracketIndex)
        {
            if (openingBracketIndex < source.Length)
            {
                int brackets = 1;
                for (int i = openingBracketIndex + 1; i < source.Length; i++)
                {
                    if (OpeningBrackets.Contains(source[i]))
                        brackets++;
                    else
                    if (ClosingBrackets.Contains(source[i]))
                    {
                        brackets--;
                        if (brackets == 0)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Получает позицию в которой число заканчивается
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <param name="strartIndex">Позиция в которой число начинается</param>
        /// <returns></returns>
        private static int GetIndexOfNumberEnd(string source, int strartIndex)
        {
            if (strartIndex < source.Length)
                for (int i = strartIndex; i < source.Length; i++)
                {
                    if (!(char.IsNumber(source[i + 1]) || ".,^".Contains(source[i + 1])))
                        return i;
                }
            return -1;
        }

        private static string RemoveClearSpaces(ref string source)
        {
            char[] arr = source.ToCharArray();
            string temp = "";

            foreach (char c in arr)
            {
                if (!ClearSpaces.Contains(c))
                    temp += c;
            }
            source = temp;
            return source;

        }


        /// <summary>
        /// Получает операцию из символа
        /// </summary>
        /// <param name="op">символ операции (действия)</param>
        /// <returns></returns>
        private static Operator GetOperator(char op)
        {
            switch (op)
            {
                case '+': return Operator.Add;
                case '-': return Operator.Subtract;
                case '*': return Operator.Multiply;
                case '/': return Operator.Divide;
                case '^': return Operator.Power;
                default: return Operator.Undefined;
            }
        }

        /// <summary>
        /// Арифметические операции
        /// </summary>
        private enum Operator { Add, Subtract, Multiply, Divide, Power, Undefined }

        public static ParameterExpression VariableExpression = Expression.Parameter(typeof(double), "x");
        /// <summary>
        /// Откраывющиеся скобки
        /// </summary>
        public static string OpeningBrackets { get { return "("; ; } }

        /// <summary>
        /// Закрывающие скобки
        /// </summary>
        public static string ClosingBrackets { get { return ")"; } }

        /// <summary>
        /// Последовательность арифметических действий при анализе
        /// </summary>
        public static string CorrectOrderOfOperation { get { return "+-*/^"; } }

        public static string ClearSpaces { get { return " "; } }

    }
}

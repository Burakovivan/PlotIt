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
            return (Func<double, double>)Expression.Lambda(Analyze(source), VariableExpression).Compile();
        }

        public static Expression Analyze(string source)
        {
            //-sin(-x)
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
                    Result = Expression.Negate(VariableExpression);
                else
                    Result = VariableExpression;
            }
            #endregion
            else
            if (isMathFunction(source))
            {
                if (source[0] == '-')
                    Result = Expression.Negate(Analyze(source.Substring(1)));
                else
                    Result = GetUnaryExpression(source);
            }
            else
            #region Brackets
            if (isContainsBrackets(source))
            {
                if (source[0] == '-' && (OpeningBrackets).Contains(source[1]) && ClosingBrackets.Contains(source.Last()) & isSameBrackets(source, 2, source.Length - 1))
                {
                    return Expression.Multiply(Analyze(source.Substring(2, source.Length - 3)), Expression.Constant(-1d));
                }
                //Если скобка в начале
                if ((OpeningBrackets).Contains(source.First()))
                {
                    //Если скобка которая открылась в начале закрывается в конце
                    if (ClosingBrackets.Contains(source.Last()) & isSameBrackets(source, 1, source.Length - 1))
                    {
                        Result = Analyze(source.Substring(1, source.Length - 2));
                    }
                    else
                    {
                        Result = GetBinaryExpression(source);
                    }
                }
                else
                {
                    Result = GetBinaryExpression(source);
                }
            }
            else
            {
                Result = GetBinaryExpression(source);
            }
            #endregion


            return Result;
        }

        private static Expression GetUnaryExpression(string source)
        {
            foreach (string func in MathFunc)
            {
                if (source.Contains(func))
                {
                    if (source.Substring(0, func.Length) == func)
                    {
                        Expression exp = Analyze(source.Substring(func.Length, source.Length - func.Length));
                        switch (GetOperator(func))
                        {

                            case Operator.Abs: return Expression.Call(null, typeof(Math).GetMethods().First(x => x.Name == "Abs" && x.ReturnParameter.ParameterType == typeof(double)), exp);
                            case Operator.Cos: return Expression.Call(null, typeof(Math).GetMethod("Cos"), exp);
                            case Operator.Sin: return Expression.Call(null, typeof(Math).GetMethod("Sin"), exp);
                            case Operator.Tg: return Expression.Call(null, typeof(Math).GetMethod("Tan"), exp);
                            case Operator.Ctg: return Expression.Divide(Expression.Constant(1d), Expression.Call(null, typeof(Math).GetMethod("Tan"), exp));
                            case Operator.Lg: return Expression.Call(null, typeof(Math).GetMethod("Log10"), exp);
                            case Operator.Ln: return Expression.Call(null, typeof(Math).GetMethods().First(x => x.Name == "Log" && x.ReturnParameter.ParameterType == typeof(double)), exp);
                            case Operator.Sqrt: return Expression.Call(null, typeof(Math).GetMethod("Sqrt"), exp);
                        }
                    }
                }
            }
            return Expression.Default(typeof(double));
        }

        private static bool isMathFunction(string source)
        {

            foreach (string func in MathFunc)
            {
                if (source.Contains(func))
                {
                    if (source[0] == '-') source = source.Substring(1);

                    if (source.Substring(0, func.Length) == func)
                    {
                        if (isValidOpeningClosingBrackets(source.Substring(func.Length, source.Length - func.Length)) && GetIndexOfClosingBracket(source, func.Length) == source.Length - 1)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Возвращает индекс символа операции вне скобок по которому следует производить деление
        /// </summary>
        /// <param name="source">исходная строка</param>
        /// <returns></returns>
        public static int[] GetFreeOperationIndex(string source)
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
                    else
                        throw new ArgumentException();
                }

                if (brackets == 0 && !dimas) temp += source[i]; else { temp += "#"; dimas = false; }
            }

            foreach (string c in CorrectOrderOfOperation)
            {
                if (temp.Contains(c))
                {
                    int[] oper = new[] { temp.LastIndexOf(c), c.Length };
                    return oper;
                }
            }
            throw new ArgumentException();

        }



        private static bool isParametrExpression(string source)
        {
            source = source.ToLower();
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
        /// Создает выражение из двух выражений и действия над ними
        /// </summary>
        /// <param name="left">левое выражение</param>
        /// <param name="right">правой выражение</param>
        /// <param name="operation">выполняемое действие</param>
        /// <returns></returns>
        private static Expression GetBinaryExpression(string source)
        {
            int[] firstFree = GetFreeOperationIndex(source);

            Expression left = Analyze(source.Substring(0, firstFree[0]));
            Expression right = Analyze(source.Substring(firstFree[0] + 1));

            switch (GetOperator(source.Substring(firstFree[0], firstFree[1])))
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

        /// <summary>
        /// Убирает пробелы
        /// </summary>
        /// <param name="source">исходная строка</param>
        /// <returns></returns>
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
        private static Operator GetOperator(string op)
        {
            switch (op.ToLower())
            {
                case "+": return Operator.Add;
                case "-": return Operator.Subtract;
                case "*": return Operator.Multiply;
                case "/": return Operator.Divide;
                case "^": return Operator.Power;
                case "sin": return Operator.Sin;
                case "cos": return Operator.Cos;
                case "abs": return Operator.Abs;
                case "sqrt": return Operator.Sqrt;
                case "tg": return Operator.Tg;
                case "tan": return Operator.Tg;
                case "ctg": return Operator.Ctg;
                case "cot": return Operator.Ctg;
                case "ln": return Operator.Ln;
                case "lg": return Operator.Lg;
                default: return Operator.Undefined;
            }
        }

        /// <summary>
        /// Арифметические операции
        /// </summary>
        private enum Operator { Add, Subtract, Multiply, Divide, Power, Sin, Cos, Abs, Sqrt, Tg, Ctg, Ln, Lg, Undefined }


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
        public static string[] CorrectOrderOfOperation { get { return new[] { "+", "/", "*", "^", "lg", "ln", "ctg", "cot", "tan", "tg", "cos", "sin", "sqrt", "abs" }; } }

        public static string[] MathFunc { get { return new[] { "lg", "ln", "ctg", "cot", "tan", "tg", "cos", "sin", "sqrt", "abs" }; } }


        public static string ClearSpaces { get { return " "; } }

    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SimpleDrawing
{
    public static class Simulation
    {
        public static List<Dictionary<string, List<double>>> SolveEquation(string equation, double step, double time, double startPoint)
        {
            var equationLines = new List<string>();
            equationLines = BrakeStringInLines(equation);
            var variables = CreateVariablesList(equationLines);
            var derivatives = CreateDerivativesList(equationLines);

            var variablesHistory = new Dictionary<string, List<double>>();
            foreach (var derivativeName in variables.Keys)
            {
                variablesHistory.Add(derivativeName, new List<double>());
                variablesHistory[derivativeName].Add(variables[derivativeName]);
            }

            var xValues = new List<double>();
            for (var t = startPoint; t <= time; t += step)
            {
                xValues.Add(t);
                foreach (var derivative in derivatives)
                {
                    var expression = derivative.Value;
                    foreach (var variable in variables)
                        expression = expression.Replace(variable.Key, variable.Value.ToString());

                    var at = SolveExpression(expression);
                    var nextValue = variables[derivative.Key] + step * at;
                    variablesHistory[derivative.Key].Add(nextValue);
                }

                foreach (var variable in variablesHistory)
                    variables[variable.Key] = variable.Value[variable.Value.Count - 1];
            }

            var results = new List<Dictionary<string, List<double>>>();

            var dict = new Dictionary<string, List<double>>();
            foreach (var pair in variablesHistory)
            {
                if (pair.Value.Count > 1)
                {
                    dict = new Dictionary<string, List<double>>();
                    dict.Add(pair.Key, pair.Value.GetRange(0, pair.Value.Count - 1));
                    results.Add(dict);
                }
            }

            return results;
        }

        private static Dictionary<string, double> CreateVariablesList(List<string> equationLines)
        {
            var variables = new Dictionary<string, double>();

            foreach (var line in equationLines)
            {
                var currentLine = line.Replace(" ", "");
                if (currentLine.Contains("'="))
                    continue;

                var variableName = currentLine.Substring(0, currentLine.IndexOf("="));
                var variableExpression = currentLine.Substring(currentLine.IndexOf("=") + 1);

                foreach (var variable in variables)
                    variableExpression = variableExpression.Replace(variable.Key, variable.Value.ToString());

                variables.Add(variableName, SolveExpression(variableExpression));
            }

            return variables;
        }

        private static Dictionary<string, string> CreateDerivativesList(List<string> equationLines)
        {
            var derivatives = new Dictionary<string, string>();

            foreach (var line in equationLines)
            {
                var currentLine = line.Replace(" ", "");
                if (!currentLine.Contains("'="))
                    continue;

                var derivativeName = currentLine.Substring(0, currentLine.IndexOf("'="));
                var derivativeExpression = currentLine.Substring(currentLine.IndexOf("'=") + 2);

                derivatives.Add(derivativeName, derivativeExpression);
            }

            return derivatives;
        }

        private static double SolveExpression(string expression)
        {
            // Малко извращение с десетичната точка при различните локализации
            var uiSep = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0];
            expression = expression.Replace('.', uiSep);
            expression = expression.Replace(',', uiSep);
            if (expression.StartsWith("("))
            {
                int openingBrackets = 1, closingBrackets = 0, currentSymbol = 1;
                while (openingBrackets != closingBrackets)
                {
                    switch (expression[currentSymbol])
                    {
                        case '(':
                            openingBrackets++;
                            break;
                        case ')':
                            closingBrackets++;
                            break;
                    }

                    currentSymbol++;
                }
                var expr = expression.Substring(1, currentSymbol - 2);
                expression = expression.Remove(0, currentSymbol);

                var operation = Regex.Match(expression, @"^[\+\-\*\/]");
                if (operation.Success)
                {
                    expression = expression.Remove(0, operation.Value.Length);
                    switch (operation.Value)
                    {
                        case "+":
                            {
                                return SolveExpression(expr) + SolveExpression(expression);
                            }
                        case "-":
                            {
                                return SolveExpression(expr) - SolveExpression(expression);
                            }
                        case "*":
                            {
                                return SolveExpression(expr) * SolveExpression(expression);
                            }
                        case "/":
                            {
                                return SolveExpression(expr) / SolveExpression(expression);
                            }
                    }
                }
                else
                    return SolveExpression(expr);
            }

            var constant = Regex.Match(expression, @"(^-*\d+)((\.|\,)(\d+))?");
            if (constant.Success)
            {
                expression = expression.Remove(0, constant.Value.Length);

                var operation = Regex.Match(expression, @"^[\+\-\*\/]");
                if (operation.Success)
                {
                    expression = expression.Remove(0, operation.Value.Length);
                    switch (operation.Value)
                    {
                        case "+":
                            {
                                return double.Parse(constant.Value) + SolveExpression(expression);
                            }
                        case "-":
                            {
                                return double.Parse(constant.Value) - SolveExpression(expression);
                            }
                        case "*":
                            {
                                return double.Parse(constant.Value) * SolveExpression(expression);
                            }
                        case "/":
                            {
                                return double.Parse(constant.Value) / SolveExpression(expression);
                            }
                    }
                }
                else
                    return double.Parse(constant.Value);
            }
            else
                throw new Exception("Invalid Expression");

            return 0; // catch a system.exception here
        }

        private static List<string> BrakeStringInLines(string inputString)
        {
            var lines = new List<string>();
            var linesArray = inputString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            lines.AddRange(linesArray);
            return lines;
        }
    }
}

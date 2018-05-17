using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;

namespace Interpreter
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class Interpreter
    {
        string interpString = "";
        int pos = 0;
        Lexeme lastLexeme = Lexeme.LeftBr;
        Variable[] vars;
        Operator[] priors;

        TreeNode root;
        float curConst;
        Variable curVar;
        Operator curOp;
        ArrayList res;

        public Interpreter()
        {
            priors = new Operator[] { Operator.None, Operator.Subtr, Operator.Add, Operator.Div,
                Operator.Mul, Operator.Pow, Operator.Lg, Operator.Ln, Operator.Sin, Operator.Cos,
                Operator.Tan, Operator.Asin, Operator.Acos, Operator.Atan, Operator.UnSubtr};
            vars = new Variable[0];
        }
        public void Interpret(string str)
        {
            interpString = str;
            pos = 0;
            lastLexeme = Lexeme.LeftBr;
            Stack stack = new Stack();
            res = new ArrayList();
            int priorityBonus = 0;

            // перевод в обратную польскую запись
            while (pos < interpString.Length)
            {
                Lexeme lexeme = GetNextLexeme();
                if (lexeme == Lexeme.LeftBr)
                {
                    priorityBonus += 20;
                    lastLexeme = lexeme;
                    continue;
                }
                if (lexeme == Lexeme.RightBr)
                {
                    priorityBonus -= 20;
                    if (priorityBonus < 0)
                        throw new Exception("Неверная расстановка скобок");
                    lastLexeme = lexeme;
                    continue;
                }
                if (lexeme == Lexeme.Const)
                {
                    if (lastLexeme == Lexeme.Const || lastLexeme == Lexeme.Var ||
                        lastLexeme == Lexeme.RightBr)
                        throw new Exception("Отсутствует оператор");
                    TreeNode node = new TreeNode(curConst);
                    res.Add(node);
                    lastLexeme = lexeme;
                    continue;
                }
                if (lexeme == Lexeme.Var)
                {
                    if (lastLexeme == Lexeme.Const || lastLexeme == Lexeme.Var ||
                        lastLexeme == Lexeme.RightBr)
                        throw new Exception("Отсутствует оператор");
                    TreeNode node = new TreeNode(curVar);
                    res.Add(node);
                    lastLexeme = lexeme;
                    continue;
                }
                if (lexeme == Lexeme.Oper)
                {
                    if((lastLexeme == Lexeme.Oper || lastLexeme == Lexeme.LeftBr) &&
                        curOp != Operator.Sin && curOp != Operator.Cos &&
                        curOp != Operator.Tan && curOp != Operator.Asin &&
                        curOp != Operator.Acos && curOp != Operator.Atan &&
                        curOp != Operator.Ln && curOp != Operator.Lg &&
                        curOp != Operator.UnSubtr)
                        throw new Exception("Лишний оператор");
                    int pr = 0;
                    for (int i = 0; i < priors.Length; i++)
                        if (priors[i] == curOp) pr = i;
                    TreeNode node = new TreeNode(curOp, pr + priorityBonus);
                    if (stack.Count > 0)
                    {
                        TreeNode stackNode = stack.Peek() as TreeNode;
                        while (stack.Count > 0 && stackNode.priority > node.priority)
                        {
                            stackNode = stack.Pop() as TreeNode;
                            res.Add(stackNode);
                        }
                    }
                    stack.Push(node);
                    lastLexeme = lexeme;
                    continue;
                }
            }
            if (priorityBonus != 0)
                throw new Exception("Неверная расстановка скобок");
            while (stack.Count > 0)
                res.Add(stack.Pop());
            res.Reverse();

            // построение дерева
            root = res[0] as TreeNode;
            foreach (TreeNode node in res)
            {
                if (stack.Count != 0)
                {
                    TreeNode cur = stack.Peek() as TreeNode;
                    if (cur.opType == Operator.Sin || cur.opType == Operator.Cos ||
                        cur.opType == Operator.Tan || cur.opType == Operator.Asin ||
                        cur.opType == Operator.Acos || cur.opType == Operator.Atan ||
                        cur.opType == Operator.Lg || cur.opType == Operator.Ln ||
                        cur.opType == Operator.UnSubtr)
                    {
                        cur.left = node;
                        stack.Pop();
                    }
                    else
                        if (cur.left == null)
                            cur.left = node;
                        else if (cur.right == null)
                        {
                            cur.right = node;
                            stack.Pop();
                        }
                }
                if (node.type == Lexeme.Oper)
                    stack.Push(node);
            }
        }
        private float Calculate(TreeNode node)
        {
            if (node.type == Lexeme.Const) return node.value;
            if (node.type == Lexeme.Var) return node.variable.value;
            if (node.type == Lexeme.Oper)
            {
                float x = 0, y = 0;
                if (node.left != null)
                    x = Calculate(node.left);
                if (node.right != null)
                    y = Calculate(node.right);
                switch (node.opType)
                {
                    case Operator.Sin:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Sin(x);
                    case Operator.Cos:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Cos(x);
                    case Operator.Tan:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Tan(x);
                    case Operator.Asin:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Asin(x);
                    case Operator.Acos:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Acos(x);
                    case Operator.Atan:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Atan(x);
                    case Operator.Lg:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Log10(x);
                    case Operator.Ln:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Log(x);
                    case Operator.UnSubtr:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        return -x;
                    case Operator.Pow:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        if (node.right == null)
                            throw new Exception("Отсутствует операнд");
                        return (float)Math.Pow(x, y);
                    case Operator.Mul:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        if (node.right == null)
                            throw new Exception("Отсутствует операнд");
                        return x * y;
                    case Operator.Div:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        if (node.right == null)
                            throw new Exception("Отсутствует операнд");
                        return x / y;
                    case Operator.Add:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        if (node.right == null)
                            throw new Exception("Отсутствует операнд");
                        return x + y;
                    case Operator.Subtr:
                        if (node.left == null)
                            throw new Exception("Отсутствует операнд");
                        if (node.right == null)
                            throw new Exception("Отсутствует операнд");
                        return y - x;
                }
            }
            throw new Exception("Ошибка интерпретации");
        }
        public float Result()
        {
            return Calculate(root);
        }
        Lexeme GetNextLexeme()
        {
            Lexeme res = Lexeme.None;
            string[] generalLexemes = new string[] { "sin", "cos", "tan", "asin", "acos", "atan",
                "lg", "ln", "+", "-", "*", "/", "^", "(", ")" };
            while (pos < interpString.Length && char.IsSeparator(interpString[pos])) pos++;
            int oldPos = pos;
            foreach (string l in generalLexemes)
                if (interpString.Length - pos >= l.Length)
                    if (interpString.Substring(pos, l.Length) == l)
                    {
                        res = Lexeme.Oper;
                        if (l == "sin") curOp = Operator.Sin;
                        if (l == "cos") curOp = Operator.Cos;
                        if (l == "tan") curOp = Operator.Tan;
                        if (l == "asin") curOp = Operator.Asin;
                        if (l == "acos") curOp = Operator.UnSubtr;
                        if (l == "atan") curOp = Operator.Atan;
                        if (l == "lg") curOp = Operator.Lg;
                        if (l == "ln") curOp = Operator.Ln;
                        if (l == "+") curOp = Operator.Add;
                        if (l == "-")
                            if (lastLexeme == Lexeme.RightBr ||
                                lastLexeme == Lexeme.Var ||
                                lastLexeme == Lexeme.Const)
                                curOp = Operator.Subtr;
                            else curOp = Operator.UnSubtr;
                        if (l == "*") curOp = Operator.Mul;
                        if (l == "/") curOp = Operator.Div;
                        if (l == "^") curOp = Operator.Pow;
                        if (l == "(") res = Lexeme.LeftBr;
                        if (l == ")") res = Lexeme.RightBr;
                        pos += l.Length;
                        return res;
                    }

            foreach(Variable v in vars)
                if (interpString.Length - pos >= v.name.Length)
                    if (interpString.Substring(pos, v.name.Length) == v.name)
                    {
                        curVar = v;
                        pos += v.name.Length;
                        return Lexeme.Var;
                    }

            if (pos < interpString.Length && char.IsDigit(interpString[pos]))
            {
                while (pos < interpString.Length && char.IsDigit(interpString[pos])) pos++;
                if (pos < interpString.Length && interpString[pos] == '.') pos++;
                while (pos < interpString.Length && char.IsDigit(interpString[pos])) pos++;
                string s = interpString.Substring(oldPos, pos - oldPos);
                CultureInfo MyCultureInfo = new CultureInfo("en-US");
                MyCultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                curConst = float.Parse(s, NumberStyles.AllowDecimalPoint, MyCultureInfo);
                return Lexeme.Const;
            }
            throw new Exception("Неопознанный символ");
        }
        public void AddVariable(string name)
        {
            Variable[] newVars = new Variable[vars.Length + 1];
            vars.CopyTo(newVars, 0);
            newVars[vars.Length] = new Variable(0.0f, name);
            vars = newVars;
        }
        public float GetVarValue(string name)
        {
            foreach (Variable v in vars)
                if (v.name == name) return v.value;
            throw new Exception();
        }
        public void SetVarValue(string name, float x)
        {
            foreach (Variable v in vars)
                if (v.name == name)
                {
                    v.value = x;
                    return;
                }
        }
    }
    public class Variable
    {
        public float value;
        public string name;
        public Variable(float v, string n)
        {
            value = v;
            name = n;
        }
    }
    public class TreeNode
    {
        public Lexeme type = Lexeme.None;
        public TreeNode left = null, right = null;
        
        public float value = 0.0f;
        public Variable variable = null;
        public Operator opType = Operator.None;
        public int priority = 0;        

        public TreeNode(float c)
        {
            type = Lexeme.Const;
            value = c;
        }
        public TreeNode(Variable v)
        {
            type = Lexeme.Var;
            variable = v;
        }
        public TreeNode(Operator op, int p)
        {
            type = Lexeme.Oper;
            opType = op;
            priority = p;
        }
    }
    public enum Operator
    {
        UnSubtr, Sin, Cos, Tan, Asin, Acos, Atan,
        Lg, Ln, Pow, Mul, Div, Add, Subtr, None
    }
    public enum Lexeme
    {
        Oper, LeftBr, RightBr, Const, Var, None
    }
}
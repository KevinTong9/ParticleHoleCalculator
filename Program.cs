#define pub
// #define debug
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Security;

namespace ParticleHoleCalculator
{
    class Program
    {
        /// <summary>
        /// 递归深度
        /// </summary>
        private static readonly int MAX_DEPTH = 100;

        /// <summary>
        /// 存放可接受参数
        /// </summary>
        private static class _parameter
        {
            /// <summary>
            /// -h/--Help       帮助
            /// </summary>
            public const string helpArg1 = "-h", helpArg2 = "--Help";
            /// <summary>
            /// -d/--DEBUG      调试
            /// </summary>
            public const string debugArg1 = "-D", debugArg2 = "--Debug";
            /// <summary>
            /// -i/--in         输入文件*
            /// </summary>
            public const string fileInputArg1 = "-i", fileInputArg2 = "--in";
            /// <summary>
            /// o/--out         输出文件
            /// </summary>
            public const string fileOutArg1 = "-o", fileOutArg2 = "--out";
            /// <summary>
            /// -b/--box        箱体模糊长(a,b,c)
            /// </summary>
            public const string boxArg1 = "-b", boxArg2 = "--box";
            /// <summary>
            /// -d/--density    箱体密度
            /// </summary>
            public const string densityArg1 = "-d", densityArg2 = "--density";
            /// <summary>
            /// -t/--threshold  箱体阈值
            /// </summary>
            public const string thresholdArg1 = "-t", thresholdArg2 = "--threshold";
        }

        private class parameter
        {
            private static bool needDebug;
            private static StreamReader inputFile;
            private static StreamWriter outputFile;// = new ($@"{Environment.CurrentDirectory}\out.txt");
            private static ulong[] boxSideLength = new ulong[3] { 8, 8, 8 };
            private static ulong density = 3;
            private static ulong threshold = 1;
            private const string helpInfo = "this is help message?\n";
            /// <summary>
            /// 初步判断参数是否符合输入结构
            /// </summary>
            /// <param name="args">输入参数字符组</param>
            /// <returns></returns>
            private static bool isArgsLegal(string[] args)
            {
                if (args.Length != 0)
                {
                    for (var i = 0; i < args.Length; i++)
                        switch (args[i])
                        {
                            case _parameter.helpArg1:
                            case _parameter.helpArg2:
                                return false;
                            case _parameter.debugArg1:
                            case _parameter.debugArg2:
                                needDebug = true;
                                break;
                            case _parameter.fileInputArg1:
                            case _parameter.fileInputArg2:
                            case _parameter.fileOutArg1:
                            case _parameter.fileOutArg2:
                            case _parameter.boxArg1:
                            case _parameter.boxArg2:
                            case _parameter.densityArg1:
                            case _parameter.densityArg2:
                            case _parameter.thresholdArg1:
                            case _parameter.thresholdArg2:
                                i++;
                                if (i >= args.Length)
                                    return false;
                                break;
                            default:
                                return false;
                        }
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 判断储存后的参数是否满足必要需求
            /// </summary>
            /// <returns></returns>
            private bool isArgsFull()
            {
                if (inputFile == null || outputFile == null)
                    return false;
                return true;
            }
            /// <summary>
            /// 解析参数并存储
            /// </summary>
            /// <param name="args"></param>
            public parameter(string[] args)
            {
                foreach (var item in args)
                    logger(_loggerT.None, item + " ");

                if (isArgsLegal(args))
                {
                    for (var i = 0; i < args.Length; i++)
                        switch (args[i])
                        {
                            case _parameter.debugArg1:
                            case _parameter.debugArg2:
                                #region DEBUG配置
                                needDebug = true;
                                break;
                            #endregion
                            case _parameter.fileInputArg1:
                            case _parameter.fileInputArg2:
                                #region 输入文件配置
                                i++;
                                try
                                {
                                    inputFile = File.OpenText(@$"{args[i]}");
                                }
                                catch (UnauthorizedAccessException UAE)
                                {
                                    logger(_loggerT.Fail, $"文件权限不足\n");
                                    logger(_loggerT.DEBUG, $"{UAE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (ArgumentException AE)
                                {
                                    logger(_loggerT.Fail, $"文件未找到\n");
                                    logger(_loggerT.DEBUG, $"{AE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (PathTooLongException PTLE)
                                {
                                    logger(_loggerT.Fail, $"输入文件名过长\n");
                                    logger(_loggerT.DEBUG, $"{PTLE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (DirectoryNotFoundException DNFE)
                                {
                                    logger(_loggerT.Fail, $"指定的路径无效\n");
                                    logger(_loggerT.DEBUG, $"{DNFE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (FileNotFoundException FNFE)
                                {
                                    logger(_loggerT.Fail, $"未找到指定的文件\n");
                                    logger(_loggerT.DEBUG, $"{FNFE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (NotSupportedException NSE)
                                {
                                    logger(_loggerT.Fail, $"输入文件有误\n");
                                    logger(_loggerT.DEBUG, $"{NSE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                finally
                                {
                                }
                                break;
                            #endregion
                            case _parameter.fileOutArg1:
                            case _parameter.fileOutArg2:
                                #region 输出文件配置
                                i++;
                                try
                                {
                                    outputFile = new StreamWriter(@$"{args[i]}");
                                }
                                catch (UnauthorizedAccessException UAE)
                                {
                                    logger(_loggerT.Fail, $"无法创建文件，文件权限不足\n");
                                    logger(_loggerT.DEBUG, $"{UAE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (ArgumentException AE)
                                {
                                    logger(_loggerT.Fail, $"无输出文件\n");
                                    logger(_loggerT.DEBUG, $"{AE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (DirectoryNotFoundException DNFE)
                                {
                                    logger(_loggerT.Fail, $"指定的路径无效\n");
                                    logger(_loggerT.DEBUG, $"{DNFE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (PathTooLongException PTLE)
                                {
                                    logger(_loggerT.Fail, $"输出文件名过长\n");
                                    logger(_loggerT.DEBUG, $"{PTLE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (IOException IOE)
                                {
                                    logger(_loggerT.Fail, $"输出文件文件名非法\n");
                                    logger(_loggerT.DEBUG, $"{IOE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (SecurityException SE)
                                {
                                    logger(_loggerT.Fail, $"无权限输出文件\n");
                                    logger(_loggerT.DEBUG, $"{SE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                finally
                                {

                                }
                                break;
                            #endregion
                            case _parameter.boxArg1:
                            case _parameter.boxArg2:
                                #region 单箱边长
                                i++;
                                var apr = args[i].Split(',');
                                if (apr.Length != 3)
                                {
                                    logger(_loggerT.Fail, $"单箱参数个数有误\n");
                                    Environment.Exit(-1);
                                }
                                else
                                {
                                    try
                                    {
                                        for (var t = 0; t < 3; t++)
                                        {
                                            boxSideLength[t] = Convert.ToUInt64(apr[t]);
                                            if (boxSideLength[t] == 0)
                                            {
                                                logger(_loggerT.Fail, $"单箱边长不能为0\n");
                                                Environment.Exit(-1);
                                            }
                                        }
                                    }
                                    catch (FormatException FE)
                                    {
                                        logger(_loggerT.Fail, $"单箱边长非整或存在特殊符号\n");
                                        logger(_loggerT.DEBUG, $"{FE.Message}\n");
                                        Environment.Exit(-1);
                                    }
                                    catch (OverflowException OFE)
                                    {
                                        logger(_loggerT.Fail, $"单箱边长大小超出边界\n");
                                        logger(_loggerT.DEBUG, $"{OFE.Message}\n");
                                        Environment.Exit(-1);
                                    }
                                }
                                break;
                            #endregion
                            case _parameter.densityArg1:
                            case _parameter.densityArg2:
                                #region 粒子阈值密度
                                i++;
                                try
                                {
                                    density = Convert.ToUInt64(args[i]);
                                    if (density == 0)
                                    {
                                        logger(_loggerT.Fail, $"粒子阈值密度不能为0\n");
                                        Environment.Exit(-1);
                                    }
                                }
                                catch (FormatException FE)
                                {
                                    logger(_loggerT.Fail, $"粒子阈值密度非整或存在特殊符号\n");
                                    logger(_loggerT.DEBUG, $"{FE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (OverflowException OFE)
                                {
                                    logger(_loggerT.Fail, $"粒子阈值密度数字大小超出边界\n");
                                    logger(_loggerT.DEBUG, $"{OFE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                break;
                            #endregion
                            case _parameter.thresholdArg1:
                            case _parameter.thresholdArg2:
                                #region 箱数阈值
                                i++;
                                try
                                {
                                    threshold = Convert.ToUInt64(args[i]);
                                    if (threshold == 0)
                                    {
                                        logger(_loggerT.Fail, $"箱数阈值不能为0\n");
                                        Environment.Exit(-1);
                                    }
                                }
                                catch (FormatException FE)
                                {
                                    logger(_loggerT.Fail, $"箱数阈值非整或存在特殊符号\n");
                                    logger(_loggerT.DEBUG, $"{FE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                catch (OverflowException OFE)
                                {
                                    logger(_loggerT.Fail, $"箱数阈值数字大小超出边界\n");
                                    logger(_loggerT.DEBUG, $"{OFE.Message}\n");
                                    Environment.Exit(-1);
                                }
                                break;
                                #endregion
                        }
                }
                else
                {
                    DisplayHelp();
                    Environment.Exit(-1);
                }
                if (!isArgsFull())
                    Environment.Exit(-1);
            }

            //public override string ToString() => $"{(needDebug?"已开启DEBUG模式\n":"")}输入文件：{in}";            public bool IsNeedHelp() => needAnyHelp;
            public bool IsNeedDebug() => needDebug;
            public StreamReader GetStreamReader() => inputFile;
            public StreamWriter GetStreamWriter() => outputFile;
            public ulong GetBoxSideLength(int i) => boxSideLength[i];
            public ulong GetDensity() => density;
            public ulong GetThreshold() => threshold;
            public void DisplayHelp() => logger(_loggerT.None, helpInfo);
        }
        /// <summary>
        /// 日志记录分类
        /// </summary>
        private enum _loggerT { None = -2, Note = -1, Info = 0, Success = 1, Fail = 2, DEBUG = 3, Warning = 4, Alert = 5, ANS = 99 }
        /// <summary>
        /// /// 日志输出器
        /// </summary>
        /// <param name="type">记录种类</param>
        /// <param name="inf">记录信息</param>
        private static void logger(_loggerT type, string inf)
        {
            /// <summary>
            /// 各类型提示色
            /// </summary>
            ConsoleColor InfoBackColor = Console.BackgroundColor, InfoForeColor = ConsoleColor.DarkGreen;
            ConsoleColor NoteBackColor = Console.BackgroundColor, NoteForeColor = ConsoleColor.Cyan;
            ConsoleColor SuccessBackColor = Console.BackgroundColor, SuccessForeColor = ConsoleColor.DarkGreen;
            ConsoleColor FailBackColor = Console.BackgroundColor, FailForeColor = ConsoleColor.Red;
            ConsoleColor DEBUGBackColor = Console.BackgroundColor, DEBUGForeColor = ConsoleColor.DarkGray;
            ConsoleColor WarningBackColor = Console.BackgroundColor, WarningForeColor = ConsoleColor.DarkBlue;
            ConsoleColor AlertBackColor = Console.BackgroundColor, AlertForeColor = ConsoleColor.Yellow;
            ConsoleColor ANSBackColor = ConsoleColor.White, ANSForeColor = ConsoleColor.Black;
            switch (type)
            {
                case _loggerT.Info:
                    Console.BackgroundColor = InfoBackColor;
                    Console.ForegroundColor = InfoForeColor;
                    Console.Write($"[INFO] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.Note:
                    Console.BackgroundColor = NoteBackColor;
                    Console.ForegroundColor = NoteForeColor;
                    Console.Write($"[NOTE] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.Success:
                    Console.BackgroundColor = SuccessBackColor;
                    Console.ForegroundColor = SuccessForeColor;
                    Console.Write($"[+] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.Fail:
                    Console.BackgroundColor = FailBackColor;
                    Console.ForegroundColor = FailForeColor;
                    Console.Write($"[-] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.DEBUG:
                    Console.BackgroundColor = DEBUGBackColor;
                    Console.ForegroundColor = DEBUGForeColor;
                    Console.Write($"[DEBUG]: {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.Warning:
                    Console.BackgroundColor = WarningBackColor;
                    Console.ForegroundColor = WarningForeColor;
                    Console.Write($"[WARNING] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.Alert:
                    Console.BackgroundColor = AlertBackColor;
                    Console.ForegroundColor = AlertForeColor;
                    Console.Write($"[AlERT] {inf}");
                    Console.ResetColor();
                    break;
                case _loggerT.ANS:
                    Console.BackgroundColor = ANSBackColor;
                    Console.ForegroundColor = ANSForeColor;
                    Console.Write($"[ANSWER] {inf}");
                    Console.ResetColor();
                    break;
                default:
                    Console.BackgroundColor = Console.BackgroundColor;
                    Console.ForegroundColor = Console.ForegroundColor;
                    Console.Write($"{inf}");
                    Console.ResetColor();
                    break;
            }
        }

        private struct dot
        {
            public double x, y, z;

            public dot(double _x, double _y, double _z)
            { x = _x; y = _y; z = _z; }

            public override string ToString() => $"({x:.000},{y:.000},{z:.000})";

            public string ToString(char sep) => $"{x:.000}{sep}{y:.000}{sep}{z:.000}";

        }

        /// <summary>
        /// 单原子数据结构
        /// </summary>
        private struct ATOM
        {
            public ulong id;
            public int type;
            public dot pos;
            public ATOM(ulong _id, int _type, double _x, double _y, double _z)
            { id = _id; type = _type; pos.x = _x; pos.y = _y; pos.z = _z; }
            public ATOM(ulong _id, int _type, dot _dot)
            { id = _id; type = _type; pos.x = _dot.x; pos.y = _dot.y; pos.z = _dot.z; }
        }

        /// <summary>
        /// 单时间帧数据结构
        /// </summary>
        private struct Unit
        {
            public readonly Guid uuid;
            public readonly ulong timestep;
            public readonly ulong atomsNum;
            public readonly double[,] boxEdge = new double[3, 2];
            public ATOM[] atomsData;
            public Unit(ulong _timestep, ulong _atomsNum, double[,] _boxEdge)
            {
                this.uuid = Guid.NewGuid();
                this.timestep = _timestep;
                this.atomsNum = _atomsNum;
                this.atomsData = new ATOM[_atomsNum];
                this.boxEdge = _boxEdge;
            }
            /// <summary>
            /// 根据字符串类型向atomsData结构添加原子
            /// </summary>
            /// <param name="input"></param>
            public void AddAtom(String input)
            {
                var _input = input.Split(' ');
                Debug.Assert(_input.Length == 5);

                ulong _id = Convert.ToUInt64(_input[0]);
                int _type = Convert.ToInt32(_input[1]);
                double _x = Convert.ToDouble(_input[2]), _y = Convert.ToDouble(_input[3]), _z = Convert.ToDouble(_input[4]);
                ATOM _atom = new ATOM(_id, _type, _x, _y, _z);
                this.atomsData[_id - 1] = _atom;
                // x边界溢出检查
                if (_x < this.boxEdge[0, 0])
                {
                    this.boxEdge[0, 0] = _x;
                    logger(_loggerT.Warning, $"ATOM:{_id} [x]:{_x:0.000} out of bound [{this.boxEdge[0, 0]:0.000},{this.boxEdge[0, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[0, 0]:0.000},{this.boxEdge[0, 1]:0.000}) established{Environment.NewLine}");
                }
                else if (this.boxEdge[0, 1] <= _x)
                {
                    this.boxEdge[0, 1] = _x + 1;
                    logger(_loggerT.Warning, $"ATOM:{_id} [x]:{_x:0.000} out of bound [{this.boxEdge[0, 0]:0.000},{this.boxEdge[0, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[0, 0]:0.000},{this.boxEdge[0, 1]:0.000}) established{Environment.NewLine}");
                }
                // y界溢出检查
                if (_y < this.boxEdge[1, 0])
                {
                    this.boxEdge[1, 0] = _y;
                    logger(_loggerT.Warning, $"ATOM:{_id} [y]:{_y:0.000} out of bound [{this.boxEdge[1, 0]:0.000},{this.boxEdge[1, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[1, 0]:0.000},{this.boxEdge[1, 1]:0.000}) established{Environment.NewLine}");
                }
                else if (this.boxEdge[1, 1] <= _y)
                {
                    this.boxEdge[1, 1] = _y + 1;
                    logger(_loggerT.Warning, $"ATOM:{_id} [y]:{_y:0.000} out of bound [{this.boxEdge[1, 0]:0.000},{this.boxEdge[1, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[1, 0]:0.000},{this.boxEdge[1, 1]:0.000}) established{Environment.NewLine}");
                }
                // z边界溢出检查
                if (_z < this.boxEdge[2, 0])
                {
                    this.boxEdge[2, 0] = _z;
                    logger(_loggerT.Warning, $"ATOM:{_id} [z]:{_z:0.000} out of bound [{this.boxEdge[2, 0]:0.000},{this.boxEdge[2, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[2, 0]:0.000},{this.boxEdge[2, 1]:0.000}) established{Environment.NewLine}");
                }
                else if (this.boxEdge[2, 1] <= _z)
                {
                    this.boxEdge[2, 1] = _z + 1;
                    logger(_loggerT.Warning, $"ATOM:{_id} [z]:{_z:0.000} out of bound [{this.boxEdge[2, 0]:0.000},{this.boxEdge[2, 1]:0.000}){Environment.NewLine}" +
                    $"\tnew bound [{this.boxEdge[2, 0]:0.000},{this.boxEdge[2, 1]:0.000}) established{Environment.NewLine}");
                }
            }

            /// <summary>
            /// 根据a,b,c模糊箱长与atomLimit限制计算atomsData中孔洞数量与孔洞信息
            /// </summary>
            /// <param name="a">模糊箱长x</param>
            /// <param name="b">模糊箱长y</param>
            /// <param name="c">模糊箱长z</param>
            /// <param name="atomLimit">模糊箱粒子个数</param>
            /// <returns></returns>
            public ulong CountHole(ulong a, ulong b, ulong c, ulong atomLimit, ulong threshold)
            {
                // 各轴分离箱数
                ulong x_n = (ulong)(Math.Abs(boxEdge[0, 1] - boxEdge[0, 0])) / a;
                ulong y_n = (ulong)(Math.Abs(boxEdge[1, 1] - boxEdge[1, 0])) / b;
                ulong z_n = (ulong)(Math.Abs(boxEdge[2, 1] - boxEdge[2, 0])) / c;
                // 各轴箱单位长
                double x_i = Math.Abs(boxEdge[0, 1] - boxEdge[0, 0]) / (double)(x_n);
                double y_i = Math.Abs(boxEdge[1, 1] - boxEdge[1, 0]) / (double)(y_n);
                double z_i = Math.Abs(boxEdge[2, 1] - boxEdge[2, 0]) / (double)(z_n);

                #region 内存占用声明拓展
                List<List<List<List<ATOM>>>> box = new List<List<List<List<ATOM>>>>();
                for (ulong i = 0; i < x_n; i++)
                {
                    box.Add(new List<List<List<ATOM>>>());
                    for (ulong j = 0; j < y_n; j++)
                    {
                        box[(int)i].Add(new List<List<ATOM>>());
                        for (ulong k = 0; k < z_n; k++)
                            box[(int)i][(int)j].Add(new List<ATOM>());
                    }
                }
                #endregion

                #region 扔箱
                for (ulong i = 0; i < this.atomsNum; i++)
                {
                    var tar = this.atomsData[i];
                    for (int j = 0; j < 3; j++)
                        Debug.Assert(boxEdge[j, 0] < boxEdge[j, 1]);
#if debug
                    logger(_loggerT.DEBUG, $"ATOM:{tar.id}\t{(int)((tar.pos.x - this.boxEdge[0, 0]) / x_i)}\t" +
                    $"{(int)((tar.pos.y - this.boxEdge[1, 0]) / y_i)}\t" +
                    $"{(int)((tar.pos.z - this.boxEdge[2, 0]) / z_i)}{Environment.NewLine}");
                    logger(_loggerT.DEBUG, $"{this.boxEdge[0, 0]} - {tar.pos.x} - {this.boxEdge[0, 1]}\t" +
                     $"{this.boxEdge[1, 0]} - {tar.pos.y} - {this.boxEdge[1, 1]}\t" +
                     $"{this.boxEdge[2, 0]} - {tar.pos.z} - {this.boxEdge[2, 1]}{Environment.NewLine}");
                    Debug.Assert(tar.pos.x >= this.boxEdge[0, 0] && tar.pos.x <= this.boxEdge[0, 1]);
                    Debug.Assert(tar.pos.y >= this.boxEdge[1, 0] && tar.pos.y <= this.boxEdge[1, 1]);
                    Debug.Assert(tar.pos.z >= this.boxEdge[2, 0] && tar.pos.z <= this.boxEdge[2, 1]);
#endif
                    box[(int)((tar.pos.x - this.boxEdge[0, 0]) / x_i)]
                    [(int)((tar.pos.y - this.boxEdge[1, 0]) / y_i)]
                    [(int)((tar.pos.z - this.boxEdge[2, 0]) / z_i)].Add(tar);
                }
                #endregion
#if debug
                for (ulong i = 0; i < x_n; i++)
                {
                    logger(_loggerT.None, $"#x:{i}");
                    for (ulong k = 0; k < z_n; k++)
                        logger(_loggerT.None, $"\t#z:[{k}]");
                    logger(_loggerT.None, $"{Environment.NewLine}");
                    for (ulong j = 0; j < y_n; j++)
                    {
                        logger(_loggerT.None, $"y:[{j}]");
                        for (ulong k = 0; k < z_n; k++)
                            logger(_loggerT.None, $"\t{box[(int)i][(int)j][(int)k].Count}");
                        logger(_loggerT.None, $"{Environment.NewLine}");
                    }
                    logger(_loggerT.None, $"{Environment.NewLine}");
                }
#endif
                #region BC二元分类
                bool[,,] box_score = new bool[x_n, y_n, z_n];
                for (ulong i = 0; i < x_n; i++)
                    for (ulong j = 0; j < y_n; j++)
                        for (ulong k = 0; k < z_n; k++)
                            box_score[i, j, k] = ((ulong)(box[(int)i][(int)j][(int)k].Count) <= atomLimit);
                #endregion
#if debug
                for (ulong i = 0; i < x_n; i++)
                {
                    logger(_loggerT.None, $"#{i}:{Environment.NewLine}");
                    for (ulong k = 0; k < z_n + 2; k++)
                        logger(_loggerT.None, $"*");
                    logger(_loggerT.None, $"{Environment.NewLine}");
                    for (ulong j = 0; j < y_n; j++)
                    {
                        logger(_loggerT.None, $"*");
                        for (ulong k = 0; k < z_n; k++)
                            logger(_loggerT.None, (box_score[i, j, k] ? "#" : " "));
                        logger(_loggerT.None, $"*{Environment.NewLine}");
                    }
                    for (ulong k = 0; k < z_n + 2; k++)
                        logger(_loggerT.None, $"*");
                    logger(_loggerT.None, $"{Environment.NewLine}");
                }
#endif
                #region 前端BFS算法
                ulong holesNum = 0;
                for (long i = 0; (ulong)i < x_n; i++)
                    for (long j = 0; (ulong)j < y_n; j++)
                        for (long k = 0; (ulong)k < z_n; k++)
                            if (box_score[i, j, k])
                            {
                                var result = frontBFS(ref box_score, new long[] { i, j, k }, 0);
                                var tmpResult = result;
                                while (true)
                                {
                                    var moreResult = new List<dot>();
                                    foreach (var item in tmpResult)
                                        if ((long)item.x > i && (long)item.y > j && (long)item.z > k)
                                            moreResult.AddRange(frontBFS(ref box_score, new long[] { (long)item.x, (long)item.y, (long)item.z }, 0));
                                    if (moreResult.Count == 0)
                                    { break; }
                                    else
                                    {
                                        result.AddRange(moreResult);
                                        tmpResult = moreResult;
                                    }
                                };
                                if ((ulong)result.Count > threshold)
                                {
                                    holesNum++;
                                    // logger(_loggerT.None, "{Environment.NewLine}");
                                    // logger(_loggerT.Success, $"Hole #{holesNum}:{Environment.NewLine}");
                                    // logger(_loggerT.Info, $"BoxNum:{result.Count}{Environment.NewLine}");
                                    ulong type1Num = 0, type2Num = 0;
                                    foreach (var item in result)
                                    {
                                        // logger(_loggerT.None, $"id\ttype\tx.\ty\tz{Environment.NewLine}");
                                        foreach (var atom_item in box[(int)item.x][(int)item.y][(int)item.z])
                                        {
                                            // logger(_loggerT.None, $"{atom_item.id}\t{atom_item.type}\t{atom_item.pos.ToString('\t')}{Environment.NewLine}");
                                            if (atom_item.type == 1) type1Num++; else type2Num++;
                                        }
                                    }
                                    logger(_loggerT.Info, $"AtomNum:{type1Num + type2Num}\ttype1Num:{type1Num}\\{((double)type1Num * 100 / (double)(type1Num + type2Num)):0.00}%\ttype2Num:{type2Num}\\{((double)type2Num * 100 / (double)(type1Num + type2Num)):0.00}%{Environment.NewLine}");
                                }
                            }
                #endregion

                return holesNum;
            }
        }
        /// <summary>
        /// 返回连接的所有为T的坐标集,一次最多深度为MAX_DEPTH,防止stack overflow，会截止，数据会不全，需多次遍历
        /// </summary>
        /// <param name="box_score">三维BC分类表bool[x,y,z]</param>
        /// <param name="loc">初始坐标{x,y,z}</param>
        /// <returns></returns>
        static List<dot> frontBFS(ref bool[,,] box_score, long[] loc, int depth)
        {
            //logger(_loggerT.None, $"{loc[0]},{loc[1]},{loc[2]}\t");
            List<dot> dots = new List<dot>();
            //Debug.Assert(box_score[loc[0], loc[1], loc[2]]);
            if (box_score[loc[0], loc[1], loc[2]])
            {
                //logger(_loggerT.DEBUG, $"ADD dot[{loc[0]}, {loc[1]}, {loc[2]}]{Environment.NewLine}");
                box_score[loc[0], loc[1], loc[2]] = false;
                dots.Add(new dot(loc[0], loc[1], loc[2]));
            }
            for (ulong i = 0; i < 3; i++)
            {
                var x = loc[0] + (i == 0 ? 1 : 0);
                var y = loc[1] + (i == 1 ? 1 : 0);
                var z = loc[2] + (i == 2 ? 1 : 0);
                if (loc[i] == box_score.GetLength((int)i) - 1)
                {
                    x = (i == 0 ? 0 : x);
                    y = (i == 1 ? 0 : y);
                    z = (i == 2 ? 0 : z);
                }
                if (box_score[x, y, z])
                    if (depth < MAX_DEPTH)
                        dots.AddRange(frontBFS(ref box_score, new long[] { x, y, z }, ++depth));

                x = loc[0] - (i == 0 ? 1 : 0);
                y = loc[1] - (i == 1 ? 1 : 0);
                z = loc[2] - (i == 2 ? 1 : 0);
                if (loc[i] == 0)
                {
                    x = (i == 0 ? box_score.GetLength((int)i) - 1 : x);
                    y = (i == 1 ? box_score.GetLength((int)i) - 1 : y);
                    z = (i == 2 ? box_score.GetLength((int)i) - 1 : z);
                }
                if (box_score[x, y, z])
                    if (depth < MAX_DEPTH)
                        dots.AddRange(frontBFS(ref box_score, new long[] { x, y, z }, ++depth));
            }
            return dots;
        }

#if debug
        static void disUnitInfo(Unit unit)
        {
            logger(_loggerT.DEBUG, $"{unit.uuid.ToString("B")}{Environment.NewLine}" +
            $"TIMESTEP:{unit.timestep}{Environment.NewLine}" +
            $"Number of atoms:{unit.atomsNum}{Environment.NewLine}" +
            $"Box bounds:{Environment.NewLine}" +
            $"{unit.boxEdge[0, 0]}\t{unit.boxEdge[0, 1]}{Environment.NewLine}" +
            $"{unit.boxEdge[1, 0]}\t{unit.boxEdge[1, 1]}{Environment.NewLine}" +
            $"{unit.boxEdge[2, 0]}\t{unit.boxEdge[2, 1]}{Environment.NewLine}"
            );
            logger(_loggerT.DEBUG, "Please input id to get atom data:");
            string _index_str = String.Empty;
            try
            {
                while ((_index_str = Console.ReadLine()) != String.Empty)
                {
                    ulong _index = Convert.ToUInt64(_index_str);
                    var tmp = unit.atomsData[_index - 1];
                    logger(_loggerT.None, $"id\ttype\tx\ty\tz{Environment.NewLine}"
                    + $"{tmp.id}\t{tmp.type}\t{tmp.pos.x}\t{tmp.pos.y}\t{tmp.pos.z}{Environment.NewLine}");
                    logger(_loggerT.DEBUG, "Please input id to get atom data:");
                }
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
#endif
        static void Main(string[] args)
        {
            // 转化参数
            parameter _para = new parameter(args);
            bool DebugFlag = _para.IsNeedDebug();
            // 模糊边长ABC
            // ulong a = 15, b = 15, c = 15, atomLimit = 50;

            #region 初始化，系统检查
            if (Stopwatch.IsHighResolution)
                logger(_loggerT.Success, "HRPC ENABLED{Environment.NewLine}");
            else
                logger(_loggerT.Fail, "high-resolution performance counter is not supported on this device{Environment.NewLine}");
            logger(_loggerT.Info, $"TF:{Stopwatch.Frequency},ACC:{(1000L * 1000L * 1000L) / Stopwatch.Frequency} ns{Environment.NewLine}");
            #endregion

            Stopwatch watch = new Stopwatch();
            Stopwatch watchA = new Stopwatch();

            using (StreamReader lammp = _para.GetStreamReader())
            using (StreamWriter newFile = _para.GetStreamWriter())
            {
                watchA.Restart();
                while (!lammp.EndOfStream)
                {
                    // SpinWait.SpinUntil(() => false, 2000);
                    #region 程序主结构
                    watch.Restart();
                    string input = String.Empty;
                    #region 读取数据头
                    ulong _timestep = new ulong();
                    ulong _atomsNum = new ulong();
                    double[,] _boxEdge = new double[3, 2];
                    for (var i = 0; i < 9; i++)
                        if ((input = lammp.ReadLine()) != null)
                            switch (i)
                            {
                                case 0:
                                    Debug.Assert(input == "ITEM: TIMESTEP");
                                    break;
                                case 1:
                                    _timestep = Convert.ToUInt64(input);
                                    break;
                                case 2:
                                    Debug.Assert(input == "ITEM: NUMBER OF ATOMS");
                                    break;
                                case 3:
                                    _atomsNum = Convert.ToUInt64(input);
                                    break;
                                case 4:
                                    Debug.Assert(input == "ITEM: BOX BOUNDS pp pp pp");
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                    // Convert.ToDouble(s)
                                    _boxEdge[i - 5, 0] = Convert.ToDouble(input.Split(' ')[0]);
                                    _boxEdge[i - 5, 1] = Convert.ToDouble(input.Split(' ')[1]);
                                    break;
                                case 8:
                                    Debug.Assert(input == "ITEM: ATOMS id type x y z");
                                    break;
                                default:
                                    Debug.Assert(false);
                                    break;
                            }
                        else
                            Debug.Assert(false);
                    #endregion

                    Unit unit = new Unit(_timestep, _atomsNum, _boxEdge);
                    logger(_loggerT.Note, $"{DateTime.Now}{Environment.NewLine}");
                    logger(_loggerT.Note, $"{unit.uuid.ToString("B")}{Environment.NewLine}");
                    logger(_loggerT.Note, $"TIMESTEP:{unit.timestep}{Environment.NewLine}");

                    #region 读取数据
                    for (ulong i = 0; i < unit.atomsNum; i++)
                        if ((input = lammp.ReadLine()) != null)
                            unit.AddAtom(input);
                        else
                            Debug.Assert(false);
                    #endregion

                    logger(_loggerT.ANS, $"{unit.timestep} {unit.CountHole(_para.GetBoxSideLength(0), _para.GetBoxSideLength(1), _para.GetBoxSideLength(2), _para.GetDensity(), _para.GetThreshold())}{Environment.NewLine}");
                    logger(_loggerT.None, $"{Environment.NewLine}");
#if debug
                disUnitInfo(unit);
#endif
                    watch.Stop();
                    logger(_loggerT.Note, $"TIME:{(double)watch.ElapsedMilliseconds / 1000:0.0000} secs{Environment.NewLine}");
                    #endregion
                }
                watchA.Stop();
                logger(_loggerT.Note, $"TOTAL_TIME:{(double)watchA.ElapsedMilliseconds / 1000:0.0000} secs{Environment.NewLine}");
                Console.WriteLine($"TOTAL_TIME:{(double)watchA.ElapsedMilliseconds / 1000:0.0000} secs{Environment.NewLine}");
            }

            logger(_loggerT.Note, "Press any key to exit.");
            System.Console.ReadKey();
        }
    }

}